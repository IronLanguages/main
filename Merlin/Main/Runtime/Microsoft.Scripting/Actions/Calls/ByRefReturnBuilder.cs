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

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = System.Linq.Expressions.Expression;

    internal sealed class ByRefReturnBuilder : ReturnBuilder {
        private IList<int> _returnArgs;

        public ByRefReturnBuilder(IList<int> returnArgs)
            : base(typeof(object)) {
            _returnArgs = returnArgs;
        }

        internal override Expression ToExpression(ParameterBinder parameterBinder, IList<ArgBuilder> args, IList<Expression> parameters, Expression ret) {
            if (_returnArgs.Count == 1) {
                if (_returnArgs[0] == -1) {
                    return ret;
                }
                return Ast.Block(ret, args[_returnArgs[0]].ToReturnExpression(parameterBinder));
            }

            Expression[] retValues = new Expression[_returnArgs.Count];
            int rIndex = 0;
            bool usesRet = false;
            foreach (int index in _returnArgs) {
                if (index == -1) {
                    usesRet = true;
                    retValues[rIndex++] = ret;
                } else {
                    retValues[rIndex++] = args[index].ToReturnExpression(parameterBinder);
                }
            }

            Expression retArray = AstUtils.NewArrayHelper(typeof(object), retValues);
            if (!usesRet) {
                retArray = Ast.Block(ret, retArray);
            }

            return parameterBinder.Binder.GetByRefArrayExpression(retArray);
        }

        private static object GetValue(object[] args, object ret, int index) {
            if (index == -1) return ConvertToObject(ret);
            return ConvertToObject(args[index]);
        }

        public override int CountOutParams {
            get { return _returnArgs.Count; }
        }


    }
}
