/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;

namespace Microsoft.Scripting.Debugging {
    /// <summary>
    /// Used to provide information about locals/parameters at debug time.
    /// </summary>
    internal sealed class VariableInfo {
        private string _name;
        private Type _type;
        private bool _parameter;    // Indicates whether the symbol represents a local variable or parameter
        private bool _hidden;       // Indicates whether the symbol should be hidden during inspection
        private bool _strongBoxed;  // Indicates whether the lifted value of the variable is exposed through byref or strongbox
        private int _localIndex;    // Index within byref variables list or within strongbox variables list
        private int _globalIndex;   // Index within the combined list

        internal VariableInfo(string name, Type type, bool parameter, bool hidden, bool strongBoxed, int localIndex, int globalIndex) {
            _name = name;
            _type = type;
            _parameter = parameter;
            _hidden = hidden;
            _strongBoxed = strongBoxed;
            _localIndex = localIndex;
            _globalIndex = globalIndex;
        }

        internal VariableInfo(string name, Type type, bool parameter, bool hidden, bool strongBoxed)
            : this(name, type, parameter, hidden, strongBoxed, Int32.MaxValue, Int32.MaxValue) {
            _name = name;
            _type = type;
            _parameter = parameter;
            _hidden = hidden;
            _strongBoxed = strongBoxed;
        }

        internal bool Hidden {
            get { return _hidden; }
        }

        internal bool IsStrongBoxed {
            get { return _strongBoxed; }
        }

        internal int LocalIndex {
            get { Debug.Assert(_localIndex != Int32.MaxValue); return _localIndex; }
        }

        internal int GlobalIndex {
            get { Debug.Assert(_globalIndex != Int32.MaxValue); return _globalIndex; }
        }

        /// <summary>
        /// Type
        /// </summary>
        internal Type VariableType {
            get { return _type; }
        }

        /// <summary>
        /// Name
        /// </summary>
        internal string Name {
            get { return _name; }
        }

        /// <summary>
        /// Parameter
        /// </summary>
        internal bool IsParameter {
            get { return _parameter; }
        }
    }
}
