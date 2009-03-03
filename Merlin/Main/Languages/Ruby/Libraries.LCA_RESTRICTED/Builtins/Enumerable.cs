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
using IronRuby.Runtime.Calls;

using EachSite = System.Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyContext, object, IronRuby.Builtins.Proc, object>;

namespace IronRuby.Builtins {
    [RubyModule("Enumerable")]
    public static class Enumerable {
        internal static object Each(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, object self, Proc/*!*/ block) {
            var site = each.GetCallSite("each", new RubyCallSignature(0, RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasBlock));
            return site.Target(site, context, self, block);
        }

        #region all?, any?

        [RubyMethod("all?")]
        public static object TrueForAll(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, BlockParam predicate, object self) {
            return TrueForItems(each, context, predicate, self, true);
        }

        [RubyMethod("any?")]
        public static object TrueForAny(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, BlockParam predicate, object self) {
            return TrueForItems(each, context, predicate, self, false);
        }

        private static object TrueForItems(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, BlockParam predicate, object self, bool expected) {
            bool result = expected;
            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (predicate != null) {
                    if (predicate.Yield(item, out item)) {
                        return item;
                    }
                }

                bool isTrue = Protocols.IsTrue(item);
                if (isTrue != result) {
                    result = isTrue;
                    return selfBlock.Break(ScriptingRuntimeHelpers.BooleanToObject(isTrue));
                }

