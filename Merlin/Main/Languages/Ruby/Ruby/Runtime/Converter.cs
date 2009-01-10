/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using RuntimeHelpers = Microsoft.Scripting.Runtime.ScriptingRuntimeHelpers;

namespace IronRuby.Runtime {

    public static partial class Converter {
        #region Conversion entry points

        //
        // ConvertToInt32 - fast paths and custom logic
        //
        public static Int32 ConvertToInt32(object value) {
            // Fast Paths
            Extensible<int> ei;
            BigInteger bi;
            if (value is Int32) return (Int32)value;
            if ((ei = value as Extensible<int>) != null) return ei.Value;
            if (value is Boolean) return ((Boolean)value) ? 1 : 0;
            if ((Object)(bi = value as BigInteger) != null) return bi.ToInt32();

            // Fall back to comprehensive conversion
            Int32 result;
            if (ConvertToInt32Impl(value, out result)) return result;

            if (value is double) return checked((int)((double)value));

            // Fall back to __xxx__ method call
            object newValue;
            //if(PythonOps.TryInvokeOperator(DefaultContext.Default,
            //                Operators.ConvertToInt32,
            //                value,
            //                out newValue)) {            
            //    // Convert resulting object to the desired type
            //    if (ConvertToInt32Impl(newValue, out result)) return result;
            //}

            if (TryConvertObject(value, typeof(Int32), out newValue) && newValue is Int32) return (Int32)newValue;

            throw CannotConvertTo("Int32", value);
        }

        //
        // ConvertToDouble - fast paths and custom logic
        //
        public static Double ConvertToDouble(object value) {
            // Fast Paths
            Extensible<int> ei;
            Extensible<double> ef;
            if (value is Double) return (Double)value;
            if (value is Int32) return (Double)(Int32)value;
            if ((ef = value as Extensible<double>) != null) return ef.Value;
            if ((ei = value as Extensible<int>) != null) return ei.Value;

            // Fall back to comprehensive conversion
            Double result;
            if (ConvertToDoubleImpl(value, out result)) return result;

            // Fall back to __xxx__ method call
            object newValue;
            //if(PythonOps.TryInvokeOperator(DefaultContext.Default,
            //    Operators.ConvertToDouble,
            //    value,
            //    out newValue)) {

            //    // Convert resulting object to the desired type
            //    if (ConvertToDoubleImpl(newValue, out result)) return result;
            //}

            if (TryConvertObject(value, typeof(Double), out newValue) && newValue is Double) return (Double)newValue;

            throw CannotConvertTo("Double", value);
        }

        //
        // ConvertToBigInteger - fast paths and custom logic
        //
        public static BigInteger ConvertToBigInteger(object value) {
            // Fast Paths
            BigInteger bi;
            Extensible<BigInteger> el;

            if ((Object)(bi = value as BigInteger) != null) return bi;
            if (value is Int32) return BigInteger.Create((Int32)value);
            if ((el = value as Extensible<BigInteger>) != null) return el.Value;
            if (value == null) return null;

            // Fall back to comprehensive conversion
            BigInteger result;
            if (ConvertToBigIntegerImpl(value, out result)) return result;

            // Fall back to __xxx__ method call
            object newValue;
            //if(PythonOps.TryInvokeOperator(DefaultContext.Default,
            //    Operators.ConvertToBigInteger,
            //    value,
            //    out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToBigIntegerImpl(newValue, out result)) return result;
            //}

            if (TryConvertObject(value, typeof(BigInteger), out newValue) && newValue is BigInteger) return (BigInteger)newValue;

            throw CannotConvertTo("BigInteger", value);
        }

