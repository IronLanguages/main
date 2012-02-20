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

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;

    internal class RubyFieldInfo : RubyMemberInfo {
        private readonly FieldInfo/*!*/ _fieldInfo;
        private readonly bool _isSetter;

        // True if the member is defined in Ruby (e.g. via alias) - such definition "detaches" it from the underlying type.
        private readonly bool _isDetached;

        public RubyFieldInfo(FieldInfo/*!*/ fieldInfo, RubyMemberFlags flags, RubyModule/*!*/ declaringModule, bool isSetter, bool isDetached)
            : base(flags, declaringModule) {
            Assert.NotNull(fieldInfo, declaringModule);
            _fieldInfo = fieldInfo;
            _isSetter = isSetter;
            _isDetached = isDetached;
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyFieldInfo(_fieldInfo, flags, module, _isSetter, true);
        }

        internal override bool IsRubyMember {
            get { return _isDetached; }
        }

        internal override bool IsDataMember {
            get { return true; }
        }

        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return new MemberInfo[] { _fieldInfo };
        }

        public override RubyMemberInfo TrySelectOverload(Type/*!*/[]/*!*/ parameterTypes) {
            return parameterTypes.Length == 0 ? this : null;
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            Expression instance = _fieldInfo.IsStatic ? null : Ast.Convert(args.TargetExpression, _fieldInfo.DeclaringType);

            if (_isSetter) {
                var actualArgs = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 1, 1);
                if (!metaBuilder.Error) {
                    metaBuilder.Result = Ast.Assign(
                        Ast.Field(instance, _fieldInfo),
                        Converter.ConvertExpression(
                            actualArgs[0].Expression, 
                            _fieldInfo.FieldType,
                            args.RubyContext, 
                            args.MetaContext.Expression,
                            true
                        )
                    );
                }
            } else {
                RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
                if (!metaBuilder.Error) {
                    if (_fieldInfo.IsLiteral) {
                        // TODO: seems like Compiler should correctly handle the literal field case
                        // (if you emit a read to a literal field, you get a NotSupportedExpception from
                        // FieldHandle when we try to emit)
                        metaBuilder.Result = AstUtils.Constant(_fieldInfo.GetValue(null));
                    } else {
                        metaBuilder.Result = Ast.Field(instance, _fieldInfo);
                    }
                }
            }
        }
    }
}
