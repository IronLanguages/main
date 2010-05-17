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
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Generation;

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

        #region chr

        [RubyMethod("chr")]
        public static MutableString/*!*/ ToChr(RubyContext/*!*/ context, [DefaultProtocol]int self) {
            if (self < 0 || self > 255) {
                throw RubyExceptions.CreateRangeError("{0} out of char range", self);
            }
            return MutableString.CreateBinary(new byte[] { (byte)self });
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
            if (self >= other && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

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
                    if (block == null) {
                        throw RubyExceptions.NoBlockGiven();
                    }

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

        #region integer?

        /// <summary>
        /// Always returns true.
        /// </summary>
        /// <returns>true</returns>
        [RubyMethod("integer?")]
        public static new bool IsInteger(object/*!*/ self) {
            return true;
        }

        #endregion

        #region next, succ

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

        #endregion

        #region times

        /// <summary>
        /// Iterates block self times, passing in values from zero to self - 1, where self is Fixnum.
        /// </summary>
        /// <returns>self</returns>
        /// <example>
        ///  5.times do |i|
        ///    print i, " "
        ///  end
        /// produces: 
        ///  0 1 2 3 4
        /// </example>
        [RubyMethodAttribute("times")]
        public static object Times(BlockParam block, int self) {
            if (self > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

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
            object i = 0;
            var lessThan = lessThanStorage.GetCallSite("<");
            while (RubyOps.IsTrue(lessThan.Target(lessThan, i, self))) {
                if (block == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

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
            if (block == null && self <= other) {
                throw RubyExceptions.NoBlockGiven();
            }

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
                    if (block == null) {
                        throw RubyExceptions.NoBlockGiven();
                    }

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
 