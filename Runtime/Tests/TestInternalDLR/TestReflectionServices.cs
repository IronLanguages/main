/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RowanTest.Common;
using System.Reflection;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace TestInternalDLR {
    public class TestReflectionServices : BaseTest {
        public void TestExtensionMethods1() {
            bool expectExact = typeof(Enumerable).Assembly.FullName == "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

            Dictionary<string, int> expected = new Dictionary<string, int>();
            foreach (string name in new[] {
                "AsQueryable", "AsQueryable", "Where", "Where", "OfType", "Cast", "Select", "Select", "SelectMany", "SelectMany", "SelectMany", "SelectMany", "Join", "Join", "GroupJoin", 
                "GroupJoin", "OrderBy", "OrderBy", "OrderByDescending", "OrderByDescending", "ThenBy", "ThenBy", "ThenByDescending", "ThenByDescending", "Take", "TakeWhile", 
                "TakeWhile", "Skip", "SkipWhile", "SkipWhile", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "Distinct", "Distinct", 
                "Concat", "Zip", "Union", "Union", "Intersect", "Intersect", "Except", "Except", "First", "First", "FirstOrDefault", "FirstOrDefault", "Last", "Last", 
                "LastOrDefault", "LastOrDefault", "Single", "Single", "SingleOrDefault", "SingleOrDefault", "ElementAt", "ElementAtOrDefault", "DefaultIfEmpty", "DefaultIfEmpty", 
                "Contains", "Contains", "Reverse", "SequenceEqual", "SequenceEqual", "Any", "Any", "All", "Count", "Count", "LongCount", "LongCount", "Min", "Min", "Max", "Max", 
                "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Average", "Average",
                "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average",
                "Average", "Average", "Average", "Aggregate", "Aggregate", "Aggregate", "Where", "Where", "Select", "Select", "SelectMany", "SelectMany", "SelectMany", "SelectMany", 
                "Take", "TakeWhile", "TakeWhile", "Skip", "SkipWhile", "SkipWhile", "Join", "Join", "GroupJoin", "GroupJoin", "OrderBy", "OrderBy", "OrderByDescending", 
                "OrderByDescending", "ThenBy", "ThenBy", "ThenByDescending", "ThenByDescending", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy", 
                "GroupBy", "Concat", "Zip", "Distinct", "Distinct", "Union", "Union", "Intersect", "Intersect", "Except", "Except", "Reverse", "SequenceEqual", "SequenceEqual",
                "AsEnumerable", "ToArray", "ToList", "ToDictionary", "ToDictionary", "ToDictionary", "ToDictionary", "ToLookup", "ToLookup", "ToLookup", "ToLookup", "DefaultIfEmpty", 
                "DefaultIfEmpty", "OfType", "Cast", "First", "First", "FirstOrDefault", "FirstOrDefault", "Last", "Last", "LastOrDefault", "LastOrDefault", "Single", "Single", 
                "SingleOrDefault", "SingleOrDefault", "ElementAt", "ElementAtOrDefault", "Any", "Any", "All", "Count", "Count", "LongCount", "LongCount", "Contains", "Contains", 
                "Aggregate", "Aggregate", "Aggregate", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", 
                "Sum", "Sum", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", 
                "Min", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", 
                "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average",
                "Average", "Average", "Average", "Average", "Average", "AsParallel", "AsParallel", "AsOrdered", "AsOrdered", "AsUnordered", "AsParallel", "AsSequential",
                "WithDegreeOfParallelism", "WithCancellation", "WithExecutionMode", "WithMergeOptions", "ForAll", "Where", "Where", "Select", "Select", "Zip", "Zip", "Join", "Join", 
                "Join", "Join", "GroupJoin", "GroupJoin", "GroupJoin", "GroupJoin", "SelectMany", "SelectMany", "SelectMany", "SelectMany", "OrderBy", "OrderBy", "OrderByDescending", 
                "OrderByDescending", "ThenBy", "ThenBy", "ThenByDescending", "ThenByDescending", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy", "GroupBy",
                "Aggregate", "Aggregate", "Aggregate", "Aggregate", "Aggregate", "Count", "Count", "LongCount", "LongCount", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", 
                "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Sum", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min",
                "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Min", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max",
                "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Max", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", 
                "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Average", "Any", "Any", "All", "Contains", "Contains", "Take", 
                "TakeWhile", "TakeWhile", "Skip", "SkipWhile", "SkipWhile", "Concat", "Concat", "SequenceEqual", "SequenceEqual", "SequenceEqual", "SequenceEqual", "Distinct", 
                "Distinct", "Union", "Union", "Union", "Union", "Intersect", "Intersect", "Intersect", "Intersect", "Except", "Except", "Except", "Except", "AsEnumerable", "ToArray",
                "ToList", "ToDictionary", "ToDictionary", "ToDictionary", "ToDictionary", "ToLookup", "ToLookup", "ToLookup", "ToLookup", "Reverse", "OfType", "Cast", "First", "First", 
                "FirstOrDefault", "FirstOrDefault", "Last", "Last", "LastOrDefault", "LastOrDefault", "Single", "Single", "SingleOrDefault", "SingleOrDefault", "DefaultIfEmpty", 
                "DefaultIfEmpty", "ElementAt", "ElementAtOrDefault", "Unwrap", "Unwrap", 
            }) {
                int count;
                expected.TryGetValue(name, out count);
                expected[name] = count + 1;
            }

            var methods = ReflectionUtils.GetVisibleExtensionMethods(typeof(Enumerable).Assembly);
            new List<MethodInfo>(ReflectionUtils.GetVisibleExtensionMethodsSlow(typeof(Enumerable).Assembly));

            Dictionary<string, int> actual = new Dictionary<string, int>();
            foreach (MethodInfo method in methods) {
                int count;
                if (!actual.TryGetValue(method.Name, out count)) {
                    count = 0;
                }
                actual[method.Name] = count + 1;
            }

            foreach (string name in actual.Keys) {
                Assert(expected.ContainsKey(name));
                Assert(expectExact ? expected[name] == actual[name] : expected[name] >= actual[name]);
            }
        }
    }
}
