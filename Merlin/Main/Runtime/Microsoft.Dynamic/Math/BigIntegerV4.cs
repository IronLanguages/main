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
#if !CLR2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;
using BigInt = System.Numerics.BigInteger;

namespace Microsoft.Scripting.Math {
    /// <summary>
    /// arbitrary precision integers
    /// </summary>
    [Serializable]
    public sealed class BigInteger : IFormattable, IComparable, IConvertible, IEquatable<BigInteger> {
        private readonly BigInt _value;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigInteger Zero = new BigInteger((BigInt)0);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigInteger One = new BigInteger((BigInt)1);

        public BigInteger(BigInt value) {
            _value = value;
        }

        [CLSCompliant(false)]
        public static BigInteger Create(ulong v) {
            return new BigInteger(new BigInt(v));
        }

        [CLSCompliant(false)]
        public static BigInteger Create(uint v) {
            return new BigInteger(new BigInt(v));
        }

        public static BigInteger Create(long v) {
            return new BigInteger(new BigInt(v));
        }

        public static BigInteger Create(int v) {
            return new BigInteger(new BigInt(v));
        }

        public static BigInteger Create(decimal v) {
            return new BigInteger(new BigInt(v));
        }

        public static BigInteger Create(byte[] v) {
            return new BigInteger(new BigInt(v));
        }

        public static BigInteger Create(double v) {
            return new BigInteger(new BigInt(v));
        }

