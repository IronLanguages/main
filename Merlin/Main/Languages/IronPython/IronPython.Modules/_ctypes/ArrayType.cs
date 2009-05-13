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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

#if !SILVERLIGHT

namespace IronPython.Modules {
    /// <summary>
    /// Provides support for interop with native code from Python code.
    /// </summary>
    public static partial class CTypes {
        private static WeakDictionary<PythonType, Dictionary<int, ArrayType>> _arrayTypes = new WeakDictionary<PythonType, Dictionary<int, ArrayType>>();

        /// <summary>
        /// The meta class for ctypes array instances.
        /// </summary>
        [PythonType, PythonHidden]
        public class ArrayType : PythonType, INativeType {
            private int _length;
            private INativeType _type;

            public ArrayType(CodeContext/*!*/ context, string name, PythonTuple bases, IAttributesCollection dict)
                : base(context, name, bases, dict) {
                object len;
                int iLen;
                if (!dict.TryGetValue(SymbolTable.StringToId("_length_"), out len) || !(len is int) || (iLen = (int)len) < 0) {
                    throw PythonOps.AttributeError("arrays must have _length_ attribute and it must be a positive integer");
                }

                object type;
                if (!dict.TryGetValue(SymbolTable.StringToId("_type_"), out type)) {
                    throw PythonOps.AttributeError("class must define a '_type_' attribute");
                }

                _length = iLen;
                _type = (INativeType)type;

                if (_type is SimpleType) {
                    SimpleType st = (SimpleType)_type;
                    if (st._type == SimpleTypeKind.Char) {
                        // TODO: (c_int * 2).value isn't working
                        SetCustomMember(context,
                            SymbolTable.StringToId("value"),
                            new ReflectedExtensionProperty(
                                new ExtensionPropertyInfo(this, typeof(CTypes).GetMethod("GetCharArrayValue")),
                                NameType.Property | NameType.Python
                            )
                        );

                        SetCustomMember(context,
                            SymbolTable.StringToId("raw"),
                            new ReflectedExtensionProperty(
                                new ExtensionPropertyInfo(this, typeof(CTypes).GetMethod("GetWCharArrayRaw")),
                                NameType.Property | NameType.Python
                            )
                        );
                    } else if (st._type == SimpleTypeKind.WChar) {
                        SetCustomMember(context,
                            SymbolTable.StringToId("value"),
                            new ReflectedExtensionProperty(
                                new ExtensionPropertyInfo(this, typeof(CTypes).GetMethod("GetWCharArrayValue")),
                                NameType.Property | NameType.Python
                            )
                        );

                        SetCustomMember(context,
                            SymbolTable.StringToId("raw"),
                            new ReflectedExtensionProperty(
                                new ExtensionPropertyInfo(this, typeof(CTypes).GetMethod("GetWCharArrayRaw")),
                                NameType.Property | NameType.Python
                            )
                        );
                    }
                }
            }

            private ArrayType(Type underlyingSystemType)
                : base(underlyingSystemType) {
            }

            /// <summary>
            /// Converts an object into a function call parameter.
            /// </summary>
            public object from_param(object obj) {
                return null;
            }

            public _Array from_address(CodeContext/*!*/ context, int ptr) {
                _Array res = (_Array)CreateInstance(context);
                res.SetAddress(new IntPtr(ptr));
                return res;
            }

            public _Array from_address(CodeContext/*!*/ context, BigInteger ptr) {
                _Array res = (_Array)CreateInstance(context);
                res.SetAddress(new IntPtr(ptr.ToInt64()));
                return res;
            }

            internal static PythonType MakeSystemType(Type underlyingSystemType) {
                return PythonType.SetPythonType(underlyingSystemType, new ArrayType(underlyingSystemType));
            }

            public static ArrayType/*!*/ operator *(ArrayType type, int count) {
                return MakeArrayType(type, count);
            }

            public static ArrayType/*!*/ operator *(int count, ArrayType type) {
                return MakeArrayType(type, count);
            }

            #region INativeType Members

            int INativeType.Size {
                get {
                    return GetSize();
                }
            }

            private int GetSize() {
                return _length * _type.Size;
            }

            int INativeType.Alignment {
                get {
                    return _type.Alignment;
                }
            }

            object INativeType.GetValue(MemoryHolder owner, int offset, bool raw) {
                if (IsStringType) {
                    SimpleType st = (SimpleType)_type;
                    string str;
                    if (st._type == SimpleTypeKind.Char) {
                        str = owner.ReadAnsiString(offset, _length);
                    } else {
                        str = owner.ReadUnicodeString(offset, _length);
                    }

                    // remove any trailing nulls
                    for (int i = 0; i < str.Length; i++) {
                        if (str[i] == '\x00') {
                            return str.Substring(0, i);
                        }
                    }

                    return str;
                }

