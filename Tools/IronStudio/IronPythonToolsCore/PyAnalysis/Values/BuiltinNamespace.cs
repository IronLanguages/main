/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using IronPython.Compiler.Ast;
using Microsoft.PyAnalysis.Interpreter;
using IronPython.Runtime.Types;

namespace Microsoft.PyAnalysis.Values {
    /// <summary>
    /// Base class for things which get their members primarily via a built-in .NET type.
    /// </summary>
    class BuiltinNamespace : Namespace {
        private readonly LazyDotNetDict _dict;
        internal PythonType _type;

        public BuiltinNamespace(LazyDotNetDict dict) {
            _dict = dict;
        }

        public BuiltinNamespace(PythonType pythonType, ProjectState projectState) {
            // TODO: Complete member initialization
            _type = pythonType;
            _dict = new LazyDotNetDict(_type, projectState, true);
        }

        public override ISet<Namespace> GetMember(Node node, AnalysisUnit unit, string name) {
            bool showClr = (unit != null) ? unit.DeclaringModule.ShowClr : false;
            var res = VariableDict.GetClr(name, showClr, null);
            if (res != null) {
                return res;
            }
            return EmptySet<Namespace>.Instance;
        }

        public override IDictionary<string, ISet<Namespace>> GetAllMembers(bool showClr) {
            if (showClr) {
                return VariableDict;
            }

            var alwaysAvail = Utils.DirHelper(_type, false);
            var result = new Dictionary<string, ISet<Namespace>>(VariableDict.Count);
            foreach (var name in alwaysAvail) {
                ISet<Namespace> value;
                if (VariableDict.TryGetValue(name, out value)) {
                    result[name] = value;
                }
            }

            return result;
        }

        public override PythonType PythonType {
            get { return _type; }
        }

        public LazyDotNetDict VariableDict {
            get {
                return _dict;
            }
        }

        public ProjectState ProjectState {
            get {
                return _dict.ProjectState;
            }
        }
    }
}
