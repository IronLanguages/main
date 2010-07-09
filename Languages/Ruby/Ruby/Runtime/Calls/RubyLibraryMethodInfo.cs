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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler.Generation;
using IronRuby.Compiler;

using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;
    
    /// <summary>
    /// Performs method binding for calling CLR methods.
    /// Currently this is used for all builtin libary methods and interop calls to CLR methods
    /// </summary>
    public sealed class RubyLibraryMethodInfo : RubyMethodGroupBase {
        private readonly LibraryOverload/*!*/[]/*!*/ _overloads;

        /// <summary>
        /// Creates a Ruby method implemented by a method group of CLR methods.
        /// </summary>
        internal RubyLibraryMethodInfo(LibraryOverload/*!*/[]/*!*/ overloads, RubyMemberFlags flags, RubyModule/*!*/ declaringModule)
            : base(null, flags, declaringModule) {
            Assert.NotNullItems(overloads);
            Assert.NotEmpty(overloads);
            _overloads = overloads;
        }

        public RubyLibraryMethodInfo(LibraryOverload/*!*/[]/*!*/ overloads, RubyMethodVisibility visibility, RubyModule/*!*/ declaringModule) 
            : this(overloads, (RubyMemberFlags)visibility & RubyMemberFlags.VisibilityMask, declaringModule) {
            ContractUtils.RequiresNotNull(declaringModule, "declaringModule");
            ContractUtils.RequiresNotNullItems(overloads, "overloads");
        }

        // copy ctor
        private RubyLibraryMethodInfo(RubyLibraryMethodInfo/*!*/ info, OverloadInfo/*!*/[]/*!*/ methods)
            : base(methods, info.Flags, info.DeclaringModule) {
        }

        internal LibraryOverload/*!*/[]/*!*/ Overloads {
            get { return _overloads; }
        }

        internal override SelfCallConvention CallConvention {
            get { return SelfCallConvention.SelfIsParameter; }
        }

        internal override bool ImplicitProtocolConversions {
            get { return false; }
        }

        internal protected override OverloadInfo/*!*/[]/*!*/ MethodBases {
            get {
                Debug.Assert(base.MethodBases != null || _overloads != null);

                // don't need to lock MethodBases since all values calculated by multiple threads are the same: 
                return base.MethodBases ?? SetMethodBasesNoLock(_overloads);
            }
        }

        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return ArrayUtils.ConvertAll(MethodBases, (o) => o.ReflectionInfo);
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyLibraryMethodInfo(_overloads, flags, module);
        }

        protected override RubyMemberInfo/*!*/ Copy(OverloadInfo/*!*/[]/*!*/ methods) {
            return new RubyLibraryMethodInfo(this, methods);
        }

        internal override MemberDispatcher GetDispatcher(Type/*!*/ delegateType, RubyCallSignature signature, object target, int version) {
            if (!(target is IRubyObject)) {
                return null;
            }

            int arity;
            if (!IsEmpty || (arity = GetArity()) != 1) {
                return null;
            }

            return MethodDispatcher.CreateRubyObjectDispatcher(
                delegateType, new Func<object, Proc, object, object>(EmptyRubyMethodStub1), arity, signature.HasScope, signature.HasBlock, version
            );
        }

        public static object EmptyRubyMethodStub1(object self, Proc block, object arg0) {
            // nop
            return null;
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            BuildCallNoFlow(metaBuilder, args, name, MethodBases, CallConvention, ImplicitProtocolConversions);
        }
    }
}