        //
        // ConvertToComplex64 - fast paths and custom logic
        //
        public static Complex64 ConvertToComplex64(object value) {
            // Fast Paths
            if (value is Complex64) return (Complex64)value;
            if (value is Double) return Complex64.MakeReal((Double)value);

            // Fall back to comprehensive conversion
            Complex64 result;
            if (ConvertToComplex64Impl(value, out result)) return result;

            // Fall back to __xxx__ method call
            object newValue;
            //if (PythonOps.TryInvokeOperator(DefaultContext.Default,
            //    Operators.ConvertToComplex,
            //    value,
            //    out newValue)) {               // Convert resulting object to the desired type
            //    if (ConvertToComplex64Impl(newValue, out result)) return result;
            //}

            // Try converting to double and use it as a real part of the complex number
            Double dresult;
            if (ConvertToDoubleImpl(value, out dresult)) return new Complex64(dresult);

            // Fall back to __xxx__ method call
            //if (PythonOps.TryInvokeOperator(DefaultContext.Default,
            //    Operators.ConvertToDouble,
            //    value,
            //    out newValue)) {        
        
            //    if (newValue is double) {
            //        dresult = (double)newValue;
            //    } else if (newValue is Extensible<double>) {
            //        dresult = ((Extensible<double>)newValue).Value;
            //    } else {
            //        throw RubyExceptions.CreateTypeError("__float__ returned non-float");
            //    }
            //    return new Complex64(dresult);
            //}

            if (TryConvertObject(value, typeof(Complex64), out newValue) && newValue is Complex64) return (Complex64)newValue;

            throw CannotConvertTo("Complex64", value);
        }

        //
        // ConvertToString - fast paths and custom logic
        //
        public static String ConvertToString(object value) {
            // Fast Paths
            Object res;
            String result;

            if ((result = value as String) != null) return result;
            if (value == null) return null;
            if (value is Char) return RuntimeHelpers.CharToString((Char)value);
            if (TryConvertObject(value, typeof(String), out res) && res is String) return (String)res;

            throw CannotConvertTo("String", value);
        }

        //
        // ConvertToChar - fast paths and custom logic
        //
        public static Char ConvertToChar(object value) {
            // Fast Paths
            Object res;
            string str;

            if (value is Char) return (Char)value;
            if ((object)(str = value as string) != null && str.Length == 1) return str[0];
            if (TryConvertObject(value, typeof(Char), out res) && res is Char) return (Char)res;

            throw CannotConvertTo("Char", value);
        }

        //
        // ConvertToBoolean - fast paths and custom logic
        //
        public static Boolean ConvertToBoolean(object value) {
            // Fast Paths
            if (value is Int32) return (Int32)value != 0;
            if (value is Boolean) return (Boolean)value;
            if (value == null) return false;

            return SlowConvertToBoolean(value);
        }

        private static bool SlowConvertToBoolean(object value) {
            Boolean result;

            // Fall back to comprehensive conversion
            if (ConvertToBooleanImpl(value, out result)) return result;

            // Additional logic to convert to bool
            if (value == null) return false;
            if (value is ICollection) return ((ICollection)value).Count != 0;

            // Explictly block conversion of References to bool
            if (value is IStrongBox) {
                throw RuntimeHelpers.SimpleTypeError("Can't convert a Reference<> instance to a bool");
            }

            // Fall back to __xxx__ method call
            object newValue;

            // First, try __nonzero__
            //if(PythonOps.TryInvokeOperator(DefaultContext.Default,
            //    Operators.ConvertToBoolean,
            //    value,
            //    out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (newValue is bool || newValue is Int32) {
            //        if (ConvertToBooleanImpl(newValue, out result)) return result;
            //    }
            //    throw RubyExceptions.CreateTypeError("__nonzero__ should return bool or int, returned {0}", PythonOps.GetClassName(newValue));
            //}

            // Then, try __len__
            //try {
            //        if(PythonOps.TryInvokeOperator(DefaultContext.Default,
            //                        Operators.Length,
            //                        value,
            //                        out newValue)) {
            //        // Convert resulting object to the desired type
            //        if (newValue is Int32 || newValue is BigInteger) {
            //            if (ConvertToBooleanImpl(newValue, out result)) return result;
            //        }
            //        throw RubyExceptions.CreateTypeError("an integer is required");
            //    }
            //} catch (MissingMemberException) {
            //    // old-style __len__ throws if we don't have __len__ defined on the instance
            //}

            // Try Extensible types as last due to possible __nonzero__ overload
            if (value is Extensible<int>) return (Int32)((Extensible<int>)value).Value != (Int32)0;
            if (value is Extensible<BigInteger>) return ((Extensible<BigInteger>)value).Value != BigInteger.Zero;
            if (value is Extensible<double>) return ((Extensible<double>)value).Value != (Double)0;

            if (TryConvertObject(value, typeof(bool), out newValue) && newValue is Boolean) return (bool)newValue;

            // Non-null value is true
            result = true;
            return true;
        }

