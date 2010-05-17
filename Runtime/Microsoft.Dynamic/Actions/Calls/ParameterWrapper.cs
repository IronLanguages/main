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
    [Flags]
    public enum ParameterBindingFlags {
        None = 0,
        ProhibitNull = 1,
        ProhibitNullItems = 2,
        IsParamArray = 4,
        IsParamDictionary = 8,
        IsHidden = 16,
    }

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
        private readonly ParameterBindingFlags _flags;
        private readonly string _name;

        // Type and other properties may differ from the values on the info; info could also be unspecified.
        private readonly ParameterInfo _info;

        /// <summary>
        /// ParameterInfo is not available.
        /// </summary>
        [Obsolete("Use ParameterBindingAttributes overload")]
        public ParameterWrapper(Type type, string name, bool prohibitNull)
            : this(null, type, name, prohibitNull, false, false, false) {
        }

        [Obsolete("Use ParameterBindingAttributes overload")]
        public ParameterWrapper(ParameterInfo info, Type type, string name, bool prohibitNull, bool isParams, bool isParamsDict, bool isHidden) 
            : this(info, type, name, 
            (prohibitNull ? ParameterBindingFlags.ProhibitNull : 0) |
            (isParams ? ParameterBindingFlags.IsParamArray : 0) |
            (isParamsDict ? ParameterBindingFlags.IsParamDictionary : 0) |
            (isHidden ? ParameterBindingFlags.IsHidden : 0)) {
        }

        public ParameterWrapper(ParameterInfo info, Type type, string name, ParameterBindingFlags flags) {
            ContractUtils.RequiresNotNull(type, "type");
            
            _type = type;
            _info = info;
            _flags = flags;

            // params arrays & dictionaries don't allow assignment by keyword
            _name = (IsParamsArray || IsParamsDict || name == null) ? "<unknown>" : name;
        }

        public ParameterWrapper Clone(string name) {
            return new ParameterWrapper(_info, _type, name, _flags);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return _type; }
        }

        public ParameterInfo ParameterInfo {
            get { return _info; }
        }

        public string Name {
            get { return _name; }
        }

        public ParameterBindingFlags Flags {
            get { return _flags; }
        }

        public bool ProhibitNull {
            get { return (_flags & ParameterBindingFlags.ProhibitNull) != 0; }
        }

        public bool ProhibitNullItems {
            get { return (_flags & ParameterBindingFlags.ProhibitNullItems) != 0; }
        }

        public bool IsHidden {
            get { return (_flags & ParameterBindingFlags.IsHidden) != 0; }
        }

        public bool IsByRef {
            get { return _info != null && _info.ParameterType.IsByRef; }
        }

        /// <summary>
        /// True if the wrapper represents a params-array parameter (false for parameters created by expansion of a params-array).
        /// </summary>
        public bool IsParamsArray { // TODO: rename to IsParamArray
            get { return (_flags & ParameterBindingFlags.IsParamArray) != 0; }
        }

        /// <summary>
        /// True if the wrapper represents a params-dict parameter (false for parameters created by expansion of a params-dict).
        /// </summary>
        public bool IsParamsDict { // TODO: rename to IsParamDictionary
            get { return (_flags & ParameterBindingFlags.IsParamDictionary) != 0; }
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
            Debug.Assert(IsParamsArray);
            return new ParameterWrapper(_info, _type.GetElementType(), null, 
                (ProhibitNullItems ? ParameterBindingFlags.ProhibitNull : 0) | 
                (IsHidden ? ParameterBindingFlags.IsHidden : 0)
            );
        }
    }

}
