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

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.Utils {

#if SILVERLIGHT
    // TODO: finalizers in user types aren't supported in Silverlight
    // we need to come up with another solution for Python's _weakref library
    public struct WeakHandle {
        private WeakReference _weakRef;

        public WeakHandle(object target, bool trackResurrection) {
            _weakRef = new WeakReference(target, trackResurrection);
            GC.SuppressFinalize(this._weakRef);
        }

        public bool IsAlive { get { return _weakRef != null && _weakRef.IsAlive; } }
        public object Target { get { return _weakRef != null ? _weakRef.Target : null; } }

        public void Free() {
            if (_weakRef != null) {
                GC.ReRegisterForFinalize(_weakRef);
                _weakRef.Target = null;
                _weakRef = null;
            }
        }
    }
#else
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")] // TODO: fix
    public struct WeakHandle {

        private GCHandle weakRef;

        public WeakHandle(object target, bool trackResurrection) {
            this.weakRef = GCHandle.Alloc(target, trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
        }

        public bool IsAlive { get { return weakRef.IsAllocated; } }
        public object Target { get { return weakRef.Target; } }
        public void Free() { weakRef.Free(); }
    }
#endif
}
