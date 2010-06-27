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
using System.Runtime.CompilerServices;
using System.Text;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Math;

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
                // TODO: implement copy-on-write
                return new RubyArray(union.First);
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
        public static RubyArray/*!*/ ToExplicitArray(RubyArray/*!*/ self) {
            return self;
        }

        #region pack

        [RubyMethod("pack")]
        public static MutableString/*!*/ Pack(
            ConversionStorage<IntegerValue>/*!*/ integerConversion,
            ConversionStorage<double>/*!*/ floatConversion,
            ConversionStorage<MutableString>/*!*/ stringCast,
            ConversionStorage<MutableString>/*!*/ tosConversion,
            RubyArray/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ format) {

            return RubyEncoder.Pack(integerConversion, floatConversion, stringCast, tosConversion, self, format);
        }

        #endregion

        #region sort!, sort

        private sealed class BreakException : Exception {
        }

        [RubyMethod("sort")]
        public static object Sort(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam block, RubyArray/*!*/ self) {

            RubyArray result = self.CreateInstance();
            IListOps.Replace(result, self);
            return SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, block, result);
        }

        [RubyMethod("sort!")]
        public static object SortInPlace(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam block, RubyArray/*!*/ self) {

            StrongBox<object> breakResult;
            RubyArray result = SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, block, self, out breakResult);
            if (breakResult != null) {
                return breakResult.Value;
            } else {
                return result;
            }
        }

        public static RubyArray/*!*/ SortInPlace(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            RubyArray/*!*/ self) {

            StrongBox<object> breakResult;
            RubyArray result = SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, null, self, out breakResult);
            Debug.Assert(result != null && breakResult == null);
            return result;
        }

        internal static RubyArray SortInPlace(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,            
            BlockParam block,
            RubyArray/*!*/ self,
            out StrongBox<object> breakResult) {

            breakResult = null;
            var context = comparisonStorage.Context;

            // TODO: this does more comparisons (and in a different order) than
            // Ruby's sort. Also, control flow won't work because List<T>.Sort wraps
            // exceptions from the comparer & rethrows. We need to rewrite a version of quicksort
            // that behaves like Ruby's sort.
            if (block == null) {
                self.Sort((x, y) => Protocols.Compare(comparisonStorage, lessThanStorage, greaterThanStorage, x, y));
            } else {
                object nonRefBreakResult = null;
                try {
                    self.Sort((x, y) =>
                    {
                        object result = null;
                        if (block.Yield(x, y, out result)) {
                            nonRefBreakResult = result;
                            throw new BreakException();
                        }

                        if (result == null) {
                            throw RubyExceptions.MakeComparisonError(context, x, y);
                        }

                        return Protocols.ConvertCompareResult(lessThanStorage, greaterThanStorage, result);
                    });
                } catch (InvalidOperationException e) {
                    if (e.InnerException == null) {
                        throw;
                    }

                    if (e.InnerException is BreakException) {
                        breakResult = new StrongBox<object>(nonRefBreakResult);
                        return null;
                    } else {
                        throw e.InnerException;
                    }
                }
            }

            return self;
        }
        #endregion

        #region reverse!, reverse_each

        [RubyMethod("reverse!")]
        public static RubyArray/*!*/ InPlaceReverse(RubyContext/*!*/ context, RubyArray/*!*/ self) {
            self.Reverse();
            return self;
        }

        [RubyMethod("reverse_each")]
        public static object ReverseEach(RubyContext/*!*/ context, BlockParam block, RubyArray/*!*/ self) {
            Assert.NotNull(context, self);

            if (self.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            foreach (int index in IListOps.ReverseEnumerateIndexes(self)) {
                object result;
                if (block.Yield(self[index], out result)) {
                    return result;
                }
            }
            return self;
        }

        #endregion
    }
}
