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

using System.Collections.Generic;
using IronPython.Compiler.Ast;
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis.Interpreter {
    abstract class InterpreterScope {
        private readonly Namespace _ns;
        private Dictionary<string, VariableDef> _variables = new Dictionary<string, VariableDef>();

        public InterpreterScope(Namespace ns) {
            _ns = ns;
        }

        public abstract string Name {
            get;
        }

        public void SetVariable(Node node, AnalysisUnit unit, string name, IEnumerable<Namespace> value, bool addRef = true) {
            var variable = CreateVariable(node, unit, name, false);
            
            variable.AddTypes(node, unit, value);
            if (addRef) {
                variable.AddAssignment(node, unit);
            }
        }

        public VariableDef GetVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            VariableDef res;
            if (_variables.TryGetValue(name, out res)) {
                if (addRef) {
                    res.AddReference(node, unit);
                }
                return res;
            }
            return null;
        }

        public VariableDef CreateVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            var res = GetVariable(node, unit, name, addRef);
            if (res == null) {
                _variables[name] = res = new VariableDef();
                if (addRef) {
                    res.AddReference(node, unit);
                }
            }
            return res;
        }

        protected VariableDef CreateVariableWorker(Node node, AnalysisUnit unit, string name) {
            VariableDef res;
            if (!_variables.TryGetValue(name, out res)) {
                _variables[name] = res = new VariableDef();
            }
            return res;
        }

        public IDictionary<string, VariableDef> Variables {
            get {
                return _variables;
            }
        }

        public virtual bool VisibleToChildren {
            get {
                return true;
            }
        }

        public Namespace Namespace {
            get {
                return _ns;
            }
        }
    }
}
