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

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Compiler;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {

    [RubyClass("Integer"), Includes(typeof(Precision))]
    public class Integer : Numeric {
        public Integer(RubyClass/*!*/ cls) 
            : base(cls) { 
        }

        #region induced_from

        /// <summary>
        /// Convert obj to an Integer, where obj is Fixnum
        /// </summary>
        /// <remarks>Just returns the Fixnum</remarks>
        [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
        public static object InducedFrom(RubyClass/*!*/ self, int obj) {
            return obj;
        }

        /// <summary>
        /// Convert obj to an Integer, where obj is Bignum
        /// </summary>
        /// <remarks>Just returns the Bignum</remarks>
        [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
        public static object InducedFrom(RubyClass/*!*/ self, [NotNull]BigInteger/*!*/ obj) {
            return obj;
        }

        /// <summary>
        /// Convert obj to an Integer, where obj is Float
        /// </summary>
        [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
        public static object InducedFrom(UnaryOpStorage/*!*/ toiStorage, RubyClass/*!*/ self, double obj) {
            var site = toiStorage.GetCallSite("to_i");
            return site.Target(site, obj);
        }

        /// <summary>
        /// Convert obj to an Integer, where obj is not Fixnum, Bignum or Float
        /// </summary>
        /// <remarks>Just throws an error</remarks>
        /// <exception cref="InvalidOperationException">Assumption is object cannot be induced to Integer</exception>
        [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
        public static int InducedFrom(RubyClass/*!*/ self, object obj) {
            throw RubyExceptions.CreateTypeError("failed to convert {0} into Integer", self.Context.GetClassDisplayName(obj));
        }

        #endregion

        #region to_i, to_int, floor, ceil, round, truncate

        /// <summary>
        /// As self is already an Integer, just return self.
        /// </summary>
        [RubyMethod("to_i")]
        [RubyMethod("to_int")]
        [RubyMethod("floor")]
        [RubyMethod("ceil")]
        [RubyMethod("round")]
        [RubyMethod("truncate")]
        public static object ToInteger(object/*!*/ self) {
            return self;
        }

        #endregion

        #region numerator/ord, denominator, to_r, rationalize

        [RubyMethod("numerator")]
        [RubyMethod("ord")]
        public static object/*!*/ Numerator(object/*!*/ self) {
            return self;
        }

        [RubyMethod("denominator")]
        public static object/*!*/ Denominator(object/*!*/ self) {
            return ClrInteger.One;
        }

        [RubyMethod("to_r")]
        [RubyMethod("rationalize")]
        public static object ToRational(CallSiteStorage<Func<CallSite, object, object, object, object>>/*!*/ toRational, RubyScope/*!*/ scope, object/*!*/ self) {
           // TODO: reimplement Rational
            return KernelOps.ToRational(toRational, scope, self, self, ClrInteger.One);
        }

        #endregion

        #region chr

        [RubyMethod("chr")]
        public static MutableString/*!*/ ToChr(ConversionStorage<MutableString>/*!*/ toStr, [DefaultProtocol]int self, 
            [Optional]object encoding) {

            RubyEncoding enc;
            RubyEncoding resultEncoding;
            if (encoding != Missing.Value) {
                resultEncoding = enc = Protocols.ConvertToEncoding(toStr, encoding);
            } else {
                enc = toStr.Context.DefaultInternalEncoding ?? RubyEncoding.Ascii;
                resultEncoding = (self <= 0x7f) ? RubyEncoding.Ascii : (self <= 0xff) ? RubyEncoding.Binary : enc;
            }

            return ToChr(enc, resultEncoding, self);
        }

        internal static MutableString/*!*/ ToChr(RubyEncoding/*!*/ encoding, RubyEncoding/*!*/ resultEncoding, int codepoint) {
            if (codepoint < 0) {
                throw RubyExceptions.CreateRangeError("{0} out of char range", codepoint);
            }

            switch (encoding.CodePage) {
                case RubyEncoding.CodePageUTF7:
                case RubyEncoding.CodePageUTF8:
                case RubyEncoding.CodePageUTF16BE:
                case RubyEncoding.CodePageUTF16LE:
                case RubyEncoding.CodePageUTF32BE:
                case RubyEncoding.CodePageUTF32LE:
                    if (codepoint > 0x10ffff) {
                        throw RubyExceptions.CreateRangeError("{0} is not a valid Unicode code point (0..0x10ffff)", codepoint);
                    }
                    return MutableString.CreateMutable(Tokenizer.UnicodeCodePointToString(codepoint), resultEncoding);

                case RubyEncoding.CodePageSJIS:
                    if (codepoint >= 0x81 && codepoint <= 0x9f || codepoint >= 0xe0 && codepoint <= 0xfc) {
                        throw RubyExceptions.CreateArgumentError("invalid codepoint 0x{0:x2} in Shift_JIS", codepoint);
                    }
                    goto default;

                case RubyEncoding.CodePageEUCJP:
                    // MRI's bahavior is strange - bug?
                    if (codepoint >= 0x80) {
                        throw RubyExceptions.CreateRangeError("{0} out of char range", codepoint);
                    }
                    goto default;

                default:
                    if (codepoint <= 0xff) {
                        return MutableString.CreateBinary(new[] { (byte)codepoint }, resultEncoding);
                    }
                    if (encoding.IsDoubleByteCharacterSet) {
                        if (codepoint <= 0xffff) {
                            return MutableString.CreateBinary(new[] { (byte)(codepoint >> 8), (byte)(codepoint & 0xff) }, resultEncoding);
                        }
                        throw RubyExceptions.CreateRangeError("{0} out of char range", codepoint);
                    }
                    if (encoding.IsSingleByteCharacterSet) {
                        throw RubyExceptions.CreateRangeError("{0} out of char range", codepoint);
                    }
                    throw new NotSupportedException(RubyExceptions.FormatMessage("Encoding {0} code points not supported", encoding));
            }
        }

        #endregion

        #region integer?, odd?, even?

        /// <summary>
        /// Always returns true.
        /// </summary>
        /// <returns>true</returns>
        [RubyMethod("integer?")]
        public static new bool IsInteger(object/*!*/ self) {
            return true;
        }

        [RubyMethod("odd?")]
        public static bool IsOdd(int self) {
            return (self & 1) != 0;
        }

        [RubyMethod("odd?")]
        public static bool IsOdd(BigInteger/*!*/ self) {
            return !self.IsEven;
        }

        [RubyMethod("even?")]
        public static bool IsEven(int self) {
            return (self & 1) == 0;
        }

        [RubyMethod("even?")]
        public static bool IsEven(BigInteger/*!*/ self) {
            return self.IsEven;
        }

        #endregion

        #region next, succ, pred

        /// <summary>
        /// Returns the Integer equal to self + 1, where self is Fixnum.
        /// </summary>
        /// <returns>May return Fixnum or Bignum depending on overflow/underflow.</returns>
        /// <example>
        /// 1.next      #=> 2
        /// (-1).next   #=> 0
        /// </example>
        [RubyMethod("succ")]
        [RubyMethod("next")]
        public static object Next(int self) {
            return ClrInteger.Add(self, 1);
        }

        /// <summary>
        /// Returns the Integer equal to self + 1, where self is not Fixnum (probably Bignum).
        /// </summary>
        /// <returns>May return Fixnum or Bignum depending on overflow/underflow.</returns>
        /// <remarks>Dynamically invokes "+" operator to get next value.</remarks>
        [RubyMethod("succ")]
        [RubyMethod("next")]
        public static object Next(BinaryOpStorage/*!*/ addStorage, object/*!*/ self) {
            var site = addStorage.GetCallSite("+");
            return site.Target(site, self, ClrInteger.One);
        }

        [RubyMethod("pred")]
        public static object Pred(int self) {
            return ClrInteger.Subtract(self, 1);
        }

        [RubyMethod("pred")]
        public static object Pred(BinaryOpStorage/*!*/ subStorage, object/*!*/ self) {
            var site = subStorage.GetCallSite("-");
            return site.Target(site, self, ClrInteger.One);
        }

        #endregion

        #region times

        /// <summary>
        /// Iterates block self times, passing in values from zero to self - 1, where self is Fixnum.
        /// </summary>
        /// <returns>self</returns>
        [RubyMethodAttribute("times")]
        public static object Times(BlockParam/*!*/ block, int self) {
            return (block != null) ? TimesImpl(block, self) : new Enumerator((_, innerBlock) => TimesImpl(innerBlock, self));
        }

        private static object TimesImpl(BlockParam/*!*/ block, int self) {
            int i = 0;
            while (i < self) {
                object result;
                if (block.Yield(i, out result)) {
                    return result;
                }
                i++;
            }

            return self;
        }

        /// <summary>
        /// Iterates block self times, passing in values from zero to self - 1, where self is not Fixnum (probably Bignum).
        /// </summary>
        /// <returns>self</returns>
        /// <remarks>
        /// Dynamically invokes "+" operator to find next item.
        /// Dynamically invokes "&lt;" operator to check if we have reached self - 1.
        /// </remarks>
        [RubyMethodAttribute("times")]
        public static object Times(BinaryOpStorage/*!*/ lessThanStorage, BinaryOpStorage/*!*/ addStorage, BlockParam block, object/*!*/ self) {
            return (block != null) ? TimesImpl(lessThanStorage, addStorage, block, self) : 
                new Enumerator((_, innerBlock) => TimesImpl(lessThanStorage, addStorage, innerBlock, self));
        }

        public static object TimesImpl(BinaryOpStorage/*!*/ lessThanStorage, BinaryOpStorage/*!*/ addStorage, BlockParam/*!*/ block, object/*!*/ self) {
            object i = 0;
            var lessThan = lessThanStorage.GetCallSite("<");
            while (RubyOps.IsTrue(lessThan.Target(lessThan, i, self))) {
                object result;
                if (block.Yield(i, out result)) {
                    return result;
                }

                var add = addStorage.GetCallSite("+");
                i = add.Target(add, i, 1);
            }
            return self;
        }

        #endregion

        #region upto

        /// <summary>
        /// Iterates block, passing in integer values from self up to and including other, where both self and other are Fixnum. 
        /// </summary>
        /// <returns>self</returns>
        /// <example>
        ///    5.upto(10) { |i| print i, " " }
        /// produces: 
        ///    5 6 7 8 9 10
        /// </example>
        /// <remarks>
        /// Since both self and other are Fixnum then this algorithm doesn't need to worry about overflowing into Bignum.
        /// </remarks>
        [RubyMethod("upto")]
        public static object UpTo(BlockParam block, int self, int other) {
            return (block != null) ? UpToImpl(block, self, other) : new Enumerator((_, innerBlock) => UpToImpl(innerBlock, self, other));
        }

        private static object UpToImpl(BlockParam block, int self, int other) {
            int i = self;
            while (i <= other) {
                object result;
                if (block.Yield(i, out result)) {
                    return result;
                }
                i++;
            }
            return self;
        }

        /// <summary>
        /// Iterates block, passing in integer values from self up to and including other, where both self and other are Fixnum. 
        /// </summary>
        /// <returns>self</returns>
        /// <remarks>
        /// Dynamically invokes "+" operator to find next item down.
        /// Dynamically invokes "&gt;" operator and takes the negation to see if we have reached the bottom.
        /// This approach automatically deals with Floats and overflow/underflow between Fixnum and Bignum.
        /// </remarks>
        [RubyMethod("upto")]
        public static object UpTo(BinaryOpStorage/*!*/ greaterThanStorage, BinaryOpStorage/*!*/ addStorage, 
            BlockParam block, object/*!*/ self, object other) {
            return (block != null) ? UpToImpl(greaterThanStorage, addStorage, block, self, other) :
                new Enumerator((_, innerBlock) => UpToImpl(greaterThanStorage, addStorage, innerBlock, self, other));
        }

        private static object UpToImpl(BinaryOpStorage/*!*/ greaterThanStorage, BinaryOpStorage/*!*/ addStorage,
            BlockParam/*!*/ block, object/*!*/ self, object other) {

            object i = self;
            object compare = null;
            var greaterThan = greaterThanStorage.GetCallSite(">");
            while (RubyOps.IsFalse(compare)) {
                // Rather than test i <= other we test !(i > other)
                compare = greaterThan.Target(greaterThan, i, other);

                // If the comparison failed (i.e. returned null) then we throw an error.
                if (compare == null) {
                    throw RubyExceptions.MakeComparisonError(greaterThanStorage.Context, i, other);
                }

                // If the comparison worked but returned false then we carry on
                if (RubyOps.IsFalse(compare)) {
                    object result;
                    if (block.Yield(i, out result)) {
                        return result;
                    }

                    var add = addStorage.GetCallSite("+");
                    i = add.Target(add, i, 1);
                }
            }
            return self;
        }

        #endregion

        #region downto

        /// <summary>
        /// Iterates block, passing decreasing values from self down to and including other, where both self and other are Fixnum.
        /// </summary>
        /// <returns>self</returns>
        /// <example>
        /// 5.downto(1) { |n| print n, ".. " }
        ///   print "  Liftoff!\n"
        /// produces: 
        /// 5.. 4.. 3.. 2.. 1..   Liftoff!
        /// </example>
        /// <remarks>
        /// Since both self and other are Fixnum then this algorithm doesn't need to worry about overflowing into Bignum.
        /// </remarks>
        [RubyMethod("downto")]
        public static object DownTo(BlockParam block, int self, int other) {
            return (block != null) ? DownToImpl(block, self, other) : new Enumerator((_, innerBlock) => DownToImpl(innerBlock, self, other));
        }

        private static object DownToImpl(BlockParam/*!*/ block, int self, int other) {
            int i = self;
            while (i >= other) {
                object result;
                if (block.Yield(i, out result)) {
                    return result;
                }
                i--;
            }
            return self;
        }

        /// <summary>
        /// Iterates block, passing decreasing values from self down to and including other, where other is not Fixnum (probably Bignum or Float).
        /// </summary>
        /// <returns>self</returns>
        /// <remarks>
        /// Dynamically invokes "-" operator to find next item down.
        /// Dynamically invokes "&lt;" operator and takes the negation to see if we have reached the bottom.
        /// This approach automatically deals with Floats and overflow/underflow between Fixnum and Bignum.
        /// </remarks>
        [RubyMethod("downto")]
        public static object DownTo(BinaryOpStorage/*!*/ lessThanStorage, BinaryOpStorage/*!*/ subtractStorage,
            BlockParam block, object/*!*/ self, object other) {

            return (block != null) ? DownToImpl(lessThanStorage, subtractStorage, block, self, other) :
                new Enumerator((_, innerBlock) => DownToImpl(lessThanStorage, subtractStorage, innerBlock, self, other));
        }

        private static object DownToImpl(BinaryOpStorage/*!*/ lessThanStorage, BinaryOpStorage/*!*/ subtractStorage,
            BlockParam block, object/*!*/ self, object other) {
            object i = self;
            object compare = null;

            var lessThan = lessThanStorage.GetCallSite("<");
            while (RubyOps.IsFalse(compare)) {
                // Rather than test i >= other we test !(i < other)
                compare = lessThan.Target(lessThan, i, other);

                // If the comparison failed (i.e. returned null) then we throw an error.
                if (compare == null) {
                    throw RubyExceptions.MakeComparisonError(lessThanStorage.Context, i, other);
                }

                // If the comparison worked but returned false then we 
                if (RubyOps.IsFalse(compare)) {
                    object result;
                    if (block.Yield(i, out result)) {
                        return result;
                    }

                    var subtract = subtractStorage.GetCallSite("-");
                    i = subtract.Target(subtract, i, 1);
                }
            }
            return self;
        }

        #endregion

        #region gcd, lcm, gcdlcm

        private static int SignedGcd(int a, int b) {
            // avoid overflow (Int32.MinValue % -1)
            if (b == -1) {
                return -1;
            }

            while (b != 0) {
                int t = b;
                b = a % b;
                a = t;
            }

            return a;
        }

        private static BigInteger/*!*/ SignedGcd(BigInteger/*!*/ a, BigInteger/*!*/ b) {
            while (!b.IsZero()) {
                BigInteger t = b;
                b = a % b;
                a = t;
            }

            return a;
        }

        private static object/*!*/ Lcm(int self, int other, int gcd) {
            return gcd == 0 ? ClrInteger.Zero : Protocols.Normalize(Math.Abs((long)self / gcd * other));
        }

        private static object/*!*/ Lcm(BigInteger/*!*/ self, BigInteger/*!*/ other, BigInteger/*!*/ gcd) {
            return gcd == 0 ? ClrInteger.Zero : Protocols.Normalize((self / gcd * other).Abs());
        }

        [RubyMethod("gcd")]
        public static object/*!*/ Gcd(int self, int other) {
            return ClrInteger.Abs(SignedGcd(self, other));
        }

        [RubyMethod("gcd")]
        public static object/*!*/ Gcd(BigInteger/*!*/ self, BigInteger/*!*/ other) {
            return ClrBigInteger.Abs(SignedGcd(self, other));
        }

        [RubyMethod("gcd")]
        public static object/*!*/ Gcd(object/*!*/ self, object other) {
            throw RubyExceptions.CreateTypeError("not an integer");
        }

        [RubyMethod("lcm")]
        public static object/*!*/ Lcm(int self, int other) {
            return Lcm(self, other, SignedGcd(self, other));
        }

        [RubyMethod("lcm")]
        public static object/*!*/ Lcm(BigInteger/*!*/ self, BigInteger/*!*/ other) {
            return Lcm(self, other, SignedGcd(self, other));
        }

        [RubyMethod("lcm")]
        public static object/*!*/ Lcm(object/*!*/ self, object other) {
            throw RubyExceptions.CreateTypeError("not an integer");
        }

        [RubyMethod("gcdlcm")]
        public static RubyArray/*!*/ GcdLcm(int self, int other) {
            int gcd = SignedGcd(self, other);
            return new RubyArray { ClrInteger.Abs(gcd), Lcm(self, other, gcd) };
        }

        [RubyMethod("gcdlcm")]
        public static RubyArray/*!*/ GcdLcm(BigInteger/*!*/ self, BigInteger/*!*/ other) {
            BigInteger gcd = SignedGcd(self, other);
            return new RubyArray { ClrBigInteger.Abs(gcd), Lcm(self, other, gcd) };
        }

        [RubyMethod("gcdlcm")]
        public static RubyArray/*!*/ GcdLcm(object/*!*/ self, object other) {
            throw RubyExceptions.CreateTypeError("not an integer");
        }

        #endregion

        #region Helpers

        public static object TryUnaryMinus(object obj) {
            if (obj is int) {
                int i = (int)obj;
                return (i != Int32.MinValue) ? ScriptingRuntimeHelpers.Int32ToObject(-i) : -BigInteger.Create(i);
            }

            BigInteger bignum = obj as BigInteger;
            if ((object)bignum != null) {
                return -bignum;
            }

            return null;
        }

        #endregion
    }
}
 