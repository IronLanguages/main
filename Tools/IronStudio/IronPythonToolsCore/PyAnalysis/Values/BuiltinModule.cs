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
using IronPython.Runtime;
using Microsoft.PyAnalysis.Interpreter;

namespace Microsoft.PyAnalysis.Values {
    internal class BuiltinModule : BuiltinNamespace, IReferenceableContainer {
        private readonly string _name;
        private readonly MemberReferences _references = new MemberReferences();

        public BuiltinModule(PythonModule module, ProjectState projectState, bool showClr)
            : base(new LazyDotNetDict(new object[] { module }, projectState, showClr)) {
            object name;
            if (!module.Get__dict__().TryGetValue("__name__", out name) || !(name is string)) {
                _name = String.Empty;
            } else {
                _name = name as string;
            }
        }

        public override ISet<Namespace> GetMember(Node node, AnalysisUnit unit, string name) {
            var res = base.GetMember(node, unit, name);
            if (res.Count > 0) {
                _references.AddReference(node, unit, name);
            }
            return res;
        }

        public override IDictionary<string, ISet<Namespace>> GetAllMembers(bool showClr) {
            // no showClr filtering on modules
            return VariableDict;
        }

        public override string Description {
            get {
                return "built-in module " + _name;
            }
        }

        public string Name {
            get {
                return _name;
            }
        }

        public override bool IsBuiltin {
            get { return true; }
        }

        public override ResultType ResultType {
            get { return ResultType.Module; }
        }

        #region IReferenceableContainer Members

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            return _references.GetDefinitions(name);
        }

        #endregion
    }
}
