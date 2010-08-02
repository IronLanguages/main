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
using Microsoft.Scripting.Runtime;

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