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
using Microsoft.PyAnalysis.Interpreter;
using Microsoft.Scripting;

namespace Microsoft.PyAnalysis.Values {
    class BoundBuiltinMethodInfo : Namespace {
        private readonly BuiltinMethodInfo _method;
        private OverloadResult[] _overloads;

        public BoundBuiltinMethodInfo(BuiltinMethodInfo method) {
            _method = method;
        }

        public override bool IsBuiltin {
            get {
                return true;
            }
        }

        public override ResultType ResultType {
            get {
                return ResultType.Method;
            }
        }

        public override string Documentation {
            get {
                return _method.Documentation;
            }
        }

        public ProjectState ProjectState {
            get {
                return _method.ProjectState;
            }
        }

        public override string Description {
            get {
                return "bound built-in method " + _method.Name;
            }
        }

        public override ISet<Namespace> Call(Node node, AnalysisUnit unit, ISet<Namespace>[] args, string[] keywordArgNames) {
            return _method.ReturnTypes;
        }

        public override ICollection<OverloadResult> Overloads {
            get {
                if (_overloads == null) {
                    var overloads = _method.BuiltinFunctions;
                    var result = new OverloadResult[overloads.Length];
                    for (int i = 0; i < result.Length; i++) {
                        result[i] = new BuiltinFunctionOverloadResult(_method.ProjectState, overloads[i], 0);
                    }
                    _overloads = result;
                }
                return _overloads;
            }
        }
    }
}
