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

        protected internal override Func<object[], object> ToDelegate(ParameterBinder parameterBinder, IList<DynamicMetaObject> knownTypes, bool[] hasBeenUsed) {
            List<Func<object[], object>> indexes = new List<Func<object[], object>>(_count);
            for (int i = _start; i < _start + _count; i++) {
                if (!hasBeenUsed[i]) {
                    indexes.Add(parameterBinder.ConvertObject(i + 1, knownTypes[i], ParameterInfo, _elementType));
                    hasBeenUsed[i] = true;
                }
            }

            if (_elementType == typeof(object)) {
                return new ParamArrayDelegate<object>(indexes.ToArray(), _start).MakeParamsArray;
            }

            Type genType = typeof(ParamArrayDelegate<>).MakeGenericType(_elementType);
            return (Func<object[], object>)Delegate.CreateDelegate(
                typeof(Func<object[], object>), 
                Activator.CreateInstance(genType, indexes.ToArray(), _start),
                genType.GetMethod("MakeParamsArray"));
        }

        class ParamArrayDelegate<T> {
            private readonly Func<object[], object>[] _indexes;
            private readonly int _start;

            public ParamArrayDelegate(Func<object[], object>[] indexes, int start) {
                _indexes = indexes;
                _start = start;
            }

            public T[] MakeParamsArray(object[] args) {
                T[] res = new T[_indexes.Length];
                for (int i = 0; i < _indexes.Length; i++) {
                    if (_indexes[i] == null) {
                        res[i] = (T)args[_start + i + 1];
                    } else {
                        res[i] = (T)_indexes[i](args);
                    }
                }

                return res;
            }
        }

        internal override bool CanGenerateDelegate {
            get {
                return true;
            }
        }

        public override Type Type {
            get {
                return _elementType.MakeArrayType();
            }
        }
    }
}
