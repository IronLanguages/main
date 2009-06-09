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
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = System.Linq.Expressions.Expression;
    using Microsoft.Scripting.Utils;

    /// <summary>
    /// Builds the parameter for a params dictionary argument - this collects all the extra name/value
    /// pairs provided to the function into a SymbolDictionary which is passed to the function.
    /// </summary>
    internal sealed class ParamsDictArgBuilder : ArgBuilder {
        private readonly string[] _names;
        private readonly int[] _nameIndexes;
        private readonly int _argIndex;

        public ParamsDictArgBuilder(ParameterInfo info, int argIndex, string[] names, int[] nameIndexes) 
            : base(info) {
            Assert.NotNull(names, nameIndexes);

            _argIndex = argIndex;
            _names = names;
            _nameIndexes = nameIndexes;
        }

        public override int ConsumedArgumentCount {
            get { return AllArguments; }
        }

        public override int Priority {
            get { return 3; }
        }

        internal protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed) {
            Expression res = Ast.Call(
                typeof(BinderOps).GetMethod("MakeSymbolDictionary"),
                Ast.NewArrayInit(typeof(string), ConstantNames()),
                AstUtils.NewArrayHelper(typeof(object), GetParameters(args, hasBeenUsed))
            );

            return res;
        }

        public override Type Type {
            get {
                return typeof(IAttributesCollection);
            }
        }

        private List<Expression> GetParameters(RestrictedArguments args, bool[] hasBeenUsed) {
            List<Expression> res = new List<Expression>(_nameIndexes.Length);
            for (int i = 0; i < _nameIndexes.Length; i++) {
                int parameterIndex = _nameIndexes[i] + _argIndex;
                if (!hasBeenUsed[parameterIndex]) {
                    res.Add(args.GetObject(parameterIndex).Expression);
                    hasBeenUsed[parameterIndex] = true;
                }
            }
            return res;
        }

        private int[] GetParameters(bool[] hasBeenUsed) {
            var res = new List<int>(_nameIndexes.Length);
            for (int i = 0; i < _nameIndexes.Length; i++) {
                int parameterIndex = _nameIndexes[i] + _argIndex;
                if (!hasBeenUsed[parameterIndex]) {
                    res.Add(parameterIndex);
                    hasBeenUsed[parameterIndex] = true;
                }
            }
            return res.ToArray();
        }

        private Expression[] ConstantNames() {
            Expression[] res = new Expression[_names.Length];
            for (int i = 0; i < _names.Length; i++) {
                res[i] = AstUtils.Constant(_names[i]);
            }
            return res;
        }

        protected internal override Func<object[], object> ToDelegate(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed) {
            string[] names = _names;
            int[] indexes = GetParameters(hasBeenUsed);

            return (actualArgs) => {
                object[] values = new object[indexes.Length];
                for (int i = 0; i < indexes.Length; i++) {
                    values[i] = actualArgs[indexes[i] + 1];
                }
                return BinderOps.MakeSymbolDictionary(names, values);
            };
        }
    }
}
