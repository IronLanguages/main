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
using System.Collections.ObjectModel;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace Microsoft.PyAnalysis.Values {
    internal class BuiltinFunctionInfo : BuiltinNamespace {
        private BuiltinFunction _function;
        private string _doc;
        private ReadOnlyCollection<OverloadResult> _overloads;
        private readonly ISet<Namespace> _returnTypes;

        public BuiltinFunctionInfo(BuiltinFunction function, ProjectState projectState)
            : base(ClrModule.GetPythonType(typeof(BuiltinFunction)), projectState) {
            // TODO: get return information, parameters, members
            _function = function;

            _returnTypes = Utils.GetReturnTypes(function, projectState);                        
            _doc = null;
        }

        public override ISet<Namespace> Call(Node node, Interpreter.AnalysisUnit unit, ISet<Namespace>[] args, string[] keywordArgNames) {
            return _returnTypes;
        }

        public BuiltinFunction Function {
            get {
                return _function;
            }
        }

        public override string Description {
            get {
                return "built-in function " + _function.__name__;
            }
        }

        public override ICollection<OverloadResult> Overloads {
            get {
                if (_overloads == null) {
                    var overloads = _function.Overloads.Functions;
                    var result = new OverloadResult[overloads.Count];
                    for (int i = 0; i < result.Length; i++) {
                        var bf = overloads[i] as BuiltinFunction;
                        result[i] = new BuiltinFunctionOverloadResult(ProjectState, bf, 0);
                    }
                    _overloads = new ReadOnlyCollection<OverloadResult>(result);
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
                    _doc = Utils.StripDocumentation(_function.__doc__);
                }
                return _doc;
            }
        }

        public override ResultType ResultType {
            get {
                return ResultType.Function;
            }
        }
    }
}
