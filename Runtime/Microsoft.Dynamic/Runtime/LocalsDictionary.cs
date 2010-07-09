/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {

    /// <summary>
    /// Creates a dictionary of locals in this scope
    /// </summary>
    public sealed class LocalsDictionary : CustomStringDictionary {
        private readonly IRuntimeVariables _locals;
        private readonly string[] _symbols;
        private Dictionary<string, int> _boxes;

        public LocalsDictionary(IRuntimeVariables locals, string[] symbols) {
            Assert.NotNull(locals, symbols);
            _locals = locals;
            _symbols = symbols;
        }

        private void EnsureBoxes() {
            if (_boxes == null) {
                int count = _symbols.Length;
                Dictionary<string, int> boxes = new Dictionary<string, int>(count);
                for (int i = 0; i < count; i++) {
                    boxes[_symbols[i]] = i;
                }
                _boxes = boxes;
            }
        }

        public override string[] GetExtraKeys() {
            return _symbols;
        }

        protected internal override bool TrySetExtraValue(string key, object value) {
            EnsureBoxes();

            int index;
            if (_boxes.TryGetValue(key, out index)) {
                _locals[index] = value;
                return true;
            }

            return false;
        }

        protected internal override bool TryGetExtraValue(string key, out object value) {
            EnsureBoxes();

            int index;
            if (_boxes.TryGetValue(key, out index)) {
                value = _locals[index];
                return true;
            }
            value = null;
            return false;
        }
    }
}