        #endregion

        public static Char ExplicitConvertToChar(object value) {
            string str;
            if (value is Char) return (Char)value;
            if (value is Int32) return checked((Char)(Int32)value);
            if ((Object)(str = value as string) != null && str.Length == 1) return str[0];
            if (value is SByte) return checked((Char)(SByte)value);
            if (value is Int16) return checked((Char)(Int16)value);
            if (value is UInt32) return checked((Char)(UInt32)value);
            if (value is UInt64) return checked((Char)(UInt64)value);
            if (value is Decimal) return checked((Char)(Decimal)value);
            if (value is Int64) return checked((Char)(Int64)value);
            if (value is Byte) return (Char)(Byte)value;
            if (value is UInt16) return checked((Char)(UInt16)value);

            throw CannotConvertTo("char", value);
        }

        public static T Convert<T>(object value) {
            return (T)Convert(value, typeof(T));
        }

        /// <summary>
        /// General conversion routine TryConvert - tries to convert the object to the desired type.
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvert(object value, Type to, out object result) {
            try {
                result = Convert(value, to);
                return true;
            } catch {
                result = default(object);
                return false;
            }
        }

        internal static object Convert(object value, Type to) {
            if (value == null) {
                if (to == typeof(bool)) return RuntimeHelpers.False;

                if (to.IsValueType &&                    
                    (!to.IsGenericType || to.GetGenericTypeDefinition() != typeof(Nullable<>))) {

                    throw MakeTypeError(to, value);
                }
                return null;
            }

            Type from = value.GetType();
            if (from == to || to == ObjectType) return value;
            if (to.IsInstanceOfType(value)) return value;

            if (to == TypeType) return ConvertToType(value);
            if (to == Int32Type) return ConvertToInt32(value);
            if (to == DoubleType) return ConvertToDouble(value);
            if (to == BooleanType) return ConvertToBoolean(value);

            if (to == CharType) return ConvertToChar(value);
            if (to == StringType) return ConvertToString(value);

            if (to == BigIntegerType) return ConvertToBigInteger(value);
            if (to == Complex64Type) return ConvertToComplex64(value);

            if (to == ByteType) return ConvertToByte(value);
            if (to == SByteType) return ConvertToSByte(value);
            if (to == Int16Type) return ConvertToInt16(value);
            if (to == UInt32Type) return ConvertToUInt32(value);
            if (to == UInt64Type) return ConvertToUInt64(value);
            if (to == UInt16Type) return ConvertToUInt16(value);
            if (to == SingleType) return ConvertToSingle(value);
            if (to == Int64Type) return ConvertToInt64(value);
            if (to == DecimalType) return ConvertToDecimal(value);

            if (to == IEnumerableType) return ConvertToIEnumerable(value);

            if (DelegateType.IsAssignableFrom(to)) return ConvertToDelegate(value, to);

            if (to.IsArray) return ConvertToArray(value, to);

            Object result;
            if (TrySlowConvert(value, to, out result)) return result;

            throw MakeTypeError(to, value);
        }

