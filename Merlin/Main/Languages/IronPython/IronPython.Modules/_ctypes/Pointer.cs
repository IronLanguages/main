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

using IronPython.Runtime;
using IronPython.Runtime.Types;

#if !SILVERLIGHT
namespace IronPython.Modules {
    /// <summary>
    /// Provides support for interop with native code from Python code.
    /// </summary>
    public static partial class CTypes {        
        [PythonType("_Pointer")]
        public abstract class Pointer : CData {
            private CData _object;

            public Pointer() {
                _memHolder = new MemoryHolder(IntPtr.Size);
            }

            public Pointer(CData value) {
                _object = value; // Keep alive the object, more to do here.
                _memHolder = new MemoryHolder(IntPtr.Size);
                _memHolder.WriteIntPtr(0, value._memHolder);
            }

            public object contents {
                get {
                    PythonType elementType = (PythonType)((PointerType)NativeType)._type;

                    CData res = (CData)elementType.CreateInstance(elementType.Context.DefaultBinderState.Context);
                    res._memHolder = _memHolder.ReadMemoryHolder(0);
                    return res;
                }
                set {
                }
            }

            public object this[int index] {
                get {
                    INativeType type = ((PointerType)NativeType)._type;
                    MemoryHolder address = _memHolder.ReadMemoryHolder(0);

                    return type.GetValue(address, checked(type.Size * index), false);
                }
                set {
                    MemoryHolder address = _memHolder.ReadMemoryHolder(0);

                    INativeType type = ((PointerType)NativeType)._type;
                    type.SetValue(address, checked(type.Size * index), value);
                }
            }

            public bool __nonzero__() {
                return _memHolder.ReadIntPtr(0) != IntPtr.Zero;
            }

            public object this[Slice index] {
                get {
                    throw new NotImplementedException();
                }
            }

            public List __getslice__(int start, int stop) {
                if (start < 0) {
                    start = 0;
                }

                if (stop < start) {
                    return new List();
                }

                List res = new List(stop - start);
                INativeType type = ((PointerType)NativeType)._type;
                for (int i = start; i < stop; i++) {
                    res.AddNoLock(
                        type.GetValue(_memHolder, checked(type.Size * i), false)
                    );
                }

                return res;
            }
        }
    }
}
#endif
