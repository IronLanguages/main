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

using System;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Calls {
    using Ast = System.Linq.Expressions.Expression;

    internal class RubyFieldInfo : RubyMemberInfo {
        private readonly FieldInfo/*!*/ _fieldInfo;
        private readonly bool _isSetter;

        public RubyFieldInfo(FieldInfo/*!*/ fieldInfo, RubyMemberFlags flags, RubyModule/*!*/ declaringModule, bool isSetter)
            : base(flags, declaringModule) {
            Assert.NotNull(fieldInfo, declaringModule);
            _fieldInfo = fieldInfo;
            _isSetter = isSetter;
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyFieldInfo(_fieldInfo, flags, module, _isSetter);
        }

        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return new MemberInfo[] { _fieldInfo };
        }

        public override RubyMemberInfo TrySelectOverload(Type/*!*/[]/*!*/ parameterTypes) {
            return parameterTypes.Length == 0 ? this : null;
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            Expression expr = null;
            Expression instance = _fieldInfo.IsStatic ? null : Ast.Convert(args.TargetExpression, _fieldInfo.DeclaringType);

            if (_isSetter) {
                // parameters should be: instance/type, value
                if (args.SimpleArgumentCount == 0 && args.Signature.HasRhsArgument) {
                    expr = Ast.Assign(
                        Ast.Field(instance, _fieldInfo),
                        // TODO: remove
                        args.RubyContext.Binder.ConvertExpression(
                            args.GetRhsArgumentExpression(), 
                            _fieldInfo.FieldType, 
                            ConversionResultKind.ExplicitCast, 
                            args.ScopeExpression
                        )
                    );
                }
            } else {
                // parameter should be: instance/type
                if (args.SimpleArgumentCount == 0) {
                    if (_fieldInfo.IsLiteral) {
                        // TODO: seems like Compiler should correctly handle the literal field case
                        // (if you emit a read to a literal field, you get a NotSupportedExpception from
                        // FieldHandle when we try to emit)
                        expr = Ast.Constant(_fieldInfo.GetValue(null));
                    } else {
                        expr = Ast.Field(instance, _fieldInfo);
                    }
                }
            }

            if (expr != null) {
                metaBuilder.Result = expr;
            } else {
                metaBuilder.SetError(
                    Methods.MakeInvalidArgumentTypesError.OpCall(Ast.Constant(_isSetter ? name + "=" : name))
                );
            }
        }
    }
}
