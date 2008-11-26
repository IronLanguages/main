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
using System.Collections.Generic;
using System.Text;
using System.Dynamic.Binders;
using System.Linq.Expressions;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Runtime {
    public class RestrictedMetaObject : MetaObject, IRestrictedMetaObject {
        public RestrictedMetaObject(Expression expression, Restrictions restriction, object value)  : base(expression, restriction, value) {
        }

        public RestrictedMetaObject(Expression expression, Restrictions restriction)
            : base(expression, restriction) {
        }

        #region IRestrictedMetaObject Members

        public MetaObject Restrict(Type type) {
            if (type == LimitType) {
                return this;
            }

            if (HasValue) {
                return new RestrictedMetaObject(
                    AstUtils.Convert(Expression, type),
                    Restrictions.GetTypeRestriction(Expression, type),
                    Value
                );
            }

            return new RestrictedMetaObject(
                AstUtils.Convert(Expression, type),
                Restrictions.GetTypeRestriction(Expression, type)
            );
        }

        #endregion
    }
}
