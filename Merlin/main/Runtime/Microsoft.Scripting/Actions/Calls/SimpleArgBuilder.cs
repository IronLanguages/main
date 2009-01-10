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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// SimpleArgBuilder produces the value produced by the user as the argument value.  It
    /// also tracks information about the original parameter and is used to create extended
    /// methods for params arrays and param dictionary functions.
    /// </summary>
    public class SimpleArgBuilder : ArgBuilder {
        private int _index;
        private readonly Type _parameterType;
        private readonly bool _isParams, _isParamsDict;

        /// <summary>
        /// Parameter info is not available for this argument.
        /// </summary>
        public SimpleArgBuilder(Type parameterType, int index, bool isParams, bool isParamsDict) 
            : this(null, parameterType, index, isParams, isParamsDict) {
        }
    
        /// <summary>
        /// Type and whether the parameter is a params-array or params-dictionary is derived from info.
        /// </summary>
        public SimpleArgBuilder(ParameterInfo info, int index)
            : this(info, info.ParameterType, index, CompilerHelpers.IsParamArray(info), BinderHelpers.IsParamDictionary(info)) {
        }
        
        protected SimpleArgBuilder(ParameterInfo info, Type parameterType, int index, bool isParams, bool isParamsDict)
            : base(info) {
            ContractUtils.Requires(index >= 0);
            ContractUtils.RequiresNotNull(parameterType, "parameterType");

            _index = index;
            _parameterType = parameterType;
            _isParams = isParams;
            _isParamsDict = isParamsDict;
        }

        internal SimpleArgBuilder MakeCopy(int newIndex) {
            var result = Copy(newIndex);
            // Copy() must be overriden in derived classes and return an instance of the derived class:
            Debug.Assert(result.GetType() == GetType());
            return result;
        }

        protected virtual SimpleArgBuilder Copy(int newIndex) {
            return new SimpleArgBuilder(ParameterInfo, _parameterType, newIndex, _isParams, _isParamsDict);
        }

        public override int Priority {
            get { return 0; }
        }

        public bool IsParamsArray {
            get {
                return _isParams;
            }
        }

        public bool IsParamsDict {
            get {
                return _isParamsDict;
            }
        }

        internal protected override Expression ToExpression(ParameterBinder parameterBinder, IList<Expression> parameters, bool[] hasBeenUsed) {
            Debug.Assert(_index < parameters.Count);
            Debug.Assert(_index < hasBeenUsed.Length);
            Debug.Assert(parameters[_index] != null);
            hasBeenUsed[_index] = true;
            return parameterBinder.ConvertExpression(parameters[_index], ParameterInfo, _parameterType);
        }

        public int Index {
            get {
                return _index;
            }
        }

        public override Type Type {
            get {
                return _parameterType;
            }
        }
    }
}
