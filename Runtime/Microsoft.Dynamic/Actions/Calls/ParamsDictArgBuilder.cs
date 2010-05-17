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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = Expression;

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
            Assert.NotNull(info, names, nameIndexes);

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
            Type dictType = ParameterInfo.ParameterType;

            return Ast.Call(
                GetCreationDelegate(dictType).Method,
                Ast.NewArrayInit(typeof(string), ConstantNames()),
                AstUtils.NewArrayHelper(typeof(object), GetParameters(args, hasBeenUsed))
            );
        }

        private static InvalidOperationException BadDictionaryType(Type dictType) {
            return new InvalidOperationException(String.Format("Unsupported param dictionary type: {0}", dictType.FullName));
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

            Type dictType = ParameterInfo.ParameterType;

            Func<string[], object[], object> func = GetCreationDelegate(dictType);

            return (actualArgs) => {
                object[] values = new object[indexes.Length];
                for (int i = 0; i < indexes.Length; i++) {
                    values[i] = actualArgs[indexes[i] + 1];
                }
                return func(names, values);
            };
        }

        private Func<string[], object[], object> GetCreationDelegate(Type dictType) {
            Func<string[], object[], object> func = null;

            if (dictType == typeof(IDictionary)) {
                func = BinderOps.MakeDictionary<object, object>;
            } else if (dictType == typeof(IAttributesCollection)) {
                func = BinderOps.MakeSymbolDictionary;
            } else if (dictType.IsGenericType) {
                Type[] genArgs = dictType.GetGenericArguments();
                if (dictType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                    dictType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {

                    if (genArgs[0] == typeof(string) || genArgs[0] == typeof(object)) {
                        MethodInfo target = typeof(BinderOps).GetMethod("MakeDictionary").MakeGenericMethod(genArgs);

                        func = (Func<string[], object[], object>)Delegate.CreateDelegate(typeof(Func<string[], object[], object>), target);
                    }
                }
            }

            if (func == null) {
                throw BadDictionaryType(dictType);
            }

            return func;
        }
    }
}
