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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Provides documentation for a single parameter.
    /// </summary>
    [Serializable]
    public class ParameterDoc {
        private readonly string _name, _typeName, _doc;
        private readonly ParameterFlags _flags;

        public ParameterDoc(string name)
            : this(name, null, null, ParameterFlags.None) {
        }

        public ParameterDoc(string name, ParameterFlags paramFlags)
            : this(name, null, null, paramFlags) {
        }

        public ParameterDoc(string name, string typeName)
            : this(name, typeName, null, ParameterFlags.None) {
        }

        public ParameterDoc(string name, string typeName, string documentation)
            : this(name, typeName, documentation, ParameterFlags.None) {
        }

        public ParameterDoc(string name, string typeName, string documentation, ParameterFlags paramFlags) {
            ContractUtils.RequiresNotNull(name, "name");

            _name = name;
            _flags = paramFlags;
            _typeName = typeName;
            _doc = documentation;
        }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// The type name of the parameter or null if no type information is available.
        /// </summary>
        public string TypeName {
            get {
                return _typeName;
            }
        }

        /// <summary>
        /// Provides addition information about the parameter such as if it's a parameter array.
        /// </summary>
        public ParameterFlags Flags {
            get {
                return _flags;
            }
        }

        /// <summary>
        /// Gets the documentation string for this parameter or null if no documentation is available.
        /// </summary>
        public string Documentation {
            get {
                return _doc;
            }
        }
    }

}
