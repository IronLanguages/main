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
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using System.Text;
using System.Reflection;

namespace IronRuby.Runtime {
    public static class Utils {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly byte[] EmptyBytes = new byte[0];

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly MemberInfo[] EmptyMemberInfos = new MemberInfo[0];

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly Delegate[] EmptyDelegates = new Delegate[0];
        
        public static int IndexOf(this string[]/*!*/ array, string/*!*/ value, StringComparer/*!*/ comparer) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.RequiresNotNull(comparer, "comparer");

            for (int i = 0; i < array.Length; i++) {
                if (comparer.Equals(array[i], value)) {
                    return i;
                }
            }

            return -1;
        }

        internal static bool IsAscii(this string/*!*/ str) {
            for (int i = 0; i < str.Length; i++) {
                if (str[i] > 0x7f) {
                    return false;
                }
            }
            return true;
        }

        public static int LastCharacter(this string/*!*/ str) {
            return str.Length == 0 ? -1 : str[str.Length - 1];
        }

        public static int IndexOf(this StringBuilder/*!*/ sb, char value) {
            ContractUtils.RequiresNotNull(sb, "sb");

            for (int i = 0; i < sb.Length; i++) {
                if (sb[i] == value) {
                    return i;
                }
            }

            return -1;
        }

        public static TOutput[]/*!*/ ConvertAll<TInput, TOutput>(this TInput[]/*!*/ array, Converter<TInput, TOutput>/*!*/ converter) {
            var result = new TOutput[array.Length];
            for (int i = 0; i < array.Length; i++) {
                result[i] = converter(array[i]);
            }
            return result;
        }

        [Conditional("DEBUG")]
        public static void Log(string/*!*/ message, string/*!*/ category) {
#if !SILVERLIGHT
            Debug.WriteLine((object)message, category);
#endif
        }

        public static long DateTimeTicksFromStopwatch(long elapsedStopwatchTicks) {
#if !SILVERLIGHT
            if (Stopwatch.IsHighResolution) {
                return (long)(((double)elapsedStopwatchTicks) * 10000000.0 / (double)Stopwatch.Frequency);
            }
#endif
            return elapsedStopwatchTicks;
        }
    }
}

#if SILVERLIGHT
namespace System.Diagnostics {
    internal struct Stopwatch {
        public void Start() {
        }

        public void Stop() {
        }

        public static long GetTimestamp() {
            return 0;
        }
    }
}
#endif
