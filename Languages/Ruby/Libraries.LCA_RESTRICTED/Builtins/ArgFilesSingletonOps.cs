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
#if !SILVERLIGHT

using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using System.Runtime.InteropServices;

namespace IronRuby.Builtins {

    /// <summary>
    /// ARGF singleton trait.
    /// </summary>
    [RubyConstant("ARGF")]
    [RubySingleton(BuildConfig = "!SILVERLIGHT"), Includes(typeof(Enumerable))]
    public static class ArgFilesSingletonOps {
        #region to_i, fileno, to_s, to_a
        [RubyMethod("to_i")]
        [RubyMethod("fileno")]
        public static int FileNo(RubyContext/*!*/ context, object self) {
            return RubyIOOps.FileNo(context.InputProvider.GetCurrentStream());
        }

        [RubyMethod("file")]
        [RubyMethod("to_io")]
        public static RubyIO ToIO(RubyContext/*!*/ context, object self) {
            return RubyIOOps.ToIO(context.InputProvider.GetCurrentStream());
        }

        [RubyMethod("to_s")]
        public static MutableString ToS(RubyContext/*!*/ context, object self) {
            return MutableString.CreateAscii("ARGF");
        }

        [RubyMethod("to_a")]
        public static RubyArray/*!*/ TOA(RubyContext/*!*/ context, object self) {
            RubyArray result = new RubyArray();
            RubyArray lines;

            while (context.InputProvider.HasMoreFiles()) {
                lines = RubyIOOps.ReadLines(context, context.InputProvider.GetOrResetCurrentStream());
                //TODO: result.append(lines)???
                foreach (var line in lines) {
                    result.Add(line);
                }
            }
            return result;
        }
        #endregion

        #region pos, tell, pos=, lineno, lineno=
        [RubyMethod("pos")]
        [RubyMethod("tell")]
        public static object Pos(RubyContext/*!*/ context, object self) {
            return RubyIOOps.Pos(context.InputProvider.GetCurrentStream());
        }

        [RubyMethod("pos=")]
        public static void Pos(RubyContext/*!*/ context, object self, [DefaultProtocol]IntegerValue pos) {
            RubyIOOps.Pos(context.InputProvider.GetCurrentStream(), pos);
        }

        [RubyMethod("lineno=")]
        public static void SetLineNumber(RubyContext/*!*/ context, object self, [DefaultProtocol]int value) {
            RubyIOOps.SetLineNumber(context, context.InputProvider.GetCurrentStream(), value);
        }

        [RubyMethod("lineno")]
        public static int GetLineNumber(RubyContext/*!*/ context, object self) {
            return RubyIOOps.GetLineNumber(context.InputProvider.GetCurrentStream());
        }
        #endregion

        #region rewind, seek, skip
        [RubyMethod("rewind")]
        public static void Rewind(RubyContext/*!*/ context, object self) {
            RubyIOOps.Rewind(context, context.InputProvider.GetCurrentStream());
        }

        [RubyMethod("seek")]
        public static int Seek(RubyContext/*!*/ context, object self, [DefaultProtocol]IntegerValue pos, [DefaultProtocol, DefaultParameterValue(RubyIOOps.SEEK_SET)]int seekOrigin) {
            return RubyIOOps.Seek(context.InputProvider.GetCurrentStream(), pos, seekOrigin);
        }

        [RubyMethod("skip")]
        public static void Skip(RubyContext/*!*/ context, object self) {
            //TODO: this will currently fail specs since it will cause errors at
            //the end of the array, and it will have effect when called multiple times
            context.InputProvider.IncrementCurrentFileIndex();
        }
        #endregion

        #region each, each_line, each_byte
        [RubyMethod("each")]
        [RubyMethod("each_line")]
        public static object Each(RubyContext/*!*/ context, BlockParam block, object self){
            RubyIOOps.Each(context, block, context.InputProvider.GetOrResetCurrentStream());
            return self;
        }

        [RubyMethod("each")]
        [RubyMethod("each_line")]
        public static object Each(RubyContext/*!*/ context, BlockParam block, object self, [DefaultProtocol]MutableString separator) {
            RubyIOOps.Each(context, block, context.InputProvider.GetOrResetCurrentStream(), separator);
            return self;
        }

