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
using System.Diagnostics;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Math;

namespace IronRuby.Runtime {
    public static partial class Converter {
        #region Generated conversion helpers

        // *** BEGIN GENERATED CODE ***


        /// <summary>
        /// Conversion routine TryConvertToByte - converts object to Byte
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToByte(object value, out Byte result) {
            try {
                result = ConvertToByte(value);
                return true;
            } catch {
                result = default(Byte);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToSByte - converts object to SByte
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToSByte(object value, out SByte result) {
            try {
                result = ConvertToSByte(value);
                return true;
            } catch {
                result = default(SByte);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToInt16 - converts object to Int16
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToInt16(object value, out Int16 result) {
            try {
                result = ConvertToInt16(value);
                return true;
            } catch {
                result = default(Int16);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToInt32 - converts object to Int32
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToInt32(object value, out Int32 result) {
            try {
                result = ConvertToInt32(value);
                return true;
            } catch {
                result = default(Int32);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToInt64 - converts object to Int64
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToInt64(object value, out Int64 result) {
            try {
                result = ConvertToInt64(value);
                return true;
            } catch {
                result = default(Int64);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToUInt16 - converts object to UInt16
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToUInt16(object value, out UInt16 result) {
            try {
                result = ConvertToUInt16(value);
                return true;
            } catch {
                result = default(UInt16);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToUInt32 - converts object to UInt32
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToUInt32(object value, out UInt32 result) {
            try {
                result = ConvertToUInt32(value);
                return true;
            } catch {
                result = default(UInt32);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToUInt64 - converts object to UInt64
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToUInt64(object value, out UInt64 result) {
            try {
                result = ConvertToUInt64(value);
                return true;
            } catch {
                result = default(UInt64);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToDouble - converts object to Double
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToDouble(object value, out Double result) {
            try {
                result = ConvertToDouble(value);
                return true;
            } catch {
                result = default(Double);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToBigInteger - converts object to BigInteger
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToBigInteger(object value, out BigInteger result) {
            try {
                result = ConvertToBigInteger(value);
                return true;
            } catch {
                result = default(BigInteger);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToComplex64 - converts object to Complex64
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToComplex64(object value, out Complex64 result) {
            try {
                result = ConvertToComplex64(value);
                return true;
            } catch {
                result = default(Complex64);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToString - converts object to String
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToString(object value, out String result) {
            try {
                result = ConvertToString(value);
                return true;
            } catch {
                result = default(String);
                return false;
            }
        }

        /// <summary>
        /// Conversion routine TryConvertToChar - converts object to Char
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvertToChar(object value, out Char result) {
            try {
                result = ConvertToChar(value);
                return true;
            } catch {
                result = default(Char);
                return false;
            }
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated explicit enum conversion

        // *** BEGIN GENERATED CODE ***

        /// <summary>
        /// Explicit conversion of Enum to Int32
        /// </summary>
        internal static Int32 CastEnumToInt32(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (Int32)value;
                case TypeCode.Byte:
                    return (Int32)(Byte)value;
                case TypeCode.SByte:
                    return (Int32)(SByte)value;
                case TypeCode.Int16:
                    return (Int32)(Int16)value;
                case TypeCode.Int64:
                    return (Int32)(Int64)value;
                case TypeCode.UInt16:
                    return (Int32)(UInt16)value;
                case TypeCode.UInt32:
                    return (Int32)(UInt32)value;
                case TypeCode.UInt64:
                    return (Int32)(UInt64)value;
            }
            // Should never get here
            Debug.Assert(false, "Invalid enum detected");
            return default(Int32);
        }
        /// <summary>
        /// Explicit conversion of Enum to Byte
        /// </summary>
        internal static Byte CastEnumToByte(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (Byte)(Int32)value;
                case TypeCode.Byte:
                    return (Byte)value;
                case TypeCode.SByte:
                    return (Byte)(SByte)value;
                case TypeCode.Int16:
                    return (Byte)(Int16)value;
                case TypeCode.Int64:
                    return (Byte)(Int64)value;
                case TypeCode.UInt16:
                    return (Byte)(UInt16)value;
                case TypeCode.UInt32:
                    return (Byte)(UInt32)value;
                case TypeCode.UInt64:
                    return (Byte)(UInt64)value;
            }
            // Should never get here
            Debug.Assert(false, "Invalid enum detected");
            return default(Byte);
        }
        /// <summary>
        /// Explicit conversion of Enum to SByte
        /// </summary>
        internal static SByte CastEnumToSByte(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (SByte)(Int32)value;
                case TypeCode.Byte:
                    return (SByte)(Byte)value;
                case TypeCode.SByte:
                    return (SByte)value;
                case TypeCode.Int16:
                    return (SByte)(Int16)value;
                case TypeCode.Int64:
                    return (SByte)(Int64)value;
                case TypeCode.UInt16:
                    return (SByte)(UInt16)value;
                case TypeCode.UInt32:
                    return (SByte)(UInt32)value;
                case TypeCode.UInt64:
                    return (SByte)(UInt64)value;
            }
            // Should never get here
            Debug.Assert(false, "Invalid enum detected");
            return default(SByte);
        }
        /// <summary>
        /// Explicit conversion of Enum to Int16
        /// </summary>
        internal static Int16 CastEnumToInt16(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (Int16)(Int32)value;
                case TypeCode.Byte:
                    return (Int16)(Byte)value;
                case TypeCode.SByte:
                    return (Int16)(SByte)value;
                case TypeCode.Int16:
                    return (Int16)value;
                case TypeCode.Int64:
                    return (Int16)(Int64)value;
                case TypeCode.UInt16:
                    return (Int16)(UInt16)value;
                case TypeCode.UInt32:
                    return (Int16)(UInt32)value;
                case TypeCode.UInt64:
                    return (Int16)(UInt64)value;
            }
            // Should never get here
            Debug.Assert(false, "Invalid enum detected");
            return default(Int16);
        }
        /// <summary>
        /// Explicit conversion of Enum to Int64
        /// </summary>
        internal static Int64 CastEnumToInt64(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (Int64)(Int32)value;
                case TypeCode.Byte:
                    return (Int64)(Byte)value;
                case TypeCode.SByte:
                    return (Int64)(SByte)value;
                case TypeCode.Int16:
                    return (Int64)(Int16)value;
                case TypeCode.Int64:
                    return (Int64)value;
                case TypeCode.UInt16:
                    return (Int64)(UInt16)value;
                case TypeCode.UInt32:
                    return (Int64)(UInt32)value;
                case TypeCode.UInt64:
                    return (Int64)(UInt64)value;
            }
            // Should never get here
            Debug.Assert(false, "Invalid enum detected");
            return default(Int64);
        }
        /// <summary>
        /// Explicit conversion of Enum to UInt16
        /// </summary>
        internal static UInt16 CastEnumToUInt16(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (UInt16)(Int32)value;
                case TypeCode.Byte:
                    return (UInt16)(Byte)value;
                case TypeCode.SByte:
                    return (UInt16)(SByte)value;
                case TypeCode.Int16:
                    return (UInt16)(Int16)value;
                case TypeCode.Int64:
                    return (UInt16)(Int64)value;
                case TypeCode.UInt16:
                    return (UInt16)value;
                case TypeCode.UInt32:
                    return (UInt16)(UInt32)value;
                case TypeCode.UInt64:
                    return (UInt16)(UInt64)value;
            }
            // Should never get here
            Debug.Assert(false, "Invalid enum detected");
            return default(UInt16);
        }
        /// <summary>
        /// Explicit conversion of Enum to UInt32
        /// </summary>
        internal static UInt32 CastEnumToUInt32(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (UInt32)(Int32)value;
                case TypeCode.Byte:
                    return (UInt32)(Byte)value;
                case TypeCode.SByte:
                    return (UInt32)(SByte)value;
                case TypeCode.Int16:
                    return (UInt32)(Int16)value;
                case TypeCode.Int64:
                    return (UInt32)(Int64)value;
                case TypeCode.UInt16:
                    return (UInt32)(UInt16)value;
                case TypeCode.UInt32:
                    return (UInt32)value;
                case TypeCode.UInt64:
                    return (UInt32)(UInt64)value;
            }
            // Should never get here
            Debug.Assert(false, "Invalid enum detected");
            return default(UInt32);
        }
        /// <summary>
        /// Explicit conversion of Enum to UInt64
        /// </summary>
        internal static UInt64 CastEnumToUInt64(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (UInt64)(Int32)value;
                case TypeCode.Byte:
                    return (UInt64)(Byte)value;
                case TypeCode.SByte:
                    return (UInt64)(SByte)value;
                case TypeCode.Int16:
                    return (UInt64)(Int16)value;
                case TypeCode.Int64:
                    return (UInt64)(Int64)value;
                case TypeCode.UInt16:
                    return (UInt64)(UInt16)value;
                case TypeCode.UInt32:
                    return (UInt64)(UInt32)value;
                case TypeCode.UInt64:
                    return (UInt64)value;
            }
            // Should never get here
            Debug.Assert(false, "Invalid enum detected");
            return default(UInt64);
        }
        internal static Boolean CastEnumToBoolean(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (Int32)value != 0;
                case TypeCode.Byte:
                    return (Byte)value != 0;
                case TypeCode.SByte:
                    return (SByte)value != 0;
                case TypeCode.Int16:
                    return (Int16)value != 0;
                case TypeCode.Int64:
                    return (Int64)value != 0;
                case TypeCode.UInt16:
                    return (UInt16)value != 0;
                case TypeCode.UInt32:
                    return (UInt32)value != 0;
                case TypeCode.UInt64:
                    return (UInt64)value != 0;
            }
            // Should never get here
            Debug.Assert(false, "Invalid enum detected");
            return default(Boolean);
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated conversion implementations

        // *** BEGIN GENERATED CODE ***

        /// <summary>
        /// ConvertToByte Conversion Routine. If no conversion exists, throws TypeError false. Can throw OverflowException.
        /// </summary>
        public static Byte ConvertToByte(object value) {
            if (value is Byte) {
                return (Byte)value;
            } else if (value is Int32) {
                return checked((Byte)(Int32)value);
            } else if (value is Boolean) {
                return (Boolean)value ? (Byte)1 : (Byte)0;
            } else if (value is BigInteger) {
                UInt32 UInt32Value = ((BigInteger)value).ToUInt32();
                return checked((Byte)UInt32Value);
            } else if (value is Extensible<int>) {
                return checked((Byte)(Int32)((Extensible<int>)value).Value);
            } else if (value is Extensible<BigInteger>) {
                UInt32 UInt32Value = ((BigInteger)((Extensible<BigInteger>)value).Value).ToUInt32();
                return checked((Byte)UInt32Value);
            } else if (value is Int64) {
                return checked((Byte)(Int64)value);
            } else if (value is SByte) {
                return checked((Byte)(SByte)value);
            } else if (value is Int16) {
                return checked((Byte)(Int16)value);
            } else if (value is UInt16) {
                return checked((Byte)(UInt16)value);
            } else if (value is UInt32) {
                return checked((Byte)(UInt32)value);
            } else if (value is UInt64) {
                return checked((Byte)(UInt64)value);
            } else if (value is Decimal) {
                return checked((Byte)(Decimal)value);
            }

            Object result;
            if(TryConvertObject(value, typeof(Byte), out result) && result is Byte) {
                return (Byte)result;
            }

            throw CannotConvertTo("Byte", value);
        }
        
        /// <summary>
        /// ConvertToSByte Conversion Routine. If no conversion exists, throws TypeError false. Can throw OverflowException.
        /// </summary>
        [CLSCompliant(false)]
        public static SByte ConvertToSByte(object value) {
            if (value is SByte) {
                return (SByte)value;
            } else if (value is Int32) {
                return checked((SByte)(Int32)value);
            } else if (value is Boolean) {
                return (Boolean)value ? (SByte)1 : (SByte)0;
            } else if (value is BigInteger) {
                Int32 Int32Value = ((BigInteger)value).ToInt32();
                return checked((SByte)Int32Value);
            } else if (value is Extensible<int>) {
                return checked((SByte)(Int32)((Extensible<int>)value).Value);
            } else if (value is Extensible<BigInteger>) {
                Int32 Int32Value = ((BigInteger)((Extensible<BigInteger>)value).Value).ToInt32();
                return checked((SByte)Int32Value);
            } else if (value is Int64) {
                return checked((SByte)(Int64)value);
            } else if (value is Byte) {
                return checked((SByte)(Byte)value);
            } else if (value is Int16) {
                return checked((SByte)(Int16)value);
            } else if (value is UInt16) {
                return checked((SByte)(UInt16)value);
            } else if (value is UInt32) {
                return checked((SByte)(UInt32)value);
            } else if (value is UInt64) {
                return checked((SByte)(UInt64)value);
            } else if (value is Decimal) {
                return checked((SByte)(Decimal)value);
            }

            Object result;
            if(TryConvertObject(value, typeof(SByte), out result) && result is SByte) {
                return (SByte)result;
            }

            throw CannotConvertTo("SByte", value);
        }
        /// <summary>
        /// ConvertToInt16 Conversion Routine. If no conversion exists, throws TypeError false. Can throw OverflowException.
        /// </summary>
        public static Int16 ConvertToInt16(object value) {
            if (value is Int16) {
                return (Int16)value;
            } else if (value is Int32) {
                return checked((Int16)(Int32)value);
            } else if (value is Boolean) {
                return (Boolean)value ? (Int16)1 : (Int16)0;
            } else if (value is BigInteger) {
                Int32 Int32Value = ((BigInteger)value).ToInt32();
                return checked((Int16)Int32Value);
            } else if (value is Extensible<int>) {
                return checked((Int16)(Int32)((Extensible<int>)value).Value);
            } else if (value is Extensible<BigInteger>) {
                Int32 Int32Value = ((BigInteger)((Extensible<BigInteger>)value).Value).ToInt32();
                return checked((Int16)Int32Value);
            } else if (value is Int64) {
                return checked((Int16)(Int64)value);
            } else if (value is Byte) {
                return (Int16)(Byte)value;
            } else if (value is SByte) {
                return (Int16)(SByte)value;
            } else if (value is UInt16) {
                return checked((Int16)(UInt16)value);
            } else if (value is UInt32) {
                return checked((Int16)(UInt32)value);
            } else if (value is UInt64) {
                return checked((Int16)(UInt64)value);
            } else if (value is Decimal) {
                return checked((Int16)(Decimal)value);
            }

            Object result;
            if(TryConvertObject(value, typeof(Int16), out result) && result is Int16) {
                return (Int16)result;
            }

            throw CannotConvertTo("Int16", value);
        }

        /// <summary>
        /// ConvertToUInt16 Conversion Routine. If no conversion exists, throws TypeError false. Can throw OverflowException.
        /// </summary>
        [CLSCompliant(false)]
        public static UInt16 ConvertToUInt16(object value) {
            if (value is UInt16) {
                return (UInt16)value;
            } else if (value is Int32) {
                return checked((UInt16)(Int32)value);
            } else if (value is Boolean) {
                return (Boolean)value ? (UInt16)1 : (UInt16)0;
            } else if (value is BigInteger) {
                UInt32 UInt32Value = ((BigInteger)value).ToUInt32();
                return checked((UInt16)UInt32Value);
            } else if (value is Extensible<int>) {
                return checked((UInt16)(Int32)((Extensible<int>)value).Value);
            } else if (value is Extensible<BigInteger>) {
                UInt32 UInt32Value = ((BigInteger)((Extensible<BigInteger>)value).Value).ToUInt32();
                return checked((UInt16)UInt32Value);
            } else if (value is Int64) {
                return checked((UInt16)(Int64)value);
            } else if (value is Byte) {
                return (UInt16)(Byte)value;
            } else if (value is SByte) {
                return checked((UInt16)(SByte)value);
            } else if (value is Int16) {
                return checked((UInt16)(Int16)value);
            } else if (value is UInt32) {
                return checked((UInt16)(UInt32)value);
            } else if (value is UInt64) {
                return checked((UInt16)(UInt64)value);
            } else if (value is Decimal) {
                return checked((UInt16)(Decimal)value);
            }

            Object result;
            if(TryConvertObject(value, typeof(UInt16), out result) && result is UInt16) {
                return (UInt16)result;
            }

            throw CannotConvertTo("UInt16", value);
        }
        /// <summary>
        /// ConvertToInt32Impl Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        /// </summary>
        private static bool ConvertToInt32Impl(object value, out Int32 result) {
            if (value is Int32) {
                result = (Int32)value; return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (Int32)1 : (Int32)0; return true;
            } else if (value is BigInteger) {
                result = ((BigInteger)value).ToInt32(); return true;
            } else if (value is Extensible<int>) {
                result = (Int32)(Int32)((Extensible<int>)value).Value; return true;
            } else if (value is Extensible<BigInteger>) {
                result = ((BigInteger)((Extensible<BigInteger>)value).Value).ToInt32(); return true;
            } else if (value is Int64) {
                result = checked((Int32)(Int64)value); return true;
            } else if (value is Byte) {
                result = (Int32)(Byte)value; return true;
            } else if (value is SByte) {
                result = (Int32)(SByte)value; return true;
            } else if (value is Int16) {
                result = (Int32)(Int16)value; return true;
            } else if (value is UInt16) {
                result = (Int32)(UInt16)value; return true;
            } else if (value is UInt32) {
                result = checked((Int32)(UInt32)value); return true;
            } else if (value is UInt64) {
                result = checked((Int32)(UInt64)value); return true;
            } else if (value is Decimal) {
                result = checked((Int32)(Decimal)value); return true;
            }
            result = default(Int32);
            return false;
        }

        /// <summary>
        /// ConvertToUInt32 Conversion Routine. If no conversion exists, throws TypeError false. Can throw OverflowException.
        /// </summary>
        [CLSCompliant(false)]
        public static UInt32 ConvertToUInt32(object value) {
            if (value is UInt32) {
                return (UInt32)value;
            } else if (value is Int32) {
                return checked((UInt32)(Int32)value);
            } else if (value is Boolean) {
                return (Boolean)value ? (UInt32)1 : (UInt32)0;
            } else if (value is BigInteger) {
                return ((BigInteger)value).ToUInt32();
            } else if (value is Extensible<int>) {
                return checked((UInt32)(Int32)((Extensible<int>)value).Value);
            } else if (value is Extensible<BigInteger>) {
                return ((BigInteger)((Extensible<BigInteger>)value).Value).ToUInt32();
            } else if (value is Int64) {
                return checked((UInt32)(Int64)value);
            } else if (value is Byte) {
                return (UInt32)(Byte)value;
            } else if (value is SByte) {
                return checked((UInt32)(SByte)value);
            } else if (value is Int16) {
                return checked((UInt32)(Int16)value);
            } else if (value is UInt16) {
                return (UInt32)(UInt16)value;
            } else if (value is UInt64) {
                return checked((UInt32)(UInt64)value);
            } else if (value is Decimal) {
                return checked((UInt32)(Decimal)value);
            }

            Object result;
            if(TryConvertObject(value, typeof(UInt32), out result) && result is UInt32) {
                return (UInt32)result;
            }

            throw CannotConvertTo("UInt32", value);
        }
        /// <summary>
        /// ConvertToInt64 Conversion Routine. If no conversion exists, throws TypeError false. Can throw OverflowException.
        /// </summary>
        public static Int64 ConvertToInt64(object value) {
            if (value is Int64) {
                return (Int64)value;
            } else if (value is Int32) {
                return (Int64)(Int32)value;
            } else if (value is Boolean) {
                return (Boolean)value ? (Int64)1 : (Int64)0;
            } else if (value is BigInteger) {
                return ((BigInteger)value).ToInt64();
            } else if (value is Extensible<int>) {
                return (Int64)(Int32)((Extensible<int>)value).Value;
            } else if (value is Extensible<BigInteger>) {
                return ((BigInteger)((Extensible<BigInteger>)value).Value).ToInt64();
            } else if (value is Byte) {
                return (Int64)(Byte)value;
            } else if (value is SByte) {
                return (Int64)(SByte)value;
            } else if (value is Int16) {
                return (Int64)(Int16)value;
            } else if (value is UInt16) {
                return (Int64)(UInt16)value;
            } else if (value is UInt32) {
                return (Int64)(UInt32)value;
            } else if (value is UInt64) {
                return checked((Int64)(UInt64)value);
            } else if (value is Decimal) {
                return checked((Int64)(Decimal)value);
            }

            Object result;
            if(TryConvertObject(value, typeof(Int64), out result) && result is Int64) {
                return (Int64)result;
            }

            throw CannotConvertTo("Int64", value);
        }
        /// <summary>
        /// ConvertToUInt64 Conversion Routine. If no conversion exists, throws TypeError false. Can throw OverflowException.
        /// </summary>
        [CLSCompliant(false)]
        public static UInt64 ConvertToUInt64(object value) {
            if (value is UInt64) {
                return (UInt64)value;
            } else if (value is Int32) {
                return checked((UInt64)(Int32)value);
            } else if (value is Boolean) {
                return (Boolean)value ? (UInt64)1 : (UInt64)0;
            } else if (value is BigInteger) {
                return ((BigInteger)value).ToUInt64();
            } else if (value is Extensible<int>) {
                return checked((UInt64)(Int32)((Extensible<int>)value).Value);
            } else if (value is Extensible<BigInteger>) {
                return ((BigInteger)((Extensible<BigInteger>)value).Value).ToUInt64();
            } else if (value is Int64) {
                return checked((UInt64)(Int64)value);
            } else if (value is Byte) {
                return (UInt64)(Byte)value;
            } else if (value is SByte) {
                return checked((UInt64)(SByte)value);
            } else if (value is Int16) {
                return checked((UInt64)(Int16)value);
            } else if (value is UInt16) {
                return (UInt64)(UInt16)value;
            } else if (value is UInt32) {
                return (UInt64)(UInt32)value;
            } else if (value is Decimal) {
                return checked((UInt64)(Decimal)value);
            }

            Object result;
            if(TryConvertObject(value, typeof(UInt64), out result) && result is UInt64) {
                return (UInt64)result;
            }

            throw CannotConvertTo("UInt64", value);
        }
        /// <summary>
        /// ConvertToSingle Conversion Routine. If no conversion exists, throws TypeError false. Can throw OverflowException.
        /// </summary>
        public static Single ConvertToSingle(object value) {
            if (value is Single) {
                return (Single)value;
            } else if (value is Int32) {
                return (Single)(Int32)value;
            } else if (value is Boolean) {
                return (Boolean)value ? (Single)1 : (Single)0;
            } else if (value is BigInteger) {
                Single SingleValue = checked((Single)((BigInteger)value).ToFloat64());
                if (Single.IsInfinity(SingleValue)) {
                    //throw PythonOps.OverflowError("{0} won't fit into Single", value);
                }
                return SingleValue;
            } else if (value is Double) {
                Single SingleValue = checked((Single)(Double)value);
                if (Single.IsInfinity(SingleValue)) {
                    //throw PythonOps.OverflowError("{0} won't fit into Single", value);
                }
                return SingleValue;
            } else if (value is Extensible<int>) {
                return (Single)(Int32)((Extensible<int>)value).Value;
            } else if (value is Extensible<BigInteger>) {
                Single SingleValue = checked((Single)((BigInteger)((Extensible<BigInteger>)value).Value).ToFloat64());
                if (Single.IsInfinity(SingleValue)) {
                    //throw PythonOps.OverflowError("{0} won't fit into Single", ((Extensible<BigInteger>)value).Value);
                }
                return SingleValue;
            } else if (value is Extensible<double>) {
                Single SingleValue = checked((Single)(Double)((Extensible<double>)value).Value);
                if (Single.IsInfinity(SingleValue)) {
                    //throw PythonOps.OverflowError("{0} won't fit into Single", ((Extensible<double>)value).Value);
                }
                return SingleValue;
            } else if (value is Int64) {
                return (Single)(Int64)value;
            } else if (value is Byte) {
                return (Single)(Byte)value;
            } else if (value is SByte) {
                return (Single)(SByte)value;
            } else if (value is Int16) {
                return (Single)(Int16)value;
            } else if (value is UInt16) {
                return (Single)(UInt16)value;
            } else if (value is UInt32) {
                return (Single)(UInt32)value;
            } else if (value is UInt64) {
                return (Single)(UInt64)value;
            } else if (value is Decimal) {
                return (Single)(Decimal)value;
            }

            Object result;
            if(TryConvertObject(value, typeof(Single), out result) && result is Single) {
                return (Single)result;
            }

            throw CannotConvertTo("Single", value);
        }
        /// <summary>
        /// ConvertToDoubleImpl Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        /// </summary>
        private static bool ConvertToDoubleImpl(object value, out Double result) {
            if (value is Double) {
                result = (Double)value; return true;
            } else if (value is Int32) {
                result = (Double)(Int32)value; return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (Double)1 : (Double)0; return true;
            } else if (value is BigInteger) {
                result = ((BigInteger)value).ToFloat64(); return true;
            } else if (value is Extensible<int>) {
                result = (Double)(Int32)((Extensible<int>)value).Value; return true;
            } else if (value is Extensible<BigInteger>) {
                result = ((BigInteger)((Extensible<BigInteger>)value).Value).ToFloat64(); return true;
            } else if (value is Extensible<double>) {
                result = (Double)(Double)((Extensible<double>)value).Value; return true;
            } else if (value is Int64) {
                result = (Double)(Int64)value; return true;
            } else if (value is Byte) {
                result = (Double)(Byte)value; return true;
            } else if (value is SByte) {
                result = (Double)(SByte)value; return true;
            } else if (value is Int16) {
                result = (Double)(Int16)value; return true;
            } else if (value is UInt16) {
                result = (Double)(UInt16)value; return true;
            } else if (value is UInt32) {
                result = (Double)(UInt32)value; return true;
            } else if (value is UInt64) {
                result = (Double)(UInt64)value; return true;
            } else if (value is Single) {
                result = (Double)(Single)value; return true;
            } else if (value is Decimal) {
                result = (Double)(Decimal)value; return true;
            }
            result = default(Double);
            return false;
        }
        /// <summary>
        /// ConvertToDecimal Conversion Routine. If no conversion exists, throws TypeError false. Can throw OverflowException.
        /// </summary>
        public static Decimal ConvertToDecimal(object value) {
            if (value is Decimal) {
                return (Decimal)value;
            } else if (value is Int32) {
                return (Decimal)(Int32)value;
            } else if (value is Boolean) {
                return (Boolean)value ? (Decimal)1 : (Decimal)0;
            } else if (value is BigInteger) {
                return ((BigInteger)value).ToDecimal();
            } else if (value is Double) {
                return checked((Decimal)(Double)value);
            } else if (value is Extensible<int>) {
                return (Decimal)(Int32)((Extensible<int>)value).Value;
            } else if (value is Extensible<BigInteger>) {
                return ((BigInteger)((Extensible<BigInteger>)value).Value).ToDecimal();
            } else if (value is Extensible<double>) {
                return checked((Decimal)(Double)((Extensible<double>)value).Value);
            } else if (value is Int64) {
                return (Decimal)(Int64)value;
            } else if (value is Byte) {
                return (Decimal)(Byte)value;
            } else if (value is SByte) {
                return (Decimal)(SByte)value;
            } else if (value is Int16) {
                return (Decimal)(Int16)value;
            } else if (value is UInt16) {
                return (Decimal)(UInt16)value;
            } else if (value is UInt32) {
                return (Decimal)(UInt32)value;
            } else if (value is UInt64) {
                return (Decimal)(UInt64)value;
            } else if (value is Single) {
                return checked((Decimal)(Single)value);
            }

            Object result;
            if(TryConvertObject(value, typeof(Decimal), out result) && result is Decimal) {
                return (Decimal)result;
            }

            throw CannotConvertTo("Decimal", value);
        }
        /// <summary>
        /// ConvertToBigIntegerImpl Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        /// </summary>
        private static bool ConvertToBigIntegerImpl(object value, out BigInteger result) {
            if (value is BigInteger) {
                result = (BigInteger)value; return true;
            } else if (value is Int32) {
                result = (BigInteger)(Int32)value; return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? BigInteger.One : BigInteger.Zero; return true;
            } else if (value is Extensible<int>) {
                result = (BigInteger)(Int32)((Extensible<int>)value).Value; return true;
            } else if (value is Extensible<BigInteger>) {
                result = (BigInteger)((Extensible<BigInteger>)value).Value; return true;
            } else if (value is Int64) {
                result = (BigInteger)(Int64)value; return true;
            } else if (value is Byte) {
                result = (BigInteger)(Byte)value; return true;
            } else if (value is SByte) {
                result = (BigInteger)(SByte)value; return true;
            } else if (value is Int16) {
                result = (BigInteger)(Int16)value; return true;
            } else if (value is UInt16) {
                result = (BigInteger)(UInt16)value; return true;
            } else if (value is UInt32) {
                result = (BigInteger)(UInt32)value; return true;
            } else if (value is UInt64) {
                result = (BigInteger)(UInt64)value; return true;
            } else if (value is Decimal) {
                result = (BigInteger)(Decimal)value; return true;
            } else if (value == null) {
                result = null; return true;
            }
            result = default(BigInteger);
            return false;
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
