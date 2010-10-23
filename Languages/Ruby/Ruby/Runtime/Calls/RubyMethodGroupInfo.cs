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
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions.Calls;

namespace IronRuby.Runtime.Calls {
    using Ast = MSA.Expression;

    /// <summary>
    /// A group of CLR methods that are treated as a single Ruby method.
    /// </summary>
    public class RubyMethodGroupInfo : RubyMethodGroupBase {
        // True: The group contains only static methods and can only be called statically (with no receiver).
        // False: The group contain instance methods and/or extension methods, or operators.
        private readonly bool _isStatic;

        internal RubyMethodGroupInfo(OverloadInfo/*!*/[]/*!*/ methods, RubyModule/*!*/ declaringModule, bool isStatic)
            : base(methods, RubyMemberFlags.Public, declaringModule) {
            _isStatic = isStatic;
        }

        // copy ctor
        private RubyMethodGroupInfo(RubyMethodGroupInfo/*!*/ info, RubyMemberFlags flags, RubyModule/*!*/ module)
            : base(info.MethodBases, flags, module) {
            _isStatic = info._isStatic;
        }

        // copy ctor
        private RubyMethodGroupInfo(RubyMethodGroupInfo/*!*/ info, OverloadInfo/*!*/[] methods)
            : base(methods, info.Flags, info.DeclaringModule) {
            _isStatic = info._isStatic;
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyMethodGroupInfo(this, flags, module);
        }

        protected override RubyMemberInfo/*!*/ Copy(OverloadInfo/*!*/[]/*!*/ methods) {
            return new RubyMethodGroupInfo(this, methods);
        }

        internal override SelfCallConvention CallConvention {
            get { return _isStatic ? SelfCallConvention.NoSelf : SelfCallConvention.SelfIsInstance; }
        }

        internal bool IsStatic {
            get { return _isStatic; }
        }

        internal override bool ImplicitProtocolConversions {
            get { return true; }
        }

        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return ArrayUtils.ConvertAll(MethodBases, (o) => o.ReflectionInfo);
        }
        
        #region Dynamic Call

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            var visibleOverloads = GetVisibleOverloads(args, MethodBases, false);
            if (visibleOverloads.Count == 0) {
                metaBuilder.SetError(Methods.MakeClrProtectedMethodCalledError.OpCall(
                    args.MetaContext.Expression, args.MetaTarget.Expression, Ast.Constant(name)
                ));
            } else {
                BuildCallNoFlow(metaBuilder, args, name, visibleOverloads, CallConvention, ImplicitProtocolConversions);
            }
        }

        #endregion
    }
}

