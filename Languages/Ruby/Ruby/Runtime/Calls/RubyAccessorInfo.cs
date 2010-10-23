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
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using AstFactory = IronRuby.Compiler.Ast.AstFactory;

    public abstract class RubyAttributeAccessorInfo : RubyMemberInfo {
        private readonly string/*!*/ _instanceVariableName;

        protected string/*!*/ InstanceVariableName { get { return _instanceVariableName; } }

        protected RubyAttributeAccessorInfo(RubyMemberFlags flags, RubyModule/*!*/ declaringModule, string/*!*/ variableName)
            : base(flags, declaringModule) {
            Assert.NotEmpty(variableName);
            Debug.Assert(variableName.StartsWith("@"));
            _instanceVariableName = variableName;
        }

        internal override bool IsDataMember {
            get { return true; }
        }

        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return Utils.EmptyMemberInfos;
        }
    }

    public sealed class RubyAttributeReaderInfo : RubyAttributeAccessorInfo {
        public RubyAttributeReaderInfo(RubyMemberFlags flags, RubyModule/*!*/ declaringModule, string/*!*/ variableName)
            : base(flags, declaringModule, variableName) {
        }

        internal override MemberDispatcher GetDispatcher(Type/*!*/ delegateType, RubyCallSignature signature, object target, int version) {
            if (!(target is IRubyObject)) {
                return null;
            } 
            
            if (signature.ArgumentCount != 0 || signature.HasRhsArgument || signature.HasBlock || !signature.HasScope) {
                return null;
            }

            var dispatcher = new RubyObjectAttributeReaderDispatcherWithScope();
            dispatcher.Initialize(InstanceVariableName, version);
            return dispatcher;
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
            if (!metaBuilder.Error) {
                metaBuilder.Result = Methods.GetInstanceVariable.OpCall(
                    AstUtils.Convert(args.MetaScope.Expression, typeof(RubyScope)),
                    AstUtils.Box(args.TargetExpression),
                    AstUtils.Constant(InstanceVariableName)
                );
            }
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyAttributeReaderInfo(flags, module, InstanceVariableName);
        }

        public override RubyMemberInfo TrySelectOverload(Type/*!*/[]/*!*/ parameterTypes) {
            return parameterTypes.Length == 0 ? this : null;
        }
    }

    public sealed class RubyAttributeWriterInfo : RubyAttributeAccessorInfo {
        public RubyAttributeWriterInfo(RubyMemberFlags flags, RubyModule/*!*/ declaringModule, string/*!*/ name)
            : base(flags, declaringModule, name) {
        }

        internal override MemberDispatcher GetDispatcher(Type/*!*/ delegateType, RubyCallSignature signature, object target, int version) {
            if (!(target is IRubyObject)) {
                return null;
            } 
            
            if (signature.ArgumentCount + (signature.HasRhsArgument ? 1 : 0) != 1 || signature.HasBlock) {
                return null;
            }

            return AttributeDispatcher.CreateRubyObjectWriterDispatcher(delegateType, InstanceVariableName, version);
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            var actualArgs = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 1, 1);
            if (!metaBuilder.Error) {
                metaBuilder.Result = Methods.SetInstanceVariable.OpCall(
                    AstUtils.Box(args.TargetExpression),
                    AstUtils.Box(actualArgs[0].Expression),
                    AstUtils.Convert(args.MetaScope.Expression, typeof(RubyScope)),
                    AstUtils.Constant(InstanceVariableName)
                );
            }
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyAttributeWriterInfo(flags, module, InstanceVariableName);
        }

        public override RubyMemberInfo TrySelectOverload(Type/*!*/[]/*!*/ parameterTypes) {
            return parameterTypes.Length == 1 && parameterTypes[0] == typeof(object) ? this : null;
        }
    }
}