        public static implicit operator BigInteger(byte i) {
            return new BigInteger((BigInt)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(sbyte i) {
            return new BigInteger((BigInt)i);
        }

        public static implicit operator BigInteger(short i) {
            return new BigInteger((BigInt)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(ushort i) {
            return new BigInteger((BigInt)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(uint i) {
            return new BigInteger((BigInt)i);
        }

        public static implicit operator BigInteger(int i) {
            return new BigInteger((BigInt)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(ulong i) {
            return new BigInteger((BigInt)i);
        }

        public static implicit operator BigInteger(long i) {
            return new BigInteger((BigInt)i);
        }

        public static implicit operator BigInteger(decimal self) {
            return new BigInteger((BigInt)self);
        }

        public static explicit operator BigInteger(double self) {
            return new BigInteger((BigInt)self);
        }

        public static explicit operator BigInteger(float self) {
            return new BigInteger((BigInt)self);
        }

        public static explicit operator double(BigInteger self) {
            return (double)self._value;
        }


        public static explicit operator float(BigInteger self) {
            return (float)self._value;
        }

        public static explicit operator decimal(BigInteger self) {
            return (decimal)self._value;
        }

        public static explicit operator byte(BigInteger self) {
            return (byte)self._value;
        }

        [CLSCompliant(false)]
        public static explicit operator sbyte(BigInteger self) {
            return (sbyte)self._value;
        }

        [CLSCompliant(false)]
        public static explicit operator UInt16(BigInteger self) {
            return (UInt16)self._value;
        }

        public static explicit operator Int16(BigInteger self) {
            return (Int16)self._value;
        }

        [CLSCompliant(false)]
        public static explicit operator UInt32(BigInteger self) {
            return (UInt32)self._value;
        }

        public static explicit operator Int32(BigInteger self) {
            return (Int32)self._value;
        }

        public static explicit operator Int64(BigInteger self) {
            return (Int64)self._value;
        }

        [CLSCompliant(false)]
        public static explicit operator UInt64(BigInteger self) {
            return (UInt64)self._value;
        }

        public BigInteger(BigInteger copy) {
            if (object.ReferenceEquals(copy, null)) {
                throw new ArgumentNullException("copy");
            }
            _value = copy._value;
        }

        public BigInteger(int sign, byte[] data) {
            ContractUtils.RequiresNotNull(data, "data");
            ContractUtils.Requires(sign >= -1 && sign <= +1, "sign");

            _value = new BigInt(data);
            if (sign < 0) {
                _value = -_value;
            }
        }
        
        [CLSCompliant(false)]
        public BigInteger(int sign, uint[] data) {
            ContractUtils.RequiresNotNull(data, "data");
            ContractUtils.Requires(sign >= -1 && sign <= +1, "sign");
            int length = GetLength(data);
            ContractUtils.Requires(length == 0 || sign != 0, "sign");
            if (length == 0) {
                _value = 0;
                return;
            }

            bool highest = (data[length - 1] & 0x80000000) != 0;
            byte[] bytes = new byte[length * 4 + (highest ? 1 : 0)];
            int j = 0;
            for (int i = 0; i < length; i++) {
                ulong w = data[i];
                bytes[j++] = (byte)(w & 0xff);
                bytes[j++] = (byte)((w >> 8) & 0xff);
                bytes[j++] = (byte)((w >> 16) & 0xff);
                bytes[j++] = (byte)((w >> 24) & 0xff);
            }

            _value = new BigInt(bytes);
            if (sign < 0) {
                _value = -_value;
            }
        }

        [CLSCompliant(false)]
        public uint[] GetWords() {
            int hi;
            byte[] bytes;
            GetHighestByte(out hi, out bytes);

            uint[] result = new uint[(hi + 1 + 3) / 4];
            int i = 0;
            int j = 0;
            uint u = 0;
            int shift = 0;
            while (i < bytes.Length) {
                u |= (uint)bytes[i++] << shift;
                if (i % 4 == 0) {
                    result[j++] = u;
                    u = 0;
                }
                shift += 8;
            }
            if (u != 0) {
                result[j] = u;
            }
            return result;
        }

        public int GetBitCount() {
            if (_value.IsZero) {
                return 0;
            }

            int hi;
            byte[] bytes;
            int b = GetHighestByte(out hi, out bytes);
            int result = hi * 8;
            do {
                b >>= 1;
                result++;
            } while (b > 0);

            return result;
        }

        public int GetWordCount() {
            int index;
            byte[] bytes;
            GetHighestByte(out index, out bytes);
            return (index + 1 + 3) / 4;
        }

        public int GetByteCount() {
            int index;
            byte[] bytes;
            GetHighestByte(out index, out bytes);
            return index + 1;
        }

        private byte GetHighestByte(out int index, out byte[] byteArray) {
            byte[] bytes = BigInt.Abs(_value).ToByteArray();
            int hi = bytes.Length;
            byte b;
            do {
                b = bytes[--hi];
            } while (b == 0);
            index = hi;
            byteArray = bytes;
            return b;
        }

        /// <summary>
        /// Return the sign of this BigInteger: -1, 0, or 1.
        /// </summary>
        public int Sign {
            get {
                return _value.Sign;
            }
        }

        public bool AsInt64(out long ret) {
            if (_value >= Int64.MinValue && _value <= Int64.MaxValue) {
                ret = (long)_value;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public bool AsUInt32(out uint ret) {
            if (_value >= UInt32.MinValue && _value <= UInt32.MaxValue) {
                ret = (UInt32)_value;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public bool AsUInt64(out ulong ret) {
            if (_value >= UInt64.MinValue && _value <= UInt64.MaxValue) {
                ret = (UInt64)_value;
                return true;
            }
            ret = 0;
            return false;
        }

        public bool AsInt32(out int ret) {
            if (_value >= Int32.MinValue && _value <= Int32.MaxValue) {
                ret = (Int32)_value;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public uint ToUInt32() {
            return (uint)_value;
        }

        public int ToInt32() {
            return (int)_value;
        }

        public decimal ToDecimal() {
            return (decimal)_value;
        }

        [CLSCompliant(false)]
        public ulong ToUInt64() {
            return (ulong)_value;
        }

        public long ToInt64() {
            return (long)_value;
        }

        public bool TryToFloat64(out double result) {
            return StringUtils.TryParseDouble(ToString(10),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat,
                out result);
        }

        public double ToFloat64() {
            return (double)_value;
        }

        private static int GetLength(uint[] data) {
            int ret = data.Length - 1;
            while (ret >= 0 && data[ret] == 0) ret--;
            return ret + 1;
        }

        public static int Compare(BigInteger x, BigInteger y) {
            return BigInt.Compare(x._value, y._value);
        }

        public static bool operator ==(BigInteger x, int y) {
            return x._value == y;
        }

        public static bool operator !=(BigInteger x, int y) {
            return x._value != y;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
        public static bool operator ==(BigInteger x, double y) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }

            // we can hold all double values, but not all double values
            // can hold BigInteger values, and we may lose precision.  Convert
            // the double to a big int, then compare.

            if ((y % 1) != 0) return false;  // not a whole number, can't be equal

            return x._value == (BigInt)y;
        }

        public static bool operator ==(double x, BigInteger y) {
            return y == x;
        }

        public static bool operator !=(BigInteger x, double y) {
            return !(x == y);
        }

        public static bool operator !=(double x, BigInteger y) {
            return !(x == y);
        }


        public static bool operator ==(BigInteger x, BigInteger y) {
            return Compare(x, y) == 0;
        }

        public static bool operator !=(BigInteger x, BigInteger y) {
            return Compare(x, y) != 0;
        }
        public static bool operator <(BigInteger x, BigInteger y) {
            return Compare(x, y) < 0;
        }
        public static bool operator <=(BigInteger x, BigInteger y) {
            return Compare(x, y) <= 0;
        }
        public static bool operator >(BigInteger x, BigInteger y) {
            return Compare(x, y) > 0;
        }
        public static bool operator >=(BigInteger x, BigInteger y) {
            return Compare(x, y) >= 0;
        }

        public static BigInteger Add(BigInteger x, BigInteger y) {
            return x + y;
        }

        public static BigInteger operator +(BigInteger x, BigInteger y) {
            return new BigInteger(x._value + y._value);
        }

        public static BigInteger Subtract(BigInteger x, BigInteger y) {
            return x - y;
        }

        public static BigInteger operator -(BigInteger x, BigInteger y) {
            return new BigInteger(x._value - y._value);
        }

        public static BigInteger Multiply(BigInteger x, BigInteger y) {
            return x * y;
        }

        public static BigInteger operator *(BigInteger x, BigInteger y) {
            return new BigInteger(x._value * y._value);
        }

        public static BigInteger Divide(BigInteger x, BigInteger y) {
            return x / y;
        }

        public static BigInteger operator /(BigInteger x, BigInteger y) {
            BigInteger dummy;
            return DivRem(x, y, out dummy);
        }

        public static BigInteger Mod(BigInteger x, BigInteger y) {
            return x % y;
        }

        public static BigInteger operator %(BigInteger x, BigInteger y) {
            BigInteger ret;
            DivRem(x, y, out ret);
            return ret;
        }

        public static BigInteger DivRem(BigInteger x, BigInteger y, out BigInteger remainder) {
            BigInt rem;
            BigInt result = BigInt.DivRem(x._value, y._value, out rem);
            remainder = new BigInteger(rem);
            return new BigInteger(result);
        }

        public static BigInteger BitwiseAnd(BigInteger x, BigInteger y) {
            return x & y;
        }

        public static BigInteger operator &(BigInteger x, BigInteger y) {
            return new BigInteger(x._value & y._value);
        }

        public static BigInteger BitwiseOr(BigInteger x, BigInteger y) {
            return x | y;
        }

        public static BigInteger operator |(BigInteger x, BigInteger y) {
            return new BigInteger(x._value | y._value);
        }

        public static BigInteger Xor(BigInteger x, BigInteger y) {
            return x ^ y;
        }

        public static BigInteger operator ^(BigInteger x, BigInteger y) {
            return new BigInteger(x._value ^ y._value);
        }

        public static BigInteger LeftShift(BigInteger x, int shift) {
            return x << shift;
        }

        public static BigInteger operator <<(BigInteger x, int shift) {
            return new BigInteger(x._value << shift);
        }

        public static BigInteger RightShift(BigInteger x, int shift) {
            return x >> shift;
        }

        public static BigInteger operator >>(BigInteger x, int shift) {
            return new BigInteger(x._value >> shift);
        }

        public static BigInteger Negate(BigInteger x) {
            return -x;
        }

        public static BigInteger operator -(BigInteger x) {
            return new BigInteger(-x._value);
        }

        public BigInteger OnesComplement() {
            return ~this;
        }

        public static BigInteger operator ~(BigInteger x) {
            return new BigInteger(~x._value);
        }

        public BigInteger Abs() {
            return new BigInteger(BigInt.Abs(_value));
        }

        public BigInteger Power(int exp) {
            return new BigInteger(BigInt.Pow(_value, exp));
        }

        public BigInteger ModPow(int power, BigInteger mod) {
            return new BigInteger(BigInt.ModPow(_value, power, mod._value));
        }

        public BigInteger ModPow(BigInteger power, BigInteger mod) {
            return new BigInteger(BigInt.ModPow(_value, power._value, mod._value));
        }

        public BigInteger Square() {
            return this * this;
        }

        public static BigInteger Parse(string str) {
            return new BigInteger(BigInt.Parse(str));
        }

        public override string ToString() {
            return ToString(10);
        }

        public string ToString(int @base) {
            return MathUtils.BigIntegerToString(GetWords(), Sign, @base);
        }

        public override int GetHashCode() {
            return _value.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as BigInteger);
        }

        public bool Equals(BigInteger other) {
            if (object.ReferenceEquals(other, null)) return false;
            return this == other;
        }

        public bool IsNegative() {
            return _value.Sign < 0;
        }

        public bool IsZero() {
            return _value.Sign == 0;
        }

        public bool IsPositive() {
            return _value.Sign > 0;
        }

        private bool IsOdd() {
            return !_value.IsEven;
        }

        public double Log(Double newBase) {
            return BigInt.Log(_value, newBase);
        }

        /// <summary>
        /// Calculates the natural logarithm of the BigInteger.
        /// </summary>
        public double Log() {
            return BigInt.Log(_value);
        }

        /// <summary>
        /// Calculates log base 10 of a BigInteger.
        /// </summary>
        public double Log10() {
            return BigInt.Log10(_value);
        }

        #region IComparable Members

        public int CompareTo(object obj) {
            if (obj == null) {
                return 1;
            }
            BigInteger o = obj as BigInteger;
            if (object.ReferenceEquals(o, null)) {
                throw new ArgumentException("expected integer");
            }
            return Compare(this, o);
        }

        #endregion

        #region IConvertible Members

        public TypeCode GetTypeCode() {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider) {
            return _value != 0;
        }

        public byte ToByte(IFormatProvider provider) {
            return (byte)_value;
        }

        /// <summary>
        /// Return the value of this BigInteger as a little-endian twos-complement
        /// byte array, using the fewest number of bytes possible. If the value is zero,
        /// return an array of one byte whose element is 0x00.
        /// </summary>
        public byte[] ToByteArray() {
            return _value.ToByteArray();
        }

        [Confined]
        public char ToChar(IFormatProvider provider) {
            return (char)_value;
        }

        [Confined]
        public DateTime ToDateTime(IFormatProvider provider) {
            throw new NotImplementedException();
        }

        [Confined]
        public decimal ToDecimal(IFormatProvider provider) {
            return (decimal)_value;
        }

        [Confined]
        public double ToDouble(IFormatProvider provider) {
            return (double)_value;
        }

        [Confined]
        public short ToInt16(IFormatProvider provider) {
            return (short)_value;
        }

        [Confined]
        public int ToInt32(IFormatProvider provider) {
            return (int)_value;
        }

        [Confined]
        public long ToInt64(IFormatProvider provider) {
            return (long)_value;
        }

        [CLSCompliant(false), Confined]
        public sbyte ToSByte(IFormatProvider provider) {
            return (sbyte)_value;
        }

        [Confined]
        public float ToSingle(IFormatProvider provider) {
            return (float)_value;
        }

        [Confined]
        public string ToString(IFormatProvider provider) {
            return _value.ToString(provider);
        }

        [Confined]
        public object ToType(Type conversionType, IFormatProvider provider) {
            if (conversionType == typeof(BigInteger)) {
                return this;
            }
            throw new NotImplementedException();
        }

        [CLSCompliant(false), Confined]
        public ushort ToUInt16(IFormatProvider provider) {
            return (ushort)_value;
        }

        [CLSCompliant(false), Confined]
        public uint ToUInt32(IFormatProvider provider) {
            return (uint)_value;
        }

        [CLSCompliant(false), Confined]
        public ulong ToUInt64(IFormatProvider provider) {
            return (ulong)_value;
        }

        #endregion

        #region IFormattable Members

        string IFormattable.ToString(string format, IFormatProvider formatProvider) {
            return _value.ToString(format, formatProvider);
        }

        #endregion
    }
}
#endif
