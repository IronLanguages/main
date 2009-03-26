/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("_heapq", typeof(IronPython.Modules.PythonHeapq))]
namespace IronPython.Modules {
    [Documentation("Heap queue algorithm (a.k.a. priority queue).\n\n"
        + "Heaps are arrays for which a[k] <= a[2*k+1] and a[k] <= a[2*k+2] for\n"
        + "all k, counting elements from 0.  For the sake of comparison,\n"
        + "non-existing elements are considered to be infinite.  The interesting\n"
        + "property of a heap is that a[0] is always its smallest element.\n\n"
        + "Usage:\n\n"
        + "heap = []            # creates an empty heap\n"
        + "heappush(heap, item) # pushes a new item on the heap\n"
        + "item = heappop(heap) # pops the smallest item from the heap\n"
        + "item = heap[0]       # smallest item on the heap without popping it\n"
        + "heapify(x)           # transforms list into a heap, in-place, in linear time\n"
        + "item = heapreplace(heap, item) # pops and returns smallest item, and adds\n"
        + "                               # new item; the heap size is unchanged\n\n"
        + "Our API differs from textbook heap algorithms as follows:\n\n"
        + "- We use 0-based indexing.  This makes the relationship between the\n"
        + "  index for a node and the indexes for its children slightly less\n"
        + "  obvious, but is more suitable since Python uses 0-based indexing.\n\n"
        + "- Our heappop() method returns the smallest item, not the largest.\n\n"
        + "These two makeit possible to view the heap as a regular Python list\n"
        + "without surprises: heap[0] is the smallest item, and heap.sort()\n"
        + "maintains the heap invariant!\n"
        )]
    public static class PythonHeapq {
        #region public API

        [Documentation("Transform list into a heap, in-place, in O(len(heap)) time.")]
        public static void heapify(CodeContext/*!*/ context, object list) {
            List l = GetList(list);

            DoHeapify(l, context);
        }

        [Documentation("Pop the smallest item off the heap, maintaining the heap invariant.")]
        public static object heappop(CodeContext/*!*/ context, object list) {
            List l = GetList(list);

            int last = l.Count - 1;
            l.FastSwap(0, last);
            object ret = l.pop();
            SiftDown(l, context, 0, last - 1);
            return ret;
        }

        [Documentation("Push item onto heap, maintaining the heap invariant.")]
        public static void heappush(CodeContext/*!*/ context, object list, object item) {
            List l = GetList(list);

            l.append(item);
            SiftUp(l, context, l.Count - 1);
        }

        [Documentation("Push item on the heap, then pop and return the smallest item\n"
            + "from the heap. The combined action runs more efficiently than\n"
            + "heappush() followed by a separate call to heappop()."
            )]
        public static object heappushpop(CodeContext/*!*/ context, object list, object item) {
            List l = GetList(list);

            return DoPushPop(l, item, context);
        }

        [Documentation("Pop and return the current smallest value, and add the new item.\n\n"
            + "This is more efficient than heappop() followed by heappush(), and can be\n"
            + "more appropriate when using a fixed-size heap. Note that the value\n"
            + "returned may be larger than item!  That constrains reasonable uses of\n"
            + "this routine unless written as part of a conditional replacement:\n\n"
            + "        if item > heap[0]:\n"
            + "            item = heapreplace(heap, item)\n"
            )]
        public static object heapreplace(CodeContext/*!*/ context, object list, object item) {
            List l = GetList(list);

            object ret = l[0];
            l[0] = item;
            SiftDown(l, context, 0, l.Count - 1);
            return ret;
        }

        [Documentation("Find the n largest elements in a dataset.\n\n"
            + "Equivalent to:  sorted(iterable, reverse=True)[:n]\n"
            )]
        public static List nlargest(CodeContext/*!*/ context, object n, object iterable) {
            int nInt = Converter.ConvertToInt32(n);
            if (nInt <= 0) {
                return new List();
            }

            List ret = new List(Math.Min(nInt, 4000)); // don't allocate anything too huge
            IEnumerator en = PythonOps.GetEnumerator(iterable);

            // populate list with first n items
            for (int i = 0; i < nInt; i++) {
                if (!en.MoveNext()) {
                    // fewer than n items; finish up here
                    HeapSort(ret, context, true);
                    return ret;
                }
                ret.append(en.Current);
            }

            // go through the remainder of the iterator, maintaining a min-heap of the n largest values
            DoHeapify(ret, context);
            while (en.MoveNext()) {
                DoPushPop(ret, en.Current, context);
            }

            // return the largest items, in descending order
            HeapSort(ret, context, true);
            return ret;
        }

        [Documentation("Find the n smallest elements in a dataset.\n\n"
            + "Equivalent to:  sorted(iterable)[:n]\n"
            )]
        public static List nsmallest(CodeContext/*!*/ context, object n, object iterable) {
            int nInt = Converter.ConvertToInt32(n);
            if (nInt <= 0) {
                return new List();
            }

            List ret = new List(Math.Min(nInt, 4000)); // don't allocate anything too huge
            IEnumerator en = PythonOps.GetEnumerator(iterable);

            // populate list with first n items
            for (int i = 0; i < nInt; i++) {
                if (!en.MoveNext()) {
                    // fewer than n items; finish up here
                    HeapSort(ret, context);
                    return ret;
                }
                ret.append(en.Current);
            }

            // go through the remainder of the iterator, maintaining a max-heap of the n smallest values
            DoHeapifyMax(ret, context);
            while (en.MoveNext()) {
                DoPushPopMax(ret, en.Current, context);
            }

            // return the smallest items, in ascending order
            HeapSort(ret, context);
            return ret;
        }