        internal static bool TrySlowConvert(object value, Type to, out object result) {
            // check for implicit conversions 
            if(CompilerHelpers.TryImplicitConversion(value, to, out result)) {
                return true;
            }

            if (to.IsGenericType) {
                Type genTo = to.GetGenericTypeDefinition();
                if (genTo == NullableOfTType) {
                    result = ConvertToNullableT(value, to.GetGenericArguments());
                    return true;
                }

                if (genTo == IListOfTType) {
                    result = ConvertToIListT(value, to.GetGenericArguments());
                    return true;
                }

                if (genTo == IDictOfTType) {
                    result = ConvertToIDictT(value, to.GetGenericArguments());
                    return true;
                }

                if (genTo == IEnumerableOfTType) {
                    result = ConvertToIEnumerableT(value, to.GetGenericArguments());
                    return true;
                }
            }

            if (value.GetType().IsValueType) {
                if (to == ValueTypeType) {
                    result = (System.ValueType)value;
                    return true;
                }
            }

#if !SILVERLIGHT
            if (value != null) {
                // try available type conversions...
                object[] tcas = to.GetCustomAttributes(typeof(TypeConverterAttribute), true);
                foreach (TypeConverterAttribute tca in tcas) {
                    TypeConverter tc = GetTypeConverter(tca);

                    if (tc == null) continue;

                    if (tc.CanConvertFrom(value.GetType())) {
                        result = tc.ConvertFrom(value);
                        return true;
                    }
                }
            }
#endif

            result = null;
            return false;
        }

        internal static bool TryConvertObject(object value, Type to, out object result) {
            //This is the fallback call for every fast path converter. If we land here,
            //then 'value' has to be a reference type which might have a custom converter
            //defined on its dynamic type. (Value Type conversions if any would have 
            //already taken place during the fast conversions and should not occur through
            //the dynamic types). 
            
            if (value == null || value.GetType().IsValueType) {
                result = null;
                return false;
            }

            return TrySlowConvert(value, to, out result);
        }

        // TODO: Make internal once JS has its own converter
        /// <summary>
        /// This function tries to convert an object to IEnumerator, or wraps it into an adapter
        /// Do not use this function directly. It is only meant to be used by Ops.GetEnumerator.
        /// </summary>
        public static bool TryConvertToIEnumerator(object o, out IEnumerator e) {
            //if (o is string) {
            //    e = StringOps.GetEnumerator((string)o);
            //    return true;
            //} else 
            if (o is IEnumerable) {
                e = ((IEnumerable)o).GetEnumerator();
                return true;
            } else if (o is IEnumerator) {
                e = (IEnumerator)o;
                return true;
            }

            //if (PythonEnumerator.TryCreate(o, out e)) {
            //    return true;
            //}
            //if (ItemEnumerator.TryCreate(o, out e)) {
            //    return true;
            //}
            e = null;
            return false;
        }

        public static IEnumerable ConvertToIEnumerable(object o) {
            if (o == null) return null;

            IEnumerable e = o as IEnumerable;
            if (e != null) return e;

            //PythonEnumerable pe;
            //if (PythonEnumerable.TryCreate(o, out pe)) {
            //    return pe;
            //}

            //ItemEnumerable ie;
            //if (ItemEnumerable.TryCreate(o, out ie)) {
            //    return ie;
            //}

            throw MakeTypeError("IEnumerable", o);
        }

        public static object ConvertToIEnumerableT(object value, Type[] enumOf) {
            //Type type = IEnumerableOfTType.MakeGenericType(enumOf);
            //if (type.IsInstanceOfType(value)) {
            //    return value;
            //}

            //IEnumerable ie = value as IEnumerable;
            //if (ie == null) {
            //    ie = ConvertToIEnumerable(value);
            //}

            ////type = IEnumerableOfTWrapperType.MakeGenericType(enumOf);
            //object res = Activator.CreateInstance(type, ie);
            //return res;
            throw new NotSupportedException();
        }

        private static object ConvertToArray(object value, Type to) {
            int rank = to.GetArrayRank();

