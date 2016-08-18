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

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.Utils {

#if SILVERLIGHT
    // TODO: finalizers in user types aren't supported in Silverlight
    // we need to come up with another solution for Python's _weakref library
    public struct WeakHandle {
        private readonly WeakReference _weakRef;

        public WeakHandle(object target, bool trackResurrection) {
            _weakRef = new WeakReference(target, trackResurrection);
            GC.SuppressFinalize(this._weakRef);
        }

        public object Target { 
            get { 
                if (!_weakRef.IsAlive)
                    return null;
                try {
                    return _weakRef.Target; 
                } catch (InvalidOperationException) {
                    return null;
                }
            } 
        }

        public void Free() {
            GC.ReRegisterForFinalize(_weakRef);
            _weakRef.Target = null;
        }
    }
#else
    /// <summary>
    /// Changes the semantics of GC handle to return <c>null</c> instead of throwing 
    /// an <see cref="InvalidOperationException"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")] // TODO: fix
    public struct WeakHandle {

        private readonly GCHandle _gcHandle;

        public WeakHandle(object target, bool trackResurrection) {
            this._gcHandle = GCHandle.Alloc(target, trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
        }

        public object Target { 
            get {
                if (!_gcHandle.IsAllocated)
                    return null;
                try {
                    return _gcHandle.Target;
                } catch (InvalidOperationException) {
                    return null;
                }
            } 
        }

        public void Free() {
            if (!_gcHandle.IsAllocated)
                return;
            try {
                _gcHandle.Free();
            }
            catch (InvalidOperationException) {
            }
        }
    }
#endif
}
