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

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Enumerator = IronRuby.StandardLibrary.Enumerator.Enumerable.Enumerator;

namespace IronRuby.Builtins {

    // TODO: All of these methods use RubySites.Each, which is not ideal (shared DynamicSite).
    //       We could have one DynamicSite per method, but what we really want is for the 
    //       "each" site to be merged into the calling site (e.g. maybe use ActionOnCallable)
    [RubyModule("Enumerable")]
    public static class Enumerable {

        private static object Each(RubyContext/*!*/ context, object self, Proc/*!*/ block) {
            if (self is Enumerator) {
                return ((Enumerator)self).Each(context, block);
            } else {
                return RubySites.Each(context, self, block);
            }
        }

        #region all?, any?

        [RubyMethod("all?")]
        public static object TrueForAll(RubyContext/*!*/ context, BlockParam predicate, object self) {
            return TrueForItems(context, predicate, self, true);
        }

        [RubyMethod("any?")]
        public static object TrueForAny(RubyContext/*!*/ context, BlockParam predicate, object self) {
            return TrueForItems(context, predicate, self, false);
        }

        private static object TrueForItems(RubyContext/*!*/ context, BlockParam predicate, object self, bool expected) {
            bool result = expected;
            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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
        public static RubyArray Map(RubyContext/*!*/ context, BlockParam collector, object self) {
            RubyArray result = new RubyArray();
            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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
        public static object Find(RubyContext/*!*/ context, BlockParam predicate, object self, [Optional]object ifNone) {
            object result = Missing.Value;

            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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
                result = RubySites.Call(context, ifNone);
            }
            return result;
        }

        #endregion

        #region each_with_index

        [RubyMethod("each_with_index")]
        public static object EachWithIndex(RubyContext/*!*/ context, BlockParam/*!*/ block, object self) {
            // for some reason each_with_index always checks for a block, even if there's nothing to yield
            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            int index = 0;

            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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
        public static RubyArray/*!*/ ToArray(RubyContext/*!*/ context, object self) {
            RubyArray data = new RubyArray();

            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                data.Add(item);
                return null;
            }));

            return data;
        }

        #endregion

        #region find_all, select, reject

        [RubyMethod("find_all")]
        [RubyMethod("select")]
        public static RubyArray/*!*/ Select(RubyContext/*!*/ context, BlockParam predicate, object self) {
            return Filter(context, predicate, self, true);
        }

        [RubyMethod("reject")]
        public static RubyArray/*!*/ Reject(RubyContext/*!*/ context, BlockParam predicate, object self) {
            return Filter(context, predicate, self, false);
        }

        private static RubyArray/*!*/ Filter(RubyContext/*!*/ context, BlockParam predicate, object self, bool acceptingValue) {
            RubyArray result = new RubyArray();

            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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
        public static RubyArray Grep(RubyContext/*!*/ context, BlockParam action, object self, object pattern) {
            RubyArray result = new RubyArray();

            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (RubySites.CaseEqual(context, pattern, item)) {
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
        public static bool Contains(RubyContext/*!*/ context, object self, object value) {
            bool result = false;

            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
                if (Protocols.IsEqual(context, item, value)) {
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
        public static object Inject(RubyContext/*!*/ context, BlockParam operation, object self, [Optional]object initial) {
            object result = initial;

            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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
        public static object GetMaximum(RubyContext/*!*/ context, BlockParam comparer, object self) {
            return GetExtreme(context, comparer, self, -1/*look for max*/);
        }
        [RubyMethod("min")]
        public static object GetMinimum(RubyContext/*!*/ context, BlockParam comparer, object self) {
            return GetExtreme(context, comparer, self, 1/*look for min*/);
        }

        private static object GetExtreme(RubyContext/*!*/ context, BlockParam comparer, object self, int comparisonValue) {
            bool firstItem = true;
            object result = null;

            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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

                    compareResult = Protocols.ConvertCompareResult(context, blockResult);
                } else {
                    compareResult = Protocols.Compare(context, result, item);
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
        public static RubyArray/*!*/ Partition(RubyContext/*!*/ context, BlockParam predicate, object self) {
            RubyArray trueSet = new RubyArray();
            RubyArray falseSet = new RubyArray();

            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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
        public static object Sort(RubyContext/*!*/ context, BlockParam keySelector, object self) {
            return ArrayOps.SortInPlace(context, keySelector, ToArray(context, self));
        }

        [RubyMethod("sort_by")]
        public static RubyArray/*!*/ SortBy(RubyContext/*!*/ context, BlockParam keySelector, object self) {
            // collect key, value pairs
            List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();

            // Collect the key, value pairs
            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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
                return Protocols.Compare(context, x.Key, y.Key);
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
        public static RubyArray/*!*/ Zip(RubyContext/*!*/ context, BlockParam block, object self, [NotNull]params object[] args) {
            RubyArray results = (block == null) ? new RubyArray() : null;

            // Call to_a on each argument
            IList[] otherArrays = new IList[args.Length];
            for (int i = 0; i < args.Length; i++) {
                otherArrays[i] = Protocols.ConvertToArray(context, args[i]);
            }

            int index = 0;
            Each(context, self, Proc.Create(context, delegate(BlockParam/*!*/ selfBlock, object item) {
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
    }
}