                object[] res = new object[_length];
                for (int i = 0; i < res.Length; i++) {
                    res[i] = _type.GetValue(owner, checked(offset + _type.Size * i), raw);
                }

                return List.FromArrayNoCopy(res);
            }

            internal string GetRawValue(MemoryHolder owner, int offset) {
                Debug.Assert(IsStringType);
                SimpleType st = (SimpleType)_type;
                string str;
                if (st._type == SimpleTypeKind.Char) {
                    str = owner.ReadAnsiString(offset, _length);
                } else {
                    str = owner.ReadUnicodeString(offset, _length);
                }

                return str;
            }

            private bool IsStringType {
                get {
                    SimpleType st = _type as SimpleType;
                    if (st != null) {
                        return st._type == SimpleTypeKind.WChar || st._type == SimpleTypeKind.Char;
                    }

                    return false;
                }
            }

            void INativeType.SetValue(MemoryHolder address, int offset, object value) {
                string str = value as string;
                if (str != null) {
                    if (!IsStringType) {
                        throw PythonOps.TypeError("expected {0} instance, got str", Name);
                    } else if (str.Length > _length) {
                        throw PythonOps.ValueError("string too long ({0}, maximum length {1})", str.Length, _length);
                    }

                    WriteString(address, offset, str);

                    return;
                } else if (IsStringType) {
                    IList<object> objList = value as IList<object>;
                    if (objList != null) {
                        StringBuilder res = new StringBuilder(objList.Count);
                        foreach (object o in objList) {
                            res.Append(Converter.ConvertToChar(o));
                        }

                        WriteString(address, offset, res.ToString());
                        return;
                    }

                    throw PythonOps.TypeError("expected string or Unicode object, {0} found", DynamicHelpers.GetPythonType(value).Name);
                }

                object[] arrArgs = value as object[];

                if (arrArgs == null) {
                    PythonTuple pt = value as PythonTuple;
                    if (pt != null) {
                        arrArgs = pt._data;
                    }

                }

                if (arrArgs != null) {
                    if (arrArgs.Length > _length) {
                        throw PythonOps.RuntimeError("invalid index");
                    }

                    for (int i = 0; i < arrArgs.Length; i++) {
                        _type.SetValue(address, checked(offset + i * _type.Size), arrArgs[i]);
                    }
                } else {
                    throw PythonOps.TypeError("unexpected {0} instance, got {1}", Name, DynamicHelpers.GetPythonType(value).Name);
                }
            }

            private void WriteString(MemoryHolder address, int offset, string str) {
                SimpleType st = (SimpleType)_type;
                if (str.Length < _length) {
                    str = str + '\x00';
                }
                if (st._type == SimpleTypeKind.Char) {
                    address.WriteAnsiString(offset, str);
                } else {
                    address.WriteUnicodeString(offset, str);
                }

            }

            Type/*!*/ INativeType.GetNativeType() {
                return typeof(IntPtr);
            }

            MarshalCleanup INativeType.EmitMarshalling(ILGenerator/*!*/ method, LocalOrArg argIndex, List<object>/*!*/ constantPool, int constantPoolArgument) {
                Type argumentType = argIndex.Type;
                argIndex.Emit(method);
                if (argumentType.IsValueType) {
                    method.Emit(OpCodes.Box, argumentType);
                }
                constantPool.Add(this);
                method.Emit(OpCodes.Ldarg, constantPoolArgument);
                method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
                method.Emit(OpCodes.Ldelem_Ref);
                method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CheckCDataType"));
                method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
                return null;
            }

            Type/*!*/ INativeType.GetPythonType() {
                return ((INativeType)this).GetNativeType();
            }

            void INativeType.EmitReverseMarshalling(ILGenerator method, LocalOrArg value, List<object> constantPool, int constantPoolArgument) {
                // TODO: Implement me
                value.Emit(method);
            }

            #endregion

            internal int Length {
                get {
                    return _length;
                }
            }

            internal INativeType ElementType {
                get {
                    return _type;
                }
            }
        }

        private static ArrayType/*!*/ MakeArrayType(PythonType type, int count) {
            if (count < 0) {
                throw PythonOps.ValueError("cannot multiply ctype by negative number");
            }

            lock (_arrayTypes) {
                ArrayType res;
                Dictionary<int, ArrayType> countDict;
                if (!_arrayTypes.TryGetValue(type, out countDict)) {
                    _arrayTypes[type] = countDict = new Dictionary<int, ArrayType>();
                }

                if (!countDict.TryGetValue(count, out res)) {
                    res = countDict[count] = new ArrayType(type.Context.SharedContext,
                        type.Name + "_Array_" + count,
                        PythonTuple.MakeTuple(Array),
                        PythonOps.MakeDictFromItems(new object[] { type, "_type_", count, "_length_" })
                    );
                }

                return res;
            }
        }
    }
}

#endif
