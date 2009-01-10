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
using Microsoft.Scripting.Utils;
using System.Reflection;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = System.Linq.Expressions.Expression;

    internal sealed class ParamsArgBuilder : ArgBuilder {
        private readonly int _start;
        private readonly int _count;
        private readonly Type _elementType;

        public ParamsArgBuilder(ParameterInfo info, Type elementType, int start, int count) 
            : base(info) {

            ContractUtils.RequiresNotNull(elementType, "elementType");
            ContractUtils.Requires(start >= 0, "start");
            ContractUtils.Requires(count >= 0, "count");

            _start = start;
            _count = count;
            _elementType = elementType;
        }

        public override int Priority {
            get { return 4; }
        }

        internal protected override Expression ToExpression(ParameterBinder parameterBinder, IList<Expression> parameters, bool[] hasBeenUsed) {
            List<Expression> elems = new List<Expression>(_count);
            for (int i = _start; i < _start + _count; i++) {
                if (!hasBeenUsed[i]) {
                    elems.Add(parameterBinder.ConvertExpression(parameters[i], ParameterInfo, _elementType));
                    hasBeenUsed[i] = true;
                }
            }

            return Ast.NewArrayInit(_elementType, elems);
        }


        public override Type Type {
            get {
                return _elementType.MakeArrayType();
            }
        }
    }
}
