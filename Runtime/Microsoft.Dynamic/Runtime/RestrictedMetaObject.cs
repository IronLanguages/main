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
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Runtime {
    public class RestrictedMetaObject : DynamicMetaObject, IRestrictedMetaObject {
        public RestrictedMetaObject(Expression expression, BindingRestrictions restriction, object value)  : base(expression, restriction, value) {
        }

        public RestrictedMetaObject(Expression expression, BindingRestrictions restriction)
            : base(expression, restriction) {
        }

        #region IRestrictedMetaObject Members

        public DynamicMetaObject Restrict(Type type) {
            if (type == LimitType) {
                return this;
            }

            if (HasValue) {
                return new RestrictedMetaObject(
                    AstUtils.Convert(Expression, type),
                    BindingRestrictionsHelpers.GetRuntimeTypeRestriction(Expression, type),
                    Value
                );
            }

            return new RestrictedMetaObject(
                AstUtils.Convert(Expression, type),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(Expression, type)
            );
        }

        #endregion
    }
}
