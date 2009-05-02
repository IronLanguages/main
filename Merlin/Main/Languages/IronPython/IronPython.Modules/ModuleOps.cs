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
using System.Runtime.InteropServices;

using Microsoft.Scripting.Math;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

#if !SILVERLIGHT
namespace IronPython.Modules {
    /// <summary>
    /// Provides helper functions which need to be called from generated code to implement various 
    /// portions of modules.
    /// </summary>
    public static partial class ModuleOps {
        public static IntPtr StringToHGlobalAnsi(string str) {
            if (str == null) {
                return IntPtr.Zero;
            }

            return Marshal.StringToHGlobalAnsi(str);
        }

        public static IntPtr StringToHGlobalUni(string str) {
            if (str == null) {
                return IntPtr.Zero;
            }

            return Marshal.StringToHGlobalUni(str);
        }

        public static object CreateMemoryHolder(IntPtr data, int size) {
            var res = new MemoryHolder(size);
            res.CopyFrom(data, new IntPtr(size));
            return res;
        }

        public static object CreateNativeWrapper(PythonType type, object holder) {
            Debug.Assert(holder is MemoryHolder);

            CTypes.CData data = (CTypes.CData)type.CreateInstance(type.Context.DefaultBinderState.Context);
            data._memHolder = (MemoryHolder)holder;
            return data;
        }

        public static object CreateCData(IntPtr dataAddress, PythonType type) {
            CTypes.INativeType nativeType = (CTypes.INativeType)type;
            CTypes.CData data = (CTypes.CData)type.CreateInstance(type.Context.DefaultBinderState.Context);
            data._memHolder = new MemoryHolder(nativeType.Size);
            data._memHolder.CopyFrom(dataAddress, new IntPtr(nativeType.Size));
            return data;
        }

        public static object CreateCFunction(IntPtr address, PythonType type) {
            return type.CreateInstance(type.Context.DefaultBinderState.Context, address);
        }

        public static CTypes.CData CheckSimpleCDataType(object o, object type) {
            CTypes.SimpleCData res = o as CTypes.SimpleCData;
            if (res != null && res.NativeType != type) {
                throw PythonOps.TypeErrorForTypeMismatch(((PythonType)type).Name, o);
            }

            return res;
        }

        public static CTypes.CData/*!*/ CheckCDataType(object o, object type) {
            CTypes.CData res = o as CTypes.CData;
            if (res == null || res.NativeType != type) {
                throw PythonOps.TypeErrorForTypeMismatch(((PythonType)type).Name, o);
            }

            return res;
        }

        public static IntPtr/*!*/ GetFunctionPointerValue(object o, object type) {
            CTypes._CFuncPtr res = o as CTypes._CFuncPtr;
            if (res == null || res.NativeType != type) {
                throw PythonOps.TypeErrorForTypeMismatch(((PythonType)type).Name, o);
            }

            return res.addr;
        }

        public static CTypes.CData TryCheckCDataPointerType(object o, object type) {
            CTypes.CData res = o as CTypes.CData;
            if (res != null && res.NativeType != ((CTypes.PointerType)type)._type) {
                throw PythonOps.TypeErrorForTypeMismatch(((PythonType)((CTypes.PointerType)type)._type).Name, o);
            }

            return res;
        }

        public static CTypes.CData CheckNativeArgument(object o, object type) {
            CTypes.NativeArgument arg = o as CTypes.NativeArgument;
            if (arg != null) {
                if (((CTypes.PointerType)type)._type != DynamicHelpers.GetPythonType(arg._obj)) {
                    throw PythonOps.TypeErrorForTypeMismatch(((PythonType)type).Name, o);
                }
                return arg._obj;
            }

            return null;
        }

        public static string CharToString(byte c) {
            return new string((char)c, 1);
        }

        public static string WCharToString(char c) {
            return new string(c, 1);
        }

        public static char StringToChar(string s) {
            return s[0];
        }

