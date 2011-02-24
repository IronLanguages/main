/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Globalization;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using Microsoft.Scripting;

namespace IronRuby.Runtime {
    public static class Utils {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly byte[] EmptyBytes = new byte[0];

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly char[] EmptyChars = new char[0];
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly MemberInfo[] EmptyMemberInfos = new MemberInfo[0];

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly Delegate[] EmptyDelegates = new Delegate[0];

        public static int IndexOf(this string[]/*!*/ array, string/*!*/ value, StringComparer/*!*/ comparer) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.RequiresNotNull(comparer, "comparer");

            for (int i = 0; i < array.Length; i++) {
                if (comparer.Equals(array[i], value)) {
                    return i;
                }
            }

            return -1;
        }

        public static bool IsAscii(this string/*!*/ str) {
            for (int i = 0; i < str.Length; i++) {
                if (str[i] > 0x7f) {
                    return false;
                }
            }
            return true;
        }

        public static bool IsAscii(this byte[]/*!*/ bytes, int count) {
            for (int i = 0; i < count; i++) {
                if (bytes[i] > 0x7f) {
                    return false;
                }
            }
            return true;
        }

        public static bool IsAscii(this char[]/*!*/ str, int count) {
            for (int i = 0; i < count; i++) {
                if (str[i] > 0x7f) {
                    return false;
                }
            }
            return true;
        }

