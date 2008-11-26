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
using System.Dynamic.Binders;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    public class OperationMetaObject : MetaObject {
        public OperationMetaObject(Expression expression, Restrictions restrictions)
            : base(expression, restrictions) {
        }

        public OperationMetaObject(Expression expression, Restrictions restrictions, object value)
            : base(expression, restrictions, value) {
        }

        [Obsolete("Use ExtensionBinaryOperationBinder or ExtensionUnaryOperationBinder")]
        public virtual MetaObject BindOperation(OperationBinder binder, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackOperation(this, args);
        }
    }
}
