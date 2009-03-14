using System;
using System.Dynamic;
using Microsoft.Scripting.Runtime;
using System.Linq.Expressions;

namespace Microsoft.Scripting.Runtime {
    public static class BindingRestrictionsHelpers {
        //If the type is Microsoft.Scripting.Runtime.DynamicNull, create an instance restriction to test null
        public static BindingRestrictions GetRuntimeTypeRestriction(Expression expr, Type type) {
            if (type == typeof(DynamicNull)) {
                return BindingRestrictions.GetInstanceRestriction(expr, null);
            }

            return BindingRestrictions.GetTypeRestriction(expr, type);
        }

        public static BindingRestrictions GetRuntimeTypeRestriction(DynamicMetaObject obj) {
            return obj.Restrictions.Merge(GetRuntimeTypeRestriction(obj.Expression, obj.GetLimitType()));
        }
    }
}