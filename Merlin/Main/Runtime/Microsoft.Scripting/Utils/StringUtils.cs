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
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.Scripting.Utils {
    internal static class StringUtils {

        public static Encoding DefaultEncoding {
            get {
#if !SILVERLIGHT
                return Encoding.Default;
#else
                return Encoding.UTF8;
#endif
            }
        }


        public static string[] Split(string str, char[] separators, int maxComponents, StringSplitOptions options) {
            ContractUtils.RequiresNotNull(str, "str");
#if SILVERLIGHT
            if (separators == null) return SplitOnWhiteSpace(str, maxComponents, options);

            bool keep_empty = (options & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries;

            List<string> result = new List<string>(maxComponents == Int32.MaxValue ? 1 : maxComponents + 1);

            int i = 0;
            int next;
            while (maxComponents > 1 && i < str.Length && (next = str.IndexOfAny(separators, i)) != -1) {

                if (next > i || keep_empty) {
                    result.Add(str.Substring(i, next - i));
                    maxComponents--;
                }

                i = next + 1;
            }

            if (i < str.Length || keep_empty) {
                result.Add(str.Substring(i));
            }

            return result.ToArray();
#else
            return str.Split(separators, maxComponents, options);
#endif
        }

#if SILVERLIGHT
        public static string[] SplitOnWhiteSpace(string str, int maxComponents, StringSplitOptions options) {
            ContractUtils.RequiresNotNull(str, "str");

            bool keep_empty = (options & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries;

            List<string> result = new List<string>(maxComponents == Int32.MaxValue ? 1 : maxComponents + 1);

            int i = 0;
            int next;
            while (maxComponents > 1 && i < str.Length && (next = IndexOfWhiteSpace(str, i)) != -1) {

                if (next > i || keep_empty) {
                    result.Add(str.Substring(i, next - i));
                    maxComponents--;
                }

                i = next + 1;
            }

            if (i < str.Length || keep_empty) {
                result.Add(str.Substring(i));
            }

            return result.ToArray();
        }

        public static int IndexOfWhiteSpace(string str, int start) {
            ContractUtils.RequiresNotNull(str, "str");
            if (start < 0 || start > str.Length) throw new ArgumentOutOfRangeException("start");

            while (start < str.Length && !Char.IsWhiteSpace(str[start])) start++;

            return (start == str.Length) ? -1 : start;
        }
#endif
    }
}
