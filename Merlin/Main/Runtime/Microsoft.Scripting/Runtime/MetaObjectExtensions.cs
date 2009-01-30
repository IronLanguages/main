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
        public static bool NeedsDeferral(this DynamicMetaObject self) {
            if (self.HasValue) {
                return false;
            }

            if (self.Expression.Type.IsSealedOrValueType()) {
                return typeof(IDynamicObject).IsAssignableFrom(self.Expression.Type);
            }

            return true;
        }

        public static DynamicMetaObject Restrict(this DynamicMetaObject self, Type type) {
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

            if (type == typeof(DynamicNull)) {
                return new DynamicMetaObject(
                    Expression.Constant(null),
                    self.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(self.Expression, null)),
                    self.Value
                );
            }

            if (self.HasValue) {
                return new DynamicMetaObject(
                    AstUtils.Convert(
                        self.Expression,
                        CompilerHelpers.GetVisibleType(type)
                    ),
                    self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, type)),
                    self.Value
                );
            }

            return new DynamicMetaObject(
                AstUtils.Convert(
                    self.Expression,
                    CompilerHelpers.GetVisibleType(type)
                ),
                self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, type))
            );
        }

        /// <summary>
        ///Returns Microsoft.Scripting.Runtime.DynamicNull if the object contains a null value,
        ///otherwise, returns self.LimitType
        /// </summary>
        public static Type GetLimitType(this DynamicMetaObject self) {
            if (self.Value == null && self.HasValue) {
                return DynamicNull.Type;
            }
            return self.LimitType;
        }

        /// <summary>
        ///Returns Microsoft.Scripting.Runtime.DynamicNull if the object contains a null value,
        ///otherwise, returns self.RuntimeType
        /// </summary>
        public static Type GetRuntimeType(this DynamicMetaObject self) {
            if (self.Value == null && self.HasValue) {
                return DynamicNull.Type;
            }
            return self.RuntimeType;
        }
    }
}
