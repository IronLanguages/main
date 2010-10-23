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
using Microsoft.Scripting.Actions;

namespace Microsoft.PyAnalysis.Values {
    /// <summary>
    /// Represents a .NET namespace as exposed to Python
    /// </summary>
    internal class ReflectedNamespace : BuiltinNamespace, IReferenceableContainer {
        private readonly MemberReferences _references = new MemberReferences();

        public ReflectedNamespace(IEnumerable<object> objects, ProjectState projectState)
            : base(new LazyDotNetDict(objects, projectState, true)) {
        }

        public override ISet<Namespace> GetMember(Node node, AnalysisUnit unit, string name) {
            var res = base.GetMember(node, unit, name);
            if (res.Count > 0) {
                _references.AddReference(node, unit, name);
            }
            return res;
        }

        public override IDictionary<string, ISet<Namespace>> GetAllMembers(bool showClr) {
            return VariableDict;
        }

        public override bool IsBuiltin {
            get { return true; }
        }

        public override ResultType ResultType {
            get {
                var modules = VariableDict.Objects;
                if (modules.Length > 1 || modules[0] is NamespaceTracker) {
                    return ResultType.Namespace;
                }
                return ResultType.Field;
            }
        }

        #region IReferenceableContainer Members

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            return _references.GetDefinitions(name);
        }

        #endregion
    }
}