        #endregion

        #region private implementation details

        private static List GetList(object list) {
            List ret = list as List;
            if (Object.ReferenceEquals(ret, null)) {
                throw PythonOps.TypeError("heap argument must be a list");
            }
            return ret;
        }

        private static bool IsLessThan(CodeContext/*!*/ context, object x, object y) {
            object ret;
            if (PythonTypeOps.TryInvokeBinaryOperator(context, x, y, Symbols.OperatorToSymbol(PythonOperationKind.LessThan), out ret) &&
                !Object.ReferenceEquals(ret, NotImplementedType.Value)) {
                return Converter.ConvertToBoolean(ret);
            } else if (PythonTypeOps.TryInvokeBinaryOperator(context, y, x, Symbols.OperatorToSymbol(PythonOperationKind.LessThanOrEqual), out ret) &&
                !Object.ReferenceEquals(ret, NotImplementedType.Value)) {
                return !Converter.ConvertToBoolean(ret);
            } else {
                return PythonContext.GetContext(context).LessThan(x, y);
            }
        }

        private static void HeapSort(List list, CodeContext/*!*/ context) {
            HeapSort(list, context, false);
        }

        private static void HeapSort(List list, CodeContext/*!*/ context, bool reverse) {
            // for an ascending sort (reverse = false), use a max-heap, and vice-versa
            if (reverse) {
                DoHeapify(list, context);
            } else {
                DoHeapifyMax(list, context);
            }

            int last = list.Count - 1;
            while (last > 0) {
                // put the root node (max if ascending, min if descending) at the end
                list.FastSwap(0, last);
                // shrink heap by 1
                last--;
                // maintain heap invariant
                if (reverse) {
                    SiftDown(list, context, 0, last);
                } else {
                    SiftDownMax(list, context, 0, last);
                }
            }
        }

        private static void DoHeapify(List list, CodeContext/*!*/ context) {
            int last = list.Count - 1;
            int start = (last - 1) / 2; // index of last parent node
            // Sift down each parent node from right to left.
            while (start >= 0) {
                SiftDown(list, context, start, last);
                start--;
            }
        }

        private static void DoHeapifyMax(List list, CodeContext/*!*/ context) {
            int last = list.Count - 1;
            int start = (last - 1) / 2; // index of last parent node
            // Sift down each parent node from right to left.
            while (start >= 0) {
                SiftDownMax(list, context, start, last);
                start--;
            }
        }

        private static object DoPushPop(List heap, object item, CodeContext/*!*/ context) {
            object first;
            if (heap.Count == 0 || !IsLessThan(context, first = heap[0], item)) {
                return item;
            }
            heap[0] = item;
            SiftDown(heap, context, 0, heap.Count - 1);
            return first;
        }

        private static object DoPushPopMax(List heap, object item, CodeContext/*!*/ context) {
            object first;
            if (heap.Count == 0 || !IsLessThan(context, item, first = heap[0])) {
                return item;
            }
            heap[0] = item;
            SiftDownMax(heap, context, 0, heap.Count - 1);
            return first;
        }

        private static void SiftDown(List heap, CodeContext/*!*/ context, int start, int stop) {
            int parent = start;
            int child;
            while ((child = parent * 2 + 1) <= stop) {
                // find the smaller sibling
                if (child + 1 <= stop && IsLessThan(context, heap[child + 1], heap[child])) {
                    child++;
                }
                // check if min-heap property is violated
                if (IsLessThan(context, heap[child], heap[parent])) {
                    heap.FastSwap(parent, child);
                    parent = child;
                } else {
                    return;
                }
            }
        }

        private static void SiftDownMax(List heap, CodeContext/*!*/ context, int start, int stop) {
            int parent = start;
            int child;
            while ((child = parent * 2 + 1) <= stop) {
                // find the larger sibling
                if (child + 1 <= stop && IsLessThan(context, heap[child], heap[child + 1])) {
                    child++;
                }
                // check if max-heap property is violated
                if (IsLessThan(context, heap[parent], heap[child])) {
                    heap.FastSwap(parent, child);
                    parent = child;
                } else {
                    return;
                }
            }
        }

        private static void SiftUp(List heap, CodeContext/*!*/ context, int index) {
            while (index > 0) {
                int parent = (index - 1) / 2;
                if (IsLessThan(context, heap[index], heap[parent])) {
                    heap.FastSwap(parent, index);
                    index = parent;
                } else {
                    return;
                }
            }
        }

        private static void SiftUpMax(List heap, CodeContext/*!*/ context, int index) {
            while (index > 0) {
                int parent = (index - 1) / 2;
                if (IsLessThan(context, heap[parent], heap[index])) {
                    heap.FastSwap(parent, index);
                    index = parent;
                } else {
                    return;
                }
            }
        }

        #endregion
    }
}
