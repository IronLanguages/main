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
using IronRuby.Builtins;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Math;
using IronRuby.Compiler;

namespace IronRuby.Tests {
    public partial class Tests {
        public void BigInteger1() {
            var bi = BigInteger.Zero;
            Assert(bi.GetWordCount() == 1);
            Assert(bi.GetWords().ValueEquals(new uint[] { 0 }));
            Assert(bi.GetByteCount() == 1);
            Assert(bi.ToByteArray().ValueEquals(new byte[] { 0 }));
            Assert(bi.GetBitCount() == 1);

            bi = BigInteger.One;
            Assert(bi.GetWordCount() == 1);
            Assert(bi.GetWords().ValueEquals(new uint[] { 1 }));
            Assert(bi.GetByteCount() == 1);
            Assert(bi.ToByteArray().ValueEquals(new byte[] { 1 }));
            Assert(bi.GetBitCount() == 1);
        }

        private BigInteger ParseBigInt(string str) {
            int i = 0;
            return Tokenizer.ParseInteger(str, 0, ref i).Bignum;
        }

        [Options(NoRuntime = true)]
        public void GcdLcm1() {
            BigInteger _4611686016279904256 = ParseBigInt("4611686016279904256");

            Assert((int)Integer.Gcd(-1, -1) == 1 && (int)Integer.Lcm(-1, -1) == 1);
            Assert((int)Integer.Gcd(0, -1)  == 1 && (int)Integer.Lcm(0, -1)  == 0);
            Assert((int)Integer.Gcd(-1, 0)  == 1 && (int)Integer.Lcm(-1, 0)  == 0);
            Assert((int)Integer.Gcd(0, 0)   == 0 && (int)Integer.Lcm(0, 0)   == 0);
            Assert((int)Integer.Gcd((BigInteger)0, 0) == 0 && (int)Integer.Lcm((BigInteger)0, 0) == 0);

            Assert((int)Integer.Gcd(Int32.MaxValue, Int32.MinValue) == 1);
            Assert((BigInteger)Integer.Lcm(Int32.MaxValue, Int32.MinValue) == _4611686016279904256);

            Assert((int)Integer.Gcd(Int32.MinValue, Int32.MaxValue) == 1);
            Assert((BigInteger)Integer.Lcm(Int32.MinValue, Int32.MaxValue) == _4611686016279904256);

            Assert((BigInteger)Integer.Gcd(Int32.MinValue, Int32.MinValue) == -(BigInteger)Int32.MinValue);
            Assert((BigInteger)Integer.Lcm(Int32.MinValue, Int32.MinValue) == -(BigInteger)Int32.MinValue);

            Assert((int)Integer.Gcd(Int32.MaxValue, Int32.MaxValue) == Int32.MaxValue);
            Assert((int)Integer.Lcm(Int32.MaxValue, Int32.MaxValue) == Int32.MaxValue);

            Assert((int)Integer.Gcd(Int32.MaxValue, -1) == 1);
            Assert((int)Integer.Lcm(Int32.MaxValue, -1) == Int32.MaxValue);

            Assert((int)Integer.Gcd(Int32.MinValue, -1) == 1);
            Assert((BigInteger)Integer.Lcm(Int32.MinValue, -1) == -(BigInteger)Int32.MinValue);
        }
    }
}