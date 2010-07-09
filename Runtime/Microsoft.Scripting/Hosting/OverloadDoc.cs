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
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Provides documentation for a single overload of an invokable object.
    /// </summary>
    [Serializable]
    public class OverloadDoc {
        private readonly string _name, _doc;
        private readonly ICollection<ParameterDoc> _params;
        private readonly ParameterDoc _returnParam;        

        public OverloadDoc(string name, string documentation, ICollection<ParameterDoc> parameters) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNullItems(parameters, "parameters");

            _name = name;
            _params = parameters;
            _doc = documentation;   
        }

        public OverloadDoc(string name, string documentation, ICollection<ParameterDoc> parameters, ParameterDoc returnParameter) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNullItems(parameters, "parameters");

            _name = name;
            _params = parameters;
            _doc = documentation;
            _returnParam = returnParameter;
        }

        /// <summary>
        /// The name of the invokable object.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// The documentation for the overload or null if no documentation is available.
        /// </summary>
        public string Documentation {
            get {
                return _doc;
            }
        }

        /// <summary>
        /// The parameters for the invokable object.
        /// </summary>
        public ICollection<ParameterDoc> Parameters {
            get {
                return _params;
            }
        }

        /// <summary>
        /// Information about the return value.
        /// </summary>
        public ParameterDoc ReturnParameter {
            get {
                return _returnParam;
            }
        }
    }

}
