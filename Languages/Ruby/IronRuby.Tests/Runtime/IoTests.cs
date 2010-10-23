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
using System.IO;
using Microsoft.Scripting.Utils;
using System.Text.RegularExpressions;

namespace IronRuby.Tests {
    public partial class Tests {
        public void File1() {
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

        [Options(NoRuntime = true)]
        public void File_AppendBytes1() {
            string s;
            string crlf = "\r\n";
            var stream = new TestStream(false, B(
                "ab\r\r\n" +
                "e" + (s = "fgh" + crlf + "ijkl" + crlf + "mnop" + crlf + crlf + crlf + crlf + "qrst") +
                crlf + "!"
            ));
            int s_crlf_count = 6;

            var io = new RubyBufferedStream(stream);
            Assert(io.PeekByte() == (byte)'a');

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

        [Options(NoRuntime = true)]
        public void File_WriteBytes1() {
            var stream = new MemoryStream();
            var io = new RubyBufferedStream(stream);

            io.Write(new byte[] { 0, 1, 2, 3 }, 1, 2);
            Assert(stream.ToArray().ValueEquals(new byte[] { 1, 2 }));
            stream.Seek(0, SeekOrigin.Begin);

            Assert(io.WriteBytes(new byte[] { 0, 1, 2, 3 }, 1, 2, true) == 2);
            Assert(stream.ToArray().ValueEquals(new byte[] { 1, 2 }));
            stream.Seek(0, SeekOrigin.Begin);

            Assert(io.WriteBytes(new byte[] { 0, 1, 2, 3 }, 1, 2, false) == 2);
            Assert(stream.ToArray().ValueEquals(new byte[] { 1, 2 }));
            stream.Seek(0, SeekOrigin.Begin);

            Assert(io.WriteBytes(new byte[] { 0, 1, (byte)'\n', 2 }, 1, 2, false) == 3);
            Assert(stream.ToArray().ValueEquals(new byte[] { 1, (byte)'\r', (byte)'\n' }));
            stream.Seek(0, SeekOrigin.Begin);
        }

        [Options(NoRuntime = true)]
        public void File_ReadLine1() {
            var stream = new TestStream(false, B(
                "a\r\n\r\nbbbbbbbbbbbbbbbbbbbbb1bbbbbbbbbbbbb2bbbbbbbbbbbbbbbbbbbbb3bbbbbbbbbbbbb4\rc\nd\n\n\n\nef")
            );

            foreach (int bufferSize in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100 }) {
                stream.Seek(0, SeekOrigin.Begin);
                var io = new RubyBufferedStream(stream, false, bufferSize);

                TestReadLine(io, true, "a\r\n");
                TestReadLine(io, true, "\r\n");
                TestReadLine(io, true, "bbbbbbbbbbbbbbbbbbbbb1bbbbbbbbbbbbb2bbbbbbbbbbbbbbbbbbbbb3bbbbbbbbbbbbb4\rc\n");
                TestReadLine(io, true, "d\n");
                TestReadLine(io, true, "\n");
                TestReadLine(io, true, "\n");
                TestReadLine(io, true, "\n");
                TestReadLine(io, true, "ef");
                TestReadLine(io, true, null);

                stream.Seek(0, SeekOrigin.Begin);
                TestReadLine(io, false, "a\n");
                TestReadLine(io, false, "\n");
                TestReadLine(io, false, "bbbbbbbbbbbbbbbbbbbbb1bbbbbbbbbbbbb2bbbbbbbbbbbbbbbbbbbbb3bbbbbbbbbbbbb4\rc\n");
                TestReadLine(io, false, "d\n");
                TestReadLine(io, false, "\n");
                TestReadLine(io, false, "\n");
                TestReadLine(io, false, "\n");
                TestReadLine(io, false, "ef");
                TestReadLine(io, false, null);
            }
        }

        private void TestReadLine(RubyBufferedStream/*!*/ io, bool preserveEolns, string expected) {
            var s = io.ReadLine(RubyEncoding.Binary, preserveEolns, -1);
            Assert(s == null && expected == null || s.ToString() == expected);
        }

        public class Pal1 : PlatformAdaptationLayer {
            // case sensitive
            public readonly Dictionary<string, bool> Entries = new Dictionary<string, bool>();

            public override void CreateDirectory(string path) {
                Entries[path] = true;
            }

            public override string[] GetFileSystemEntries(string path, string searchPattern, bool includeFiles, bool includeDirectories) {
                List<string> result = new List<string>();
                foreach (var entry in Entries) {
                    if (entry.Key.StartsWith(path)) {
                        result.Add(entry.Key);
                    }
                }
                return result.ToArray();
            }

            public override bool DirectoryExists(string path) {
                bool isDir;
                return Entries.TryGetValue(path, out isDir) && isDir;
            }

            public override bool FileExists(string path) {
                bool isDir;
                return Entries.TryGetValue(path, out isDir) && !isDir;
            }
        }

#if OBSOLETE // KCODE
        [Options(Pal = typeof(Pal1))]
        public void Dir1() {
            RubyClass dir = Context.GetClass(typeof(RubyDir));
            Pal1 pal = (Pal1)Context.Platform;
            var sjis = RubyEncoding.KCodeSJIS.StrictEncoding.GetBytes("ﾎ");
            var toPath = new ConversionStorage<MutableString>(Context);
            
            // transcode to UTF8 if no KCODE specified
            Context.KCode = null;
            RubyDir.MakeDirectory(toPath, dir, MutableString.CreateBinary(new byte[] { 0xce, 0xa3 }, RubyEncoding.Binary), null);
            Assert(pal.Entries["Σ"]);
            pal.Entries.Clear();

            // transcode to UTF8 if no KCODE specified
            Context.KCode = null;
            RubyDir.MakeDirectory(toPath, dir, MutableString.CreateMutable("ﾎｱ", RubyEncoding.KCodeSJIS), null);
            Assert(pal.Entries["α"]);
            Assert(FileTest.IsDirectory(toPath, Context.KernelModule, MutableString.CreateMutable("ﾎｱ", RubyEncoding.KCodeSJIS)));
            Assert(FileTest.IsDirectory(toPath, Context.KernelModule, MutableString.CreateMutable("α", RubyEncoding.KCodeUTF8)));
            pal.Entries.Clear();

            // transcode to KCODE if specified
            Context.KCode = RubyEncoding.KCodeUTF8;
            RubyDir.MakeDirectory(toPath, dir, MutableString.CreateBinary(new byte[] { 0xce, 0xa3 }, RubyEncoding.KCodeSJIS), null);
            Assert(pal.Entries["Σ"]);
            pal.Entries.Clear();

            // transcode to KCODE if specified
            Context.KCode = RubyEncoding.KCodeSJIS;
            RubyDir.MakeDirectory(toPath, dir, MutableString.CreateBinary(sjis, RubyEncoding.Binary), null);
            Assert(pal.Entries["ﾎ"]);
            pal.Entries.Clear();

            // ignore entries whose name cannot be encoded using the current KCODE
            Context.KCode = RubyEncoding.KCodeSJIS;
            AssertExceptionThrown<EncoderFallbackException>(() => RubyEncoding.KCodeSJIS.StrictEncoding.GetBytes("Ԋ"));
            pal.Entries["Ԋ"] = true;
            pal.Entries["ﾎ"] = true;
            var entries = RubyDir.GetEntries(toPath, dir, MutableString.CreateEmpty());

            Assert(entries.Count == 3);
            foreach (MutableString entry in entries) {
                Assert(entry.Encoding == RubyEncoding.KCodeSJIS);
            }

            Assert(((MutableString)entries[0]).Equals(MutableString.CreateAscii(".")));
            Assert(((MutableString)entries[1]).Equals(MutableString.CreateAscii("..")));
            Assert(((MutableString)entries[2]).Equals(MutableString.Create("ﾎ", RubyEncoding.KCodeSJIS)));
        }
#endif
        [Options(Pal = typeof(Pal1))]
        public void Dir2() {
            RubyClass dir = Context.GetClass(typeof(RubyDir));
            Pal1 pal = (Pal1)Context.Platform;
            var sjis = RubyEncoding.SJIS.StrictEncoding.GetBytes("ﾎ");
            var toPath = new ConversionStorage<MutableString>(Context);

            // use the string encoding if given
            RubyDir.MakeDirectory(toPath, dir, MutableString.CreateBinary(sjis, RubyEncoding.SJIS), null);
            Assert(pal.Entries["ﾎ"]);

            // IO system returns UTF8 encoded strings:
            var entries = RubyDir.GetEntries(toPath, dir, MutableString.CreateEmpty());
            Assert(entries.Count == 3);
            Assert(((MutableString)entries[0]).Equals(MutableString.CreateAscii(".")));
            Assert(((MutableString)entries[1]).Equals(MutableString.CreateAscii("..")));
            Assert(((MutableString)entries[2]).Equals(MutableString.Create("ﾎ", RubyEncoding.UTF8)));

            pal.Entries.Clear();
        }
    }
}
