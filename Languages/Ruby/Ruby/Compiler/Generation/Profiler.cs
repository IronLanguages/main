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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Scripting.Math;
using IronRuby.Builtins;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Generation {
    public sealed class Profiler {
        public struct MethodCounter {
            public readonly string/*!*/ Name;
            public readonly string/*!*/ File;
            public readonly int Line;
            public readonly long Ticks;

            public MethodCounter(string/*!*/ name, string/*!*/ file, int line, long ticks) {
                Name = name;
                File = file;
                Line = line;
                Ticks = ticks;
	        }

            public string/*!*/ Id {
                get {
                    return String.Format("{0};{1};{2}", Name, File, Line);
                }
            }
        }

        public static readonly Profiler/*!*/ Instance = new Profiler();
        internal static long[] _ProfileTicks = new long[100];
        
        private readonly Dictionary<string/*!*/, int>/*!*/ _counters;
        private readonly List<long[]>/*!*/ _profiles;
        private static int _Index;

        private Profiler() {
            _counters = new Dictionary<string, int>();
            _profiles = new List<long[]>();
        }

        public int GetTickIndex(string/*!*/ name) {
            int index;
            lock (_counters) {
                if (!_counters.TryGetValue(name, out index)) {
                    index = _Index++;
                    _counters.Add(name, index);
                }
                if (index >= _ProfileTicks.Length) {
                    long[] newProfile = new long[index * 2];
                    _profiles.Add(Interlocked.Exchange(ref _ProfileTicks, newProfile));
                }
            }
            return index;
        }

        public List<MethodCounter/*!*/>/*!*/ GetProfile() {
            var result = new List<MethodCounter>();
            lock (_counters) {
                // capture the current profile:
                long[] newProfile = new long[_ProfileTicks.Length];
                long[] total = Interlocked.Exchange(ref _ProfileTicks, newProfile);

                for (int i = 0; i < _profiles.Count; i++) {
                    for (int j = 0; j < total.Length; j++) {
                        if (j < _profiles[i].Length) {
                            total[j] += _profiles[i][j];
                        }
                    }
                }

                foreach (var counter in _counters) {
                    string methodName = counter.Key;
                    string fileName = null;
                    int line = 0;
                    if (RubyStackTraceBuilder.TryParseRubyMethodName(ref methodName, ref fileName, ref line)) {
                        result.Add(new MethodCounter(methodName, fileName, line, total[counter.Value]));
                    }
                }
            }

            return result;
        }
    }
}

namespace IronRuby.Runtime {
    public static partial class RubyOps {
        [Emitted]
        public static void UpdateProfileTicks(int index, long entryStamp) {
            Interlocked.Add(ref Profiler._ProfileTicks[index], Stopwatch.GetTimestamp() - entryStamp);
        }
    }
}