            //if (rank == 1) {
            //    Tuple tupleVal = value as Tuple;
            //    if (tupleVal != null) {
            //        Type elemType = to.GetElementType();
            //        Array ret = Array.CreateInstance(elemType, tupleVal.Count);
            //        try {
            //            tupleVal.CopyTo(ret, 0);
            //            return ret;
            //        } catch (InvalidCastException) {
            //            // invalid conversion
            //            for (int i = 0; i < tupleVal.Count; i++) {
            //                ret.SetValue(Convert(tupleVal[i], elemType), i);
            //            }
            //            return ret;
            //        }
            //    }
            //}

            throw MakeTypeError("Array", value);
        }

        internal static int ConvertToSliceIndex(object value) {
            int val;
            if (TryConvertToInt32(value, out val))
                return val;

            BigInteger bigval;
            if (TryConvertToBigInteger(value, out bigval)) {
                return bigval > 0 ? Int32.MaxValue : Int32.MinValue;
            }

            throw RubyExceptions.CreateTypeError("slice indices must be integers");
        }

        internal static Exception CannotConvertTo(string name, object value) {
            return RubyExceptions.CreateTypeError(String.Format("Cannot convert {0}({1}) to {2}", CompilerHelpers.GetType(value).Name, value, name));
        }

        private static Exception MakeTypeError(Type expectedType, object o) {
            return MakeTypeError(expectedType.Name.ToString(), o);
        }

        private static Exception MakeTypeError(string expectedType, object o) {
            return RubyExceptions.CreateTypeError(String.Format("Object '{1}' of type '{2}' cannot be converted to type '{0}'", expectedType, 
                o ?? "nil", o != null ? o.GetType().Name : "NilClass"));
        }

        #region Cached Type instances

        private static readonly Type Int16Type = typeof(System.Int16);
        private static readonly Type SByteType = typeof(System.SByte);
        private static readonly Type StringType = typeof(System.String);
        private static readonly Type UInt64Type = typeof(System.UInt64);
        private static readonly Type Int32Type = typeof(System.Int32);
        private static readonly Type DoubleType = typeof(System.Double);
        private static readonly Type DecimalType = typeof(System.Decimal);
        private static readonly Type ObjectType = typeof(System.Object);
        private static readonly Type Int64Type = typeof(System.Int64);
        private static readonly Type CharType = typeof(System.Char);
        private static readonly Type SingleType = typeof(System.Single);
        private static readonly Type BooleanType = typeof(System.Boolean);
        private static readonly Type UInt16Type = typeof(System.UInt16);
        private static readonly Type UInt32Type = typeof(System.UInt32);
        private static readonly Type ByteType = typeof(System.Byte);
        private static readonly Type BigIntegerType = typeof(BigInteger);
        private static readonly Type Complex64Type = typeof(Complex64);
        private static readonly Type DelegateType = typeof(Delegate);
        private static readonly Type IEnumerableType = typeof(IEnumerable);
        private static readonly Type ValueTypeType = typeof(ValueType);
        private static readonly Type TypeType = typeof(Type);
        private static readonly Type NullableOfTType = typeof(Nullable<>);
        private static readonly Type IListOfTType = typeof(System.Collections.Generic.IList<>);
        private static readonly Type IDictOfTType = typeof(System.Collections.Generic.IDictionary<,>);
        private static readonly Type IEnumerableOfTType = typeof(System.Collections.Generic.IEnumerable<>);
        private static readonly Type IListOfObjectType = typeof(System.Collections.Generic.IList<object>);
        private static readonly Type IDictionaryOfObjectType = typeof(System.Collections.Generic.IDictionary<object, object>);

        #endregion

