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
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Types;

#if !SILVERLIGHT
namespace IronPython.Modules {
    /// <summary>
    /// Provides support for interop with native code from Python code.
    /// </summary>
    public static partial class CTypes {
        public static readonly PythonType _SimpleCData = SimpleType.MakeSystemType(typeof(SimpleCData));
        public static readonly PythonType CFuncPtr = CFuncPtrType.MakeSystemType(typeof(_CFuncPtr));
        public static readonly PythonType Structure = StructType.MakeSystemType(typeof(_Structure));
        public static readonly PythonType Union = UnionType.MakeSystemType(typeof(_Union));
        public static readonly PythonType _Pointer = PointerType.MakeSystemType(typeof(Pointer));
        public static readonly PythonType Array = ArrayType.MakeSystemType(typeof(_Array));

        /// <summary>
        /// Base class for all ctypes interop types.
        /// </summary>
        [PythonType("_CData"), PythonHidden]
        public abstract class CData {
            internal MemoryHolder _memHolder;

            // members: __setstate__,  __reduce__ _b_needsfree_ __ctypes_from_outparam__ __hash__ _objects _b_base_ __doc__
            protected CData() {
            }

            internal int Size {
                get {
                    // TODO: What if a user directly subclasses CData?
                    return NativeType.Size;
                }
            }

            // TODO: Accesses via Ops class
            public IntPtr UnsafeAddress {
                [PythonHidden]
                get {
                    return _memHolder.UnsafeAddress;
                }
            }

            internal INativeType NativeType {
                get {
                    return (INativeType)DynamicHelpers.GetPythonType(this);
                }
            }

            public object _objects {
                get {
                    return null;
                }
            }

            internal void SetAddress(IntPtr address) {
                Debug.Assert(_memHolder == null);
                _memHolder = new MemoryHolder(address);
            }
        }
    }
}
#endif
