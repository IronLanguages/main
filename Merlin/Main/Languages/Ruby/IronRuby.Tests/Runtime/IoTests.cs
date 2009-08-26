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
using IronRuby.Builtins;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using IronRuby.Runtime;
using Microsoft.Scripting;
using System.IO;
using Microsoft.Scripting.Utils;

namespace IronRuby.Tests {
    public partial class Tests {
        public void File1() {
            Test_Read1();
        }

        public void File2() {
            TestOutput(@"
stdout = IO.open(1, 'w', &nil) 
stdout.puts('hello')
", @"
hello
");
        }

        private class TestStream : MemoryStream {
            private readonly bool _canSeek;

            public TestStream(bool canSeek, byte[] data) 
                : base(data) {
                _canSeek = canSeek;
            }

            public override bool CanSeek {
                get { return false; }
            }
        }

        private byte[] B(string str) {
            return BinaryEncoding.Instance.GetBytes(str);
        }

        private void Test_Read1() {
            string s;
            string crlf = "\r\n";
            var stream = new TestStream(false, B(
                "ab\r\r\n" +
                "e" + (s = "fgh" + crlf + "ijkl" + crlf + "mnop" + crlf + crlf + crlf + crlf + "qrst") +
                crlf + "!"
            ));
            int s_crlf_count = 6;

            var io = new RubyBufferedStream(stream);
            Assert(io.PeekByte(0) == (byte)'a');

            var buffer = MutableString.CreateBinary(B("foo:"));
            Assert(io.AppendBytes(buffer, 4, false) == 4);
            Assert(buffer.ToString() == "foo:ab\r\n");

            buffer = MutableString.CreateBinary();
            Assert(io.AppendBytes(buffer, 1, false) == 1);
            Assert(buffer.ToString() == "e");

            buffer = MutableString.CreateMutable("x:", RubyEncoding.Binary);
            int c = s.Length - s_crlf_count - 2;
            Assert(io.AppendBytes(buffer, c, false) == c);
            Assert(buffer.ToString() == "x:" + s.Replace(crlf, "\n").Substring(0, c));

            buffer = MutableString.CreateBinary();
            Assert(io.AppendBytes(buffer, 10, false) == 4);
            Assert(buffer.ToString() == "st\n!");

            buffer = MutableString.CreateBinary();
            Assert(io.AppendBytes(buffer, 10, false) == 0);
            Assert(buffer.ToString() == "");

            stream = new TestStream(false, B(s = "abcd" + crlf + "xyz" + crlf + "qqq;"));
            io = new RubyBufferedStream(stream);
            buffer = MutableString.CreateBinary();
            Assert(io.AppendBytes(buffer, Int32.MaxValue, true) == s.Length);
            io.BaseStream.Seek(0, SeekOrigin.Begin);
            Assert(io.AppendBytes(buffer, Int32.MaxValue, false) == s.Length - 2);
            Assert(buffer.ToString() == s + s.Replace(crlf, "\n"));
        }
    }
}
