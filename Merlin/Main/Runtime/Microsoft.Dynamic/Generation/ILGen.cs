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
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {

    public delegate void EmitArrayHelper(int index);

    // TODO: change to extension methods for ILGenerator
    public class ILGen {
        private readonly ILGenerator _ilg;
        private readonly KeyedQueue<Type, LocalBuilder> _freeLocals = new KeyedQueue<Type, LocalBuilder>();

        // TODO: remove Python dependency
        public ILGen(ILGenerator ilg) {
            ContractUtils.RequiresNotNull(ilg, "ilg");

            _ilg = ilg;
        }

        #region ILGenerator Methods

        /// <summary>
        /// Begins a catch block.
        /// </summary>
        public virtual void BeginCatchBlock(Type exceptionType) {
            _ilg.BeginCatchBlock(exceptionType);
        }

        /// <summary>
        /// Begins an exception block for a filtered exception.
        /// </summary>
        public virtual void BeginExceptFilterBlock() {
            _ilg.BeginExceptFilterBlock();
        }

        /// <summary>
        /// Begins an exception block for a non-filtered exception.
        /// </summary>
        /// <returns></returns>
        public virtual Label BeginExceptionBlock() {
            return _ilg.BeginExceptionBlock();
        }

        /// <summary>
        /// Begins an exception fault block
        /// </summary>
        public virtual void BeginFaultBlock() {
            _ilg.BeginFaultBlock();
        }

        /// <summary>
        /// Begins a finally block
        /// </summary>
        public virtual void BeginFinallyBlock() {
            _ilg.BeginFinallyBlock();
        }

        /// <summary>
        /// Ends an exception block.
        /// </summary>
        public virtual void EndExceptionBlock() {
            _ilg.EndExceptionBlock();
        }

        /// <summary>
        /// Begins a lexical scope.
        /// </summary>
        public virtual void BeginScope() {
            _ilg.BeginScope();
        }

        /// <summary>
        /// Ends a lexical scope.
        /// </summary>
        public virtual void EndScope() {
            _ilg.EndScope();
        }

        /// <summary>
        /// Declares a local variable of the specified type.
        /// </summary>
        public virtual LocalBuilder DeclareLocal(Type localType) {
            return _ilg.DeclareLocal(localType);
        }

        /// <summary>
        /// Declares a local variable of the specified type, optionally
        /// pinning the object referred to by the variable.
        /// </summary>
        public virtual LocalBuilder DeclareLocal(Type localType, bool pinned) {
            return _ilg.DeclareLocal(localType, pinned);
        }

        /// <summary>
        /// Declares a new label.
        /// </summary>
        public virtual Label DefineLabel() {
            return _ilg.DefineLabel();
        }

        /// <summary>
        /// Marks the label at the current position.
        /// </summary>
        public virtual void MarkLabel(Label loc) {
            _ilg.MarkLabel(loc);
        }

        /// <summary>
        /// Emits an instruction.
        /// </summary>
        public virtual void Emit(OpCode opcode) {
            _ilg.Emit(opcode);
        }

        /// <summary>
        /// Emits an instruction with a byte argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, byte arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with the metadata token for the specified contructor.
        /// </summary>
        public virtual void Emit(OpCode opcode, ConstructorInfo con) {
            _ilg.Emit(opcode, con);
        }

        /// <summary>
        /// Emits an instruction with a double argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, double arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with the metadata token for the specified field.
        /// </summary>
        public virtual void Emit(OpCode opcode, FieldInfo field) {
            _ilg.Emit(opcode, field);
        }

        /// <summary>
        /// Emits an instruction with a float argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, float arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with an int argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, int arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with a label argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, Label label) {
            _ilg.Emit(opcode, label);
        }

        /// <summary>
        /// Emits an instruction with multiple target labels (switch).
        /// </summary>
        public virtual void Emit(OpCode opcode, Label[] labels) {
            _ilg.Emit(opcode, labels);
        }

        /// <summary>
        /// Emits an instruction with a reference to a local variable.
        /// </summary>
        public virtual void Emit(OpCode opcode, LocalBuilder local) {
            _ilg.Emit(opcode, local);
        }

        /// <summary>
        /// Emits an instruction with a long argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, long arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with the metadata token for a specified method.
        /// </summary>
        public virtual void Emit(OpCode opcode, MethodInfo meth) {
            _ilg.Emit(opcode, meth);
        }

        /// <summary>
        /// Emits an instruction with a signed byte argument.
        /// </summary>
        [CLSCompliant(false)]
        public virtual void Emit(OpCode opcode, sbyte arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with a short argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, short arg) {
            _ilg.Emit(opcode, arg);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Emits an instruction with a signature token.
        /// </summary>
        public virtual void Emit(OpCode opcode, SignatureHelper signature) {
            _ilg.Emit(opcode, signature);
        }
#endif

        /// <summary>
        /// Emits an instruction with a string argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, string str) {
            _ilg.Emit(opcode, str);
        }

        /// <summary>
        /// Emits an instruction with the metadata token for a specified type argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, Type cls) {
            _ilg.Emit(opcode, cls);
        }

        /// <summary>
        /// Emits a call or a virtual call to the varargs method.
        /// </summary>
        public virtual void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes) {
            _ilg.EmitCall(opcode, methodInfo, optionalParameterTypes);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Emits an unmanaged indirect call instruction.
        /// </summary>
        public virtual void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes) {
            _ilg.EmitCalli(opcode, unmanagedCallConv, returnType, parameterTypes);
        }

        /// <summary>
        /// Emits a managed indirect call instruction.
        /// </summary>
        public virtual void EmitCalli(OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes) {
            _ilg.EmitCalli(opcode, callingConvention, returnType, parameterTypes, optionalParameterTypes);
        }
