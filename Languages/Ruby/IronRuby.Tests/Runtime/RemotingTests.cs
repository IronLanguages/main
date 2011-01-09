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
using System.Text;
using IronRuby.Builtins;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Hosting;
using IronRuby.Runtime;

namespace IronRuby.Tests {
    public partial class Tests {
        [Options(NoRuntime = true)]
        public void Serialization1() {
            if (_driver.PartialTrust) return;

            var encodings = new object[] {
                RubyEncoding.EUCJP,
                RubyEncoding.UTF8,
                RubyEncoding.SJIS,
                RubyEncoding.Ascii,
                RubyEncoding.Binary,
            };

            Assert(ArrayUtils.ValueEquals(encodings, Roundtrip(encodings)));

            var s = MutableString.Create("foo", RubyEncoding.UTF8);
            var rs = Roundtrip(s);
            Assert(s.Equals(rs));

            var e = new Exception("msg");
            var ed = RubyExceptionData.GetInstance(e);
            ed.Backtrace = new RubyArray(new[] { 1, 2, 3 });

            var re = Roundtrip(e);
            var rde = RubyExceptionData.TryGetInstance(re);
            Assert(rde != null);
            Assert(ArrayUtils.ValueEquals(rde.Backtrace.ToArray(), new object[] { 1,2,3 }));
        }

        private T Roundtrip<T>(T obj) {
            var stream = new MemoryStream();
            var f = new BinaryFormatter();
            f.Serialize(stream, obj);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)f.Deserialize(stream);
        }
    }
}
