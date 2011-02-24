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
using IronPython.Runtime.Types;
using Microsoft.PyAnalysis.Interpreter;
using Microsoft.Scripting;

namespace Microsoft.PyAnalysis.Values {
    class BuiltinInstanceInfo : BuiltinNamespace, IReferenceableContainer {
        private readonly BuiltinClassInfo _klass;

        public BuiltinInstanceInfo(BuiltinClassInfo klass)
            : base(klass.VariableDict) {
            _klass = klass;
            _type = klass._type;
        }

        public override bool IsBuiltin {
            get {
                return true;
            }
        }

        public override ICollection<OverloadResult> Overloads {
            get {
                // TODO: look for __call__ and return overloads
                return base.Overloads;
            }
        }

        public override string Description {
            get {
                return PythonType.Get__name__(_klass._type);
            }
        }

        public override string Documentation {
            get {
                return _klass.Documentation;
            }
        }

        public override ResultType ResultType {
            get {
                switch (_klass.ResultType) {
                    case ResultType.Enum: return ResultType.EnumInstance;
                    case ResultType.Delegate: return ResultType.DelegateInstance;
                    default:
                        return ResultType.Instance;
                }
            }
        }

        public override ISet<Namespace> GetIndex(Node node, AnalysisUnit unit, ISet<Namespace> index) {
            // TODO: look for __getitem__, index, get result
            return base.GetIndex(node, unit, index);
        }

        public override ISet<Namespace> GetMember(Node node, AnalysisUnit unit, string name) {
            var res = base.GetMember(node, unit, name);
            if (res.Count > 0) {
                _klass.AddMemberReference(node, unit, name);
                return res.GetDescriptor(this, unit);
            }
            return res;
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, ISet<Namespace> value) {
            var res = base.GetMember(node, unit, name);
            if (res.Count > 0) {
                _klass.AddMemberReference(node, unit, name);
            }
        }

        public override IDictionary<string, ISet<Namespace>> GetAllMembers(bool showClr) {
            Dictionary<string, ISet<Namespace>> res = new Dictionary<string, ISet<Namespace>>();
            foreach (var keyValue in base.GetAllMembers(showClr)) {
                res[keyValue.Key] = keyValue.Value.GetDescriptor(this, null);
            }
            return res;
        }

        #region IReferenceableContainer Members

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            return _klass.GetDefinitions(name);
        }

        #endregion
    }
}