        #region Implementation routines
        //
        //  ConvertToBooleanImpl Conversion Routine
        //
        private static bool ConvertToBooleanImpl(object value, out Boolean result) {
            if (value is Boolean) {
                result = (Boolean)value;
                return true;
            } else if (value is Int32) {
                result = (Int32)value != (Int32)0;
                return true;
            } else if (value is Double) {
                result = (Double)value != (Double)0;
                return true;
            } else if (value is BigInteger) {
                result = ((BigInteger)value) != BigInteger.Zero;
                return true;
            } else if (value is String) {
                result = ((String)value).Length != 0;
                return true;
            } else if (value is Complex64) {
                result = !((Complex64)value).IsZero;
                return true;
            } else if (value is Int64) {
                result = (Int64)value != (Int64)0;
                return true;
            } else if (value is Byte) {
                result = (Byte)value != (Byte)0;
                return true;
            } else if (value is SByte) {
                result = (SByte)value != (SByte)0;
                return true;
            } else if (value is Int16) {
                result = (Int16)value != (Int16)0;
                return true;
            } else if (value is UInt16) {
                result = (UInt16)value != (UInt16)0;
                return true;
            } else if (value is UInt32) {
                result = (UInt32)value != (UInt32)0;
                return true;
            } else if (value is UInt64) {
                result = (UInt64)value != (UInt64)0;
                return true;
            } else if (value is Single) {
                result = (Single)value != (Single)0;
                return true;
            } else if (value is Decimal) {
                result = (Decimal)value != (Decimal)0;
                return true;
            } else if (value is Enum) {
                return TryConvertEnumToBoolean(value, out result);
            }

            result = default(Boolean);
            return false;
        }

        private static bool TryConvertEnumToBoolean(object value, out bool result) {
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    result = (int)value != 0; return true;
                case TypeCode.Int64:
                    result = (long)value != 0; return true;
                case TypeCode.Int16:
                    result = (short)value != 0; return true;
                case TypeCode.UInt32:
                    result = (uint)value != 0; return true;
                case TypeCode.UInt64:
                    result = (ulong)value != 0; return true;
                case TypeCode.SByte:
                    result = (sbyte)value != 0; return true;
                case TypeCode.UInt16:
                    result = (ushort)value != 0; return true;
                case TypeCode.Byte:
                    result = (byte)value != 0; return true;
                default:
                    result = default(Boolean); return false;
            }
        }

        //
        // ConvertToComplex64Impl Conversion Routine
        //
        private static bool ConvertToComplex64Impl(object value, out Complex64 result) {
            if (value is Complex64) {
                result = (Complex64)value;
                return true;
            } else if (value is Double) {
                result = Complex64.MakeReal((Double)value);
                return true;
            } else if (value is Extensible<Complex64>) {
                result = ((Extensible<Complex64>)value).Value;
                return true;
            } else {
                Double DoubleValue;
                if (ConvertToDoubleImpl(value, out DoubleValue)) {
                    result = Complex64.MakeReal(DoubleValue);
                    return true;
                }
            }
            result = default(Complex64);
            return false;
        }

        #endregion

        private static object ConvertToNullableT(object value, Type[] typeOf) {
            if (value == null) return null;
            else return Convert(value, typeOf[0]);
        }

        #region Entry points called from the generated code

