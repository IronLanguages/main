/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Calls {

    public class RubyMemberInfo {
        // Singleton used to undefine methods: stops method resolution
        internal static readonly RubyMemberInfo/*!*/ UndefinedMethod = new RubyMemberInfo(RubyMemberFlags.Empty);

        // Singleton used to hide CLR methods: method resolution skips all CLR methods since encountering a hidden method.
        internal static readonly RubyMemberInfo/*!*/ HiddenMethod = new RubyMemberInfo(RubyMemberFlags.Empty);

        // Singleton used to represent interop members (these are not in method tables). This includes foreign meta-object members and CLR members.
        // Interop member represents a Ruby-public method.
        internal static readonly RubyMemberInfo/*!*/ InteropMember = new RubyMemberInfo(RubyMemberFlags.Public);

        private readonly RubyMemberFlags _flags;

        //
        // A method body can be shared by multiple method definitions, one of them is the primary definition and the others are its copies.
        // Only three cases of sharing are allowed:
        // 1) The primary definition's declaring module is a super-class of the copied definition.
        // 2) The primary definition's declaring module was duplicated and method copies are defined in the duplicate.
        // 3) The primary definition's declaring module is not a class.
        // 
        // We assume these restrictions in the super-call implementation and instance variable storage allocation.
        // See also: instance_method, method, define_method, module_function, private, protected, public.
        //
        // DeclaringModule is null for dummy methods.
        //
        private readonly RubyModule _declaringModule;
        
        #region Mutable state guarded by ClassHierarchyLock

        private bool _invalidateSitesOnOverride;
        private bool _invalidateGroupsOnRemoval;

        #endregion

        public RubyMethodVisibility Visibility {
            get { return (RubyMethodVisibility)(_flags & RubyMemberFlags.VisibilityMask); }
        }

        //
        // Notes on visibility
        // 
        // Ruby visibility is orthogonal to CLR visibility.
        // Ruby visibility is mutable, CLR visibility is not.
        // A method group can comprise of methods of various CLR visibility. Ruby visibility applies on the group as a whole.
        //

        /// <summary>
        /// True if the member is Ruby-protected. 
        /// </summary>
        /// <remarks>
        /// Ruby-protected members can only be called from a scope whose self immediate class is a descendant of the method owner.
        /// CLR-protected members can only be called if the receiver is a descendant of the method owner. 
        /// </remarks>
        public bool IsProtected {
            get { return (_flags & RubyMemberFlags.Protected) != 0; }
        }

        /// <summary>
        /// True if the member is Ruby-private. 
        /// </summary>
        /// <remarks>
        /// Ruby-private members can only be called with an implicit receiver (self).
        /// CLR-private members can only be called if in PrivateBinding mode, the receiver might be explicit or implicit.
        /// </remarks>
        public bool IsPrivate {
            get { return (_flags & RubyMemberFlags.Private) != 0; }
        }

        /// <summary>
        /// True if the member is Ruby-public. 
        /// </summary>
        public bool IsPublic {
            get { return (_flags & RubyMemberFlags.Public) != 0; }
        }

        internal bool IsEmpty {
            get { return (_flags & RubyMemberFlags.Empty) != 0; }
        }

        internal virtual bool IsSuperForwarder {
            get { return false; }
        }

        /// <summary>
        /// True if the member is defined in Ruby or for undefined and hidden members.
        /// False for members representing CLR members.
        /// </summary>
        internal virtual bool IsRubyMember {
            get { return true; }
        }

        /// <summary>
        /// True if the member behaves like a property/field: GetMember invokes the member.
        /// Otherwise the member behaves like a method: GetMember returns the method.
        /// </summary>
        internal virtual bool IsDataMember {
            get { return false; }
        }

        /// <summary>
        /// True if the member can be permanently removed.
        /// True for attached CLR members, i.e. real CLR members and extension methods.
        /// False for detached CLR members and extension methods.
        /// If the member cannot be removed we hide it.
        /// </summary>
        internal bool IsRemovable {
            get { return IsRubyMember && !IsHidden && !IsUndefined && !IsInteropMember; }
        }

        internal RubyMemberFlags Flags {
            get { return _flags; }
        }

        /// <summary>
        /// True if the method should invalidate groups below it in the inheritance hierarchy. 
        /// </summary>
        /// <remarks>
        /// Set when a Ruby method is defined that hides a CLR overload that is used in a method group below the definition.
        /// Set when an extension method is added above the Ruby method definition and the change isn't propagated below.
        /// Undefined and Hidden method singletons cannot be removed so they don't need to be marked.
        /// </remarks>
        [DebuggerDisplay("{_invalidateGroupsOnRemoval}")]
        internal bool InvalidateGroupsOnRemoval {
            get {
                Context.RequiresClassHierarchyLock();
                return _invalidateGroupsOnRemoval;
            }
            set {
                Context.RequiresClassHierarchyLock();
                Debug.Assert(IsRemovable);
                _invalidateGroupsOnRemoval = value;
            }
        }

        /// <summary>
        /// Method definition that replaces/overrides this method will cause version update of all dependent subclasses/modules, which
        /// triggers invalidation of sites that are bound to those classes.
        /// </summary>
        [DebuggerDisplay("{_invalidateSitesOnOverride}")]
        internal bool InvalidateSitesOnOverride {
            get {
                Context.RequiresClassHierarchyLock();
                return _invalidateSitesOnOverride;
            }
        }

        internal virtual void SetInvalidateSitesOnOverride() {
            _invalidateSitesOnOverride = true;
        }

        internal static void SetInvalidateSitesOnOverride(RubyMemberInfo/*!*/ member) {
            member._invalidateSitesOnOverride = true;
        }

        public RubyModule/*!*/ DeclaringModule {
            get {
                Debug.Assert(_declaringModule != null);
                return _declaringModule; 
            }
        }

        public RubyContext/*!*/ Context {
            get {
                Debug.Assert(_declaringModule != null);
                return _declaringModule.Context;
            }
        }

        // TODO: 
        public virtual int GetArity() {
            return 0;
        }

        public bool IsUndefined {
            get { return ReferenceEquals(this, UndefinedMethod); }
        }

        public bool IsHidden {
            get { return ReferenceEquals(this, HiddenMethod); }
        }

        public bool IsInteropMember {
            get { return ReferenceEquals(this, InteropMember); }
        }

        // undefined, hidden, interop method:
        private RubyMemberInfo(RubyMemberFlags flags) {
            _flags = flags;
        }

        internal RubyMemberInfo(RubyMemberFlags flags, RubyModule/*!*/ declaringModule) {
            Assert.NotNull(declaringModule);
            Debug.Assert(flags != RubyMemberFlags.Invalid);

            _flags = flags;
            _declaringModule = declaringModule;
        }

        internal protected virtual RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            throw Assert.Unreachable;
        }

        public override string/*!*/ ToString() {
            return 
                IsHidden ? "<hidden>" :
                IsUndefined ? "<undefined>" :
                (GetType().Name + " " + _flags.ToString() + " (" + _declaringModule.Name + ")");
        }

        /// <summary>
        /// Gets all the CLR members represented by this member info. 
        /// </summary>
        public virtual MemberInfo/*!*/[]/*!*/ GetMembers() {
            throw Assert.Unreachable;
        }

        /// <summary>
        /// Returns a Ruby array describing parameters of the method.
        /// </summary>
        public virtual RubyArray/*!*/ GetRubyParameterArray() {
            if (_declaringModule == null) {
                return new RubyArray();
            }

            // TODO: quick approximation, we can do better

            var context = _declaringModule.Context;

            RubyArray result = new RubyArray();
            int arity = GetArity();

            int mandatoryCount = (arity < 0) ? -arity - 1 : arity;
            var reqSymbol = context.CreateAsciiSymbol("req");
            for (int i = 0; i < mandatoryCount; i++) {
                result.Add(new RubyArray { reqSymbol });
            }

            if (arity < 0) {
                result.Add(new RubyArray { context.CreateAsciiSymbol("rest") });
            }

            return result;
        }

        /// <summary>
        /// Returns a copy of this member info that groups only those members of this member info that are generic
        /// and of generic arity equal to the length of the given array of type arguments. Returns null if there are no such generic members.
        /// All the members in the resulting info are constructed generic methods bound to the given type arguments.
        /// </summary>
        public virtual RubyMemberInfo TryBindGenericParameters(Type/*!*/[]/*!*/ typeArguments) {
            return null;
        }

        public virtual RubyMemberInfo TrySelectOverload(Type/*!*/[]/*!*/ parameterTypes) {
            throw Assert.Unreachable;
        }

        #region Dynamic Operations

        internal virtual MemberDispatcher GetDispatcher(Type/*!*/ delegateType, RubyCallSignature signature, object target, int version) {
            return null;
        }

        internal virtual void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            throw Assert.Unreachable;
        }

        internal virtual void BuildMethodMissingCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            args.InsertMethodName(name);
            BuildCallNoFlow(metaBuilder, args, Symbols.MethodMissing);
        }

        internal void BuildCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            BuildCallNoFlow(metaBuilder, args, name);
            metaBuilder.BuildControlFlow(args);
        }

        internal void BuildMethodMissingCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            BuildMethodMissingCallNoFlow(metaBuilder, args, name);
            metaBuilder.BuildControlFlow(args);
        }

        internal virtual void BuildSuperCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name, RubyModule/*!*/ declaringModule) {
            BuildCallNoFlow(metaBuilder, args, name);
        }

        internal void BuildSuperCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name, RubyModule/*!*/ declaringModule) {
            BuildSuperCallNoFlow(metaBuilder, args, name, declaringModule);
            metaBuilder.BuildControlFlow(args);
        }

        #endregion
    }
}
