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
using IronPython.Runtime;
using IronPython.Runtime.Operations;

using Microsoft.Scripting.Math; 

[assembly: PythonModule("_random", typeof(IronPython.Modules.PythonRandom))]
namespace IronPython.Modules {
    public static class PythonRandom {
        public const string __doc__ = "implements a random number generator";

        [PythonType]
        public class Random {
            private System.Random _rnd;

            public Random() {
                seed();
            }

            public Random(object seed) {
                this.seed(seed);
            }

            #region Public API surface

            public object getrandbits(int bits) {
                if (bits <= 0) {
                    throw PythonOps.ValueError("number of bits must be greater than zero");
                }

                int count;
                try {
                    count = checked((bits + 7) / 8);
                } catch (OverflowException) {
                    throw PythonOps.MemoryError("not enough memory to get all bits");
                }

                byte[] bytes = new byte[count];
                lock (this) {
                    _rnd.NextBytes(bytes);
                }

                if (bits <= 32) {
                    return (int)getfour(bytes, 0, bits);
                } else if (bits <= 64) {
                    long a = getfour(bytes, 0, bits);
                    long b = getfour(bytes, 32, bits);
                    return a | (b << 32);
                } else {
                    count = (count + 3) / 4;
                    uint[] data = new uint[count];
                    for (int i = 0; i < count; i++) {
                        data[i] = getfour(bytes, i * 32, bits);
                    }
                    int sign = (data[data.Length - 1] & 0x80000000) != 0 ? -1 : 1;
                    return new BigInteger(sign, data);
                }
            }

            public object getstate() {
                return _rnd;
            }

            public void jumpahead(int count) {
                lock (this) {
                    _rnd.NextBytes(new byte[4096]);
                }
            }

            public object random() {
                lock (this) {
                    return _rnd.NextDouble();
                }
            }

            public void seed() {
                seed(DateTime.Now);
            }

            public void seed(object s) {
                int newSeed;
                if (s is int) {
                    newSeed = (int)s;
                } else {
                    newSeed = s.GetHashCode();
                }
                lock (this) {
                    _rnd = new System.Random(newSeed);
                }
            }

            public void setstate(object state) {
                System.Random random = state as System.Random;

                lock (this) {
                    if (random != null) {
                        _rnd = random;
                    } else {
                        throw IronPython.Runtime.Operations.PythonOps.TypeError("setstate: argument must be value returned from getstate()");
                    }
                }
            }

            #endregion

            #region Private implementation details

            private static uint getfour(byte[] bytes, int start, int end) {
                uint four = 0;
                int bits = end - start;
                int shift = 0;
                if (bits > 32) bits = 32;
                start /= 8;
                while (bits > 0) {
                    uint value = bytes[start];
                    if (bits < 8) value &= (1u << bits) - 1u;
                    value <<= shift;
                    four |= value;
                    bits -= 8;
                    shift += 8;
                    start++;
                }

                return four;
            }

            #endregion
        }
    }
}