        public static object ConvertToReferenceType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) return null;
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static object ConvertToNullableType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) return null;
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static object ConvertToValueType(object fromObject, RuntimeTypeHandle typeHandle) {
            ContractUtils.RequiresNotNull(fromObject, "fromObject");
            //if (fromObject == null) throw PythonOps.InvalidType(fromObject, typeHandle);
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static Type ConvertToType(object value) {
            if (value == null) return null;

            Type TypeVal = value as Type;
            if (TypeVal != null) return TypeVal;

            TypeTracker typeTracker = value as TypeTracker;
            if (typeTracker != null) return typeTracker.Type;

            throw MakeTypeError("Type", value);
        }

        public static object ConvertToDelegate(object value, Type to) {
            if (value == null) return null;
            return BinderOps.GetDelegate(RubyContext._Default, value, to);
        }


        #endregion

        private static object ConvertToIListT(object value, Type[] listOf) {
            System.Collections.Generic.IList<object> lst = value as System.Collections.Generic.IList<object>;
            if (lst != null) {
                //Type t = ListGenericWrapperType.MakeGenericType(listOf);
                //return Activator.CreateInstance(t, lst);
            }
            throw MakeTypeError("IList<T>", value);
        }

        private static object ConvertToIDictT(object value, Type[] dictOf) {
            System.Collections.Generic.IDictionary<object, object> dict = value as System.Collections.Generic.IDictionary<object, object>;
            if (dict != null) {
                //Type t = DictionaryGenericWrapperType.MakeGenericType(dictOf);
                //return Activator.CreateInstance(t, dict);
            }
            throw MakeTypeError("IDictionary<K,V>", value);
        }

        public static bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel allowNarrowing) {
            ContractUtils.RequiresNotNull(fromType, "fromType");
            ContractUtils.RequiresNotNull(toType, "toType");

            if (toType == fromType) return true;
            if (toType.IsAssignableFrom(fromType)) return true;
            if (fromType.IsCOMObject && toType.IsInterface) return true; // A COM object could be cast to any interface

            if (HasImplicitNumericConversion(fromType, toType)) return true;

            // Handling the hole that Type is the only object that we 'box'
            if (toType == TypeType && typeof(TypeTracker).IsAssignableFrom(fromType)) return true;

            // Support extensible types with simple implicit conversions to their base types
            if (typeof(Extensible<int>).IsAssignableFrom(fromType) && CanConvertFrom(Int32Type, toType, allowNarrowing)) {
                return true;
            }
            if (typeof(Extensible<BigInteger>).IsAssignableFrom(fromType) && CanConvertFrom(BigIntegerType, toType, allowNarrowing)) {
                return true;
            }
            //if (typeof(ExtensibleString).IsAssignableFrom(fromType) && CanConvertFrom(StringType, toType, allowNarrowing)) {
            //    return true;
            //}
            if (typeof(Extensible<double>).IsAssignableFrom(fromType) && CanConvertFrom(DoubleType, toType, allowNarrowing)) {
                return true;
            }
            if (typeof(Extensible<Complex64>).IsAssignableFrom(fromType) && CanConvertFrom(Complex64Type, toType, allowNarrowing)) {
                return true;
            }

            if (typeof(MutableString).IsAssignableFrom(fromType) && toType == typeof(string)) {
                return true;
            }


#if !SILVERLIGHT
            // try available type conversions...
            object[] tcas = toType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
            foreach (TypeConverterAttribute tca in tcas) {
                TypeConverter tc = GetTypeConverter(tca);

                if (tc == null) continue;

                if (tc.CanConvertFrom(fromType)) {
                    return true;
                }
            }
#endif

            //!!!do user-defined implicit conversions here

            if (allowNarrowing == NarrowingLevel.None) return false;

            return HasNarrowingConversion(fromType, toType, allowNarrowing);
        }

#if !SILVERLIGHT
        private static TypeConverter GetTypeConverter(TypeConverterAttribute tca) {
            try {
                ConstructorInfo ci = Type.GetType(tca.ConverterTypeName).GetConstructor(Type.EmptyTypes);
                if (ci != null) return ci.Invoke(ArrayUtils.EmptyObjects) as TypeConverter;
            } catch (TargetInvocationException) {
            }
            return null;
        }
