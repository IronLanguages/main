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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
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

            if (self.Expression.Type.IsSealed) {
                return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(self.Expression.Type);
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
                if (type.IsSealed ||
                    self.Expression.NodeType == ExpressionType.New ||
                    self.Expression.NodeType == ExpressionType.NewArrayBounds ||
                    self.Expression.NodeType == ExpressionType.NewArrayInit) {
                    return self.Clone(self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, type)));
                }
            }

            if (type == typeof(DynamicNull)) {
                return self.Clone(
                    AstUtils.Constant(null),
                    self.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(self.Expression, null))
                );
            }

            Expression converted;
            // if we're converting to a value type just unbox to preserve
            // object identity.  If we're converting from Enum then we're
            // going to a specific enum value and an unbox is not allowed.
            if (type.IsValueType && self.Expression.Type != typeof(Enum)) {
                converted = Expression.Unbox(
                    self.Expression,
                    CompilerHelpers.GetVisibleType(type)
                );
            } else {
                converted = AstUtils.Convert(
                    self.Expression,
                    CompilerHelpers.GetVisibleType(type)
                );
            }

            return self.Clone(converted, self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, type)));
        }

        public static DynamicMetaObject Clone(this DynamicMetaObject self, Expression newExpression) {
            return self.Clone(newExpression, self.Restrictions);
        }

        public static DynamicMetaObject Clone(this DynamicMetaObject self, BindingRestrictions newRestrictions) {
            return self.Clone(self.Expression, newRestrictions);
        }

        public static DynamicMetaObject Clone(this DynamicMetaObject self, Expression newExpression, BindingRestrictions newRestrictions) {
            return (self.HasValue) ? 
                new DynamicMetaObject(newExpression, newRestrictions, self.Value) :
                new DynamicMetaObject(newExpression, newRestrictions);
        }

        /// <summary>
        ///Returns Microsoft.Scripting.Runtime.DynamicNull if the object contains a null value,
        ///otherwise, returns self.LimitType
        /// </summary>
        public static Type GetLimitType(this DynamicMetaObject self) {
            if (self.Value == null && self.HasValue) {
                return typeof(DynamicNull);
            }
            return self.LimitType;
        }

        /// <summary>
        ///Returns Microsoft.Scripting.Runtime.DynamicNull if the object contains a null value,
        ///otherwise, returns self.RuntimeType
        /// </summary>
        public static Type GetRuntimeType(this DynamicMetaObject self) {
            if (self.Value == null && self.HasValue) {
                return typeof(DynamicNull);
            }
            return self.RuntimeType;
        }
    }
}
