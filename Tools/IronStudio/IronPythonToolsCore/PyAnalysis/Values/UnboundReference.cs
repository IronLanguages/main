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

namespace Microsoft.PyAnalysis.Values {
    internal class UnboundReferenceParameters {
        private readonly UnboundReference _parent;

        public UnboundReferenceParameters(UnboundReference parent) {
            _parent = parent;
        }
#if FALSE
        public VariableDef GetItem(int index) {
            Debug.Assert(index >= 0);
            _parent.EnsureParameters(index);
            return _parent.Parameters[index];
        }
#endif
    }

    internal class UnboundReference : Namespace {
        private readonly string _name;

        public UnboundReference(string name) {
            _name = name;
        }
#if FALSE
        private ISet<Namespace>[] _params;

        public void EnsureParameters(int size) {
            if (_params == null) {
                _params = new ISet<Namespace>[size + 1];
            } else if (size >= _params.Length) {
                Array.Resize(ref _params, size + 1);
            }
        }

        public override ISet<Namespace>[] ParameterTypes {
            get {
                return _params;
            }
        }
#endif
    }
}
