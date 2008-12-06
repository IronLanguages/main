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
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// Builds a parameter for a reference argument when a StrongBox has not been provided.  The
    /// updated return value is returned as one of the resulting return values.
    /// </summary>
    internal sealed class ReturnReferenceArgBuilder : SimpleArgBuilder {
        private ParameterExpression _tmp;

        public ReturnReferenceArgBuilder(ParameterInfo info, int index)
            : base(info, info.ParameterType.GetElementType(), index, false, false) {
        }

        internal protected override Expression ToExpression(ParameterBinder parameterBinder, IList<Expression> parameters, bool[] hasBeenUsed) {
            if (_tmp == null) {
                _tmp = parameterBinder.GetTemporary(Type, "outParam");
            }

            return Ast.Block(Ast.Assign(_tmp, base.ToExpression(parameterBinder, parameters, hasBeenUsed)), _tmp);
        }

        protected override SimpleArgBuilder Copy(int newIndex) {
            return new ReturnReferenceArgBuilder(ParameterInfo, newIndex);
        }

        internal override Expression ToReturnExpression(ParameterBinder parameterBinder) {
            return _tmp;
        }

        internal override Expression ByRefArgument {
            get { return _tmp; }
        }

        public override int Priority {
            get {
                return 5;
            }
        }
    }
}
