/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !SILVERLIGHT
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.ComInterop {

    internal class TypeEnumMetaObject : DynamicMetaObject {
        private readonly ComTypeEnumDesc _desc;

        internal TypeEnumMetaObject(ComTypeEnumDesc desc, Expression expression)
            : base(expression, BindingRestrictions.Empty, desc) {
            _desc = desc;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            if (_desc.HasMember(binder.Name)) {
                return new DynamicMetaObject(
                    // return (.bound $arg0).GetValue("<name>")
                    AstUtils.Constant(((ComTypeEnumDesc)Value).GetValue(binder.Name), typeof(object)),
                    EnumRestrictions()
                );
            }

            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetDynamicMemberNames() {
            return _desc.GetMemberNames();
        }

        private BindingRestrictions EnumRestrictions() {
            return BindingRestrictionsHelpers.GetRuntimeTypeRestriction(
                Expression, typeof(ComTypeEnumDesc)
            ).Merge(
                // ((ComTypeEnumDesc)<arg>).TypeLib.Guid == <guid>
                BindingRestrictions.GetExpressionRestriction(
                    Expression.Equal(
                        Expression.Property(
                            Expression.Property(
                                AstUtils.Convert(Expression, typeof(ComTypeEnumDesc)),
                                typeof(ComTypeDesc).GetProperty("TypeLib")),
                            typeof(ComTypeLibDesc).GetProperty("Guid")),
                        AstUtils.Constant(_desc.TypeLib.Guid)
                    )
                )
            ).Merge(
                BindingRestrictions.GetExpressionRestriction(
                    Expression.Equal(
                        Expression.Property(
                            AstUtils.Convert(Expression, typeof(ComTypeEnumDesc)),
                            typeof(ComTypeEnumDesc).GetProperty("TypeName")
                        ),
                        AstUtils.Constant(_desc.TypeName)
                    )
                )
            );
        }
    }
}

#endif
