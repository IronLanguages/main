/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Linq.Expressions;
using System.Dynamic;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Runtime {
    public static class MetaObjectExtensions {
        public static bool NeedsDeferral(this MetaObject self) {
            if (self.HasValue) {
                return false;
            }

            if (self.Expression.Type.IsSealedOrValueType()) {
                return typeof(IDynamicObject).IsAssignableFrom(self.Expression.Type);
            }

            return true;
        }

        public static MetaObject Restrict(this MetaObject self, Type type) {
            ContractUtils.RequiresNotNull(self, "self");
            ContractUtils.RequiresNotNull(type, "type");

            IRestrictedMetaObject rmo = self as IRestrictedMetaObject;
            if (rmo != null) {
                return rmo.Restrict(type);
            }

            if (type == self.Expression.Type) {
                if (type.IsSealedOrValueType()) {
                    return self;
                }

                if (self.Expression.NodeType == ExpressionType.New ||
                    self.Expression.NodeType == ExpressionType.NewArrayBounds ||
                    self.Expression.NodeType == ExpressionType.NewArrayInit) {
                    return self;
                }
            }

            if (type == typeof(Null)) {
                return new MetaObject(
                    Expression.Constant(null),
                    self.Restrictions.Merge(Restrictions.GetInstanceRestriction(self.Expression, null)),
                    self.Value
                );
            }

            if (self.HasValue) {
                return new MetaObject(
                    AstUtils.Convert(
                        self.Expression,
                        CompilerHelpers.GetVisibleType(type)
                    ),
                    self.Restrictions.Merge(Restrictions.GetTypeRestriction(self.Expression, type)),
                    self.Value
                );
            }

            return new MetaObject(
                AstUtils.Convert(
                    self.Expression,
                    CompilerHelpers.GetVisibleType(type)
                ),
                self.Restrictions.Merge(Restrictions.GetTypeRestriction(self.Expression, type))
            );
        }
    }
}
