/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using System.Diagnostics;

namespace IronRuby.Runtime.Calls {

    public class RubyMemberInfo {
        // Singleton used to undefine methods: stops method resolution
        internal static readonly RubyMemberInfo/*!*/ UndefinedMethod = new RubyMemberInfo();
        // Singleton used to hide CLR methods: doesn't stop method resolution, skips CLR method lookup
        internal static readonly RubyMemberInfo/*!*/ HiddenMethod = new RubyMemberInfo();

        private readonly RubyMemberFlags _flags;
        private bool _invalidateSitesOnOverride;

        // Method, UnboundMethod, super, trace: Aliased methods preserve the declaring module.
        // Null for dummy methods.
        private readonly RubyModule _declaringModule;

        public RubyMethodVisibility Visibility {
            get { return (RubyMethodVisibility)(_flags & RubyMemberFlags.VisibilityMask); }            
        }

        internal bool IsModuleFunction {
            get { return (_flags & RubyMemberFlags.ModuleFunction) != 0; }
        }

        internal bool IsEmpty {
            get { return (_flags & RubyMemberFlags.Empty) != 0; }
        }

        internal RubyMemberFlags Flags {
            get { return _flags; }
        }

        /// <summary>
        /// Method definition that replaces/overrides this method will cause version update of all dependent subclasses/modules, which
        /// triggers invalidation of sites that are bound to those classes.
        /// </summary>
        internal bool InvalidateSitesOnOverride {
            get { return _invalidateSitesOnOverride; }
            set { _invalidateSitesOnOverride = value; }
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
        public virtual int Arity {
            get { return 0; } 
        }

        public bool IsUndefined {
            get { return ReferenceEquals(this, UndefinedMethod); }
        }

        public bool IsHidden {
            get { return ReferenceEquals(this, HiddenMethod); }
        }

        // undefined, hidden method:
        private RubyMemberInfo() {
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

        internal virtual void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            throw Assert.Unreachable;
        }

        internal void BuildCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            BuildCallNoFlow(metaBuilder, args, name);
            metaBuilder.BuildControlFlow(args);
        }

        internal virtual void BuildSuperCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name, RubyModule/*!*/ declaringModule) {
            BuildCallNoFlow(metaBuilder, args, name);
        }

        internal void BuildSuperCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name, RubyModule/*!*/ declaringModule) {
            BuildSuperCallNoFlow(metaBuilder, args, name, declaringModule);
            metaBuilder.BuildControlFlow(args);
        }
    }
}
