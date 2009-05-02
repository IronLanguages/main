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
using System.Collections.Generic;
using System.Reflection.Emit;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

#if !SILVERLIGHT

namespace IronPython.Modules {
    /// <summary>
    /// Provides support for interop with native code from Python code.
    /// </summary>
    public static partial class CTypes {

        /// <summary>
        /// The meta class for ctypes pointers.
        /// </summary>
        [PythonType, PythonHidden]
        public class PointerType : PythonType, INativeType {
            internal readonly INativeType _type;

            public PointerType(CodeContext/*!*/ context, string name, PythonTuple bases, IAttributesCollection members)
                : base(context, name, bases, members) {

                object type;
                if (members.TryGetValue(SymbolTable.StringToId("_type_"), out type) && !(type is INativeType)) {
                    throw PythonOps.TypeError("_type_ must be a type");
                }
                _type = (INativeType)type;
            }

            private PointerType(Type underlyingSystemType)
                : base(underlyingSystemType) {
            }

            /// <summary>
            /// Converts an object into a function call parameter.
            /// </summary>
            public object from_param(object obj) {
                return null;
            }

            /// <summary>
            /// Access an instance at the specified address
            /// </summary>
            public object from_address(object obj) {
                throw new NotImplementedException("pointer from address");
            }

            internal static PythonType MakeSystemType(Type underlyingSystemType) {
                return PythonType.SetPythonType(underlyingSystemType, new PointerType(underlyingSystemType));
            }

            public static ArrayType/*!*/ operator *(PointerType type, int count) {
                return MakeArrayType(type, count);
            }

            public static ArrayType/*!*/ operator *(int count, PointerType type) {
                return MakeArrayType(type, count);
            }

            #region INativeType Members

            int INativeType.Size {
                get {
                    return IntPtr.Size;
                }
            }

            int INativeType.Alignment {
                get {
                    return IntPtr.Size;
                }
            }

            object INativeType.GetValue(MemoryHolder owner, int offset, bool raw) {
                return ToPython(owner.ReadIntPtr(offset));
            }

            void INativeType.SetValue(MemoryHolder address, int offset, object value) {
                if (value is int) {
                    address.WriteIntPtr(offset, new IntPtr((int)value));
                } else if (value is BigInteger) {
                    address.WriteIntPtr(offset, new IntPtr(((BigInteger)value).ToInt64()));
                } else {
                    Pointer ptr = value as Pointer;
                    if (ptr != null) {
                        address.WriteIntPtr(offset, ptr._memHolder.ReadMemoryHolder(0));
                    } else {
                        throw new NotImplementedException("pointer set value");
                    }
                }

            }

            Type INativeType.GetNativeType() {
                return typeof(IntPtr);
            }

            MarshalCleanup INativeType.EmitMarshalling(ILGenerator/*!*/ method, LocalOrArg argIndex, List<object>/*!*/ constantPool, int constantPoolArgument) {
                Type argumentType = argIndex.Type;
                argIndex.Emit(method);
                if (argumentType.IsValueType) {
                    method.Emit(OpCodes.Box, argumentType);
                }
                // native argument being pased (byref)
                Label nextTry = method.DefineLabel();
                Label done = method.DefineLabel();
                constantPool.Add(this);
                method.Emit(OpCodes.Ldarg, constantPoolArgument);
                method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
                method.Emit(OpCodes.Ldelem_Ref);
                method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CheckNativeArgument"));
                method.Emit(OpCodes.Dup);
                method.Emit(OpCodes.Brfalse, nextTry);
                method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
                method.Emit(OpCodes.Br, done);

                // lone cdata being passed
                method.MarkLabel(nextTry);
                nextTry = method.DefineLabel();
                method.Emit(OpCodes.Pop);   // extra null native arg
                argIndex.Emit(method);
                if (argumentType.IsValueType) {
                    method.Emit(OpCodes.Box, argumentType);
                }
                method.Emit(OpCodes.Ldarg, constantPoolArgument);
                method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
                method.Emit(OpCodes.Ldelem_Ref);
                method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("TryCheckCDataPointerType"));
                method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
                method.Emit(OpCodes.Br, done);

                // pointer object being passed
                method.MarkLabel(nextTry);
                method.Emit(OpCodes.Pop);   // extra null cdata
                argIndex.Emit(method);
                if (argumentType.IsValueType) {
                    method.Emit(OpCodes.Box, argumentType);
                }
                method.Emit(OpCodes.Ldarg, constantPoolArgument);
                method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
                method.Emit(OpCodes.Ldelem_Ref);
                method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CheckCDataType"));
                method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
                method.Emit(OpCodes.Ldobj, typeof(IntPtr));

                method.MarkLabel(done);
                return null;
            }

            Type/*!*/ INativeType.GetPythonType() {
                return typeof(object);
            }

            void INativeType.EmitReverseMarshalling(ILGenerator method, LocalOrArg value, List<object> constantPool, int constantPoolArgument) {
                value.Emit(method);
                EmitCDataCreation(this, method, constantPool, constantPoolArgument);
            }

            #endregion
        }
    }
}

#endif