#endif

        /// <summary>
        /// Marks a sequence point.
        /// </summary>
        public virtual void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn) {
            _ilg.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }

        /// <summary>
        /// Specifies the namespace to be used in evaluating locals and watches for the
        ///     current active lexical scope.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")] // TODO: fix
        public virtual void UsingNamespace(string usingNamespace) {
            _ilg.UsingNamespace(usingNamespace);
        }

        #endregion

        #region Simple helpers

        [Conditional("DEBUG")]
        internal void EmitDebugWriteLine(string message) {
            EmitString(message);
            EmitCall(typeof(Debug), "WriteLine", new Type[] { typeof(string) });
        }

        internal void Emit(OpCode opcode, MethodBase methodBase) {
            Debug.Assert(methodBase is MethodInfo || methodBase is ConstructorInfo);

            if (methodBase.MemberType == MemberTypes.Constructor) {
                Emit(opcode, (ConstructorInfo)methodBase);
            } else {
                Emit(opcode, (MethodInfo)methodBase);
            }
        }

        #endregion

        #region Instruction helpers

        public void EmitLoadArg(int index) {
            ContractUtils.Requires(index >= 0, "index");

            switch (index) {
                case 0:
                    Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index <= Byte.MaxValue) {
                        Emit(OpCodes.Ldarg_S, (byte)index);
                    } else {
                        this.Emit(OpCodes.Ldarg, index);
                    }
                    break;
            }
        }

        public void EmitLoadArgAddress(int index) {
            ContractUtils.Requires(index >= 0, "index");

            if (index <= Byte.MaxValue) {
                Emit(OpCodes.Ldarga_S, (byte)index);
            } else {
                Emit(OpCodes.Ldarga, index);
            }
        }

        public void EmitStoreArg(int index) {
            ContractUtils.Requires(index >= 0, "index");

            if (index <= Byte.MaxValue) {
                Emit(OpCodes.Starg_S, (byte)index);
            } else {
                Emit(OpCodes.Starg, index);
            }
        }

        /// <summary>
        /// Emits a Ldind* instruction for the appropriate type
        /// </summary>
        public void EmitLoadValueIndirect(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            if (type.IsValueType) {
                if (type == typeof(int)) {
                    Emit(OpCodes.Ldind_I4);
                } else if (type == typeof(uint)) {
                    Emit(OpCodes.Ldind_U4);
                } else if (type == typeof(short)) {
                    Emit(OpCodes.Ldind_I2);
                } else if (type == typeof(ushort)) {
                    Emit(OpCodes.Ldind_U2);
                } else if (type == typeof(long) || type == typeof(ulong)) {
                    Emit(OpCodes.Ldind_I8);
                } else if (type == typeof(char)) {
                    Emit(OpCodes.Ldind_I2);
                } else if (type == typeof(bool)) {
                    Emit(OpCodes.Ldind_I1);
                } else if (type == typeof(float)) {
                    Emit(OpCodes.Ldind_R4);
                } else if (type == typeof(double)) {
                    Emit(OpCodes.Ldind_R8);
                } else {
                    Emit(OpCodes.Ldobj, type);
                }
            } else if (type.IsGenericParameter) {
                Emit(OpCodes.Ldobj, type);
            } else {
                Emit(OpCodes.Ldind_Ref);
            }
        }


        /// <summary>
        /// Emits a Stind* instruction for the appropriate type.
        /// </summary>
        public void EmitStoreValueIndirect(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            if (type.IsValueType) {
                if (type == typeof(int)) {
                    Emit(OpCodes.Stind_I4);
                } else if (type == typeof(short)) {
                    Emit(OpCodes.Stind_I2);
                } else if (type == typeof(long) || type == typeof(ulong)) {
                    Emit(OpCodes.Stind_I8);
                } else if (type == typeof(char)) {
                    Emit(OpCodes.Stind_I2);
                } else if (type == typeof(bool)) {
                    Emit(OpCodes.Stind_I1);
                } else if (type == typeof(float)) {
                    Emit(OpCodes.Stind_R4);
                } else if (type == typeof(double)) {
                    Emit(OpCodes.Stind_R8);
                } else {
                    Emit(OpCodes.Stobj, type);
                }
            } else if (type.IsGenericParameter) {
                Emit(OpCodes.Stobj, type);
            } else {
                Emit(OpCodes.Stind_Ref);
            }
        }

        // Emits the Ldelem* instruction for the appropriate type
        //CONFORMING
        public void EmitLoadElement(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            if (!type.IsValueType) {
                Emit(OpCodes.Ldelem_Ref);
            } else if (type.IsEnum) {
                Emit(OpCodes.Ldelem, type);
            } else {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                        Emit(OpCodes.Ldelem_I1);
                        break;
                    case TypeCode.Byte:
                        Emit(OpCodes.Ldelem_U1);
                        break;
                    case TypeCode.Int16:
                        Emit(OpCodes.Ldelem_I2);
                        break;
                    case TypeCode.Char:
                    case TypeCode.UInt16:
                        Emit(OpCodes.Ldelem_U2);
                        break;
                    case TypeCode.Int32:
                        Emit(OpCodes.Ldelem_I4);
                        break;
                    case TypeCode.UInt32:
                        Emit(OpCodes.Ldelem_U4);
                        break;
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        Emit(OpCodes.Ldelem_I8);
                        break;
                    case TypeCode.Single:
                        Emit(OpCodes.Ldelem_R4);
                        break;
                    case TypeCode.Double:
                        Emit(OpCodes.Ldelem_R8);
                        break;
                    default:
                        Emit(OpCodes.Ldelem, type);
                        break;
                }
            }
        }

        /// <summary>
        /// Emits a Stelem* instruction for the appropriate type.
        /// </summary>
        public void EmitStoreElement(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            if (type.IsEnum) {
                Emit(OpCodes.Stelem, type);
                return;
            }
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    Emit(OpCodes.Stelem_I1);
                    break;
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    Emit(OpCodes.Stelem_I2);
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    Emit(OpCodes.Stelem_I4);
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    Emit(OpCodes.Stelem_I8);
                    break;
                case TypeCode.Single:
                    Emit(OpCodes.Stelem_R4);
                    break;
                case TypeCode.Double:
                    Emit(OpCodes.Stelem_R8);
                    break;
                default:
                    if (type.IsValueType) {
                        Emit(OpCodes.Stelem, type);
                    } else {
                        Emit(OpCodes.Stelem_Ref);
                    }
                    break;
            }
        }

        public void EmitType(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            Emit(OpCodes.Ldtoken, type);
            EmitCall(typeof(Type), "GetTypeFromHandle");
        }

        public void EmitUnbox(Type type) {
            ContractUtils.RequiresNotNull(type, "type");
            Emit(OpCodes.Unbox_Any, type);
        }

        #endregion

        #region Fields, properties and methods

        public void EmitPropertyGet(Type type, string name) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(name, "name");

            PropertyInfo pi = type.GetProperty(name);
            ContractUtils.Requires(pi != null, "name", Strings.PropertyDoesNotExist);

            EmitPropertyGet(pi);
        }

        public void EmitPropertyGet(PropertyInfo pi) {
            ContractUtils.RequiresNotNull(pi, "pi");

            if (!pi.CanRead) {
                throw Error.CantReadProperty();
            }

            EmitCall(pi.GetGetMethod());
        }

        public void EmitPropertySet(Type type, string name) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(name, "name");

            PropertyInfo pi = type.GetProperty(name);
            ContractUtils.Requires(pi != null, "name", Strings.PropertyDoesNotExist);

            EmitPropertySet(pi);
        }

        public void EmitPropertySet(PropertyInfo pi) {
            ContractUtils.RequiresNotNull(pi, "pi");

            if (!pi.CanWrite) {
                throw Error.CantWriteProperty();
            }

            EmitCall(pi.GetSetMethod());
        }

        public void EmitFieldAddress(FieldInfo fi) {
            ContractUtils.RequiresNotNull(fi, "fi");

            if (fi.IsStatic) {
                Emit(OpCodes.Ldsflda, fi);
            } else {
                Emit(OpCodes.Ldflda, fi);
            }
        }

        public void EmitFieldGet(Type type, String name) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(name, "name");

            FieldInfo fi = type.GetField(name);
            ContractUtils.Requires(fi != null, "name", Strings.FieldDoesNotExist);
            EmitFieldGet(fi);
        }

        public void EmitFieldSet(Type type, String name) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(name, "name");

            FieldInfo fi = type.GetField(name);
            ContractUtils.Requires(fi != null, "name", Strings.FieldDoesNotExist);
            EmitFieldSet(fi);
        }

        public void EmitFieldGet(FieldInfo fi) {
            ContractUtils.RequiresNotNull(fi, "fi");

            if (fi.IsStatic) {
                Emit(OpCodes.Ldsfld, fi);
            } else {
                Emit(OpCodes.Ldfld, fi);
            }
        }

        public void EmitFieldSet(FieldInfo fi) {
            ContractUtils.RequiresNotNull(fi, "fi");

            if (fi.IsStatic) {
                Emit(OpCodes.Stsfld, fi);
            } else {
                Emit(OpCodes.Stfld, fi);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public void EmitNew(ConstructorInfo ci) {
            ContractUtils.RequiresNotNull(ci, "ci");

            if (ci.DeclaringType.ContainsGenericParameters) {
                throw Error.IllegalNew_GenericParams(ci.DeclaringType);
            }

            Emit(OpCodes.Newobj, ci);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public void EmitNew(Type type, Type[] paramTypes) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(paramTypes, "paramTypes");

            ConstructorInfo ci = type.GetConstructor(paramTypes);
            ContractUtils.Requires(ci != null, "type", Strings.TypeDoesNotHaveConstructorForTheSignature);
            EmitNew(ci);
        }

        public void EmitCall(MethodInfo mi) {
            ContractUtils.RequiresNotNull(mi, "mi");

            if (mi.IsVirtual && !mi.DeclaringType.IsValueType) {
                Emit(OpCodes.Callvirt, mi);
            } else {
                Emit(OpCodes.Call, mi);
            }
        }

        public void EmitCall(Type type, String name) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(name, "name");

            MethodInfo mi = type.GetMethod(name);
            ContractUtils.Requires(mi != null, "type", Strings.TypeDoesNotHaveMethodForName);

            EmitCall(mi);
        }

        public void EmitCall(Type type, String name, Type[] paramTypes) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(paramTypes, "paramTypes");

            MethodInfo mi = type.GetMethod(name, paramTypes);
            ContractUtils.Requires(mi != null, "type", Strings.TypeDoesNotHaveMethodForNameSignature);

            EmitCall(mi);
        }

        #endregion

        #region Constants

        public void EmitNull() {
            Emit(OpCodes.Ldnull);
        }

        public void EmitString(string value) {
            ContractUtils.RequiresNotNull(value, "value");
            Emit(OpCodes.Ldstr, value);
        }

        public void EmitBoolean(bool value) {
            if (value) {
                Emit(OpCodes.Ldc_I4_1);
            } else {
                Emit(OpCodes.Ldc_I4_0);
            }
        }

        public void EmitChar(char value) {
            EmitInt(value);
            Emit(OpCodes.Conv_U2);
        }

        public void EmitByte(byte value) {
            EmitInt(value);
            Emit(OpCodes.Conv_U1);
        }

        [CLSCompliant(false)]
        public void EmitSByte(sbyte value) {
            EmitInt(value);
            Emit(OpCodes.Conv_I1);
        }

        public void EmitShort(short value) {
            EmitInt(value);
            Emit(OpCodes.Conv_I2);
        }

        [CLSCompliant(false)]
        public void EmitUShort(ushort value) {
            EmitInt(value);
            Emit(OpCodes.Conv_U2);
        }

        public void EmitInt(int value) {
            OpCode c;
            switch (value) {
                case -1:
                    c = OpCodes.Ldc_I4_M1;
                    break;
                case 0:
                    c = OpCodes.Ldc_I4_0;
                    break;
                case 1:
                    c = OpCodes.Ldc_I4_1;
                    break;
                case 2:
                    c = OpCodes.Ldc_I4_2;
                    break;
                case 3:
                    c = OpCodes.Ldc_I4_3;
                    break;
                case 4:
                    c = OpCodes.Ldc_I4_4;
                    break;
                case 5:
                    c = OpCodes.Ldc_I4_5;
                    break;
                case 6:
                    c = OpCodes.Ldc_I4_6;
                    break;
                case 7:
                    c = OpCodes.Ldc_I4_7;
                    break;
                case 8:
                    c = OpCodes.Ldc_I4_8;
                    break;
                default:
                    if (value >= -128 && value <= 127) {
                        Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    } else {
                        Emit(OpCodes.Ldc_I4, value);
                    }
                    return;
            }
            Emit(c);
        }

        [CLSCompliant(false)]
        public void EmitUInt(uint value) {
            EmitInt((int)value);
            Emit(OpCodes.Conv_U4);
        }

        public void EmitLong(long value) {
            Emit(OpCodes.Ldc_I8, value);
        }

        [CLSCompliant(false)]
        public void EmitULong(ulong value) {
            Emit(OpCodes.Ldc_I8, (long)value);
            Emit(OpCodes.Conv_U8);
        }

        public void EmitDouble(double value) {
            Emit(OpCodes.Ldc_R8, value);
        }

        public void EmitSingle(float value) {
            Emit(OpCodes.Ldc_R4, value);
        }

        private void EmitSimpleConstant(object value) {
            if (!TryEmitConstant(value, value == null ? typeof(object) : value.GetType())) {
                throw Error.CanotEmitConstant(value, value.GetType());
            }
        }

        //CONFORMING
        //
        // Note: we support emitting a lot more things as IL constants than
        // Linq does
        internal bool TryEmitConstant(object value, Type type) {
            if (value == null) {
                // Smarter than the Linq implementation which uses the initobj
                // pattern for all value types (works, but requires a local and
                // more IL)
                EmitDefault(type);
                return true;
            }

            // Handle the easy cases
            if (TryEmitILConstant(value, type)) {
                return true;
            }

            // Check for a few more types that we support emitting as constants
            Type t = value as Type;
            if (t != null && ShouldLdtoken(t)) {
                EmitType(t);
                return true;
            }

            MethodBase mb = value as MethodBase;
            if (mb != null && ShouldLdtoken(mb)) {
                Emit(OpCodes.Ldtoken, mb);
                Type dt = mb.DeclaringType;
                if (dt != null && dt.IsGenericType) {
                    Emit(OpCodes.Ldtoken, dt);
                    EmitCall(typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));
                } else {
                    EmitCall(typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) }));
                }
                type = TypeUtils.GetConstantType(type);
                if (type != typeof(MethodBase)) {
                    Emit(OpCodes.Castclass, type);
                }
                return true;
            }

            return false;
        }

        // TODO: Can we always ldtoken and let restrictedSkipVisibility sort things out?
        public static bool ShouldLdtoken(Type t) {
            return t is TypeBuilder || t.IsGenericParameter || t.IsVisible;
        }

        public static bool ShouldLdtoken(MethodBase mb) {
            // Can't ldtoken on a DynamicMethod
            if (mb is DynamicMethod) {
                return false;
            }

            Type dt = mb.DeclaringType;
            return dt == null || ShouldLdtoken(dt);
        }

        //CONFORMING
        private bool TryEmitILConstant(object value, Type type) {
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    EmitBoolean((bool)value);
                    return true;
                case TypeCode.SByte:
                    EmitSByte((sbyte)value);
                    return true;
                case TypeCode.Int16:
                    EmitShort((short)value);
                    return true;
                case TypeCode.Int32:
                    EmitInt((int)value);
                    return true;
                case TypeCode.Int64:
                    EmitLong((long)value);
                    return true;
                case TypeCode.Single:
                    EmitSingle((float)value);
                    return true;
                case TypeCode.Double:
                    EmitDouble((double)value);
                    return true;
                case TypeCode.Char:
                    EmitChar((char)value);
                    return true;
                case TypeCode.Byte:
                    EmitByte((byte)value);
                    return true;
                case TypeCode.UInt16:
                    EmitUShort((ushort)value);
                    return true;
                case TypeCode.UInt32:
                    EmitUInt((uint)value);
                    return true;
                case TypeCode.UInt64:
                    EmitULong((ulong)value);
                    return true;
                case TypeCode.Decimal:
                    EmitDecimal((decimal)value);
                    return true;
                case TypeCode.String:
                    EmitString((string)value);
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        // TODO: deprecate in favor of the Linq versions
        #region Conversions

        public void EmitImplicitCast(Type from, Type to) {
            if (!TryEmitCast(from, to, true)) {
                throw Error.NoImplicitCast(from, to);
            }
        }

        public void EmitExplicitCast(Type from, Type to) {
            if (!TryEmitCast(from, to, false)) {
                throw Error.NoExplicitCast(from, to);
            }
        }

        public bool TryEmitImplicitCast(Type from, Type to) {
            return TryEmitCast(from, to, true);
        }

        public bool TryEmitExplicitCast(Type from, Type to) {
            return TryEmitCast(from, to, false);
        }

        private bool TryEmitCast(Type from, Type to, bool implicitOnly) {
            ContractUtils.RequiresNotNull(from, "from");
            ContractUtils.RequiresNotNull(to, "to");

            // No cast necessary if identical types
            if (from == to) {
                return true;
            }

            if (to.IsAssignableFrom(from)) {
                // T -> Nullable<T>
                if (TypeUtils.IsNullableType(to)) {
                    Type nonNullableTo = TypeUtils.GetNonNullableType(to);
                    if (TryEmitCast(from, nonNullableTo, true)) {
                        EmitNew(to.GetConstructor(new Type[] { nonNullableTo }));
                    } else {
                        return false;
                    }
                }

                if (from.IsValueType) {
                    if (to == typeof(object)) {
                        EmitBoxing(from);
                        return true;
                    }
                }

                if (to.IsInterface) {
                    Emit(OpCodes.Box, from);
                    return true;
                }

                if (from.IsEnum && to == typeof(Enum)) {
                    Emit(OpCodes.Box, from);
                    return true;
                }

                // They are assignable and reference types.
                return true;
            }

            if (to == typeof(void)) {
                Emit(OpCodes.Pop);
                return true;
            }

            if (to.IsValueType && from == typeof(object)) {
                if (implicitOnly) {
                    return false;
                }
                Emit(OpCodes.Unbox_Any, to);
                return true;
            }

            if (to.IsValueType != from.IsValueType) {
                return false;
            }

            if (!to.IsValueType) {
                if (implicitOnly) {
                    return false;
                }
                Emit(OpCodes.Castclass, to);
                return true;
            }

            if (to.IsEnum) {
                to = Enum.GetUnderlyingType(to);
            }
            if (from.IsEnum) {
                from = Enum.GetUnderlyingType(from);
            }

            if (to == from) {
                return true;
            }

            if (EmitNumericCast(from, to, implicitOnly)) {
                return true;
            }

            return false;
        }

        public bool EmitNumericCast(Type from, Type to, bool implicitOnly) {
            TypeCode fc = Type.GetTypeCode(from);
            TypeCode tc = Type.GetTypeCode(to);
            int fromx, fromy, tox, toy;

            if (!TypeUtils.GetNumericConversionOrder(fc, out fromx, out fromy) ||
                !TypeUtils.GetNumericConversionOrder(tc, out tox, out toy)) {
                // numeric <-> non-numeric
                return false;
            }

            bool isImplicit = TypeUtils.IsImplicitlyConvertible(fromx, fromy, tox, toy);

            if (implicitOnly && !isImplicit) {
                return false;
            }

            // IL conversion instruction also needed for floating point -> integer:
            if (!isImplicit || toy == 2 || tox == 2) {
                switch (tc) {
                    case TypeCode.SByte:
                        Emit(OpCodes.Conv_I1);
                        break;
                    case TypeCode.Int16:
                        Emit(OpCodes.Conv_I2);
                        break;
                    case TypeCode.Int32:
                        Emit(OpCodes.Conv_I4);
                        break;
                    case TypeCode.Int64:
                        Emit(OpCodes.Conv_I8);
                        break;
                    case TypeCode.Byte:
                        Emit(OpCodes.Conv_U1);
                        break;
                    case TypeCode.UInt16:
                        Emit(OpCodes.Conv_U1);
                        break;
                    case TypeCode.UInt32:
                        Emit(OpCodes.Conv_U2);
                        break;
                    case TypeCode.UInt64:
                        Emit(OpCodes.Conv_U4);
                        break;
                    case TypeCode.Single:
                        Emit(OpCodes.Conv_R4);
                        break;
                    case TypeCode.Double:
                        Emit(OpCodes.Conv_R8);
                        break;
                    default:
                        throw Assert.Unreachable;
                }
            }

            return true;
        }

        // TODO: we should try to remove this. It caused a 4x degrade in a
        // conversion intense lambda. And also seems like a bad idea to mess
        // with CLR boxing semantics.
        /// <summary>
        /// Boxes the value of the stack. No-op for reference types. Void is
        /// converted to a null reference. For almost all value types this
        /// method will box them in the standard way. Int32 and Boolean are
        /// handled with optimized conversions that reuse the same object for
        /// small values. For Int32 this is purely a performance optimization.
        /// For Boolean this is use to ensure that True and False are always
        /// the same objects.
        /// </summary>
        public void EmitBoxing(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            if (type.IsValueType) {
                if (type == typeof(void)) {
                    Emit(OpCodes.Ldnull);
                } else if (type == typeof(int)) {
                    EmitCall(typeof(ScriptingRuntimeHelpers), "Int32ToObject");
                } else if (type == typeof(bool)) {
                    var label = DefineLabel();
                    var end = DefineLabel();
                    Emit(OpCodes.Brtrue_S, label);
                    Emit(OpCodes.Ldsfld, typeof(ScriptingRuntimeHelpers).GetField("False"));
                    Emit(OpCodes.Br_S, end);
                    MarkLabel(label);
                    Emit(OpCodes.Ldsfld, typeof(ScriptingRuntimeHelpers).GetField("True"));
                    MarkLabel(end);
                } else {
                    Emit(OpCodes.Box, type);
                }
            } else if (type.IsGenericParameter) {
                EmitCall(typeof(GeneratorOps).GetMethod("BoxGeneric").MakeGenericMethod(type));
            }
        }

        #endregion

        #region Linq Conversions

        //CONFORMING
        // (plus support for None, Void conversions)
        internal void EmitConvertToType(Type typeFrom, Type typeTo, bool isChecked) {
            if (typeFrom == typeof(DynamicNull)) {
                typeFrom = typeof(object);
            }

            if (typeFrom == typeTo) {
                return;
            }

            // void -> non-void: default(T)
            if (typeFrom == typeof(void)) {
                EmitDefault(typeTo);
                return;
            }

            // non-void -> void: pop
            if (typeTo == typeof(void)) {
                Emit(OpCodes.Pop);
                return;
            }

            bool isTypeFromNullable = TypeUtils.IsNullableType(typeFrom);
            bool isTypeToNullable = TypeUtils.IsNullableType(typeTo);

            Type nnExprType = TypeUtils.GetNonNullableType(typeFrom);
            Type nnType = TypeUtils.GetNonNullableType(typeTo);

            if (typeFrom.IsInterface || // interface cast
               typeTo.IsInterface ||
               typeFrom == typeof(object) || // boxing cast
               typeTo == typeof(object)) {
                EmitCastToType(typeFrom, typeTo);
            } else if (isTypeFromNullable || isTypeToNullable) {
                EmitNullableConversion(typeFrom, typeTo, isChecked);
            } else if (!(TypeUtils.IsConvertible(typeFrom) && TypeUtils.IsConvertible(typeTo)) // primitive runtime conversion
                       &&
                       (nnExprType.IsAssignableFrom(nnType) || // down cast
                       nnType.IsAssignableFrom(nnExprType))) // up cast
            {
                EmitCastToType(typeFrom, typeTo);
            } else if (typeFrom.IsArray && typeTo.IsArray) {
                // See DevDiv Bugs #94657.
                EmitCastToType(typeFrom, typeTo);
            } else {
                EmitNumericConversion(typeFrom, typeTo, isChecked);
            }
        }

        //CONFORMING
        private void EmitCastToType(Type typeFrom, Type typeTo) {
            if (!typeFrom.IsValueType && typeTo.IsValueType) {
                Emit(OpCodes.Unbox_Any, typeTo);
            } else if (typeFrom.IsValueType && !typeTo.IsValueType) {
                EmitBoxing(typeFrom);
                if (typeTo != typeof(object)) {
                    Emit(OpCodes.Castclass, typeTo);
                }
            } else if (!typeFrom.IsValueType && !typeTo.IsValueType) {
                Emit(OpCodes.Castclass, typeTo);
            } else {
                throw Error.InvalidCast(typeFrom, typeTo);
            }
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void EmitNumericConversion(Type typeFrom, Type typeTo, bool isChecked) {
            bool isFromUnsigned = TypeUtils.IsUnsignedInt(typeFrom);
            bool isFromFloatingPoint = TypeUtils.IsFloatingPoint(typeFrom);
            if (typeTo == typeof(Single)) {
                if (isFromUnsigned)
                    Emit(OpCodes.Conv_R_Un);
                Emit(OpCodes.Conv_R4);
            } else if (typeTo == typeof(Double)) {
                if (isFromUnsigned)
                    Emit(OpCodes.Conv_R_Un);
                Emit(OpCodes.Conv_R8);
            } else {
                TypeCode tc = Type.GetTypeCode(typeTo);
                if (isChecked) {
                    if (isFromUnsigned) {
                        switch (tc) {
                            case TypeCode.SByte:
                                Emit(OpCodes.Conv_Ovf_I1_Un);
                                break;
                            case TypeCode.Int16:
                                Emit(OpCodes.Conv_Ovf_I2_Un);
                                break;
                            case TypeCode.Int32:
                                Emit(OpCodes.Conv_Ovf_I4_Un);
                                break;
                            case TypeCode.Int64:
                                Emit(OpCodes.Conv_Ovf_I8_Un);
                                break;
                            case TypeCode.Byte:
                                Emit(OpCodes.Conv_Ovf_U1_Un);
                                break;
                            case TypeCode.UInt16:
                            case TypeCode.Char:
                                Emit(OpCodes.Conv_Ovf_U2_Un);
                                break;
                            case TypeCode.UInt32:
                                Emit(OpCodes.Conv_Ovf_U4_Un);
                                break;
                            case TypeCode.UInt64:
                                Emit(OpCodes.Conv_Ovf_U8_Un);
                                break;
                            default:
                                throw Error.UnhandledConvert(typeTo);
                        }
                    } else {
                        switch (tc) {
                            case TypeCode.SByte:
                                Emit(OpCodes.Conv_Ovf_I1);
                                break;
                            case TypeCode.Int16:
                                Emit(OpCodes.Conv_Ovf_I2);
                                break;
                            case TypeCode.Int32:
                                Emit(OpCodes.Conv_Ovf_I4);
                                break;
                            case TypeCode.Int64:
                                Emit(OpCodes.Conv_Ovf_I8);
                                break;
                            case TypeCode.Byte:
                                Emit(OpCodes.Conv_Ovf_U1);
                                break;
                            case TypeCode.UInt16:
                            case TypeCode.Char:
                                Emit(OpCodes.Conv_Ovf_U2);
                                break;
                            case TypeCode.UInt32:
                                Emit(OpCodes.Conv_Ovf_U4);
                                break;
                            case TypeCode.UInt64:
                                Emit(OpCodes.Conv_Ovf_U8);
                                break;
                            default:
                                throw Error.UnhandledConvert(typeTo);
                        }
                    }
                } else {
                    if (isFromUnsigned) {
                        switch (tc) {
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                                Emit(OpCodes.Conv_U1);
                                break;
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Char:
                                Emit(OpCodes.Conv_U2);
                                break;
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                                Emit(OpCodes.Conv_U4);
                                break;
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                Emit(OpCodes.Conv_U8);
                                break;
                            default:
                                throw Error.UnhandledConvert(typeTo);
                        }
                    } else {
                        switch (tc) {
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                                Emit(OpCodes.Conv_I1);
                                break;
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Char:
                                Emit(OpCodes.Conv_I2);
                                break;
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                                Emit(OpCodes.Conv_I4);
                                break;
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                Emit(OpCodes.Conv_I8);
                                break;
                            default:
                                throw Error.UnhandledConvert(typeTo);
                        }
                    }
                }
            }
        }

        //CONFORMING
        private void EmitNullableToNullableConversion(Type typeFrom, Type typeTo, bool isChecked) {
            Debug.Assert(TypeUtils.IsNullableType(typeFrom));
            Debug.Assert(TypeUtils.IsNullableType(typeTo));
            Label labIfNull = default(Label);
            Label labEnd = default(Label);
            LocalBuilder locFrom = null;
            LocalBuilder locTo = null;
            locFrom = DeclareLocal(typeFrom);
            Emit(OpCodes.Stloc, locFrom);
            locTo = DeclareLocal(typeTo);
            // test for null
            Emit(OpCodes.Ldloca, locFrom);
            EmitHasValue(typeFrom);
            labIfNull = DefineLabel();
            Emit(OpCodes.Brfalse_S, labIfNull);
            Emit(OpCodes.Ldloca, locFrom);
            EmitGetValueOrDefault(typeFrom);
            Type nnTypeFrom = TypeUtils.GetNonNullableType(typeFrom);
            Type nnTypeTo = TypeUtils.GetNonNullableType(typeTo);
            EmitConvertToType(nnTypeFrom, nnTypeTo, isChecked);
            // construct result type
            ConstructorInfo ci = typeTo.GetConstructor(new Type[] { nnTypeTo });
            Emit(OpCodes.Newobj, ci);
            Emit(OpCodes.Stloc, locTo);
            labEnd = DefineLabel();
            Emit(OpCodes.Br_S, labEnd);
            // if null then create a default one
            MarkLabel(labIfNull);
            Emit(OpCodes.Ldloca, locTo);
            Emit(OpCodes.Initobj, typeTo);
            MarkLabel(labEnd);
            Emit(OpCodes.Ldloc, locTo);
        }

        //CONFORMING
        private void EmitNonNullableToNullableConversion(Type typeFrom, Type typeTo, bool isChecked) {
            Debug.Assert(!TypeUtils.IsNullableType(typeFrom));
            Debug.Assert(TypeUtils.IsNullableType(typeTo));
            LocalBuilder locTo = null;
            locTo = DeclareLocal(typeTo);
            Type nnTypeTo = TypeUtils.GetNonNullableType(typeTo);
            EmitConvertToType(typeFrom, nnTypeTo, isChecked);
            ConstructorInfo ci = typeTo.GetConstructor(new Type[] { nnTypeTo });
            Emit(OpCodes.Newobj, ci);
            Emit(OpCodes.Stloc, locTo);
            Emit(OpCodes.Ldloc, locTo);
        }

        //CONFORMING
        private void EmitNullableToNonNullableConversion(Type typeFrom, Type typeTo, bool isChecked) {
            Debug.Assert(TypeUtils.IsNullableType(typeFrom));
            Debug.Assert(!TypeUtils.IsNullableType(typeTo));
            if (typeTo.IsValueType)
                EmitNullableToNonNullableStructConversion(typeFrom, typeTo, isChecked);
            else
                EmitNullableToReferenceConversion(typeFrom);
        }

        //CONFORMING
        private void EmitNullableToNonNullableStructConversion(Type typeFrom, Type typeTo, bool isChecked) {
            Debug.Assert(TypeUtils.IsNullableType(typeFrom));
            Debug.Assert(!TypeUtils.IsNullableType(typeTo));
            Debug.Assert(typeTo.IsValueType);
            LocalBuilder locFrom = null;
            locFrom = DeclareLocal(typeFrom);
            Emit(OpCodes.Stloc, locFrom);
            Emit(OpCodes.Ldloca, locFrom);
            EmitGetValue(typeFrom);
            Type nnTypeFrom = TypeUtils.GetNonNullableType(typeFrom);
            EmitConvertToType(nnTypeFrom, typeTo, isChecked);
        }

        //CONFORMING
        private void EmitNullableToReferenceConversion(Type typeFrom) {
            Debug.Assert(TypeUtils.IsNullableType(typeFrom));
            // We've got a conversion from nullable to Object, ValueType, Enum, etc.  Just box it so that
            // we get the nullable semantics.  
            Emit(OpCodes.Box, typeFrom);
        }

        //CONFORMING
        private void EmitNullableConversion(Type typeFrom, Type typeTo, bool isChecked) {
            bool isTypeFromNullable = TypeUtils.IsNullableType(typeFrom);
            bool isTypeToNullable = TypeUtils.IsNullableType(typeTo);
            Debug.Assert(isTypeFromNullable || isTypeToNullable);
            if (isTypeFromNullable && isTypeToNullable)
                EmitNullableToNullableConversion(typeFrom, typeTo, isChecked);
            else if (isTypeFromNullable)
                EmitNullableToNonNullableConversion(typeFrom, typeTo, isChecked);
            else
                EmitNonNullableToNullableConversion(typeFrom, typeTo, isChecked);
        }

        //CONFORMING
        internal void EmitHasValue(Type nullableType) {
            MethodInfo mi = nullableType.GetMethod("get_HasValue", BindingFlags.Instance | BindingFlags.Public);
            Debug.Assert(nullableType.IsValueType);
            Emit(OpCodes.Call, mi);
        }

        //CONFORMING
        internal void EmitGetValue(Type nullableType) {
            MethodInfo mi = nullableType.GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);
            Debug.Assert(nullableType.IsValueType);
            Emit(OpCodes.Call, mi);
        }

        //CONFORMING
        internal void EmitGetValueOrDefault(Type nullableType) {
            MethodInfo mi = nullableType.GetMethod("GetValueOrDefault", System.Type.EmptyTypes);
            Debug.Assert(nullableType.IsValueType);
            Emit(OpCodes.Call, mi);
        }

        #endregion

        #region Arrays

        /// <summary>
        /// Emits an array of constant values provided in the given list.
        /// The array is strongly typed.
        /// </summary>
        public void EmitArray<T>(IList<T> items) {
            ContractUtils.RequiresNotNull(items, "items");

            EmitInt(items.Count);
            Emit(OpCodes.Newarr, typeof(T));
            for (int i = 0; i < items.Count; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);
                EmitSimpleConstant(items[i]);
                EmitStoreElement(typeof(T));
            }
        }

        /// <summary>
        /// Emits an array of values of count size.  The items are emitted via the callback
        /// which is provided with the current item index to emit.
        /// </summary>
        public void EmitArray(Type elementType, int count, EmitArrayHelper emit) {
            ContractUtils.RequiresNotNull(elementType, "elementType");
            ContractUtils.RequiresNotNull(emit, "emit");
            ContractUtils.Requires(count >= 0, "count", Strings.CountCannotBeNegative);

            EmitInt(count);
            Emit(OpCodes.Newarr, elementType);
            for (int i = 0; i < count; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);

                emit(i);

                EmitStoreElement(elementType);
            }
        }

        /// <summary>
        /// Emits an array construction code.  
        /// The code assumes that bounds for all dimensions
        /// are already emitted.
        /// </summary>
        public void EmitArray(Type arrayType) {
            ContractUtils.RequiresNotNull(arrayType, "arrayType");
            ContractUtils.Requires(arrayType.IsArray, "arrayType", Strings.ArrayTypeMustBeArray);

            int rank = arrayType.GetArrayRank();
            if (rank == 1) {
                Emit(OpCodes.Newarr, arrayType.GetElementType());
            } else {
                Type[] types = new Type[rank];
                for (int i = 0; i < rank; i++) {
                    types[i] = typeof(int);
                }
                EmitNew(arrayType, types);
            }
        }

        #endregion

        #region Support for emitting constants

        public void EmitDecimal(decimal value) {
            if (Decimal.Truncate(value) == value) {
                if (Int32.MinValue <= value && value <= Int32.MaxValue) {
                    int intValue = Decimal.ToInt32(value);
                    EmitInt(intValue);
                    EmitNew(typeof(Decimal).GetConstructor(new Type[] { typeof(int) }));
                } else if (Int64.MinValue <= value && value <= Int64.MaxValue) {
                    long longValue = Decimal.ToInt64(value);
                    EmitLong(longValue);
                    EmitNew(typeof(Decimal).GetConstructor(new Type[] { typeof(long) }));
                } else {
                    EmitDecimalBits(value);
                }
            } else {
                EmitDecimalBits(value);
            }
        }

        private void EmitDecimalBits(decimal value) {
            int[] bits = Decimal.GetBits(value);
            EmitInt(bits[0]);
            EmitInt(bits[1]);
            EmitInt(bits[2]);
            EmitBoolean((bits[3] & 0x80000000) != 0);
            EmitByte((byte)(bits[3] >> 16));
            EmitNew(typeof(decimal).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte) }));
        }

        /// <summary>
        /// Emits default(T)
        /// Semantics match C# compiler behavior
        /// </summary>
        internal void EmitDefault(Type type) {
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Object:
                case TypeCode.DateTime:
                    if (type.IsValueType) {
                        // Type.GetTypeCode on an enum returns the underlying
                        // integer TypeCode, so we won't get here.
                        Debug.Assert(!type.IsEnum);

                        // This is the IL for default(T) if T is a generic type
                        // parameter, so it should work for any type. It's also
                        // the standard pattern for structs.
                        LocalBuilder lb = GetLocal(type);
                        Emit(OpCodes.Ldloca, lb);
                        Emit(OpCodes.Initobj, type);
                        Emit(OpCodes.Ldloc, lb);
                        FreeLocal(lb);
                    } else {
                        Emit(OpCodes.Ldnull);
                    }
                    break;

                case TypeCode.Empty:
                case TypeCode.String:
                case TypeCode.DBNull:
                    Emit(OpCodes.Ldnull);
                    break;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    Emit(OpCodes.Ldc_I4_0);
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    Emit(OpCodes.Ldc_I4_0);
                    Emit(OpCodes.Conv_I8);
                    break;

                case TypeCode.Single:
                    Emit(OpCodes.Ldc_R4, default(Single));
                    break;

                case TypeCode.Double:
                    Emit(OpCodes.Ldc_R8, default(Double));
                    break;

                case TypeCode.Decimal:
                    Emit(OpCodes.Ldc_I4_0);
                    Emit(OpCodes.Newobj, typeof(Decimal).GetConstructor(new Type[] { typeof(int) }));
                    break;

                default:
                    throw Assert.Unreachable;
            }
        }

        public void EmitMissingValue(Type type) {
            LocalBuilder lb;

            switch (Type.GetTypeCode(type)) {
                default:
                case TypeCode.Object:
                    if (type == typeof(object)) {
                        // parameter of type object receives the actual Missing value
                        Emit(OpCodes.Ldsfld, typeof(Missing).GetField("Value"));
                    } else if (!type.IsValueType) {
                        // reference type
                        EmitNull();
                    } else if (type.IsSealed && !type.IsEnum) {
                        lb = DeclareLocal(type);
                        Emit(OpCodes.Ldloca, lb);
                        Emit(OpCodes.Initobj, type);
                        Emit(OpCodes.Ldloc, lb);
                    } else {
                        throw Error.NoDefaultValue();
                    }
                    break;

                case TypeCode.Empty:
                case TypeCode.DBNull:
                    EmitNull();
                    break;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    EmitInt(0);
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    EmitLong(0);
                    break;

                case TypeCode.Single:
                    EmitSingle(default(Single));
                    break;

                case TypeCode.Double:
                    Emit(OpCodes.Ldc_R8, default(Double));
                    break;

                case TypeCode.Decimal:
                    EmitFieldGet(typeof(Decimal).GetField("Zero"));
                    break;

                case TypeCode.DateTime:
                    lb = DeclareLocal(typeof(DateTime));
                    Emit(OpCodes.Ldloca, lb);
                    Emit(OpCodes.Initobj, typeof(DateTime));
                    Emit(OpCodes.Ldloc, lb);
                    break;

                case TypeCode.String:
                    EmitNull();
                    break;
            }
        }

        #endregion

        #region LocalTemps

        internal LocalBuilder GetLocal(Type type) {
            Debug.Assert(type != null);

            LocalBuilder local;
            if (_freeLocals.TryDequeue(type, out local)) {
                Debug.Assert(type == local.LocalType);
                return local;
            }

            return DeclareLocal(type);
        }

        // TODO: make "local" a ref param and null it out
        internal void FreeLocal(LocalBuilder local) {
            if (local != null) {
                _freeLocals.Enqueue(local.LocalType, local);
            }
        }

        #endregion
    }

    public static partial class GeneratorOps {
        public static object BoxGeneric<T>(T value) {
            // Duplicates functionality of ILGen.EmitBoxing at runtime for templated types
            Type type = typeof(T);
            if (type == typeof(int)) {
                return ScriptingRuntimeHelpers.Int32ToObject((int)(object)value);
            } else if (type == typeof(bool)) {
                bool bValue = (bool)(object)value;
                return bValue ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
            } else {
                return (object)value;
            }
        }
    }
}