#endif

        private static bool HasImplicitNumericConversion(Type fromType, Type toType) {
            if (fromType.IsEnum) return false;

            if (fromType == typeof(BigInteger)) {
                if (toType == typeof(double)) return true;
                if (toType == typeof(float)) return true;
                if (toType == typeof(Complex64)) return true;
                return false;
            }

            if (fromType == typeof(bool)) {
                return false;
            }

            switch (Type.GetTypeCode(fromType)) {
                case TypeCode.SByte:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Byte:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Int16:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.UInt16:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Int32:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.UInt32:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Int64:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.UInt64:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Char:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Single:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Double:
                            return true;
                        default:
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Double:
                    switch (Type.GetTypeCode(toType)) {
                        default:
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                default:
                    return false;
            }
        }

        public static Candidate PreferConvert(Type t1, Type t2) {
            if (t1 == typeof(bool) && t2 == typeof(int)) return Candidate.Two;
            if (t1 == typeof(Decimal) && t2 == typeof(BigInteger)) return Candidate.Two;
            //if (t1 == typeof(int) && t2 == typeof(BigInteger)) return Candidate.Two;

            switch (Type.GetTypeCode(t1)) {
                case TypeCode.SByte:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }
                case TypeCode.Int16:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }
                case TypeCode.Int32:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }
                case TypeCode.Int64:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }
            }
            return Candidate.Equivalent;
        }

        private static bool HasNarrowingConversion(Type fromType, Type toType, NarrowingLevel allowNarrowing) {
            if (allowNarrowing == NarrowingLevel.All) {
                if (toType == CharType && fromType == StringType) return true;
                //if (toType == Int32Type && fromType == BigIntegerType) return true;
                //if (IsIntegral(fromType) && IsIntegral(toType)) return true;

                //Check if there is an implicit convertor defined on fromType to toType
                if (HasImplicitConversion(fromType, toType)) {
                    return true;
                }

                if (IsNumeric(fromType) && IsNumeric(toType)) return true;

                //if (toType.IsArray) {
                //    return typeof(Tuple).IsAssignableFrom(fromType);
                //}

                if (toType == CharType && fromType == StringType) return true;
                if (toType == Int32Type && fromType == BooleanType) return true;

                // Everything can convert to Boolean in Python
                if (toType == BooleanType) return true;

                //if (DelegateType.IsAssignableFrom(toType) && IsPythonType(fromType)) return true;
                //if (IEnumerableType == toType && IsPythonType(fromType)) return true;

                //__int__, __float__, __long__
                //if (toType == Int32Type && HasPythonProtocol(fromType, Symbols.ConvertToInt)) return true;
                //if (toType == DoubleType && HasPythonProtocol(fromType, Symbols.ConvertToFloat)) return true;
                //if (toType == BigIntegerType && HasPythonProtocol(fromType, Symbols.ConvertToLong)) return true;
            }

            if (toType.IsGenericType) {
                Type genTo = toType.GetGenericTypeDefinition();
                if (genTo == IListOfTType) {
                    return IListOfObjectType.IsAssignableFrom(fromType);
                } else if (genTo == typeof(System.Collections.Generic.IEnumerator<>)) {
                    //if (IsPythonType(fromType)) return true;
                } else if (genTo == IDictOfTType) {
                    return IDictionaryOfObjectType.IsAssignableFrom(fromType);
                }
            }

            if (fromType == BigIntegerType && toType == Int64Type) return true;

            return false;
        }

        private static bool HasImplicitConversion(Type fromType, Type toType) {
            foreach (MethodInfo method in fromType.GetMethods()) {
                if (method.Name == "op_Implicit" &&
                    method.GetParameters()[0].ParameterType == fromType &&
                    method.ReturnType == toType) {
                    return true;
                }
            }
            return false;
        }

        private static bool IsIntegral(Type t) {
            switch (Type.GetTypeCode(t)) {
                case TypeCode.DateTime:
                case TypeCode.DBNull:
                case TypeCode.Char:
                case TypeCode.Empty:
                case TypeCode.String:
                case TypeCode.Single:
                case TypeCode.Double:
                    return false;
                case TypeCode.Object:
                    return t == BigIntegerType;
                default:
                    return true;
            }
        }

        private static bool IsNumeric(Type t) {
            if (t.IsEnum) return false;

            switch (Type.GetTypeCode(t)) {
                case TypeCode.DateTime:
                case TypeCode.DBNull:
                case TypeCode.Char:
                case TypeCode.Empty:
                case TypeCode.String:
                case TypeCode.Boolean:
                    return false;
                case TypeCode.Object:
                    return t == BigIntegerType || t == Complex64Type;
                default:
                    return true;
            }
        }

    }
}
