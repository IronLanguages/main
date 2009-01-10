/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Exposes a IDictionary&lt;SymbolId, object&gt; as a IDictionary&lt;object, object&lt;
    /// </summary>
    public class TransformDictionaryEnumerator : CheckedDictionaryEnumerator {
        private IEnumerator<KeyValuePair<SymbolId, object>> _backing;

        public TransformDictionaryEnumerator(IDictionary<SymbolId, object> backing) {
            _backing = backing.GetEnumerator();
        }

        protected override object GetKey() {
            return SymbolTable.IdToString(_backing.Current.Key);
        }

        protected override object GetValue() {
            return _backing.Current.Value;
        }

        protected override bool DoMoveNext() {
            bool result = _backing.MoveNext();
            if (result && _backing.Current.Key == CustomSymbolDictionary.ObjectKeys) {
                result = MoveNext();
            }
            return result;
        }

        protected override void DoReset() {
            _backing.Reset();
        }
    }
}