        public static bool IsAscii(this char[]/*!*/ str, int start, int count) {
            for (int i = 0; i < count; i++) {
                if (str[start + i] > 0x7f) {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsBinary(this string/*!*/ str) {
            for (int i = 0; i < str.Length; i++) {
                if (str[i] > 0xff) {
                    return false;
                }
            }
            return true;
        }

        internal static int GetCharacterCount(this string/*!*/ str) {
            int surrogateCount = 0;
            bool wasHighSurrogate = false;
            for (int i = 0; i < str.Length; i++) {
                char c = str[i];
                if (c >= '\uD800') {
                    if (c <= '\uDBFF') {
                        wasHighSurrogate = true;
                    } else if (wasHighSurrogate && c <= '\uDFFF') {
                        surrogateCount++;
                        wasHighSurrogate = false;
                    }
                }
            }
            return str.Length - surrogateCount;
        }

        /// <summary>
        /// Calculates the number of Unicode characters in given array.
        /// Assumes that the content of the array beyond count chars doesn't contain significant data and can be overwritten.
        /// </summary>
        internal static int GetCharacterCount(this char[]/*!*/ str, int count) {
            int surrogateCount = 0;
            bool wasHighSurrogate = false;
            if (count < str.Length) {
                str[count] = '\uffff';
            }

            for (int i = 0; i < str.Length; i++) {
                char c = str[i];
                if (c >= '\uD800') {
                    if (i >= count) {
                        break;
                    } else if (c <= '\uDBFF') {
                        wasHighSurrogate = true;
                    } else if (wasHighSurrogate && c <= '\uDFFF') {
                        surrogateCount++;
                        wasHighSurrogate = false;
                    }
                }
            }
            return str.Length - surrogateCount;
        }

        public static string/*!*/ ToAsciiString(this string/*!*/ str) {
            return MutableString.AppendUnicodeRepresentation(new StringBuilder(), str, MutableString.Escape.NonAscii, -1, -1).ToString();
        }

        public static int LastCharacter(this string/*!*/ str) {
            return str.Length == 0 ? -1 : str[str.Length - 1];
        }

        internal static IEnumerable<byte>/*!*/ EnumerateAsBytes(char[]/*!*/ data, int count) {
            for (int i = 0; i < count; i++) {
                yield return (byte)data[i];
            }
        }

        internal static IEnumerable<byte>/*!*/ EnumerateAsBytes(string/*!*/ data) {
            for (int i = 0; i < data.Length; i++) {
                yield return (byte)data[i];
            }
        }

        internal static IEnumerable<T>/*!*/ Enumerate<T>(T[]/*!*/ data, int count) {
            for (int i = 0; i < count; i++) {
                yield return data[i];
            }
        }

        internal const int MinListSize = 4;
        internal const int MinBufferSize = 16;

        internal static int GetExpandedSize<T>(T[]/*!*/ array, int minLength) {
            return Math.Max(minLength, Math.Max(1 + (array.Length << 1), typeof(T) == typeof(object) ? MinListSize : MinBufferSize));
        }

        internal static void Resize<T>(ref T[]/*!*/ array, int minLength) {
            if (array.Length < minLength) {
                Array.Resize(ref array, GetExpandedSize(array, minLength));
            }
        }

        internal static void TrimExcess<T>(ref T[] data, int count) {
            if (IsSparse(count, data.Length)) {
                Array.Resize(ref data, count);
            }
        }

        internal static bool IsSparse(int portionSize, int totalSize) {
            Debug.Assert(portionSize <= totalSize);
            return (long)portionSize * 10 < (long)totalSize * 9;
        }

        internal static void ResizeForInsertion<T>(ref T[]/*!*/ array, int itemCount, int index, int count) {
            int minLength = itemCount + count;
            T[] a;
            if (array.Length < minLength) {
                a = new T[GetExpandedSize(array, minLength)];
                Array.Copy(array, 0, a, 0, index);
            } else {
                a = array;
            }

            Array.Copy(array, index, a, index + count, itemCount - index);
            array = a;
        }

        internal static void Fill<T>(T[]/*!*/ array, int index, T item, int repeatCount) {
            // TODO: can be optimized for big repeatCount:
            for (int i = index; i < index + repeatCount; i++) {
                array[i] = item;
            }
        }

        private static void Fill(byte[]/*!*/ src, int srcStart, byte[]/*!*/ dst, int dstStart, int count, int repeatCount) {
            // TODO: can be optimized for big repeatCount or big count:
            if (count == 1) {
                Fill(dst, dstStart, src[srcStart], repeatCount);
            } else {
                for (int i = 0; i < repeatCount; i++) {
                    for (int j = 0; j < count; j++) {
                        dst[dstStart++] = src[srcStart + j];
                    }
                }
            }
        }

        private static int GetByteCount(string/*!*/ str, int start, int count, Encoding/*!*/ encoding, out char[]/*!*/ chars) {
            // TODO: we can special case this for some encodings and calculate the byte count w/o copying the content: 
            chars = new char[count];
            str.CopyTo(start, chars, 0, count);
            return encoding.GetByteCount(chars, 0, chars.Length);
        }

        internal static T[]/*!*/ Concatenate<T>(T[]/*!*/ array1, T[]/*!*/ array2) {
            return Concatenate(array1, array1.Length, array2, array2.Length);
        }

        internal static T[]/*!*/ Concatenate<T>(params T[][] arrays) {
            int length = 0;
            foreach (var array in arrays) {
                length += array.Length;
            }

            T[] result = new T[length];
            length = 0;
            foreach (var array in arrays) {
                Array.Copy(array, 0, result, length, array.Length);
                length += array.Length;
            }
            return result;
        }

        internal static T[]/*!*/ Concatenate<T>(T[]/*!*/ array1, int itemCount1, T[]/*!*/ array2, int itemCount2) {
            T[] result = new T[itemCount1 + itemCount2];
            Array.Copy(array1, 0, result, 0, itemCount1);
            Array.Copy(array2, 0, result, itemCount1, itemCount2);
            return result;
        }

        internal static int Append<T>(ref T[]/*!*/ array, int itemCount, T item, int repeatCount) {
            Resize(ref array, itemCount + repeatCount);
            Fill(array, itemCount, item, repeatCount);
            return itemCount + repeatCount;
        }

        internal static int Append(ref char[]/*!*/ array, int itemCount, string/*!*/ other, int start, int count) {
            int newCount = itemCount + count;
            Resize(ref array, newCount);
            other.CopyTo(start, array, itemCount, count);
            return newCount;
        }

        internal static int Append<T>(ref T[]/*!*/ array, int itemCount, T[]/*!*/ other, int start, int count) {
            int newCount = itemCount + count;
            Resize(ref array, newCount);
            Array.Copy(other, start, array, itemCount, count);
            return newCount;
        }

        internal static int Append(ref byte[]/*!*/ array, int itemCount, string/*!*/ other, int start, int count, Encoding/*!*/ encoding) {
            char[] appendChars;
            int newCount = itemCount + GetByteCount(other, start, count, encoding, out appendChars);
            Resize(ref array, newCount);
            encoding.GetBytes(appendChars, 0, appendChars.Length, array, itemCount);
            return newCount;
        }

        internal static int Append(ref byte[]/*!*/ array, int itemCount, char[]/*!*/ other, int start, int count, Encoding/*!*/ encoding) {
            int newCount = itemCount + encoding.GetByteCount(other, start, count);
            Resize(ref array, newCount);
            encoding.GetBytes(other, start, count, array, itemCount);
            return newCount;
        }

        internal static int Append(ref byte[]/*!*/ array, int itemCount, char other, int repeatCount, Encoding/*!*/ encoding) {
            if (repeatCount == 0) {
                return itemCount;
            }

            char[] chars = new char[] { other };
            int charSize = encoding.GetByteCount(chars, 0, 1);
            int newCount = itemCount + charSize * repeatCount;
            Resize(ref array, newCount);

            encoding.GetBytes(chars, 0, 1, array, itemCount);
            Fill(array, itemCount, array, itemCount + charSize, charSize, repeatCount - 1);
            
            return newCount;
        }

        internal static int InsertAt<T>(ref T[]/*!*/ array, int itemCount, int index, T other, int repeatCount) {
            ResizeForInsertion(ref array, itemCount, index, repeatCount);
            Fill(array, index, other, repeatCount);
            return itemCount + repeatCount;
        }

        internal static int InsertAt(ref char[]/*!*/ array, int itemCount, int index, string/*!*/ other, int start, int count) {
            ResizeForInsertion(ref array, itemCount, index, count);
            other.CopyTo(start, array, index, count);
            return itemCount + count;
        }

        internal static int InsertAt<T>(ref T[]/*!*/ array, int itemCount, int index, T[]/*!*/ other, int start, int count) {
            ResizeForInsertion(ref array, itemCount, index, count);
            Array.Copy(other, start, array, index, count);
            return itemCount + count;
        }

        internal static int InsertAt(ref byte[]/*!*/ array, int itemCount, int index, string/*!*/ other, int start, int count, Encoding/*!*/ encoding) {
            char[] insertChars;
            int insertedCount = GetByteCount(other, start, count, encoding, out insertChars);
            ResizeForInsertion(ref array, itemCount, index, insertedCount);
            encoding.GetBytes(insertChars, 0, insertChars.Length, array, itemCount);
            return itemCount + insertedCount;
        }

        internal static int InsertAt(ref byte[]/*!*/ array, int itemCount, int index, char[]/*!*/ other, int start, int count, Encoding/*!*/ encoding) {
            int insertedCount = encoding.GetByteCount(other, start, count);
            ResizeForInsertion(ref array, itemCount, index, insertedCount);
            encoding.GetBytes(other, start, count, array, itemCount);
            return itemCount + insertedCount;
        }

        internal static int InsertAt(ref byte[]/*!*/ array, int itemCount, int index, char other, int repeatCount, Encoding/*!*/ encoding) {
            if (repeatCount == 0) {
                return itemCount;
            }

            char[] chars = new char[] { other };
            int charSize = encoding.GetByteCount(chars, 0, 1);
            int insertedCount = charSize * repeatCount;

            ResizeForInsertion(ref array, itemCount, index, insertedCount);

            // first character:
            encoding.GetBytes(chars, 0, 1, array, itemCount);
            Fill(array, itemCount, array, itemCount + charSize, charSize, repeatCount - 1);
            return itemCount + insertedCount;
        }

        internal static int Remove<T>(ref T[]/*!*/ array, int itemCount, int start, int count) {
            T[] a;
            int remaining = itemCount - count;
            if (remaining > MinBufferSize && remaining < itemCount / 2) {
                a = new T[remaining];
                Array.Copy(array, 0, a, 0, start);
            } else {
                a = array;
            }

            Array.Copy(array, start + count, a, start, remaining - start);
            array = a;
            return remaining;
        }

        internal static T[]/*!*/ GetSlice<T>(this T[]/*!*/ array, int start, int count) {
            var copy = new T[count];
            Array.Copy(array, start, copy, 0, count);
            return copy;
        }

        internal static T[]/*!*/ GetSlice<T>(this T[]/*!*/ array, int arrayLength, int start, int count) {
            count = NormalizeCount(arrayLength, start, count);
            var copy = new T[count];
            if (count > 0) {
                Array.Copy(array, start, copy, 0, count);
            }
            return copy;
        }

        internal static string/*!*/ GetSlice(this string/*!*/ str, int start, int count) {
            count = NormalizeCount(str.Length, start, count);
            return count > 0 ? str.Substring(start, count) : String.Empty;
        }

        internal static string/*!*/ GetStringSlice(this char[]/*!*/ chars, int arrayLength, int start, int count) {
            count = NormalizeCount(arrayLength, start, count);
            return count > 0 ? new String(chars, start, count) : String.Empty;
        }

        internal static int NormalizeCount(int arrayLength, int start, int count) {
            if (count > arrayLength - start) {
                return start >= arrayLength ? 0 : arrayLength - start;
            } else {
                return count;
            }
        }

        internal static void NormalizeLastIndexOfIndices(int arrayLength, ref int start, ref int count) {
            if (start >= arrayLength) {
                count = arrayLength - (start - count + 1);
                start = arrayLength - 1;
            }
        }

        internal static int IndexOf(byte[]/*!*/ array, int arrayLength, char value, int start, int count) {
            int end = start + NormalizeCount(arrayLength, start, count);
            for (int i = start; i < end; i++) {
                if (array[i] == value) {
                    return i;
                }
            }

            return -1;
        }

        internal static int IndexOf(char[]/*!*/ array, int arrayLength, byte value, int start, int count) {
            int end = start + NormalizeCount(arrayLength, start, count);
            for (int i = start; i < end; i++) {
                if (array[i] == value) {
                    return i;
                }
            }

            return -1;
        }

        internal static int IndexOf(string/*!*/ str, byte value, int start, int count) {
            int end = start + NormalizeCount(str.Length, start, count);
            for (int i = start; i < end; i++) {
                if (str[i] == value) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Implements the same behavior as String.IndexOf on ASCII strings.
        /// </summary>
        internal static int IndexOf(byte[]/*!*/ array, int arrayLength, string/*!*/ str, int start, int count) {
            count = NormalizeCount(arrayLength, start, count);

            int finish = start + count - str.Length;
            for (int i = start; i <= finish; i++) {
                bool match = true;
                for (int j = 0; j < str.Length; j++) {
                    if (str[j] != array[i + j]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Implements the same behavior as String.IndexOf on ASCII strings.
        /// </summary>
        internal static int IndexOf(char[]/*!*/ array, int arrayLength, string/*!*/ str, int start, int count) {
            count = NormalizeCount(arrayLength, start, count);

            int finish = start + count - str.Length;
            for (int i = start; i <= finish; i++) {
                bool match = true;
                for (int j = 0; j < str.Length; j++) {
                    if (str[j] != array[i + j]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Implements the same behavior as String.IndexOf on ASCII strings.
        /// </summary>
        internal static int IndexOf(byte[]/*!*/ array, int arrayLength, byte[]/*!*/ bytes, int start, int count) {
            count = NormalizeCount(arrayLength, start, count);

            int finish = start + count - bytes.Length;
            for (int i = start; i <= finish; i++) {
                bool match = true;
                for (int j = 0; j < bytes.Length; j++) {
                    if (bytes[j] != array[i + j]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Implements the same behavior as String.IndexOf on ASCII strings.
        /// </summary>
        internal static int IndexOf(char[]/*!*/ array, int arrayLength, byte[]/*!*/ bytes, int start, int count) {
            count = NormalizeCount(arrayLength, start, count);

            int finish = start + count - bytes.Length;
            for (int i = start; i <= finish; i++) {
                bool match = true;
                for (int j = 0; j < bytes.Length; j++) {
                    if (bytes[j] != array[i + j]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Implements the same behavior as String.IndexOf on ASCII strings.
        /// </summary>
        internal static int IndexOf(string/*!*/ str, byte[]/*!*/ bytes, int start, int count) {
            count = NormalizeCount(str.Length, start, count);

            int finish = start + count - bytes.Length;
            for (int i = start; i <= finish; i++) {
                bool match = true;
                for (int j = 0; j < bytes.Length; j++) {
                    if (bytes[j] != str[i + j]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return i;
                }
            }
            return -1;
        }

        internal static int LastIndexOf(char[]/*!*/ array, int arrayLength, byte value, int start, int count) {
            NormalizeLastIndexOfIndices(arrayLength, ref start, ref count);
            int end = start - count;
            for (int i = start; i > end; i--) {
                if (array[i] == value) {
                    return i;
                }
            }

            return -1;
        }

        internal static int LastIndexOf(string/*!*/ str, byte value, int start, int count) {
            NormalizeLastIndexOfIndices(str.Length, ref start, ref count);
            int end = start - count;
            for (int i = start; i > end; i--) {
                if (str[i] == value) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Implements the same behavior as String.LastIndexOf on ASCII strings.
        /// </summary>
        internal static int LastIndexOf(byte[]/*!*/ array, int arrayLength, string/*!*/ value, int start, int count) {
            NormalizeLastIndexOfIndices(arrayLength, ref start, ref count);
            int finish = start - count + 1;

            if (value.Length == 0) {
                return start;
            }

            for (int i = start - value.Length + 1; i >= finish; i--) {
                bool match = true;
                for (int j = 0; j < value.Length; j++) {
                    if (value[j] != array[i + j]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return i;
                }
            }
            return -1;
        }

        internal static int LastIndexOf(char[]/*!*/ array, int arrayLength, string/*!*/ value, int start, int count) {
            NormalizeLastIndexOfIndices(arrayLength, ref start, ref count);
            int finish = start - count + 1;

            if (value.Length == 0) {
                return start;
            }

            for (int i = start - value.Length + 1; i >= finish; i--) {
                bool match = true;
                for (int j = 0; j < value.Length; j++) {
                    if (value[j] != array[i + j]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Implements the same behavior as String.LastIndexOf on ASCII strings.
        /// </summary>
        internal static int LastIndexOf(byte[]/*!*/ array, int arrayLength, byte[]/*!*/ value, int start, int count) {
            NormalizeLastIndexOfIndices(arrayLength, ref start, ref count);
            int finish = start - count + 1;

            if (value.Length == 0) {
                return start;
            }

            for (int i = start - value.Length + 1; i >= finish; i--) {
                bool match = true;
                for (int j = 0; j < value.Length; j++) {
                    if (value[j] != array[i + j]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Implements the same behavior as String.LastIndexOf on ASCII strings.
        /// </summary>
        internal static int LastIndexOf(char[]/*!*/ array, int arrayLength, byte[]/*!*/ value, int start, int count) {
            NormalizeLastIndexOfIndices(arrayLength, ref start, ref count);
            int finish = start - count + 1;

            if (value.Length == 0) {
                return start;
            }

            for (int i = start - value.Length + 1; i >= finish; i--) {
                bool match = true;
                for (int j = 0; j < value.Length; j++) {
                    if (value[j] != array[i + j]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Implements the same behavior as String.LastIndexOf on ASCII strings.
        /// </summary>
        internal static int LastIndexOf(string/*!*/ str, byte[]/*!*/ value, int start, int count) {
            NormalizeLastIndexOfIndices(str.Length, ref start, ref count);
            int finish = start - count + 1;

            if (value.Length == 0) {
                return start;
            }

            for (int i = start - value.Length + 1; i >= finish; i--) {
                bool match = true;
                for (int j = 0; j < value.Length; j++) {
                    if (value[j] != str[i + j]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return i;
                }
            }
            return -1;
        }

        internal static int ValueCompareTo(this byte[]/*!*/ array, int itemCount, byte[]/*!*/ other) {
            return ValueCompareTo(array, itemCount, other, other.Length);
        }

        internal static int ValueCompareTo(this byte[]/*!*/ array, int itemCount, byte[]/*!*/ other, int otherCount) {
            int min = itemCount;
            int defaultResult;
            if (min < otherCount) {
                defaultResult = -1;
            } else if (min > otherCount) {
                min = otherCount;
                defaultResult = +1;
            } else {
                defaultResult = 0;
            }

            for (int i = 0; i < min; i++) {
                if (array[i] != other[i]) {
                    return (int)array[i] - other[i];
                }
            }

            return defaultResult;
        }

        internal static int ValueCompareTo(this char[]/*!*/ array, int itemCount, char[]/*!*/ other, int otherCount) {
            int min = itemCount;
            int defaultResult;
            if (min < otherCount) {
                defaultResult = -1;
            } else if (min > otherCount) {
                min = otherCount;
                defaultResult = +1;
            } else {
                defaultResult = 0;
            }

            for (int i = 0; i < min; i++) {
                if (array[i] != other[i]) {
                    return (int)array[i] - other[i];
                }
            }

            return defaultResult;
        }

        internal static int ValueCompareTo(this char[]/*!*/ array, int itemCount, string/*!*/ other) {
            int min = itemCount, defaultResult;
            if (min < other.Length) {
                defaultResult = -1;
            } else if (min > other.Length) {
                min = other.Length;
                defaultResult = +1;
            } else {
                defaultResult = 0;
            }

            for (int i = 0; i < min; i++) {
                if (array[i] != other[i]) {
                    return (int)array[i] - other[i];
                }
            }

            return defaultResult;
        }

        internal static int ValueCompareTo(this byte[]/*!*/ array, int itemCount, string/*!*/ other) {
            int min = itemCount;
            int defaultResult;
            if (min < other.Length) {
                defaultResult = -1;
            } else if (min > other.Length) {
                min = other.Length;
                defaultResult = +1;
            } else {
                defaultResult = 0;
            }

            for (int i = 0; i < min; i++) {
                if (array[i] != other[i]) {
                    return (int)array[i] - other[i];
                }
            }

            return defaultResult;
        }

        internal static int ValueCompareTo(this string/*!*/ str, string/*!*/ other) {
            int min = str.Length, defaultResult;
            if (min < other.Length) {
                defaultResult = -1;
            } else if (min > other.Length) {
                min = other.Length;
                defaultResult = +1;
            } else {
                defaultResult = 0;
            }

            for (int i = 0; i < min; i++) {
                if (str[i] != other[i]) {
                    return (int)str[i] - other[i];
                }
            }

            return defaultResult;
        }

        internal static bool SubstringEquals(string/*!*/ name, int start, int count, string/*!*/ other) {
            if (count != other.Length) {
                return false;
            }

            for (int i = 0; i < count; i++) {
                if (name[start + i] != other[i]) {
                    return false;
                }
            }
            return true;
        }

        internal static bool ValueEquals<T>(T[] array, int arrayCount, T[] other, int otherCount) {
            Debug.Assert(arrayCount <= array.Length);
            Debug.Assert(otherCount <= other.Length);

            if (arrayCount != otherCount) {
                return false;
            }

            for (int i = 0; i < arrayCount; i++) {
                if (!Object.Equals(array[i], other[i])) {
                    return false;
                }
            }

            return true;
        }

        public static TOutput[]/*!*/ ConvertAll<TInput, TOutput>(this TInput[]/*!*/ array, Converter<TInput, TOutput>/*!*/ converter) {
            var result = new TOutput[array.Length];
            for (int i = 0; i < array.Length; i++) {
                result[i] = converter(array[i]);
            }
            return result;
        }

        internal static void AddRange(IList/*!*/ list, IList/*!*/ range) {
            Assert.NotNull(list, range);

            List<object> objList;
            IEnumerable<object> enumerableRange;
            RubyArray array;
            if ((array = list as RubyArray) != null) {
                array.AddRange(range);
            } else if ((objList = list as List<object>) != null && (enumerableRange = range as IEnumerable<object>) != null) {
                objList.AddRange(enumerableRange);
            } else {
                foreach (var item in range) {
                    list.Add(item);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void Log(string/*!*/ message, string/*!*/ category) {
#if !SILVERLIGHT
            Debug.WriteLine((object)message, category);
#endif
        }

        public static long DateTimeTicksFromStopwatch(long elapsedStopwatchTicks) {
#if !SILVERLIGHT
            if (Stopwatch.IsHighResolution) {
                return (long)(((double)elapsedStopwatchTicks) * 10000000.0 / (double)Stopwatch.Frequency);
            }
#endif
            return elapsedStopwatchTicks;
        }

        public static char ToLowerHexDigit(this int digit) {
            return (char)((digit < 10) ? '0' + digit : 'a' + digit - 10);
        }

        public static char ToUpperHexDigit(this int digit) {
            return (char)((digit < 10) ? '0' + digit : 'A' + digit - 10);
        }

        public static char ToUpperInvariant(this char c) {
            return Char.ToUpper(c, CultureInfo.InvariantCulture);
        }

        public static char ToLowerInvariant(this char c) {
            return Char.ToLower(c, CultureInfo.InvariantCulture);
        }

#if SILVERLIGHT
        public static string/*!*/ ToUpperInvariant(this string/*!*/ str) {
            return str.ToUpper(CultureInfo.InvariantCulture);
        }

        public static string/*!*/ ToLowerInvariant(this string/*!*/ str) {
            return str.ToLower(CultureInfo.InvariantCulture);
        }
#endif
        internal static IEnumerable<Expression/*!*/>/*!*/ ToExpressions(this IEnumerable<DynamicMetaObject>/*!*/ metaObjects) {
            foreach (var metaObject in metaObjects) {
                yield return metaObject != null ? metaObject.Expression : null;
            }
        }

        internal static Action<RubyModule> CloneInvocationChain(Action<RubyModule> chain) {
            if (chain == null) {
                return null;
            }

            Delegate[] delegates = chain.GetInvocationList();
            Action<RubyModule> result;
#if SILVERLIGHT
            int i = 0;
            result = (_) => {};
#else
            int i = 1;
            result = (Action<RubyModule>)delegates[0].Clone();
#endif
            for (; i < delegates.Length; i++) {
                result += (Action<RubyModule>)delegates[i];
            }

            return result;
        }

        internal static void CopyTupleFields(MutableTuple/*!*/ src, MutableTuple/*!*/ dst) {
            Debug.Assert(src.Capacity == dst.Capacity);
            for (int i = 0; i < src.Capacity; i++) {
                dst.SetValue(i, src.GetValue(i));
            }
        }

#if !SILVERLIGHT
        private sealed class CheckDecoderFallback : DecoderFallback {
            public bool HasInvalidCharacters { get; private set; }

            public CheckDecoderFallback() {
            }

            public override int MaxCharCount {
                get { return 1; }
            }

            public override DecoderFallbackBuffer CreateFallbackBuffer() {
                return new Buffer(this);
            }

            internal sealed class Buffer : DecoderFallbackBuffer {
                private readonly CheckDecoderFallback _fallback;

                public Buffer(CheckDecoderFallback/*!*/ fallback) {
                    _fallback = fallback;
                }

                public override bool Fallback(byte[]/*!*/ bytesUnknown, int index) {
                    _fallback.HasInvalidCharacters = true;
                    return true;
                }

                public override char GetNextChar() {
                    return '\0';
                }

                public override bool MovePrevious() {
                    return false;
                }

                public override int Remaining {
                    get { return 0; }
                }
            }
        }

        private sealed class CheckEncoderFallback : EncoderFallback {
            public bool HasInvalidCharacters { get; private set; }

            public CheckEncoderFallback() {
            }

            public override int MaxCharCount {
                get { return 1; }
            }

            public override EncoderFallbackBuffer CreateFallbackBuffer() {
                return new Buffer(this);
            }

            internal sealed class Buffer : EncoderFallbackBuffer {
                private readonly CheckEncoderFallback _fallback;

                public Buffer(CheckEncoderFallback/*!*/ fallback) {
                    _fallback = fallback;
                }

                public override bool Fallback(char charUnknown, int index) {
                    _fallback.HasInvalidCharacters = true;
                    return true;
                }

                public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index) {
                    _fallback.HasInvalidCharacters = true;
                    return true;
                }

                public override char GetNextChar() {
                    return '\0';
                }

                public override bool MovePrevious() {
                    return false;
                }

                public override int Remaining {
                    get { return 0; }
                }
            }
        }

        internal static bool ContainsInvalidCharacters(byte[]/*!*/ bytes, int start, int count, Encoding/*!*/ encoding) {
            var decoder = encoding.GetDecoder();
            var fallback = new CheckDecoderFallback();
            decoder.Fallback = fallback;
            decoder.GetCharCount(bytes, start, count, true);
            return fallback.HasInvalidCharacters;
        }

        internal static bool ContainsInvalidCharacters(char[]/*!*/ chars, int start, int count, Encoding/*!*/ encoding) {
            var encoder = encoding.GetEncoder();
            var fallback = new CheckEncoderFallback();
            encoder.Fallback = fallback;
            encoder.GetByteCount(chars, start, count, true);
            return fallback.HasInvalidCharacters;
        }
#else
        internal static bool ContainsInvalidCharacters(byte[]/*!*/ bytes, int start, int count, Encoding/*!*/ encoding) {
            try {
                encoding.GetCharCount(bytes, start, count);
                return true;
            } catch (DecoderFallbackException){
                return false;
            }
        }

        internal static bool ContainsInvalidCharacters(char[]/*!*/ chars, int start, int count, Encoding/*!*/ encoding) {
            try {
                encoding.GetByteCount(chars, start, count);
                return true;
            } catch (EncoderFallbackException) {
                return false;
            }
        }
#endif
    }
}

#if SILVERLIGHT
namespace System.Diagnostics {
    internal struct Stopwatch {
        public void Start() {
        }

        public void Stop() {
        }

        public static long GetTimestamp() {
            return 0;
        }
    }
}
#endif
