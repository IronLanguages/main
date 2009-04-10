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
using System.Reflection;
using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// ParameterWrapper represents the logical view of a parameter. For eg. the byref-reduced signature
    /// of a method with byref parameters will be represented using a ParameterWrapper of the underlying
    /// element type, since the logical view of the byref-reduced signature is that the argument will be
    /// passed by value (and the updated value is included in the return value).
    /// 
    /// Contrast this with ArgBuilder which represents the real physical argument passed to the method.
    /// </summary>
    public sealed class ParameterWrapper {
        private readonly Type _type;
        private readonly bool _prohibitNull, _isParams, _isParamsDict, _isHidden;
        private readonly string _name;

        // Type and other properties may differ from the values on the info; info could also be unspecified.
        private readonly ParameterInfo _info;

        /// <summary>
        /// ParameterInfo is not available.
        /// </summary>
        public ParameterWrapper(Type type, string name, bool prohibitNull)
            : this(null, type, name, prohibitNull, false, false, false) {
        }

        public ParameterWrapper(ParameterInfo info, Type type, string name, bool prohibitNull, bool isParams, bool isParamsDict, bool isHidden) {
            ContractUtils.RequiresNotNull(type, "type");
            
            _type = type;
            _prohibitNull = prohibitNull;
            _info = info;
            _isParams = isParams;
            _isParamsDict = isParamsDict;
            _isHidden = isHidden;

            // params arrays & dictionaries don't allow assignment by keyword
            _name = (_isParams || _isParamsDict || name == null) ? "<unknown>" : name;
        }

        public ParameterWrapper(ParameterInfo info) 
            : this(info, info.ParameterType, info.Name, 
            CompilerHelpers.ProhibitsNull(info),  
            CompilerHelpers.IsParamArray(info), 
            BinderHelpers.IsParamDictionary(info),
            false
        ) {
        }

        public ParameterWrapper Clone(string name) {
            return new ParameterWrapper(_info, _type, name, _prohibitNull, _isParams, _isParamsDict, _isHidden);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return _type; }
        }

        public bool ProhibitNull {
            get { return _prohibitNull; }
        }

        public ParameterInfo ParameterInfo {
            get { return _info; }
        }

        public string Name {
            get { return _name; }
        }

        public bool IsHidden {
            get { return _isHidden; }
        }

        /// <summary>
        /// True if the wrapper represents a params-array parameter (false for parameters created by expansion of a params-array).
        /// </summary>
        public bool IsParamsArray {
            get { return _isParams; }
        }

        /// <summary>
        /// True if the wrapper represents a params-dict parameter (false for parameters created by expansion of a params-dict).
        /// </summary>
        public bool IsParamsDict {
            get { return _isParamsDict; }
        }

        internal static int IndexOfParamsArray(IList<ParameterWrapper> parameters) {
            for (int i = 0; i < parameters.Count; i++) {
                if (parameters[i].IsParamsArray) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Creates a parameter that represents an expanded item of params-array.
        /// </summary>
        internal ParameterWrapper Expand() {
            Debug.Assert(_isParams);
            return new ParameterWrapper(_info, _type.GetElementType(), null, CompilerHelpers.ProhibitsNullItems(_info), false, false, _isHidden);
        }
    }

}
