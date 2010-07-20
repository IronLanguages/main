using System;
using System.Collections;

namespace Microsoft.Web.Scripting.Util {
    /*
     * Fast implementation of a collection with a single object
     */
    internal class SingleObjectCollection : ICollection {

        private class SingleObjectEnumerator : IEnumerator {
            private object _object;
            private bool done;

            public SingleObjectEnumerator(object o) { _object = o; }
            public object Current { get { return _object; } }
            public bool MoveNext() {
                if (!done) {
                    done = true;
                    return true;
                }

                return false;
            }
            public void Reset() { done = false; }
        }

        private object _object;

        public SingleObjectCollection(object o) { _object = o; }

        IEnumerator IEnumerable.GetEnumerator() { return new SingleObjectEnumerator(_object); }
        public int Count { get { return 1; } }
        bool ICollection.IsSynchronized { get { return true; } }
        object ICollection.SyncRoot { get { return this; } }

        public void CopyTo(Array array, int index) {
            array.SetValue(_object, index);
        }
    }
}
