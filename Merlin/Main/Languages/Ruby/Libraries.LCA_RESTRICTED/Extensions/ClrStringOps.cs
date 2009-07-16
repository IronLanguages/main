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

using System.Text;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using System.Collections;
using System;
using IronRuby.Compiler;
using System.Runtime.InteropServices;
using Microsoft.Scripting;

namespace IronRuby.Builtins {

    [RubyClass(Extends = typeof(string), Restrictions = ModuleRestrictions.None)]
    [Includes(typeof(ClrString), typeof(Enumerable), typeof(Comparable))]
    public static class ClrStringOps {
        [RubyConstructor]
        public static string/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol]MutableString/*!*/ str) {
            return str.ToString();
        }

        [RubyConstructor]
        public static string/*!*/ Create(RubyClass/*!*/ self, char c, [DefaultParameterValue(1)]int repeatCount) {
            return new String(c, repeatCount);
        }

        [RubyConstructor]
        public static string/*!*/ Create(RubyClass/*!*/ self, [NotNull]char[]/*!*/ chars) {
            return new String(chars);
        }

        [RubyConstructor]
        public static string/*!*/ Create(RubyClass/*!*/ self, [NotNull]char[]/*!*/ chars, int startIndex, int length) {
            return new String(chars, startIndex, length);
        }
    }

    /// <summary>
    /// Mixed into System::String and System::Char.
    /// </summary>
    [RubyModule("String", DefineIn = typeof(IronRubyOps.ClrOps))]
    public static class ClrString {
        #region %, *, +

        [RubyMethod("%")]
        public static string/*!*/ Format(StringFormatterSiteStorage/*!*/ storage, string/*!*/ self, object arg) {
            IList args = arg as IList ?? new object[] { arg };
            StringFormatter formatter = new StringFormatter(storage, self, args);
            return formatter.Format().ToString();
        }

        [RubyMethod("*")]
        public static string/*!*/ Repeat(string/*!*/ self, [DefaultProtocol]int times) {
            if (times < 0) {
                throw RubyExceptions.CreateArgumentError("negative argument");
            }

            var result = new StringBuilder();
            for (int i = 0; i < times; i++) {
                result.Append(self);
            }

            return result.ToString();
        }

        [RubyMethod("+")]
        public static string/*!*/ Concatenate(string/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ other) {
            return self + other.ToString();
        }

        #endregion

        #region <=>, ==, ===

        [RubyMethod("<=>")]
        public static int Compare(string/*!*/ self, [NotNull]string/*!*/ other) {
            return Math.Sign(self.CompareTo(other));
        }

        [RubyMethod("<=>")]
        public static int Compare(string/*!*/ self, [NotNull]MutableString/*!*/ other) {
            // TODO: do not create MS
            return -Math.Sign(other.CompareTo(MutableString.Create(self, RubyEncoding.UTF8)));
        }

        [RubyMethod("<=>")]
        public static object Compare(BinaryOpStorage/*!*/ comparisonStorage, RespondToStorage/*!*/ respondToStorage, string/*!*/ self, object other) {
            return MutableStringOps.Compare(comparisonStorage, respondToStorage, self, other);
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool StringEquals(string/*!*/ lhs, [NotNull]string/*!*/ rhs) {
            return lhs.Equals(rhs);
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool StringEquals(string/*!*/ lhs, [NotNull]MutableString/*!*/ rhs) {
            // TODO: do not create MS
            return rhs.Equals(MutableString.Create(lhs, RubyEncoding.UTF8));
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool Equals(RespondToStorage/*!*/ respondToStorage, BinaryOpStorage/*!*/ equalsStorage, string/*!*/ self, object other) {
            return MutableStringOps.Equals(respondToStorage, equalsStorage, self, other);
        }

        #endregion

        #region  TODO: [], slice

        #endregion

        #region TODO: casecmp, capitalize, downcase, swapcase, upcase

        #endregion

        #region TODO: center

        #endregion

        #region TODO: chomp, chop
        #endregion

        #region dump, inspect

        [RubyMethod("inspect", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ Inspect(string/*!*/ self) {
            return MutableString.Create(MutableString.AppendUnicodeRepresentation(
                new StringBuilder().Append('\''), self, false, false, '\'', -1).Append('\'').ToString()
            );
        }

        [RubyMethod("dump", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ Dump(string/*!*/ self) {
            return MutableString.Create(MutableString.AppendUnicodeRepresentation(
                new StringBuilder().Append('\''), self, false, true, '\'', -1).Append('\'').ToString()
            );
        }

        #endregion

        #region TODO: each, each_byte, each_line

        #endregion

        #region empty?, size, length, encoding

        [RubyMethod("empty?")]
        public static bool IsEmpty(string/*!*/ self) {
            return self.Length == 0;
        }

        [RubyMethod("size")]
        public static int GetLength(string/*!*/ self) {
            return self.Length;
        }

        [RubyMethod("encoding")]
        public static RubyEncoding/*!*/ GetEncoding(string/*!*/ self) {
            return RubyEncoding.UTF8;
        }

        #endregion

        #region TODO: sub, gsub
        #endregion

        #region TODO: index, rindex
        #endregion

        #region TODO: delete
        #endregion

        #region TODO: count
        #endregion

        #region TODO: include?

        #endregion

        #region TODO: insert

        #endregion

        #region TODO: match
        #endregion

        #region TODO: scan
        #endregion

        #region TODO: succ
        #endregion

        #region TODO: split
        #endregion

        #region TODO: strip, lstrip, rstrip
        #endregion

        #region TODO: squeeze
        #endregion

        #region to_i, hex, oct

        [RubyMethod("to_i")]
        public static object/*!*/ ToInteger(string/*!*/ self, [DefaultProtocol, DefaultParameterValue(10)]int @base) {
            if (@base == 1 || @base < 0 || @base > 36) {
                throw RubyExceptions.CreateArgumentError(String.Format("illegal radix {0}", @base));
            }
            return Tokenizer.ParseInteger(self, @base).ToObject();
        }

        [RubyMethod("hex")]
        public static object/*!*/ ToIntegerHex(string/*!*/ self) {
            return Tokenizer.ParseInteger(self, 16).ToObject();
        }

        [RubyMethod("oct")]
        public static object/*!*/ ToIntegerOctal(string/*!*/ self) {
            return Tokenizer.ParseInteger(self, 8).ToObject();
        }

        #endregion

        #region to_f, to_s, to_str, to_clr_string, to_sym, intern

        [RubyMethod("to_f")]
        public static double ToDouble(string/*!*/ self) {
            double result;
            bool complete;
            return Tokenizer.TryParseDouble(self, out result, out complete) ? result : 0.0;
        }

        [RubyMethod("to_str", RubyMethodAttributes.PublicInstance)]
        [RubyMethod("to_s", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ ToStr(string/*!*/ self) {
            return MutableString.Create(self, RubyEncoding.UTF8);
        }

        [RubyMethod("to_clr_string", RubyMethodAttributes.PublicInstance)]
        public static string/*!*/ ToClrString(string/*!*/ self) {
            return self;
        }

        [RubyMethod("to_sym")]
        [RubyMethod("intern")]
        public static SymbolId ToSymbol(string/*!*/ self) {
            if (self.Length == 0) {
                throw RubyExceptions.CreateArgumentError("interning empty string");
            }

            // Cannot convert a string that contains null to a symbol
            if (self.IndexOf('\0') >= 0) {
                throw RubyExceptions.CreateArgumentError("symbol string may not contain '\0'");
            }

            return SymbolTable.StringToId(self);
        }

        #endregion

        #region TODO: upto
        #endregion

        #region TODO: replace, reverse
        #endregion

        #region TODO: tr, tr_s
        #endregion

        #region TODO: ljust
        #endregion

        #region TODO: rjust
        #endregion

        #region TODO: unpack
        #endregion

        #region TODO: sum
        #endregion

        #region TODO: Encodings (1.9)

        //ascii_only?
        //bytes
        //bytesize
        //chars
        //codepoints
        //each_byte
        //each_char
        //each_codepoint
        //encode
        //encoding
        //valid_esncoding?

        #endregion

        #region method_missing

        [RubyMethod("method_missing", RubyMethodAttributes.PrivateInstance)]
        [RubyStackTraceHidden]
        public static object MethodMissing(RubyScope/*!*/ scope, BlockParam block, string/*!*/ self, SymbolId symbol, [NotNull]params object[]/*!*/ args) {
            string name = SymbolTable.IdToString(symbol);

            if (name.EndsWith("=") || name.EndsWith("!")) {
                throw new InvalidOperationException(String.Format("Mutating method `{0}' called for an immutable string (System::String)", name));
            }

            // TODO: forward to MutableString until we implement the methods here:
            return KernelOps.SendMessageOpt(scope, block, ToStr(self), name, args);
        }

        #endregion
    }
}