        public static string EnsureString(object o) {
            string res = o as string;
            if (res == null) {
                throw PythonOps.TypeErrorForTypeMismatch("str", o);
            }

            return res;
        }

        public static bool CheckFunctionId(CTypes._CFuncPtr func, int id) {
            return func.Id == id;
        }

        public static IntPtr GetWCharPointer(object value) {
            string strVal = value as string;
            if (strVal != null) {
                return Marshal.StringToCoTaskMemUni(strVal);
            }


            if (value == null) {
                return IntPtr.Zero;
            }

            throw PythonOps.TypeErrorForTypeMismatch("wchar pointer", value);
        }

        public static IntPtr GetCharPointer(object value) {
            string strVal = value as string;
            if (strVal != null) {
                return Marshal.StringToCoTaskMemAnsi(strVal);
            }

            if (value == null) {
                return IntPtr.Zero;
            }

            throw PythonOps.TypeErrorForTypeMismatch("char pointer", value);
        }

        public static IntPtr GetPointer(object value) {
            if (value is int) {
                int iVal = (int)value;
                if (iVal >= 0) {
                    return new IntPtr(iVal);
                }
            }

            BigInteger bi = value as BigInteger;
            if (!Object.ReferenceEquals(bi, null)) {
                return new IntPtr(bi.ToInt64());
            }

            if (value == null) {
                return IntPtr.Zero;
            }

            throw PythonOps.TypeErrorForTypeMismatch("pointer", value);
        }

        public static IntPtr GetObject(object value) {
            GCHandle handle = GCHandle.Alloc(value);

            // TODO: Need to free the handle at some point
            return GCHandle.ToIntPtr(handle);
        }

        public static long GetSignedLongLong(object value) {
            int? res = Converter.ImplicitConvertToInt32(value);
            if (res != null) {
                return res.Value;
            }

            BigInteger bi = value as BigInteger;
            if (!Object.ReferenceEquals(bi, null)) {
                return bi.ToInt64();
            }

            throw PythonOps.TypeErrorForTypeMismatch("signed long long ", value);
        }

        public static long GetUnsignedLongLong(object value) {
            int? res = Converter.ImplicitConvertToInt32(value);
            if (res != null && res.Value >= 0) {
                return res.Value;
            }

            BigInteger bi = value as BigInteger;
            if (!Object.ReferenceEquals(bi, null)) {
                return (long)bi.ToUInt64();
            }

            throw PythonOps.TypeErrorForTypeMismatch("unsigned long long", value);
        }

        public static double GetDouble(object value) {
            if (value is double) {
                return (double)value;
            } else if (value is float) {
                return (float)value;
            } else if (value is int) {
                return (double)(int)value;
            } else if (value is BigInteger) {
                return (double)((BigInteger)value).ToFloat64();
            }

            return Converter.ConvertToDouble(value);
        }

        public static float GetSingle(object value) {
            if (value is double) {
                return (float)(double)value;
            } else if (value is float) {
                return (float)value;
            } else if (value is int) {
                return (float)(int)value;
            } else if (value is BigInteger) {
                return (float)((BigInteger)value).ToFloat64();
            }

            return (float)Converter.ConvertToDouble(value);
        }
        public static long GetDoubleBits(object value) {
            if (value is double) {
                return BitConverter.ToInt64(BitConverter.GetBytes((double)value), 0);
            } else if (value is float) {
                return BitConverter.ToInt64(BitConverter.GetBytes((float)value), 0);
            } else if (value is int) {
                return BitConverter.ToInt64(BitConverter.GetBytes((double)(int)value), 0);
            } else if (value is BigInteger) {
                return BitConverter.ToInt64(BitConverter.GetBytes((double)((BigInteger)value).ToFloat64()), 0);
            }

            return BitConverter.ToInt64(BitConverter.GetBytes(Converter.ConvertToDouble(value)), 0);
        }

