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
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Text;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    
    /// <summary>
    /// Array inherits from Object, mixes in Enumerable.
    /// Ruby array is basically List{object}.
    /// </summary>
    [RubyClass("Array", Extends = typeof(RubyArray), Inherits = typeof(object)), Includes(typeof(IList), Copy = true)]
    public static class ArrayOps {

        #region Constructors

        [RubyConstructor]
        public static RubyArray/*!*/ CreateArray(RubyClass/*!*/ self) {
            return new RubyArray();
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyArray/*!*/ Reinitialize(RubyContext/*!*/ context, RubyArray/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);
            self.Clear();
            return self;
        }

        [RubyConstructor]
        public static object CreateArray(ConversionStorage<Union<IList, int>>/*!*/ toAryToInt,
            BlockParam block, RubyClass/*!*/ self, [NotNull]object/*!*/ arrayOrSize) {

            var site = toAryToInt.GetSite(CompositeConversionAction.Make(toAryToInt.Context, CompositeConversion.ToAryToInt));
            var union = site.Target(site, arrayOrSize);

            if (union.First != null) {
                // block ignored
                return CreateArray(union.First);
            } else if (block != null) {
                return CreateArray(block, union.Second);
            } else {
                return CreateArray(self, union.Second, null);
            }
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static object Reinitialize(ConversionStorage<Union<IList, int>>/*!*/ toAryToInt,
            BlockParam block, RubyArray/*!*/ self, [NotNull]object/*!*/ arrayOrSize) {

            var context = toAryToInt.Context;
            RubyUtils.RequiresNotFrozen(context, self);

            var site = toAryToInt.GetSite(CompositeConversionAction.Make(context, CompositeConversion.ToAryToInt));
            var union = site.Target(site, arrayOrSize);
            
            if (union.First != null) {
                // block ignored
                return Reinitialize(self, union.First);
            } else if (block != null) {
                return Reinitialize(block, self, union.Second);
            } else {
                return ReinitializeByRepeatedValue(context, self, union.Second, null);
            }
        }

        private static RubyArray/*!*/ CreateArray(IList/*!*/ other) {
            return Reinitialize(new RubyArray(other.Count), other);
        }

        private static RubyArray/*!*/ Reinitialize(RubyArray/*!*/ self, IList/*!*/ other) {
            Assert.NotNull(self, other);
            if (other != self) {
                self.Clear();
                IListOps.AddRange(self, other);
            }
            return self;
        }

        private static object CreateArray(BlockParam/*!*/ block, int size) {
            return Reinitialize(block, new RubyArray(), size);
        }

        [RubyConstructor]
        public static RubyArray/*!*/ CreateArray(BlockParam/*!*/ block, RubyClass/*!*/ self, [DefaultProtocol]int size, object value) {
            return Reinitialize(block, new RubyArray(), size, value);
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyArray/*!*/ Reinitialize(BlockParam/*!*/ block, RubyArray/*!*/ self, int size, object value) {
            block.RubyContext.ReportWarning("block supersedes default value argument");
            Reinitialize(block, self, size);
            return self;
        }

        private static object Reinitialize(BlockParam/*!*/ block, RubyArray/*!*/ self, int size) {
            if (size < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size");
            }

            self.Clear();
            for (int i = 0; i < size; i++) {
                object item;
                if (block.Yield(i, out item)) {
                    return item;
                }
                self.Add(item);
            }

            return self;
        }

        [RubyConstructor]
        public static RubyArray/*!*/ CreateArray(RubyClass/*!*/ self, [DefaultProtocol]int size, object value) {
            if (size < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size");
            }

            return new RubyArray().AddMultiple(size, value);
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyArray/*!*/ ReinitializeByRepeatedValue(RubyContext/*!*/ context, RubyArray/*!*/ self, [DefaultProtocol]int size, object value) {
            RubyUtils.RequiresNotFrozen(context, self);
            if (size < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size");
            }

            self.Clear();
            self.AddMultiple(size, value);

            return self;
        }

        [RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ MakeArray(RubyClass/*!*/ self, params object[] args) {
            // neither "new" nor "initialize" is called:
            RubyArray result = RubyArray.CreateInstance(self);
            foreach (object obj in args) {
                result.Add(obj);
            }
            return result;
        }

        #endregion

        [RubyMethod("to_a")]
        public static RubyArray/*!*/ ToArray(RubyArray/*!*/ self) {
            return self is RubyArray.Subclass ? new RubyArray(self) : self;
        }

        [RubyMethod("to_ary")]
        public static RubyArray/*!*/ ToAry(RubyArray/*!*/ self) {
            return self;
        }

        #region class FormatDirective is used by Array.pack and String.unpack

        internal struct FormatDirective {
            internal readonly char Directive;
            internal readonly int? Count;
            private static readonly Dictionary<char, char> _native;

            static FormatDirective() {
                bool is64bit = (IntPtr.Size == 8);
                _native = new Dictionary<char, char>(6);
                _native['s'] = 's';
                _native['S'] = 'S';
                _native['i'] = 'i';
                _native['I'] = 'I';
                _native['l'] = is64bit ? 'i' : 'q';
                _native['L'] = is64bit ? 'I' : 'Q';
            }

            internal FormatDirective(char directive, int? count) {
                Directive = directive;
                Count = count;
            }

            internal static IEnumerable<FormatDirective>/*!*/ Enumerate(string format) {
                for (int i = 0; i < format.Length; i++) {
                    char c = format[i];
                    if (!Char.IsLetter(c) && c != '@') {
                        continue;
                    }
                    i++;
                    int? count = 1;
                    char c2 = (i < format.Length) ? format[i] : '\0';
                    if (c2 == '_') {
                        char tmp;
                        if (!_native.TryGetValue(c, out tmp)) {
                            throw RubyExceptions.CreateArgumentError("'_' allowed only after types sSiIlL");
                        }
                        c = tmp;
                        i++;
                        c2 = (i < format.Length) ? format[i] : '\0';
                    }
                    if (Char.IsDigit(c2)) {
                        int pos1 = i;
                        i++;
                        while (i < format.Length && Char.IsDigit(format[i])) {
                            i++;
                        }
                        count = Int32.Parse(format.Substring(pos1, (i - pos1)));
                        i--;
                    } else if (c2 == '*') {
                        count = null;
                    } else {
                        i--;
                    }
                    
                    yield return new FormatDirective(c, count);
                }
            }
        }

        #endregion

        #region pack

        [RubyMethod("pack")]
        public static MutableString/*!*/ Pack(
            ConversionStorage<IntegerValue>/*!*/ integerConversion, 
            ConversionStorage<MutableString>/*!*/ stringCast, 
            RubyContext/*!*/ context, RubyArray/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ format) {

            using (MutableStringStream stream = new MutableStringStream()) {
                BinaryWriter writer = new BinaryWriter(stream);
                int i = 0;
                foreach (FormatDirective directive in FormatDirective.Enumerate(format.ConvertToString())) {
                    int remaining = (self.Count - i);
                    int count = directive.Count.HasValue ? directive.Count.Value : remaining;
                    if (count > remaining) {
                        count = remaining;
                    }

                    MutableString str;
                    switch (directive.Directive) {
                        case '@':
                            count = 0;
                            stream.Position = directive.Count.HasValue ? directive.Count.Value : 1;
                            break;

                        case 'A':
                        case 'a':
                        case 'Z':
                            count = 1;
                            str = Protocols.CastToString(stringCast, self[i]);
                            char[] cstr = (str == null) ? new char[0] : str.ToString().ToCharArray();
                            int dataLen;
                            int paddedLen;
                            if (directive.Count.HasValue) {
                                paddedLen = directive.Count.Value;
                                dataLen = Math.Min(cstr.Length, paddedLen);
                            } else {
                                paddedLen = cstr.Length;
                                dataLen = cstr.Length;
                            }
                            writer.Write(cstr, 0, dataLen);
                            if (paddedLen > dataLen) {
                                byte fill = (directive.Directive == 'A') ? (byte)' ' : (byte)0;
                                for (int j = 0; j < (paddedLen - dataLen); j++) {
                                    writer.Write(fill);
                                }
                            }
                            if (directive.Directive == 'Z' && !directive.Count.HasValue) {
                                writer.Write((byte)0);
                            }
                            break;

                        case 'Q':
                        case 'q':
                            for (int j = 0; j < count; j++) {
                                writer.Write(Protocols.CastToUInt64Unchecked(integerConversion, self[i + j]));
                            }
                            break;

                        case 'l':
                        case 'i':
                            for (int j = 0; j < count; j++) {
                                writer.Write(unchecked((int)Protocols.CastToUInt32Unchecked(integerConversion, self[i + j])));
                            }
                            break;

                        case 'L':
                        case 'I':
                            for (int j = 0; j < count; j++) {
                                writer.Write(Protocols.CastToUInt32Unchecked(integerConversion, self[i + j]));
                            }
                            break;

                        case 'N': // unsigned 4-byte big-endian
                            WriteUInt32(integerConversion, writer, self, i, count, BitConverter.IsLittleEndian);
                            break;

                        case 'n': // unsigned 2-byte big-endian
                            WriteUInt16(integerConversion, writer, self, i, count, BitConverter.IsLittleEndian);
                            break;

                        case 'V': // unsigned 4-byte little-endian
                            WriteUInt32(integerConversion, writer, self, i, count, !BitConverter.IsLittleEndian);
                            break;

                        case 'v': // unsigned 2-byte little-endian
                            WriteUInt16(integerConversion, writer, self, i, count, !BitConverter.IsLittleEndian);
                            break;

                        case 's':
                            for (int j = 0; j < count; j++) {
                                writer.Write(unchecked((short)Protocols.CastToUInt32Unchecked(integerConversion, self[i + j])));
                            }
                            break;

                        case 'S':
                            for (int j = 0; j < count; j++) {
                                writer.Write(unchecked((ushort)Protocols.CastToUInt32Unchecked(integerConversion, self[i + j])));
                            }
                            break;

                        case 'c':
                            for (int j = 0; j < count; j++) {
                                writer.Write(unchecked((sbyte)Protocols.CastToUInt32Unchecked(integerConversion, self[i + j])));
                            }
                            break;

                        case 'C':
                            for (int j = 0; j < count; j++) {
                                writer.Write(unchecked((byte)Protocols.CastToUInt32Unchecked(integerConversion, self[i + j])));
                            }
                            break;

                        case 'm':
                            count = 1;
                            str = Protocols.CastToString(stringCast, self[i]);
                            char[] base64 = Convert.ToBase64String(str.ToByteArray()).ToCharArray();
                            for (int j = 0; j < base64.Length; j += 60) {
                                int len = base64.Length - j;
                                if (len > 60) {
                                    len = 60;
                                }
                                writer.Write(base64, j, len);
                                writer.Write('\n');
                            }
                            break;

                        case 'U':
                            char[] buffer = new char[count];
                            for (int j = 0; j < count; j++) {
                                buffer[j] = unchecked((char)Protocols.CastToUInt32Unchecked(integerConversion, self[i + j]));
                            }
                            writer.Write(Encoding.UTF8.GetBytes(buffer));
                            break;

                        case 'X':
                            count = 0;
                            int len3 = directive.Count.HasValue ? directive.Count.Value : 0;
                            if (len3 > stream.Position) {
                                throw RubyExceptions.CreateArgumentError("X outside of string");
                            }
                            stream.Position -= len3;
                            break;

                        case 'x':
                            count = 0;
                            int len4 = directive.Count.HasValue ? directive.Count.Value : 0;
                            for (int j = 0; j < len4; j++) {
                                writer.Write((byte)0);
                            }
                            break;

                        case 'h':
                        case 'H':
                            // MRI skips null, unlike in "m" directive:
                            if (self[i] != null) {
                                str = Protocols.CastToString(stringCast, self[i]);
                                FromHex(writer, str, directive.Count ?? str.GetByteCount(), directive.Directive == 'h');
                            }
                            break;

                        default:
                            throw RubyExceptions.CreateArgumentError(
                                String.Format("Unknown format directive '{0}'", directive.Directive));
                    }
                    i += count;
                    if (i >= self.Count) {
                        break;
                    }
                }
                stream.SetLength(stream.Position);
                return stream.String;
            }
        }

        private static void WriteUInt64(ConversionStorage<IntegerValue>/*!*/ integerConversion, 
            BinaryWriter/*!*/ writer, RubyArray/*!*/ self, int i, int count, bool swap) {
            for (int j = 0; j < count; j++) {
                uint n = Protocols.CastToUInt32Unchecked(integerConversion, self[i + j]);
                if (swap) {
                    writer.Write((byte)(n >> 24));
                    writer.Write((byte)((n >> 16) & 0xff));
                    writer.Write((byte)((n >> 8) & 0xff));
                    writer.Write((byte)(n & 0xff));
                } else {
                    writer.Write(n);
                }
            }
        }
        
        private static void WriteUInt32(ConversionStorage<IntegerValue>/*!*/ integerConversion, 
            BinaryWriter/*!*/ writer, RubyArray/*!*/ self, int i, int count, bool swap) {
            for (int j = 0; j < count; j++) {
                uint n = Protocols.CastToUInt32Unchecked(integerConversion, self[i + j]);
                if (swap) {
                    writer.Write((byte)(n >> 24));
                    writer.Write((byte)((n >> 16) & 0xff));
                    writer.Write((byte)((n >> 8) & 0xff));
                    writer.Write((byte)(n & 0xff));
                } else {
                    writer.Write(n);
                }
            }
        }

        private static void WriteUInt16(ConversionStorage<IntegerValue>/*!*/ integerConversion, 
            BinaryWriter/*!*/ writer, RubyArray/*!*/ self, int i, int count, bool swap) {
            for (int j = 0; j < count; j++) {
                uint n = Protocols.CastToUInt32Unchecked(integerConversion, self[i + j]);
                if (swap) {
                    writer.Write((byte)((n >> 8) & 0xff));
                    writer.Write((byte)(n & 0xff));
                } else {
                    writer.Write((ushort)(n & 0xffff));
                }
            }
        }

        private static void FromHex(BinaryWriter/*!*/ writer, MutableString/*!*/ str, int nibbleCount, bool swap) {
            int maxCount = Math.Min(nibbleCount, str.GetByteCount());
            for (int i = 0, j = 0; i < (nibbleCount + 1) / 2; i++, j += 2) {
                int hiNibble = (j < maxCount) ? FromHexDigit(str.GetByte(j)) : 0;
                int loNibble = (j + 1 < maxCount) ? FromHexDigit(str.GetByte(j + 1)) : 0;
                Debug.Assert(hiNibble >= 0 && hiNibble < 16 && loNibble >= 0 && loNibble < 16);

                int c = (swap) ? (loNibble << 4) | hiNibble : (hiNibble << 4) | loNibble;
                writer.Write((byte)c);
            }
        }

        // hexa digits -> values
        private static int FromHexDigit(int c) {
            c = Tokenizer.ToDigit(c);
            if (c < 16) return c;
            
            // MRI does some magic here:
            throw new NotSupportedException("directives `H' and `h' expect hexadecimal digits in input string");
        }

        #endregion

        #region sort!, sort
        
        private sealed class BreakException : Exception {
        }

        [RubyMethod("sort")]
        public static RubyArray/*!*/ Sort(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam block, RubyArray/*!*/ self) {

            RubyArray result = self.CreateInstance();
            IListOps.Replace(comparisonStorage.Context, result, self);
            return SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, block, result);
        }

        [RubyMethod("sort!")]
        public static RubyArray/*!*/ SortInPlace(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,            
            BlockParam block, RubyArray/*!*/ self) {

            var context = comparisonStorage.Context;
            RubyUtils.RequiresNotFrozen(context, self);

            // TODO: this does more comparisons (and in a different order) than
            // Ruby's sort. Also, control flow won't work because List<T>.Sort wraps
            // exceptions from the comparer & rethrows. We need to rewrite a version of quicksort
            // that behaves like Ruby's sort.
            if (block == null) {
                self.Sort(delegate(object x, object y) {
                    return Protocols.Compare(comparisonStorage, lessThanStorage, greaterThanStorage, x, y);
                });
            } else {
                try {
                    self.Sort(delegate(object x, object y) {
                        object result;
                        if (block.Yield(x, y, out result)) {
                            // TODO: this doesn't work
                            throw new BreakException();
                        }

                        if (result == null) {
                            throw RubyExceptions.MakeComparisonError(context, x, y);
                        }

                        return Protocols.ConvertCompareResult(lessThanStorage, greaterThanStorage, result);
                    });
                } catch (BreakException) {
                }
            }

            return self;
        }
        #endregion

        #region reverse!, reverse_each

        [RubyMethod("reverse!")]
        public static RubyArray/*!*/ InPlaceReverse(RubyContext/*!*/ context, RubyArray/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);
            self.Reverse();
            return self;
        }

        [RubyMethod("reverse_each")]
        public static object ReverseEach(RubyContext/*!*/ context, BlockParam block, RubyArray/*!*/ self) {
            Assert.NotNull(context, self);

            if (self.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            for (int originalSize = self.Count, i = originalSize - 1; i >= 0; i--) {
                object result;
                if (block.Yield(self[i], out result)) {
                    return result;
                }
                if (self.Count < originalSize) {
                    i = originalSize - i - 1 + self.Count;
                    originalSize = self.Count;
                }
            }
            return self;
        }

        #endregion
    }
}
