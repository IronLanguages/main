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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    using EachSiteN = Func<CallSite, object, Proc, IList, object>;

    [RubyModule("Enumerable")]
    public static class Enumerable {
        internal static object Each(CallSiteStorage<EachSite>/*!*/ each, object self, Proc/*!*/ block) {
            var site = each.GetCallSite("each", new RubyCallSignature(0, RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasBlock));
            return site.Target(site, self, block);
        }

        internal static object Each(CallSiteStorage<EachSiteN>/*!*/ each, object self, IList/*!*/ args, Proc/*!*/ block) {
            var site = each.GetCallSite("each", new RubyCallSignature(0, RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasBlock | RubyCallFlags.HasSplattedArgument));
            return site.Target(site, self, block, args);
        }

        #region all?, any?, none?

        [RubyMethod("all?")]
        public static object TrueForAll(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return TrueForItems(each, predicate, self, false, false);
        }

        [RubyMethod("none?")]
        public static object TrueForNone(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return TrueForItems(each, predicate, self, true, false);
        }

        [RubyMethod("any?")]
        public static object TrueForAny(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return TrueForItems(each, predicate, self, true, true);
        }

        private static object TrueForItems(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self, bool stop, bool positiveResult) {
            object result = ScriptingRuntimeHelpers.BooleanToObject(!positiveResult);
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (predicate != null) {
                    object blockResult;
                    if (predicate.Yield(item, out blockResult)) {
                        result = blockResult;
                        return selfBlock.PropagateFlow(predicate, blockResult);
                    }
                    item = blockResult;
                }

                bool isTrue = Protocols.IsTrue(item);
                if (isTrue == stop) {
                    result = ScriptingRuntimeHelpers.BooleanToObject(positiveResult); 
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
        public static Enumerator/*!*/ GetMapEnumerator(CallSiteStorage<EachSite>/*!*/ each, BlockParam collector, object self) {
            return new Enumerator((_, block) => Map(each, block, self));
        }

        /// <summary>
        /// <code>
        /// def map
        ///   result = []
        ///   each do |*args|
        ///     result.push yield(*args)
        ///   end    
        ///   result
        /// end
        /// </code>
        /// </summary>
        [RubyMethod("collect")]
        [RubyMethod("map")]
        public static object Map(CallSiteStorage<EachSite>/*!*/ each, [NotNull]BlockParam/*!*/ collector, object self) {
            RubyArray resultArray = new RubyArray();
            object result = resultArray;

            if (collector.Proc.Dispatcher.ParameterCount <= 1 && !collector.Proc.Dispatcher.HasUnsplatParameter && !collector.Proc.Dispatcher.HasProcParameter) {
                // optimize for a block with a single parameter:
                Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                    object blockResult;
                    if (collector.Yield(item, out blockResult)) {
                        result = blockResult;
                        return selfBlock.PropagateFlow(collector, blockResult);
                    }
                    resultArray.Add(blockResult);
                    return null;
                }));
            } else {
                // general case:
                Each(each, self, Proc.Create(each.Context, 0, delegate(BlockParam/*!*/ selfBlock, object _, object[] __, RubyArray args) {
                    Debug.Assert(__.Length == 0);

                    object blockResult;
                    if (collector.YieldSplat(args, out blockResult)) {
                        result = blockResult;
                        return selfBlock.PropagateFlow(collector, blockResult);
                    }
                    resultArray.Add(blockResult);
                    return null;
                }));
            }
            return result;
        }

        #endregion

        #region detect, find, find_index

        [RubyMethod("detect")]
        [RubyMethod("find")]
        public static Enumerator/*!*/ GetFindEnumerator(CallSiteStorage<EachSite>/*!*/ each, CallSiteStorage<Func<CallSite, object, object>>/*!*/ callStorage,
            BlockParam predicate, object self, [Optional]object ifNone) {
            return new Enumerator((_, block) => Find(each, callStorage, block, self, ifNone));
        }

        [RubyMethod("detect")]
        [RubyMethod("find")]
        public static object Find(CallSiteStorage<EachSite>/*!*/ each, CallSiteStorage<Func<CallSite, object, object>>/*!*/ callStorage,
            [NotNull]BlockParam/*!*/ predicate, object self, [Optional]object ifNone) {
            object result = Missing.Value;

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
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

        [RubyMethod("find_index")]
        public static Enumerator/*!*/ GetFindIndexEnumerator(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return new Enumerator((_, block) => FindIndex(each, block, self));
        }

        [RubyMethod("find_index")]
        public static object FindIndex(CallSiteStorage<EachSite>/*!*/ each, [NotNull]BlockParam/*!*/ predicate, object self) {
            int index = 0;

            object result = null;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                object blockResult;
                if (predicate.Yield(item, out blockResult)) {
                    result = blockResult;
                    return selfBlock.PropagateFlow(predicate, blockResult);
                }

                if (Protocols.IsTrue(blockResult)) {
                    result = ScriptingRuntimeHelpers.Int32ToObject(index);
                    return selfBlock.Break(null);
                }

                index++;
                return null;
            }));

            return result;
        }

        [RubyMethod("find_index")]
        public static object FindIndex(CallSiteStorage<EachSite>/*!*/ each, BinaryOpStorage/*!*/ equals, BlockParam predicate, object self, object value) {
            if (predicate != null) {
                each.Context.ReportWarning("given block not used");
            }

            int index = 0;
            object result = null;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (Protocols.IsEqual(equals, item, value)) {
                    result = ScriptingRuntimeHelpers.Int32ToObject(index);
                    return selfBlock.Break(null);
                }

                index++;
                return null;
            }));

            return result;
        }

        #endregion

        #region each_with_index

        [RubyMethod("each_with_index")]
        public static Enumerator/*!*/ GetEachWithIndexEnumerator(CallSiteStorage<EachSite>/*!*/ each, BlockParam block, object self) {
            return new Enumerator((_, innerBlock) => EachWithIndex(each, innerBlock, self));
        }

        [RubyMethod("each_with_index")]
        public static object EachWithIndex(CallSiteStorage<EachSite>/*!*/ each, [NotNull]BlockParam/*!*/ block, object self) {
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

        [RubyMethod("entries")]
        [RubyMethod("to_a")]
        public static RubyArray/*!*/ ToArray(CallSiteStorage<EachSiteN>/*!*/ each, object self, params object[] args) {
            RubyArray data = new RubyArray();

            Each(each, self, args, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
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
            return (predicate != null) ? FilterImpl(each, predicate, self, true) : FilterEnum(each, predicate, self, true);
        }

        [RubyMethod("reject")]
        public static object Reject(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return (predicate != null) ? FilterImpl(each, predicate, self, false) : FilterEnum(each, predicate, self, false);
        }

        private static Enumerator/*!*/ FilterEnum(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self, bool acceptingValue) {
            return new Enumerator((_, block) => FilterImpl(each, block, self, acceptingValue));
        }

        private static object FilterImpl(CallSiteStorage<EachSite>/*!*/ each, BlockParam/*!*/ predicate, object self, bool acceptingValue) {
            RubyArray resultArray = new RubyArray();
            object result = resultArray;

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
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
                    if (action != null && action.Yield(item, out item)) {
                        result = item;
                        return selfBlock.PropagateFlow(action, item);
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

        #region inject/reduce

        [RubyMethod("reduce")]
        [RubyMethod("inject")]
        public static object Inject(CallSiteStorage<EachSite>/*!*/ each, [NotNull]BlockParam/*!*/ operation, object self, [Optional]object initial) {
            return Inject(each, operation, null, null, self, initial);
        }

        [RubyMethod("reduce")]
        [RubyMethod("inject")]
        public static object Inject(CallSiteStorage<EachSite>/*!*/ each, RubyScope/*!*/ scope, object self, [Optional]object initial, 
            [DefaultProtocol, NotNull]string/*!*/ operatorName) {

            return Inject(each, null, scope, operatorName, self, initial);
        }

        // def inject(result = Undefined)
        //   each do |*args|
        //     arg = args.size <= 1 ? args[0] : args
        //     if result == Undefined
        //       result = arg
        //     else
        //       result = yield(result, arg)
        //     end
        //   end
        //   result
        // end
        internal static object Inject(CallSiteStorage<EachSite>/*!*/ each, BlockParam operation, RubyScope scope, string operatorName, object self, object initial) {
            Debug.Assert(operation != null ^ (scope != null && operatorName != null));

            var site = (operatorName == null) ? null : each.Context.GetOrCreateSendSite<Func<CallSite, RubyScope, object, object, object>>(
                operatorName, new RubyCallSignature(1, RubyCallFlags.HasScope | RubyCallFlags.HasImplicitSelf)
            );

            object result = initial;
            Each(each, self, Proc.Create(each.Context, 0, delegate(BlockParam/*!*/ selfBlock, object _, object[] __, RubyArray/*!*/ args) {
                Debug.Assert(__.Length == 0);

                // TODO: this is weird but is actually exploited in Rack::Utils::HeaderHash
                // TODO: Can we optimize (special dispatcher)? We allocate splatte array for each iteration.
                object value = args.Count == 0 ? null : args.Count == 1 ? args[0] : args;

                if (result == Missing.Value) {
                    result = value;
                    return null;
                }

                if (site != null) {
                    result = site.Target(site, scope, result, value);
                } else if (operation.Yield(result, value, out result)) {
                    return selfBlock.PropagateFlow(operation, result);
                }

                return null;
            }));

            return result != Missing.Value ? result : null;
        }

        #endregion

        #region min, max, minmax

        [RubyMethod("max")]
        public static object GetMaximum(CallSiteStorage<EachSite>/*!*/ each, ComparisonStorage/*!*/ comparisonStorage, BlockParam comparer, object self) {
            return GetExtreme(each, comparisonStorage, comparer, self, +1);
        }

        [RubyMethod("min")]
        public static object GetMinimum(CallSiteStorage<EachSite>/*!*/ each, ComparisonStorage/*!*/ comparisonStorage, BlockParam comparer, object self) {
            return GetExtreme(each, comparisonStorage, comparer, self, -1);
        }

        private static object GetExtreme(CallSiteStorage<EachSite>/*!*/ each, ComparisonStorage/*!*/ comparisonStorage, BlockParam comparer, object self, int comparisonValue) {
            bool firstItem = true;
            object result = null;

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (firstItem) {
                    result = item;
                    firstItem = false;
                    return null;
                }

                object blockResult;
                int? compareResult = CompareItems(comparisonStorage, item, result, comparer, out blockResult);
                if (compareResult == null) {
                    result = blockResult;
                    return selfBlock.PropagateFlow(comparer, blockResult);
                }

                // Check if we have found the new minimum or maximum (+1 to select max, -1 to select min)
                if (compareResult == comparisonValue) {
                    result = item;
                }

                return null;
            }));

            return result;
        }

        [RubyMethod("minmax")]
        public static object GetExtremes(CallSiteStorage<EachSite>/*!*/ each, ComparisonStorage/*!*/ comparisonStorage, BlockParam comparer, object self) {
            bool hasOddItem = false, hasMinMax = false, blockJumped = false;
            object oddItem = null;
            object blockResult = null;

            object min = null, max = null;

	     Func<IronRuby.Runtime.BlockParam,object,object,object> blockProc = delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (hasOddItem) {
                    hasOddItem = false;

                    int? compareResult = CompareItems(comparisonStorage, oddItem, item, comparer, out blockResult);
                    if (compareResult == null) {
                        goto BlockJumped;
                    }
                    if (compareResult > 0) {
                        // oddItem > item
                        object obj = item;
                        item = oddItem;
                        oddItem = obj;
                    }

                    // oddItem <= item
                    if (hasMinMax) {
                        compareResult = CompareItems(comparisonStorage, oddItem, min, comparer, out blockResult);
                        if (compareResult == null) {
                            goto BlockJumped;
                        }
                        if (compareResult < 0) {
                            // oddItem < min
                            min = oddItem;
                        }

                        compareResult = CompareItems(comparisonStorage, item, max, comparer, out blockResult);
                        if (compareResult == null) {
                            goto BlockJumped;
                        }
                        if (compareResult > 0) {
                            // item > max
                            max = item;
                        }
                    } else {
                        min = oddItem;
                        max = item;
                        hasMinMax = true;
                    }
                } else {
                    hasOddItem = true;
                    oddItem = item;
                }

                return null;

            BlockJumped:
                blockJumped = true;
                return selfBlock.PropagateFlow(comparer, blockResult);
            };

            Each(each, self, Proc.Create(each.Context, blockProc));

            if (blockJumped) {
                return blockResult;
            }

            if (!hasMinMax) {
                return hasOddItem ? new RubyArray(2) { oddItem, oddItem } : new RubyArray(2) { null, null };
            }

            if (hasOddItem) {
                int? compareResult = CompareItems(comparisonStorage, oddItem, min, comparer, out blockResult);
                if (compareResult == null) {
                    return blockResult;
                }
                if (compareResult < 0) {
                    min = oddItem;
                }

                compareResult = CompareItems(comparisonStorage, oddItem, max, comparer, out blockResult);
                if (compareResult == null) {
                    return blockResult;
                }
                if (compareResult > 0) {
                    max = oddItem;
                }
            }

            return new RubyArray(2) { min, max };
        }

        private static int? CompareItems(ComparisonStorage/*!*/ comparisonStorage, object left, object right, BlockParam comparer, out object blockResult) {
            if (comparer != null) {
                if (comparer.Yield(left, right, out blockResult)) {
                    return null;
                }

                if (blockResult == null) {
                    throw RubyExceptions.MakeComparisonError(comparisonStorage.Context, left, right);
                }

                return Protocols.ConvertCompareResult(comparisonStorage, blockResult);
            } else {
                blockResult = null;
                return Protocols.Compare(comparisonStorage, left, right);
            }
        }

        #endregion

        #region TODO: min_by, max_by, minmax_by

        #endregion

        #region partition

        [RubyMethod("partition")]
        public static Enumerator/*!*/ GetPartitionEnumerator(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return new Enumerator((_, block) => Partition(each, block, self));
        }

        [RubyMethod("partition")]
        public static object Partition(CallSiteStorage<EachSite>/*!*/ each, [NotNull]BlockParam/*!*/ predicate, object self) {
            RubyArray trueSet = new RubyArray();
            RubyArray falseSet = new RubyArray();
            RubyArray pair = new RubyArray(2);
            pair.Add(trueSet);
            pair.Add(falseSet);
            object result = pair;

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
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
        public static object Sort(CallSiteStorage<EachSite>/*!*/ each, ComparisonStorage/*!*/ comparisonStorage, BlockParam keySelector, object self) {
            return ArrayOps.SortInPlace(comparisonStorage, keySelector, ToArray(each, self));
        }

        [RubyMethod("sort_by")]
        public static object SortBy(CallSiteStorage<EachSite>/*!*/ each, ComparisonStorage/*!*/ comparisonStorage, BlockParam keySelector, object self) {
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
                return Protocols.Compare(comparisonStorage, x.Key, y.Key);
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

        [RubyMethod("zip")]
        public static object Zip(CallSiteStorage<EachSite>/*!*/ each, BlockParam block, object self, [DefaultProtocol, NotNullItems]params IList/*!*/[]/*!*/ args) {
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
                    } else {
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
                } else {
                    results.Add(array);
                }
                return null;
            }));

            return result;
        }

        #endregion

        #region count, one?

        [RubyMethod("count")]
        public static int Count(CallSiteStorage<EachSite>/*!*/ each, object self) {
            int result = 0;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                result++;
                return null;
            }));
            return result;
        }

        [RubyMethod("count")]
        public static int Count(CallSiteStorage<EachSite>/*!*/ each, BinaryOpStorage/*!*/ equals, BlockParam comparer, object self, object value) {
            if (comparer != null) {
                each.Context.ReportWarning("given block not used");
            }

            int result = 0;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (Protocols.IsEqual(equals, item, value)) {
                    result++;
                }
                return null;
            }));
            return result;
        }

        [RubyMethod("count")]
        public static object Count(CallSiteStorage<EachSite>/*!*/ each, BinaryOpStorage/*!*/ equals, [NotNull]BlockParam/*!*/ comparer, object self) {
            int count = 0;

            object result = null;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                object blockResult;
                if (comparer.Yield(item, out blockResult)) {
                    count = -1;
                    result = blockResult;
                    return selfBlock.PropagateFlow(comparer, blockResult); 
                }

                if (Protocols.IsTrue(blockResult)) {
                    count++;
                }
                return null;
            }));

            return (count >= 0) ? ScriptingRuntimeHelpers.Int32ToObject(count) : result;
        }

        [RubyMethod("one?")]
        public static object One(CallSiteStorage<EachSite>/*!*/ each, BinaryOpStorage/*!*/ equals, BlockParam comparer, object self) {
            int count = 0;

            object result = null;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                object blockResult;
                if (comparer == null) {
                    blockResult = item;
                } else if (comparer.Yield(item, out blockResult)) {
                    count = -1;
                    result = blockResult;
                    return selfBlock.PropagateFlow(comparer, blockResult);
                }

                if (Protocols.IsTrue(blockResult) && ++count > 1) {
                    selfBlock.Break(null);
                }
                return null;
            }));

            return (count >= 0) ? ScriptingRuntimeHelpers.BooleanToObject(count == 1) : result;
        }

        #endregion

        #region first, take, take_while, drop, drop_while, cycle

        [RubyMethod("first")]
        public static object First(CallSiteStorage<EachSite>/*!*/ each, object self) {
            object result = null;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                result = item;
                selfBlock.Break(null);
                return null;
            }));
            return result;
        }

        [RubyMethod("first")]
        [RubyMethod("take")]
        public static RubyArray/*!*/ Take(CallSiteStorage<EachSite>/*!*/ each, object self, [DefaultProtocol]int count) {
            if (count < 0) {
                throw RubyExceptions.CreateArgumentError("attempt to take negative size");
            }

            var result = new RubyArray(count);
            if (count == 0) {
                return result;
            }

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                result.Add(item);
                if (--count == 0) {
                    selfBlock.Break(null);
                }
                return null;
            }));

            return result;
        }

        [RubyMethod("take_while")]
        public static Enumerator/*!*/ GetTakeWhileEnumerator(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return new Enumerator((_, block) => TakeWhile(each, block, self));
        }

        [RubyMethod("take_while")]
        public static object TakeWhile(CallSiteStorage<EachSite>/*!*/ each, [NotNull]BlockParam/*!*/ predicate, object self) {
            RubyArray resultArray = new RubyArray();

            object result = resultArray;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                object blockResult;
                if (predicate.Yield(item, out blockResult)) {
                    result = blockResult;
                    return selfBlock.PropagateFlow(predicate, blockResult);
                }

                if (Protocols.IsTrue(blockResult)) {
                    resultArray.Add(item);
                } else {
                    selfBlock.Break(null);
                }

                return null;
            }));

            return result;
        }

        [RubyMethod("drop")]
        public static RubyArray/*!*/ Drop(CallSiteStorage<EachSite>/*!*/ each, object self, [DefaultProtocol]int count) {
            if (count < 0) {
                throw RubyExceptions.CreateArgumentError("attempt to drop negative size");
            } 
            
            var result = new RubyArray();

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (count > 0) {
                    count--;
                } else {
                    result.Add(item);
                }

                return null;
            }));

            return result;
        }

        [RubyMethod("drop_while")]
        public static Enumerator/*!*/ GetDropWhileEnumerator(CallSiteStorage<EachSite>/*!*/ each, BlockParam predicate, object self) {
            return new Enumerator((_, block) => DropWhile(each, block, self));
        }

        [RubyMethod("drop_while")]
        public static object DropWhile(CallSiteStorage<EachSite>/*!*/ each, [NotNull]BlockParam/*!*/ predicate, object self) {
            RubyArray resultArray = new RubyArray();

            bool dropping = true;
            object result = resultArray; 
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (dropping) {
                    object blockResult;
                    if (predicate.Yield(item, out blockResult)) {
                        result = blockResult;
                        return selfBlock.PropagateFlow(predicate, blockResult);
                    }

                    dropping = Protocols.IsTrue(blockResult);
                }

                if (!dropping) {
                    resultArray.Add(item);
                }

                return null;
            }));

            return result;
        }

        [RubyMethod("cycle")]
        public static Enumerator/*!*/ GetCycleEnumerator(CallSiteStorage<EachSite>/*!*/ each, BlockParam block, object self,
            [DefaultProtocol, DefaultParameterValue(Int32.MaxValue)]int iterations) {
            return new Enumerator((_, innerBlock) => Cycle(each, innerBlock, self, iterations));
        }

        [RubyMethod("cycle")]
        public static object Cycle(CallSiteStorage<EachSite>/*!*/ each, BlockParam block, object self, DynamicNull iterations) {
            return (block != null) ? Cycle(each, block, self, Int32.MaxValue) : GetCycleEnumerator(each, block, self, Int32.MaxValue);
        }

        [RubyMethod("cycle")]
        public static object Cycle(CallSiteStorage<EachSite>/*!*/ each, [NotNull]BlockParam/*!*/ block, object self,
            [DefaultProtocol, DefaultParameterValue(Int32.MaxValue)]int iterations) {

            if (iterations <= 0) {
                return null;
            }

            List<object> items = (iterations > 1) ? new List<object>() : null;

            // call "each" only in the first iteration:
            object result = null;
            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (block.Yield(item, out result)) {
                    iterations = -1;
                    return selfBlock.PropagateFlow(block, result);
                }
                if (items != null) {
                    items.Add(item);
                }
                return null;
            }));

            if (items == null) {
                return result;
            }

            // the rest of the iterations read from cached values:
            while (iterations == Int32.MaxValue || --iterations > 0) {
                foreach (var item in items) {
                    if (block.Yield(item, out result)) {
                        return result;
                    }
                }
            }

            return result;
        }

        #endregion

        #region each_cons, each_slice

        [RubyMethod("each_cons")]
        public static Enumerator/*!*/ GetEachConsEnumerator(CallSiteStorage<EachSite>/*!*/ each, BlockParam block, object self, [DefaultProtocol]int sliceSize) {
            return new Enumerator((_, innerBlock) => EachCons(each, innerBlock, self, sliceSize));
        }

        [RubyMethod("each_cons")]
        public static object EachCons(CallSiteStorage<EachSite>/*!*/ each, [NotNull]BlockParam/*!*/ block, object self, [DefaultProtocol]int sliceSize) {
            return EachSlice(each, block, self, sliceSize, false, (slice) => {
                RubyArray newSlice = new RubyArray(slice.Count);
                for (int i = 1; i < slice.Count; i++) {
                    newSlice.Add(slice[i]);
                }
                return newSlice;
            });
        }

        [RubyMethod("each_slice")]
        public static Enumerator/*!*/ GetEachSliceEnumerator(CallSiteStorage<EachSite>/*!*/ each, BlockParam block, object self, [DefaultProtocol]int sliceSize) {
            return new Enumerator((_, innerBlock) => EachSlice(each, innerBlock, self, sliceSize));
        }

        [RubyMethod("each_slice")]
        public static object EachSlice(CallSiteStorage<EachSite>/*!*/ each, [NotNull]BlockParam/*!*/ block, object self, [DefaultProtocol]int sliceSize) {
            return EachSlice(each, block, self, sliceSize, true, (slice) => null);
        }

        private static object EachSlice(CallSiteStorage<EachSite>/*!*/ each, BlockParam/*!*/ block, object self, int sliceSize,
            bool includeIncomplete, Func<RubyArray/*!*/, RubyArray>/*!*/ newSliceFactory) {

            if (sliceSize <= 0) {
                throw RubyExceptions.CreateArgumentError("invalid slice size");
            }

            RubyArray slice = null;

            object result = null;

            Each(each, self, Proc.Create(each.Context, delegate(BlockParam/*!*/ selfBlock, object _, object item) {
                if (slice == null) {
                    slice = new RubyArray(sliceSize);
                }

                slice.Add(item);

                if (slice.Count == sliceSize) {
                    var completeSlice = slice;
                    slice = newSliceFactory(slice);

                    object blockResult;
                    if (block.Yield(completeSlice, out blockResult)) {
                        result = blockResult;
                        return selfBlock.PropagateFlow(block, blockResult);
                    }
                }

                return null;
            }));

            if (slice != null && includeIncomplete) {
                object blockResult;
                if (block.Yield(slice, out blockResult)) {
                    return blockResult;
                }
            }

            return result;
        }

        #endregion

        #region enum_cons, enum_slice, enum_with_index

        [RubyMethod("enum_cons")]
        public static Enumerator/*!*/ GetConsEnumerator(object self, [DefaultProtocol]int sliceSize) {
            return new Enumerator(self, "each_cons", sliceSize);
        }

        [RubyMethod("enum_slice")]
        public static Enumerator/*!*/ GetSliceEnumerator(object self, [DefaultProtocol]int sliceSize) {
            return new Enumerator(self, "each_slice", sliceSize);
        }

        [RubyMethod("enum_with_index")]
        public static Enumerator/*!*/ GetEnumeratorWithIndex(object self) {
            return new Enumerator(self, "each_with_index", null);
        }

        #endregion

        // TODO: 
        // chunk
        // collect_concat
        // reverse_each
        // flat_map
        // join
    }
}