                return null;
            }));

            return ScriptingRuntimeHelpers.BooleanToObject(result);
        }

        #endregion

        #region collect, map

        [RubyMethod("collect")]
        [RubyMethod("map")]
        public static RubyArray Map(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, BlockParam collector, object self) {
            RubyArray result = new RubyArray();
            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (collector != null) {
                    if (collector.Yield(item, out item)) {
                        return item;
                    }
                }
                result.Add(item);
                return null;
            }));
            return result;
        }

        #endregion

        #region detect, find

        [RubyMethod("detect")]
        [RubyMethod("find")]
        public static object Find(CallSiteStorage<EachSite>/*!*/ each, 
            CallSiteStorage<Func<CallSite, RubyContext, object, object>>/*!*/ callStorage,
            RubyContext/*!*/ context, BlockParam predicate, object self, [Optional]object ifNone) {
            object result = Missing.Value;

            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (predicate == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object blockResult;
                if (predicate.Yield(item, out blockResult)) {
                    result = blockResult;
                    return selfBlock.Break(blockResult);
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
                result = site.Target(site, context, ifNone);
            }
            return result;
        }

        #endregion

        #region each_with_index

        [RubyMethod("each_with_index")]
        public static object EachWithIndex(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, BlockParam/*!*/ block, object self) {
            // for some reason each_with_index always checks for a block, even if there's nothing to yield
            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            int index = 0;

            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                object blockResult;
                if (block.Yield(item, index, out blockResult)) {
                    return blockResult;
                }
                index += 1;
                return null;
            }));

            return self;
        }

        #endregion

        #region entries, to_a

        [RubyMethod("entries")]
        [RubyMethod("to_a")]
        public static RubyArray/*!*/ ToArray(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, object self) {
            RubyArray data = new RubyArray();

            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                data.Add(item);
                return null;
            }));

            return data;
        }

        #endregion

        #region find_all, select, reject

        [RubyMethod("find_all")]
        [RubyMethod("select")]
        public static RubyArray/*!*/ Select(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, BlockParam predicate, object self) {
            return Filter(each, context, predicate, self, true);
        }

        [RubyMethod("reject")]
        public static RubyArray/*!*/ Reject(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, BlockParam predicate, object self) {
            return Filter(each, context, predicate, self, false);
        }

        private static RubyArray/*!*/ Filter(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, 
            BlockParam predicate, object self, bool acceptingValue) {

            RubyArray result = new RubyArray();

            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (predicate == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object blockResult;
                if (predicate.Yield(item, out blockResult)) {
                    return blockResult;
                }

                // Check if the result is what we expect (use true to select, false to reject)
                if (Protocols.IsTrue(blockResult) == acceptingValue) {
                    result.Add(item);
                }
                return null;
            }));

            return result;
        }

        #endregion

        #region grep

        [RubyMethod("grep")]
        public static RubyArray Grep(CallSiteStorage<EachSite>/*!*/ each, 
            BinaryOpStorage/*!*/ caseEquals, RubyContext/*!*/ context, BlockParam action, object self, object pattern) {
            RubyArray result = new RubyArray();
            var site = caseEquals.GetCallSite("===");

            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (RubyOps.IsTrue(site.Target(site, context, pattern, item))) {
                    if (action != null) {
                        if (action.Yield(item, out item)) {
                            return item;
                        }
                    }
                    result.Add(item);
                }
                return null;
            }));

            return result;
        }

        #endregion

        #region include?, member?

        [RubyMethod("include?")]
        [RubyMethod("member?")]
        public static bool Contains(CallSiteStorage<EachSite>/*!*/ each, BinaryOpStorage/*!*/ equals, 
            RubyContext/*!*/ context, object self, object value) {
            bool result = false;

            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (Protocols.IsEqual(equals, context, item, value)) {
                    result = true;
                    return selfBlock.Break(result);
                }
                return null;
            }));

            return result;
        }

        #endregion

        #region inject

        [RubyMethod("inject")]
        public static object Inject(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, 
            BlockParam operation, object self, [Optional]object initial) {

            object result = initial;
            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (result == Missing.Value) {
                    result = item;
                    return null;
                }

                if (operation == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                if (operation.Yield(result, item, out result)) {
                    return result;
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
            RubyContext/*!*/ context, BlockParam comparer, object self) {
            return GetExtreme(each, compareStorage, lessThanStorage, greaterThanStorage, context, comparer, self, -1/*look for max*/);
        }

        [RubyMethod("min")]
        public static object GetMinimum(
            CallSiteStorage<EachSite>/*!*/ each, 
            BinaryOpStorage/*!*/ compareStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            RubyContext/*!*/ context, BlockParam comparer, object self) {
            return GetExtreme(each, compareStorage, lessThanStorage, greaterThanStorage, context, comparer, self, 1/*look for min*/);
        }

        private static object GetExtreme(
            CallSiteStorage<EachSite>/*!*/ each, 
            BinaryOpStorage/*!*/ compareStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            
            RubyContext/*!*/ context, BlockParam comparer, object self, int comparisonValue) {
            bool firstItem = true;
            object result = null;

            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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
                        return blockResult;
                    }

                    if (blockResult == null) {
                        throw RubyExceptions.MakeComparisonError(context, result, item);
                    }

                    compareResult = Protocols.ConvertCompareResult(lessThanStorage, greaterThanStorage, context, blockResult);
                } else {
                    compareResult = Protocols.Compare(compareStorage, lessThanStorage, greaterThanStorage, context, result, item);
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
        public static RubyArray/*!*/ Partition(CallSiteStorage<EachSite>/*!*/ each, RubyContext/*!*/ context, BlockParam predicate, object self) {
            RubyArray trueSet = new RubyArray();
            RubyArray falseSet = new RubyArray();

            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (predicate == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object blockResult;
                if (predicate.Yield(item, out blockResult)) {
                    return blockResult;
                }

                if (Protocols.IsTrue(blockResult)) {
                    trueSet.Add(item);
                } else {
                    falseSet.Add(item);
                }

                return null;
            }));

            RubyArray pair = new RubyArray(2);
            pair.Add(trueSet);
            pair.Add(falseSet);
            return pair;
        }

        #endregion

        #region sort, sort_by

        [RubyMethod("sort")]
        public static object Sort(
            CallSiteStorage<EachSite>/*!*/ each, 
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            RubyContext/*!*/ context, BlockParam keySelector, object self) {

            return ArrayOps.SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, context, keySelector, ToArray(each, context, self));
        }

        [RubyMethod("sort_by")]
        public static RubyArray/*!*/ SortBy(
            CallSiteStorage<EachSite>/*!*/ each, 
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            RubyContext/*!*/ context, BlockParam keySelector, object self) {

            // collect key, value pairs
            List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();

            // Collect the key, value pairs
            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (keySelector == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object key;
                if (keySelector.Yield(item, out key)) {
                    return key;
                }

                keyValuePairs.Add(new KeyValuePair<object, object>(key, item));
                return null;
            }));

            // sort by keys
            keyValuePairs.Sort(delegate(KeyValuePair<object, object> x, KeyValuePair<object, object> y) {
                return Protocols.Compare(comparisonStorage, lessThanStorage, greaterThanStorage, context, x.Key, y.Key);
            });

            // return values
            RubyArray result = new RubyArray(keyValuePairs.Count);
            foreach (KeyValuePair<object, object> pair in keyValuePairs) {
                result.Add(pair.Value);
            }

            return result;
        }

        #endregion

        #region zip

        [RubyMethod("zip")]
        public static RubyArray/*!*/ Zip(CallSiteStorage<EachSite>/*!*/ each, ConversionStorage<IList>/*!*/ tryToA, RubyContext/*!*/ context, BlockParam block,
            object self, [NotNull]params object[] args) {
            RubyArray results = (block == null) ? new RubyArray() : null;

            // Call to_a on each argument
            IList[] otherArrays = new IList[args.Length];
            for (int i = 0; i < args.Length; i++) {
                otherArrays[i] = Protocols.TryConvertToArray(tryToA, context, args[i]);
            }

            int index = 0;
            Each(each, context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                // Collect items
                RubyArray array = new RubyArray(otherArrays.Length + 1);
                array.Add(item);
                foreach (IList otherArray in otherArrays) {
                    if (index < otherArray.Count) {
                        array.Add(otherArray[index]);
                    } else {
                        array.Add(null);
                    }
                }

                index += 1;

                if (block != null) {
                    object blockResult;
                    if (block.Yield(array, out blockResult)) {
                        return blockResult;
                    }
                } else {
                    results.Add(array);
                }
                return null;
            }));

            return results;
        }

        #endregion

        #region TODO: count, none?, one?, first (1.9)

        #endregion

        #region TODO: cycle, drop, drop_while, find_index, group_by, max_by, min_by, minmax, minmax_by, reduce, take_take_while (1.9)

        #endregion

    }
}
