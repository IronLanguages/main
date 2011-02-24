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
using System.Linq;
using System.Text;
using IronPython.Compiler.Ast;
using Microsoft.PyAnalysis.Interpreter;

namespace Microsoft.PyAnalysis.Values {
    /// <summary>
    /// Represents a list object with tracked type information.
    /// </summary>
    class ListInfo : SequenceInfo {
        private ListAppendBoundBuiltinMethodInfo _appendMethod;
        private ListPopBoundBuiltinMethodInfo _popMethod;
        private ListInsertBoundBuiltinMethodInfo _insertMethod;
        private ListExtendBoundBuiltinMethodInfo _extendMethod;

        public ListInfo(ISet<Namespace>[] indexTypes, BuiltinClassInfo seqType)
            : base(indexTypes, seqType) {
                EnsureAppend();
        }

        public override ISet<Namespace> GetMember(Node node, AnalysisUnit unit, string name) {
            switch (name) {
                case "append":
                    EnsureAppend();
                    return _appendMethod.SelfSet;
                case "pop":
                    EnsurePop();
                    return _popMethod.SelfSet;
                case "insert":
                    EnsureInsert();
                    return _insertMethod.SelfSet;
                case "extend":
                    EnsureExtend();
                    return _extendMethod.SelfSet;                
            }

            return base.GetMember(node, unit, name);
        }


        internal void AppendItem(ISet<Namespace> set) {
            ISet<Namespace> newTypes = set;
            bool madeSet = false;
            foreach (var type in IndexTypes) {
                newTypes = newTypes.Union(type, ref madeSet);
            }
            
            if (IndexTypes.Length != 1 || IndexTypes[0].Count != newTypes.Count) {
                ReturnValue.EnqueueDependents();
            }

            UnionType = newTypes;
            IndexTypes = new[] { newTypes };
        }

        private void EnsureAppend() {
            if (_appendMethod == null) {
                var appendMeth = VariableDict["append"];
                _appendMethod = new ListAppendBoundBuiltinMethodInfo(this, (BuiltinMethodInfo)appendMeth.First());
            }
        }

        private void EnsurePop() {
            if (_popMethod == null) {
                var popMethod = VariableDict["pop"];
                _popMethod = new ListPopBoundBuiltinMethodInfo(this, (BuiltinMethodInfo)popMethod.First());
            }
        }

        private void EnsureInsert() {
            if (_insertMethod == null) {
                var method = VariableDict["insert"];
                _insertMethod = new ListInsertBoundBuiltinMethodInfo(this, (BuiltinMethodInfo)method.First());
            }
        }

        private void EnsureExtend() {
            if (_extendMethod == null) {
                var method = VariableDict["extend"];
                _extendMethod = new ListExtendBoundBuiltinMethodInfo(this, (BuiltinMethodInfo)method.First());
            }
        }
    }
}
