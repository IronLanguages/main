using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;

[assembly: PythonModule("_bisect", typeof(IronPython.Modules.PythonBisectModule))]
namespace IronPython.Modules {
    class PythonBisectModule {
        public static string __doc__ = @"Bisection algorithms.

This module provides support for maintaining a list in sorted order without
having to sort the list after each insertion. For long lists of items with
expensive comparison operations, this can be an improvement over the more
common approach.
";

        #region Private Implementation Details

        private static void ParseArgs(object[] args, IDictionary<object, object> kwArgs, out object list, out object item, out int lo, out int hi) {
            list = null;
            item = null;
            lo = 0;
            hi = -1;

            if (args.Length > 0)
                list = args[0];
            if (args.Length > 1)
                item = args[1];
            if (args.Length > 2) {
                if (!(args[2] is int))
                    throw PythonOps.TypeError("an integer is required");
                lo = (int)args[2];
            }
            if (args.Length > 3) {
                if (!(args[3] is int))
                    throw PythonOps.TypeError("an integer is required");
                hi = (int)args[3];
            }

            if (kwArgs.ContainsKey("a"))
                list = kwArgs["a"];
            if (kwArgs.ContainsKey("x"))
                item = kwArgs["x"];

            if (list == null)
                throw PythonOps.TypeError("Required argument 'a' (pos 1) not found");
            if (item == null)
                throw PythonOps.TypeError("Required argument 'x' (pos 2) not found");

            if (kwArgs.ContainsKey("lo")) {
                object loObj = kwArgs["lo"];
                if (!(loObj is int)) {
                    throw PythonOps.TypeError("an integer is required");
                }
                lo = (int)loObj;
            }

            if (kwArgs.ContainsKey("hi")) {
                object hiObj = kwArgs["hi"];
                if (!(hiObj is int)) {
                    throw PythonOps.TypeError("an integer is required");
                }
                hi = (int)hiObj;
            }
        }
        
        private static int InternalBisectLeft(CodeContext/*!*/ context, object list, object item, int lo, int hi) {
            int mid, res;
            object litem;

            if (lo < 0)
                throw PythonOps.ValueError("lo must be non-negative");

            if (hi == -1) {
                hi = PythonOps.Length(list);
                if (hi < 0)
                    return -1;
            }
            
            while (lo < hi) {
                mid = (lo + hi) / 2;
                litem = PythonOps.GetIndex(context, list, mid);
                if (litem == null)
                    return -1;
                res = PythonOps.Compare(litem, item);
                if(PythonOps.CompareLessThan(res) == ScriptingRuntimeHelpers.True)
                    lo = mid + 1;
                else
                    hi = mid;
            }
            return lo;
        }

        private static int InternalBisectRight(CodeContext/*!*/ context, object list, object item, int lo, int hi) {
            object litem;
            int mid, res;

            if (lo < 0)
                throw PythonOps.ValueError("lo must be non-negative");
 
            if (hi == -1) {
                hi = PythonOps.Length(list);
                if (hi < 0)
                    return -1;
            }
            while (lo < hi) {
                mid = (lo + hi) / 2;
                litem = PythonOps.GetIndex(context, list, mid);
                if (litem == null)
                    return -1;
                res = PythonOps.Compare(item, litem);                
                if (PythonOps.CompareLessThan(res) == ScriptingRuntimeHelpers.True)
                    hi = mid;
                else
                    lo = mid + 1;
            }
            return lo;
        }
        
        #endregion

        #region Public API Surface

        [Documentation(@"bisect_right(a, x[, lo[, hi]]) -> index

Return the index where to insert item x in list a, assuming a is sorted.

The return value i is such that all e in a[:i] have e <= x, and all e in
a[i:] have e > x.  So if x already appears in the list, i points just
beyond the rightmost x already there

Optional args lo (default 0) and hi (default len(a)) bound the
slice of a to be searched.
")]
        public static object bisect_right(CodeContext/*!*/ context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args) {
            int lo = 0;
            int hi = -1;
            object list = null;
            object item = null;
            ParseArgs(args, kwArgs, out list, out item, out lo, out hi);
            int index = InternalBisectRight(context, list, item, lo, hi);
            if (index < 0)
                return null;
            return index;
        }

        [Documentation(@"insort_right(a, x[, lo[, hi]])

Insert item x in list a, and keep it sorted assuming a is sorted.

If x is already in a, insert it to the right of the rightmost x.

Optional args lo (default 0) and hi (default len(a)) bound the
slice of a to be searched.
")]
        public static void insort_right(CodeContext/*!*/ context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args) {
            int lo = 0;
            int hi = -1;
            object list = null;
            object item = null;
            ParseArgs(args, kwArgs, out list, out item, out lo, out hi);
            int index = InternalBisectRight(context, list, item, lo, hi);
            if(index < 0) {
                // TODO: need to raise exception here
            }
            if (list.GetType() == typeof(List)) { 
                // must check exact
                ((List)list).Insert(index, item);
            } else {
                PythonOps.Invoke(context, list, "insert", index, item);
            }
        }

        [Documentation(@"bisect_left(a, x[, lo[, hi]]) -> index

Return the index where to insert item x in list a, assuming a is sorted.

The return value i is such that all e in a[:i] have e < x, and all e in
a[i:] have e >= x.  So if x already appears in the list, i points just
before the leftmost x already there.

Optional args lo (default 0) and hi (default len(a)) bound the
slice of a to be searched.
")]
        public static object bisect_left(CodeContext/*!*/ context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args) {
            int lo = 0;
            int hi = -1;
            object list = null;
            object item = null;
            ParseArgs(args, kwArgs, out list, out item, out lo, out hi);
            int index = InternalBisectLeft(context, list, item, lo, hi);
            if(index < 0) {
                // throw exception?
            }
            return index;
        }

        [Documentation(@"insort_left(a, x[, lo[, hi]])

Insert item x in list a, and keep it sorted assuming a is sorted.

If x is already in a, insert it to the left of the leftmost x.

Optional args lo (default 0) and hi (default len(a)) bound the
slice of a to be searched.
")]
        public static void insort_left(CodeContext/*!*/ context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args) {
            int lo = 0;
            int hi = -1;
            object list = null;
            object item = null;
            ParseArgs(args, kwArgs, out list, out item, out lo, out hi);
            int index = InternalBisectLeft(context, list, item, lo, hi);
            if (index < 0) {
                // TODO: need to raise exception here?
            }

            if (list.GetType() == typeof(List)) {
                // must check exact
                ((List)list).Insert(index, item);
            } else {
                PythonOps.Invoke(context, list, "insert", index, item);
            }
        }

        [Documentation("Alias for bisect_right().")]
        public static object bisect(CodeContext/*!*/ context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args) {
            return bisect_right(context, kwArgs, args);
        }

        [Documentation("Alias for insort_right().")]
        public static void insort(CodeContext/*!*/ context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args) {
            insort_right(context, kwArgs, args);
        }

        #endregion
    }
}
