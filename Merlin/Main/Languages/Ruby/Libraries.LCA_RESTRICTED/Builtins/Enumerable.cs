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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {
    using EachSite = Func<CallSite, object, Proc, object>;

    [RubyModule("Enumerable")]
    public static class Enumerable {
        internal static object Each(CallSiteStorage<EachSite>/*!*/ each, object self, Proc/*!*/ block) {
            var site = each.GetCallSite("each", new RubyCallSignature(0, RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasBlock));
            return site.Target(site, self, block);
        }

        #region all?, any?

        [RubyMethod("all?")]
        public static object TrueForAll(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return TrueForItems(each, predicate, self, true);
        }

        [RubyMethod("any?")]
        public static object TrueForAny(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return TrueForItems(each, predicate, self, false);
        }

        private static object TrueForItems(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self, bool expected) {
            object result = ScriptingRuntimeHelpers.BooleanToObject(expected);
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (predicate != null) {
                    if (predicate.Yield(item, out item)) {
                        result = item;
                        return selfBlock.PropagateFlow(predicate, item);
                    }
                }

                bool isTrue = Protocols.IsTrue(item);
                if (isTrue != expected) {
                    result = ScriptingRuntimeHelpers.BooleanToObject(!expected);
                    return selfBlock.Break(result);
                }

                return null;
            }));

            return result;
        }

        #endregion

        #region collect, map

        [RubyMethod("collect")]
        [RubyMethod("map")]
        public static object Map(CallSiteStorage<EachSite>/*!*/ each, BlockParam collector, object self) {
            RubyArray resultArray = new RubyArray();
            object result = resultArray;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (collector != null) {
                    if (collector.Yield(item, out item)) {
                        result = item;
                        return selfBlock.PropagateFlow(collector, item);
                    }
                }
                resultArray.Add(item);
                return null;
            }));
            return result;
        }

        #endregion

        #region detect, find

        [RubyMethod("detect")]
        [RubyMethod("find")]
        public static object Find(CallSiteStorage<EachSite>/*!*/ each, CallSiteStorage<Func<CallSite, object, object>>/*!*/ callStorage,
            BlockParam predicate, object self, [Optional]object ifNone) {
            object result = Missing.Value;

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (predicate == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object blockResult;
                if (predicate.Yield(item, out blockResult)) {
                    result = blockResult;
                    return selfBlock.PropagateFlow(predicate, blockResult);
                }

                if (Protocols.IsTrue(blockResult)) {
                    result = item;
                    return selfBlock.Break(item);
                }

                return null;
            }));

            if (result == Missing.Value) {
                if (ifNone == Missing.Value || ifNone == null) {
                    return null;
                }

                var site = callStorage.GetCallSite("call", 0);
                result = site.Target(site, ifNone);
            }
            return result;
        }

        #endregion

        #region each_with_index

        [RubyMethod("each_with_index")]
        public static object EachWithIndex(CallSiteStorage<EachSite>/*!*/ each, BlockParam/*!*/ block, object self) {
            // for some reason each_with_index always checks for a block, even if there's nothing to yield
            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            int index = 0;
            object result = self;

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                object blockResult;
                if (block.Yield(item, index, out blockResult)) {
                    result = blockResult;
                    return selfBlock.PropagateFlow(block, blockResult);
                }
                index += 1;
                return null;
            }));

            return result;
        }

        #endregion

        #region entries, to_a

        [RubyMethod("entries")]
        [RubyMethod("to_a")]
        public static RubyArray/*!*/ ToArray(CallSiteStorage<EachSite>/*!*/ each, object self) {
            RubyArray data = new RubyArray();

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                data.Add(item);
                return null;
            }));

            return data;
        }

        #endregion

        #region find_all, select, reject

        [RubyMethod("find_all")]
        [RubyMethod("select")]
        public static object Select(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return Filter(each, predicate, self, true);
        }

        [RubyMethod("reject")]
        public static object Reject(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return Filter(each, predicate, self, false);
        }

        private static object Filter(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self, bool acceptingValue) {
            RubyArray resultArray = new RubyArray();
            object result = resultArray;

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (predicate == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object blockResult;
                if (predicate.Yield(item, out blockResult)) {
                    result = blockResult;
                    return selfBlock.PropagateFlow(predicate, blockResult);
                }

                // Check if the result is what we expect (use true to select, false to reject)
                if (Protocols.IsTrue(blockResult) == acceptingValue) {
                    resultArray.Add(item);
                }
                return null;
            }));

            return result;
        }

        #endregion

        #region grep

        [RubyMethod("grep")]
        public static object Grep(CallSiteStorage<EachSite>/*!*/ each, BinaryOpStorage/*!*/ caseEquals, 
            BlockParam action, object self, object pattern) {

            RubyArray resultArray = new RubyArray();
            object result = resultArray;
            var site = caseEquals.GetCallSite("===");

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (RubyOps.IsTrue(site.Target(site, pattern, item))) {
                    if (action != null) {
                        if (action.Yield(item, out item)) {
                            result = item;
                            return selfBlock.PropagateFlow(action, item);
                        }
                    }
                    resultArray.Add(item);
                }
                return null;
            }));

            return result;
        }

        #endregion

        #region include?, member?

        [RubyMethod("include?")]
        [RubyMethod("member?")]
        public static object Contains(CallSiteStorage<EachSite>/*!*/ each, BinaryOpStorage/*!*/ equals, object self, object value) {
            object result = ScriptingRuntimeHelpers.BooleanToObject(false);

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (Protocols.IsEqual(equals, item, value)) {
                    result = ScriptingRuntimeHelpers.BooleanToObject(true);
                    return selfBlock.Break(result);
                }
                return null;
            }));

            return result;
        }

        #endregion

        #region inject

        [RubyMethod("inject")]
        public static object Inject(CallSiteStorage<EachSite>/*!*/ each, BlockParam operation, object self, [Optional]object initial) {

            object result = initial;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (result == Missing.Value) {
                    result = item;
                    return null;
                }

                if (operation == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                if (operation.Yield(result, item, out result)) {
                    return selfBlock.PropagateFlow(operation, result);
                }

                return null;
            }));

            return result != Missing.Value ? result : null;
        }

        #endregion

        #region max, min

        [RubyMethod("max")]
        public static object GetMaximum(
            CallSiteStorage<EachSite>/*!*/ each, 
            BinaryOpStorage/*!*/ compareStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam comparer, object self) {
            return GetExtreme(each, compareStorage, lessThanStorage, greaterThanStorage, comparer, self, -1/*look for max*/);
        }

        [RubyMethod("min")]
        public static object GetMinimum(
            CallSiteStorage<EachSite>/*!*/ each, 
            BinaryOpStorage/*!*/ compareStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam comparer, object self) {
            return GetExtreme(each, compareStorage, lessThanStorage, greaterThanStorage, comparer, self, 1/*look for min*/);
        }

        private static object GetExtreme(
            CallSiteStorage<EachSite>/*!*/ each, 
            BinaryOpStorage/*!*/ compareStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam comparer, object self, int comparisonValue) {

            bool firstItem = true;
            object result = null;

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                // Check for first element
                if (firstItem) {
                    result = item;
                    firstItem = false;
                    return null;
                }

                int compareResult;
                if (comparer != null) {
                    object blockResult;
                    if (comparer.Yield(result, item, out blockResult)) {
                        result = blockResult;
                        return selfBlock.PropagateFlow(comparer, blockResult);
                    }

                    if (blockResult == null) {
                        throw RubyExceptions.MakeComparisonError(selfBlock.RubyContext, result, item);
                    }

                    compareResult = Protocols.ConvertCompareResult(lessThanStorage, greaterThanStorage, blockResult);
                } else {
                    compareResult = Protocols.Compare(compareStorage, lessThanStorage, greaterThanStorage, result, item);
                }

                // Check if we have found the new minimum or maximum (-1 to select max, 1 to select min)
                if (compareResult == comparisonValue) {
                    result = item;
                }

                return null;
            }));
            return result;
        }
        #endregion

        #region partition

        [RubyMethod("partition")]
        public static object Partition(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            RubyArray trueSet = new RubyArray();
            RubyArray falseSet = new RubyArray();
            RubyArray pair = new RubyArray(2);
            pair.Add(trueSet);
            pair.Add(falseSet);
            object result = pair;

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (predicate == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object blockResult;
                if (predicate.Yield(item, out blockResult)) {
                    result = blockResult;
                    return selfBlock.PropagateFlow(predicate, blockResult);
                }

                if (Protocols.IsTrue(blockResult)) {
                    trueSet.Add(item);
                } else {
                    falseSet.Add(item);
                }

                return null;
            }));

            return result;
        }

        #endregion

        #region sort, sort_by

        [RubyMethod("sort")]
        public static object Sort(
            CallSiteStorage<EachSite>/*!*/ each, 
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam keySelector, object self) {

            return ArrayOps.SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, keySelector, ToArray(each, self));
        }

        [RubyMethod("sort_by")]
        public static object SortBy(
            CallSiteStorage<EachSite>/*!*/ each, 
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam keySelector, object self) {

            // collect key, value pairs
            List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();
            object result = null;

            // Collect the key, value pairs
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (keySelector == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object key;
                if (keySelector.Yield(item, out key)) {
                    keyValuePairs = null;
                    result = key;
                    return selfBlock.PropagateFlow(keySelector, key);
                }

                keyValuePairs.Add(new KeyValuePair<object, object>(key, item));
                return null;
            }));

            if (keyValuePairs == null) {
                return result;
            }

            // sort by keys
            keyValuePairs.Sort(delegate(KeyValuePair<object, object> x, KeyValuePair<object, object> y) {
                return Protocols.Compare(comparisonStorage, lessThanStorage, greaterThanStorage, x.Key, y.Key);
            });

            // return values
            RubyArray results = new RubyArray(keyValuePairs.Count);
            foreach (KeyValuePair<object, object> pair in keyValuePairs) {
                results.Add(pair.Value);
            }

            return results;
        }

        #endregion

        #region zip

        internal static object Zip(CallSiteStorage<EachSite>/*!*/ each, ConversionStorage<IList>/*!*/ tryToA, BlockParam block,
            object self, params IList[]/*!*/ args) {

            RubyArray results = (block == null) ? new RubyArray() : null;
            object result = results;

            int index = 0;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                // Collect items
                RubyArray array = new RubyArray(args.Length + 1);
                array.Add(item);
                foreach (IList otherArray in args) {
                    if (index < otherArray.Count) {
                        array.Add(otherArray[index]);
                    }
                    else {
                        array.Add(null);
                    }
                }

                index += 1;

                if (block != null) {
                    object blockResult;
                    if (block.Yield(array, out blockResult)) {
                        result = blockResult;
                        return selfBlock.PropagateFlow(block, blockResult);
                    }
                }
                else {
                    results.Add(array);
                }
                return null;
            }));

            return result;
        }

        [RubyMethod("zip")]
        public static object Zip(CallSiteStorage<EachSite>/*!*/ each, ConversionStorage<IList>/*!*/ tryToA, BlockParam block,
            object self, params object[]/*!*/ args) {

            // Call to_a on each argument
            IList[] otherArrays = new IList[args.Length];
            for (int i = 0; i < args.Length; i++) {
                otherArrays[i] = (args[i] as IList) ?? Protocols.TryConvertToArray(tryToA, args[i]);
            }

            return Zip(each, tryToA, block, self, otherArrays);
        }

        #endregion

        #region TODO: count, none?, one?, first (1.9)

        #endregion

        #region TODO: cycle, drop, drop_while, find_index, group_by, max_by, min_by, minmax, minmax_by, reduce, take_take_while (1.9)

        #endregion
    }
}