        public static int GetSingleBits(object value) {
            if (value is double) {
                return BitConverter.ToInt32(BitConverter.GetBytes((float)(double)value), 0);
            } else if (value is float) {
                return BitConverter.ToInt32(BitConverter.GetBytes((float)value), 0);
            } else if (value is int) {
                return BitConverter.ToInt32(BitConverter.GetBytes((float)(int)value), 0);
            } else if (value is BigInteger) {
                return BitConverter.ToInt32(BitConverter.GetBytes((float)((BigInteger)value).ToFloat64()), 0);
            }

            return BitConverter.ToInt32(BitConverter.GetBytes((float)Converter.ConvertToDouble(value)), 0);
        }

        public static int GetSignedLong(object value) {
            if (value is int) {
                return (int)value;
            }

            int? res = Converter.ImplicitConvertToInt32(value);
            if (res != null) {
                return res.Value;
            }

            throw PythonOps.TypeErrorForTypeMismatch("signed long", value);
        }

        public static int GetUnsignedLong(object value) {
            int? res = Converter.ImplicitConvertToInt32(value);
            if (res != null && res.Value >= 0) {
                return res.Value;
            }

            if (value is BigInteger) {
                uint ures;
                if (((BigInteger)value).AsUInt32(out ures)) {
                    return (int)ures;
                }
            }

            throw PythonOps.TypeErrorForTypeMismatch("unsigned long", value);
        }

        public static int GetUnsignedInt(object value) {
            int? res = Converter.ImplicitConvertToInt32(value);
            if (res != null && res.Value >= 0) {
                return res.Value;
            }

            throw PythonOps.TypeErrorForTypeMismatch("unsigned int", value);
        }

        public static int GetSignedInt(object value) {
            int? res = Converter.ImplicitConvertToInt32(value);
            if (res != null) {
                return res.Value;
            }
            throw PythonOps.TypeErrorForTypeMismatch("signed int", value);
        }

        public static short GetUnsignedShort(object value) {
            int? res = Converter.ImplicitConvertToInt32(value);
            if (res != null) {
                int iVal = res.Value;
                if (iVal >= ushort.MinValue && iVal <= ushort.MaxValue) {
                    return (short)(ushort)iVal;
                }
            }
            throw PythonOps.TypeErrorForTypeMismatch("unsigned short", value);
        }

        public static short GetSignedShort(object value) {
            int? res = Converter.ImplicitConvertToInt32(value);
            if (res != null) {
                int iVal = res.Value;
                if (iVal >= short.MinValue && iVal <= short.MaxValue) {
                    return (short)iVal;
                }
            }
            throw PythonOps.TypeErrorForTypeMismatch("signed short", value);
        }

        public static byte GetUnsignedByte(object value) {
            int? res = Converter.ImplicitConvertToInt32(value);
            if (res != null) {
                return (byte)res.Value;
            }

            throw PythonOps.TypeErrorForTypeMismatch("unsigned byte", value);
        }

        public static byte GetSignedByte(object value) {
            int? res = Converter.ImplicitConvertToInt32(value);
            if (res != null) {
                int iVal = res.Value;
                if (iVal >= sbyte.MinValue && iVal <= sbyte.MaxValue) {
                    return (byte)(sbyte)iVal;
                }
            }
            throw PythonOps.TypeErrorForTypeMismatch("signed byte", value);
        }


        public static byte GetBoolean(object value) {
            if (value is bool) {
                return ((bool)value) ? (byte)1 : (byte)0;
            }

            throw PythonOps.TypeErrorForTypeMismatch("bool", value);
        }

        public static byte GetChar(object value) {
            string strVal = value as string;
            if (strVal != null && strVal.Length == 1) {
                return (byte)strVal[0];
            }

            throw PythonOps.TypeErrorForTypeMismatch("char", value);
        }

        public static char GetWChar(object value) {
            string strVal = value as string;
            if (strVal != null && strVal.Length == 1) {
                return strVal[0];
            }

            throw PythonOps.TypeErrorForTypeMismatch("wchar", value);
        }

    }
}
#endif