        [RubyMethod("each_byte")]
        public static object EachByte(RubyContext/*!*/ context, BlockParam block, object self) {
            RubyIOOps.EachByte(block, context.InputProvider.GetOrResetCurrentStream());
            return self;
        }
        #endregion

        #region read, readline, readlines
        [RubyMethod("readline")]
        public static MutableString/*!*/ ReadLine(RubyScope/*!*/ scope, object self) {
            return RubyIOOps.ReadLine(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream());
        }

        [RubyMethod("readline")]
        public static MutableString/*!*/ ReadLine(RubyScope/*!*/ scope, object self, [DefaultProtocol]MutableString separator) {
            return RubyIOOps.ReadLine(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream(), separator);
        }

        [RubyMethod("read")]
        public static MutableString/*!*/ Read(RubyContext/*!*/ context, object self) {
            return RubyIOOps.Read(context.InputProvider.GetOrResetCurrentStream());
        }

        [RubyMethod("read")]
        public static MutableString/*!*/ Read(RubyContext/*!*/ context, DynamicNull bytes, [DefaultProtocol, Optional]MutableString buffer) {
            return RubyIOOps.Read(context.InputProvider.GetOrResetCurrentStream(), bytes, buffer);
        }

        [RubyMethod("read")]
        public static MutableString/*!*/ Read(RubyContext/*!*/ context, [DefaultProtocol]int bytes, [DefaultProtocol, Optional]MutableString buffer) {
            return RubyIOOps.Read(context.InputProvider.GetOrResetCurrentStream(), bytes, buffer);
        }

        [RubyMethod("readchar")]
        public static int ReadChar(RubyContext/*!*/ context, object self) {
            return RubyIOOps.ReadChar(context.InputProvider.GetOrResetCurrentStream());
        }

        [RubyMethod("readlines")]
        public static RubyArray/*!*/ ReadLines(RubyContext/*!*/ context, object self) {
            return RubyIOOps.ReadLines(context, context.InputProvider.GetOrResetCurrentStream());
        }

        [RubyMethod("readlines")]
        public static RubyArray/*!*/ ReadLines(RubyContext/*!*/ context, object self, [DefaultProtocol]MutableString separator) {
            return RubyIOOps.ReadLines(context, context.InputProvider.GetOrResetCurrentStream(), separator);
        }
        #endregion

        [RubyMethod("eof")]
        [RubyMethod("eof?")]
        public static bool EoF(RubyContext/*!*/ context, object self) {
            return RubyIOOps.Eof(context.InputProvider.GetCurrentStream());
        }

        #region getc, gets
        [RubyMethod("getc")]
        public static object Getc(RubyContext/*!*/ context, object self) {
            return RubyIOOps.Getc(context.InputProvider.GetOrResetCurrentStream());
        }

        [RubyMethod("gets")]
        public static MutableString Gets(RubyScope/*!*/ scope, object self) {
            return Gets(scope, self, scope.RubyContext.InputSeparator);
        }

        [RubyMethod("gets")]
        public static MutableString Gets(RubyScope/*!*/ scope, object self, [DefaultProtocol]MutableString separator) {
            return RubyIOOps.Gets(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream(), separator);
        }
        #endregion

        [RubyMethod("path")]
        [RubyMethod("filename")]
        public static MutableString/*!*/ GetCurrentFileName(RubyContext/*!*/ context, object self) {
            return context.InputProvider.CurrentFileName;
        }

        #region close, closed?
        [RubyMethod("close")]
        public static object Close(RubyContext/*!*/ context, object self) {
            //TODO: See http://redmine.ruby-lang.org/issues/show/1633
            // this should raise an error if called twice on an object, currently 
            // it will just advance the stream which actually matches 1.8.6 behavior
            // but matz has said current 1.8.6 behavior is a bug.
            RubyIOOps.Close(context.InputProvider.GetOrResetCurrentStream());
            return self;
        }

        [RubyMethod("closed?")]
        public static bool Closed(RubyContext/*!*/ context, object self) {
            return RubyIOOps.Closed(context.InputProvider.GetCurrentStream());
        }
        #endregion

        [RubyMethod("binmode")]
        public static object BinMode(RubyContext/*!*/ context, object self) {
            RubyIOOps.Binmode(context.InputProvider.GetCurrentStream());
            context.InputProvider.DefaultMode = context.InputProvider.DefaultMode | IOMode.PreserveEndOfLines;
            return self;
        }

    }
}
#endif