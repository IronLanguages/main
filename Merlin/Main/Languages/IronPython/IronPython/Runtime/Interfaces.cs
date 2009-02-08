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

using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {

    public interface ICodeFormattable {
        string/*!*/ __repr__(CodeContext/*!*/ context);
    }

    public interface ISequence {
        int __len__();

        object this[int index] {
            get;
        }
        object this[Slice slice] {
            get;
        }

        // deprecated __getslice__ method
        object __getslice__(int start, int stop);
    }

    public interface IMutableSequence : ISequence {
        new object this[int index] {
            get;
            set;
        }
        new object this[Slice slice] {
            get;
            set;
        }

        void __delitem__(int index);
        void __delitem__(Slice slice);

        // deprecated __setslice__ and __delslice__ methods
        void __setslice__(int start, int stop, object value);
        void __delslice__(int start, int stop);
    }

    /// <summary>
    /// Defines the internal interface used for accessing weak references and adding finalizers
    /// to user-defined types.
    /// </summary>
    public interface IWeakReferenceable {
        /// <summary>
        /// Gets the current WeakRefTracker for an object that can be used to
        /// append additional weak references.
        /// </summary>
        WeakRefTracker GetWeakRef();

        /// <summary>
        /// Attempts to set the WeakRefTracker for an object.  Used on the first
        /// addition of a weak ref tracker to an object.  If the object doesn't
        /// support adding weak references then it returns false.
        /// </summary>
        bool SetWeakRef(WeakRefTracker value);

        /// <summary>
        /// Sets a WeakRefTracker on an object for the purposes of supporting finalization.
        /// All user types (new-style and old-style) support finalization even if they don't
        /// support weak-references, and therefore this function always succeeds.  Note the
        /// slot used to store the WeakRefTracker is still shared between SetWeakRef and 
        /// SetFinalizer if a type supports both.
        /// </summary>
        /// <param name="value"></param>
        void SetFinalizer(WeakRefTracker value);
    }

    public interface IProxyObject {
        object Target { get; }
    }
}