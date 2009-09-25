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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Scripting.Utils {
    /// <summary>
    /// Not all .NET enumerators throw exceptions if accessed in an invalid state. This type
    /// can be used to throw exceptions from enumerators implemented in IronPython.
    /// </summary>
    public abstract class CheckedDictionaryEnumerator : IDictionaryEnumerator, IEnumerator<KeyValuePair<object, object>> {
        private EnumeratorState _enumeratorState = EnumeratorState.NotStarted;

        private void CheckEnumeratorState() {
            if (_enumeratorState == EnumeratorState.NotStarted)
                throw Error.EnumerationNotStarted();
            else if (_enumeratorState == EnumeratorState.Ended)
                throw Error.EnumerationFinished();
        }

        #region IDictionaryEnumerator Members
        public DictionaryEntry Entry {
            get {
                CheckEnumeratorState();
                return new DictionaryEntry(Key, Value);
            }
        }

        public object Key {
            get {
                CheckEnumeratorState();
                return GetKey();
            }
        }

        public object Value {
            get {
                CheckEnumeratorState();
                return GetValue();
            }
        }
        #endregion

        #region IEnumerator Members
        public bool MoveNext() {
            if (_enumeratorState == EnumeratorState.Ended)
                throw Error.EnumerationFinished();

            bool result = DoMoveNext();
            if (result)
                _enumeratorState = EnumeratorState.Started;
            else
                _enumeratorState = EnumeratorState.Ended;
            return result;
        }

        public object Current { get { return Entry; } }

        public void Reset() {
            DoReset();
            _enumeratorState = EnumeratorState.NotStarted;
        }
        #endregion

        #region IEnumerator<KeyValuePair<object,object>> Members

        KeyValuePair<object, object> IEnumerator<KeyValuePair<object, object>>.Current {
            get { return new KeyValuePair<object, object>(Key, Value); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods that a sub-type needs to implement

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // TODO: fix
        protected abstract object GetKey();
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // TODO: fix
        protected abstract object GetValue();

        protected abstract bool DoMoveNext();
        protected abstract void DoReset();

        #endregion

        private enum EnumeratorState {
            NotStarted,
            Started,
            Ended
        }
    }
}
