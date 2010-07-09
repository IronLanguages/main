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

using System.Collections.Generic;
using System.Dynamic;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.ComInterop {

    internal class TypeLibMetaObject : DynamicMetaObject {
        private readonly ComTypeLibDesc _lib;

        internal TypeLibMetaObject(Expression expression, ComTypeLibDesc lib)
            : base(expression, BindingRestrictions.Empty, lib) {
            _lib = lib;
        }

        private DynamicMetaObject TryBindGetMember(string name) {
            if (_lib.HasMember(name)) {
                BindingRestrictions restrictions =
                    BindingRestrictions.GetTypeRestriction(
                        Expression, typeof(ComTypeLibDesc)
                    ).Merge(
                        BindingRestrictions.GetExpressionRestriction(
                            Expression.Equal(
                                Expression.Property(
                                    AstUtils.Convert(
                                        Expression, typeof(ComTypeLibDesc)
                                    ),
                                    typeof(ComTypeLibDesc).GetProperty("Guid")
                                ),
                                AstUtils.Constant(_lib.Guid)
                            )
                        )
                    );

                return new DynamicMetaObject(
                    AstUtils.Constant(
                        ((ComTypeLibDesc)Value).GetTypeLibObjectDesc(name)
                    ),
                    restrictions
                );
            }

            return null;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            return TryBindGetMember(binder.Name) ?? base.BindGetMember(binder);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
            var result = TryBindGetMember(binder.Name);
            if (result != null) {
                return binder.FallbackInvoke(result, args, null);
            }

            return base.BindInvokeMember(binder, args);
        }

        public override IEnumerable<string> GetDynamicMemberNames() {
            return _lib.GetMemberNames();
        }
    }
}

#endif
