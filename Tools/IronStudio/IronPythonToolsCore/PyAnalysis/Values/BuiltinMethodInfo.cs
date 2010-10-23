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
using System.Text;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace Microsoft.PyAnalysis.Values {
    internal class BuiltinMethodInfo : BuiltinNamespace {
        private BuiltinMethodDescriptor _method;
        private string _doc;
        private readonly ISet<Namespace> _returnTypes;
        private BoundBuiltinMethodInfo _boundMethod;
        private OverloadResult[] _overloads;

        public BuiltinMethodInfo(BuiltinMethodDescriptor method, ProjectState projectState)
            : base(ClrModule.GetPythonType(typeof(BuiltinMethodDescriptor)), projectState) {
            // TODO: get return information, parameters, members
            _method = method;

            var function = PythonOps.GetBuiltinMethodDescriptorTemplate(method);
            _returnTypes = Utils.GetReturnTypes(function, projectState);
            _doc = null;
        }

        public override ISet<Namespace> Call(Node node, Interpreter.AnalysisUnit unit, ISet<Namespace>[] args, string[] keywordArgNames) {
            return _returnTypes;
        }

        public override ISet<Namespace> GetDescriptor(Namespace instance, Interpreter.AnalysisUnit unit) {
            if (_boundMethod == null) {
                _boundMethod = new BoundBuiltinMethodInfo(this);
            }

            return _boundMethod.SelfSet;
        }

        public override string Description {
            get {
                return "built-in method " + _method.__name__;
            }
        }

        public ISet<Namespace> ReturnTypes {
            get {
                return _returnTypes;
            }
        }

        public BuiltinFunction[] BuiltinFunctions {
            get {
                var func = PythonOps.GetBuiltinMethodDescriptorTemplate(_method);
                // skip methods that are virtual base helpers (e.g. methods like
                // object.Equals(Object_#1, object other))

                var result = new List<BuiltinFunction>();
                foreach (var ov in func.Overloads.Functions) {
                    BuiltinFunction overload = (ov as BuiltinFunction);
                    if (overload.Overloads.Targets[0].DeclaringType.IsAssignableFrom(_method.DeclaringType) ||
                        overload.Overloads.Targets[0].DeclaringType.FullName.StartsWith("IronPython.Runtime.Operations.")) {
                        result.Add(overload);
                    }
                }
                return result.ToArray();
            }
        }

        public override ICollection<OverloadResult> Overloads {
            get {
                if (_overloads == null) {
                    var overloads = BuiltinFunctions;
                    var result = new OverloadResult[overloads.Length];
                    for (int i = 0; i < result.Length; i++) {
                        result[i] = new BuiltinFunctionOverloadResult(ProjectState, overloads[i], 0, new ParameterResult("self"));
                    }
                    _overloads = result;
                }
                return _overloads;
            }
        }

        public override bool IsBuiltin {
            get {
                return true;
            }
        }

        public override string Documentation {
            get {
                if (_doc == null) {
                    var doc = new StringBuilder();
                    foreach (var overload in BuiltinFunctions) {
                        doc.Append(Utils.StripDocumentation(overload.__doc__));
                    }
                    _doc = doc.ToString();
                }
                return _doc;
            }
        }

        public override ResultType ResultType {
            get {
                return ResultType.Method;
            }
        }

        public string Name { get { return _method.__name__; } }
    }
}
