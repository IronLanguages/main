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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Dynamic;
using System.Collections.Generic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = Expression;

    internal sealed class ByRefReturnBuilder : ReturnBuilder {
        private IList<int> _returnArgs;

        public ByRefReturnBuilder(IList<int> returnArgs)
            : base(typeof(object)) {
            _returnArgs = returnArgs;
        }

        internal override Expression ToExpression(OverloadResolver resolver, IList<ArgBuilder> builders, RestrictedArguments args, Expression ret) {
            if (_returnArgs.Count == 1) {
                if (_returnArgs[0] == -1) {
                    return ret;
                }
                return Ast.Block(ret, builders[_returnArgs[0]].ToReturnExpression(resolver));
            }

            Expression[] retValues = new Expression[_returnArgs.Count];
            int rIndex = 0;
            bool usesRet = false;
            foreach (int index in _returnArgs) {
                if (index == -1) {
                    usesRet = true;
                    retValues[rIndex++] = ret;
                } else {
                    retValues[rIndex++] = builders[index].ToReturnExpression(resolver);
                }
            }

            Expression retArray = AstUtils.NewArrayHelper(typeof(object), retValues);
            if (!usesRet) {
                retArray = Ast.Block(ret, retArray);
            }

            return resolver.GetByRefArrayExpression(retArray);
        }

        public override int CountOutParams {
            get { return _returnArgs.Count; }
        }


    }
}
