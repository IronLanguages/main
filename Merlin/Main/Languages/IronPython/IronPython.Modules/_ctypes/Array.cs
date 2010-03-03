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
using System.Collections;
using System.Text;

using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using System.Collections.Generic;

#if CLR2
using Microsoft.Scripting.Math;
#else
using System.Numerics;
#endif

#if !SILVERLIGHT
namespace IronPython.Modules {
    /// <summary>
    /// Provides support for interop with native code from Python code.
    /// </summary>
    public static partial class CTypes {
        
        [PythonType("Array")]
        public abstract class _Array : CData {

            public void __init__(params object[] args) {
                INativeType nativeType = NativeType;

                _memHolder = new MemoryHolder(nativeType.Size);

                if (args.Length > ((ArrayType)nativeType).Length) {
                    throw PythonOps.IndexError("too many arguments");
                }
                nativeType.SetValue(_memHolder, 0, args);
            }

            public object this[int index] {
                get {
                    index = PythonOps.FixIndex(index, ((ArrayType)NativeType).Length);
                    INativeType elementType = ElementType;
                    return elementType.GetValue(
                        _memHolder,
                        this,
                        checked(index * elementType.Size),
                        false
                    );
                }
                set {
                    index = PythonOps.FixIndex(index, ((ArrayType)NativeType).Length);
                    INativeType elementType = ElementType;

                    object keepAlive = elementType.SetValue(
                        _memHolder,
                        checked(index * elementType.Size),
                        value
                    );

                    if (keepAlive != null) {
                        _memHolder.AddObject(index.ToString(), keepAlive);
                    }
                }
            }

            public object this[[NotNull]Slice slice] {
                get {
                    int start, stop, step;
                    int size = ((ArrayType)NativeType).Length;
                    SimpleType elemType = ((ArrayType)NativeType).ElementType as SimpleType;

                    slice.indices(size, out start, out stop, out step);
                    if ((step > 0 && start >= stop) || (step < 0 && start <= stop)) {
                        if (elemType != null && (elemType._type == SimpleTypeKind.WChar || elemType._type == SimpleTypeKind.Char)) {
                            return String.Empty;
                        }
                        return new List();
                    }

                    int n = (int)(step > 0 ? (0L + stop - start + step - 1) / step : (0L + stop - start + step + 1) / step);
                    if (elemType != null && (elemType._type == SimpleTypeKind.WChar || elemType._type == SimpleTypeKind.Char)) {
                        int elmSize = ((INativeType)elemType).Size;
                        StringBuilder res = new StringBuilder(n);

                        for (int i = 0, index = start; i < n; i++, index += step) {
                            char c = elemType.ReadChar(_memHolder, checked(index * elmSize));
                            res.Append(c);
                        }

                        return res.ToString();
                    } else {
                        object[] ret = new object[n];
                        int ri = 0;
                        for (int i = 0, index = start; i < n; i++, index += step) {
                            ret[ri++] = this[index];
                        }

                        return new List(ret);
                    }
                }
                set {
                    int start, stop, step;
                    int size = ((ArrayType)NativeType).Length;
                    SimpleType elemType = ((ArrayType)NativeType).ElementType as SimpleType;

                    slice.indices(size, out start, out stop, out step);

                    int n = (int)(step > 0 ? (0L + stop - start + step - 1) / step : (0L + stop - start + step + 1) / step);
                    IEnumerator ie = PythonOps.GetEnumerator(value);
                    for (int i = 0, index = start; i < n; i++, index += step) {
                        if (!ie.MoveNext()) {
                            throw PythonOps.ValueError("sequence not long enough");
                        }
                        this[index] = ie.Current;
                    }

                    if (ie.MoveNext()) {
                        throw PythonOps.ValueError("not all values consumed while slicing");
                    }
                }
            }

            public int __len__() {
                return ((ArrayType)NativeType).Length;
            }

            private INativeType ElementType {
                get {
                    return ((ArrayType)NativeType).ElementType;
                }
            }

            
            internal override PythonTuple GetBufferInfo() {
                INativeType elemType = ElementType;
                int dimensions = 1;
                List<object> shape = new List<object>();
                shape.Add((BigInteger)__len__());
                while (elemType is ArrayType) {
                    dimensions++;
                    shape.Add((BigInteger)((ArrayType)elemType).Length);
                    elemType = ((ArrayType)elemType).ElementType;
                }


                return PythonTuple.MakeTuple(
                    NativeType.TypeFormat,
                    dimensions,
                    PythonTuple.Make(shape)
                );
            }
        }
    }
}
#endif
