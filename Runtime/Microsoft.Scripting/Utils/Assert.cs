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

#define DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Scripting.Utils {

    static class Assert {
        [Conditional("DEBUG")]
        public static void NotNull(object var) {
            Debug.Assert(var != null);
        }

        [Conditional("DEBUG")]
        public static void NotNull(object var1, object var2) {
            Debug.Assert(var1 != null && var2 != null);
        }

        [Conditional("DEBUG")]
        public static void NotNull(object var1, object var2, object var3) {
            Debug.Assert(var1 != null && var2 != null && var3 != null);
        }

        [Conditional("DEBUG")]
        public static void NotNullItems<T>(IEnumerable<T> items) where T : class {
            Debug.Assert(items != null);
            foreach (object item in items) {
                Debug.Assert(item != null);
            }
        }

        [Conditional("DEBUG")]
        public static void NotEmpty(string str) {
            Debug.Assert(!String.IsNullOrEmpty(str));
        }
    }
}
