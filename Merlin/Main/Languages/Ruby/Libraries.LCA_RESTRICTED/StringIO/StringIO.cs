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

using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.StringIO {

    [RubyClass("StringIO")]
    public class StringIO : RubyIO {
        public StringIO(RubyContext/*!*/ context)
            : base(context) {
        }

        public StringIO(RubyContext/*!*/ context, MutableStringStream/*!*/ stream, string/*!*/ mode)
            : base(context, stream, mode) {
        }

        protected MutableStringStream Data {
            get {
                MutableStringStream stream = (this.Stream as MutableStringStream);
                if (stream == null) {
                    throw RubyExceptions.CreateArgumentError("stream is not a StringIO");
                }
                return stream;
            }
        }

        #region Public Singleton Methods

        [RubyConstructor]
        public static RubyIO CreateIO(RubyClass/*!*/ self, [Optional]MutableString initialString, [Optional]MutableString mode) {
            MutableStringStream stream = new MutableStringStream(initialString ?? MutableString.CreateBinary());
            string ioMode = (mode != null) ? mode.ConvertToString() : "rb+";
            return new StringIO(self.Context, stream, ioMode);
        }

        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static object OpenIO([NotNull]BlockParam/*!*/ block, RubyClass/*!*/ self, [Optional]MutableString initialString, [Optional]MutableString mode) {
            MutableStringStream stream = new MutableStringStream(initialString ?? MutableString.CreateBinary());
            string ioMode = (mode != null) ? mode.ConvertToString() : "rb+";
            RubyIO io = new StringIO(self.Context, stream, ioMode);

            object result;
            block.Yield(io, out result);
            if (!io.Closed) {
                io.Close();
            }
            return result;
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("length")]
        [RubyMethod("size")]
        public static int GetLength(StringIO/*!*/ self) {
            return (int)self.Data.Length;
        }

        [RubyMethod("path")]
        public static object GetPath(StringIO/*!*/ self) {
            return null;
        }

        [RubyMethod("string")]
        public static MutableString/*!*/ GetString(StringIO/*!*/ self) {
            return self.Data.String;
        }

        [RubyMethod("string=")]
        public static MutableString/*!*/ SetString(StringIO/*!*/ self, [NotNull]MutableString/*!*/ str) {
            self.Data.String = str;
            return str;
        }

        [RubyMethod("truncate")]
        public static int SetLength(StringIO/*!*/ self, int length) {
            self.Data.SetLength(length);
            return length;
        }

        [RubyMethod("ungetc")]
        public static void UnGetCharacter(StringIO/*!*/ self, [DefaultProtocol]int ch) {
            if (!RubyIO.IsReadable(self.Mode) || self.Closed) {
                throw RubyExceptions.CreateIOError("not opened for reading");
            }

            long offset = self.Data.Position;
            if (offset == 0) {
                return;
            }

            self.Data.String.SetByte((int)(offset - 1), (byte)ch);
            self.Data.Seek(-1, System.IO.SeekOrigin.Current);
        }

        #endregion
    }
}
