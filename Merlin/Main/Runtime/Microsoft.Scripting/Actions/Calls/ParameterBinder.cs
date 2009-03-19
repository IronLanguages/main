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
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// Helper class for emitting calls via the MethodBinder.
    /// </summary>
    public class ParameterBinder {
        private readonly ActionBinder _actionBinder;
        private List<ParameterExpression> _temps;

        public ParameterBinder(ActionBinder actionBinder) {
            Assert.NotNull(actionBinder);

            _actionBinder = actionBinder;
        }

        public ActionBinder Binder {
            get { return _actionBinder; }
        }

        internal List<ParameterExpression> Temps {
            get { return _temps; }
        }

        internal ParameterExpression GetTemporary(Type type, string name) {
            Assert.NotNull(type);

            if (_temps == null) {
                _temps = new List<ParameterExpression>();
            }

            ParameterExpression res = Expression.Variable(type, name);
            _temps.Add(res);
            return res;
        }

        public virtual Expression ConvertExpression(Expression expr, ParameterInfo info, Type toType) {
            Assert.NotNull(expr, toType);

            return _actionBinder.ConvertExpression(expr, toType, ConversionResultKind.ExplicitCast, null);
        }

        public virtual Func<object[], object> ConvertObject(int index, DynamicMetaObject knownType, ParameterInfo info, Type toType) {
            throw new NotImplementedException();
        }

        public virtual Expression GetDynamicConversion(Expression value, Type type) {
            return Expression.Dynamic(OldConvertToAction.Make(_actionBinder, type), type, value);
        }
    }

}
