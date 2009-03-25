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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Ast = System.Linq.Expressions.Expression;

namespace IronRuby.Builtins {

    /// <summary>
    /// Implementation of IO builtin class. 
    /// </summary>
    [RubyClass("IO", Extends = typeof(RubyIO)), Includes(typeof(RubyFileOps.Constants), typeof(Enumerable))]
    public class RubyIOOps {

        #region Constants

        [RubyConstant]
        public const int SEEK_SET = RubyIO.SEEK_SET;

        [RubyConstant]
        public const int SEEK_CUR = RubyIO.SEEK_CUR;

        [RubyConstant]
        public const int SEEK_END = RubyIO.SEEK_END;

        #endregion

        #region Ruby Constructors

        [RubyConstructor]
        public static RubyIO/*!*/ CreateIO(RubyClass/*!*/ self) {
            // TODO: should create an IO object with an uninitialized stream
            throw new NotImplementedException();
        }

        [RubyConstructor]
        public static RubyIO/*!*/ CreateIO(RubyClass/*!*/ self, 
            [DefaultProtocol]int fileDescriptor, [DefaultProtocol, NotNull, Optional]MutableString modeString) {

            // TODO: a new RubyIO should be created here
            RubyIO result = self.Context.GetDescriptor(fileDescriptor);
            if (modeString != null) {
                result.ResetIOMode(modeString.ConvertToString());
            }
            return result;
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static void CreateIO(RubyIO/*!*/ self) {
            // TODO:
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static void CreateIO(RubyIO/*!*/ self,
            [DefaultProtocol]int fileDescriptor, [DefaultProtocol, NotNull, Optional]MutableString modeString) {

            // TODO:
            if (modeString != null) {
                self.ResetIOMode(modeString.ConvertToString());
            }
        }

        //initialize_copy

        [RubyMethod("for_fd", RubyMethodAttributes.PublicSingleton)]
        public static RuleGenerator/*!*/ ForFileDescriptor() {
            return new RuleGenerator(RuleGenerators.InstanceConstructor);
        }

        #endregion

        #region Public singleton methods

        internal static object TryInvokeOpenBlock(RubyContext/*!*/ context, BlockParam/*!*/ block, RubyIO/*!*/ io) {
            if (block == null)
                return io;

            using (io) {
                object result;
                block.Yield(io, out result);
                io.Close();
                return result;
            }
        }

        #region foreach

        [RubyMethod("foreach", RubyMethodAttributes.PublicSingleton)]
        public static void ForEach(BlockParam block, RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            ForEach(block, self, path, self.Context.InputSeparator);
        }

        [RubyMethod("foreach", RubyMethodAttributes.PublicSingleton)]
        public static void ForEach(BlockParam block, RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path, MutableString separator) {
            using (RubyIO io = new RubyIO(self.Context, File.OpenRead(path.ConvertToString()), "r")) {
                Each(block, io, separator);
            }
        }

        #endregion

        #region open

        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static RuleGenerator/*!*/ Open() {
            return new RuleGenerator((metaBuilder, args, name) => {
                var targetClass = (RubyClass)args.Target;
                targetClass.BuildObjectConstructionNoFlow(metaBuilder, args, name);

                // TODO: initialize yields the block?
                // TODO: null block check
                if (args.Signature.HasBlock) {
                    // ignore flow builder set up so far, we need one that creates a BlockParam for library calls:
                    metaBuilder.ControlFlowBuilder = null;

                    if (metaBuilder.BfcVariable == null) {
                        metaBuilder.BfcVariable = metaBuilder.GetTemporary(typeof(BlockParam), "#bfc");
                    }

                    metaBuilder.Result = Ast.Call(typeof(RubyIOOps).GetMethod("InvokeOpenBlock"), 
                        args.ContextExpression, 
                        metaBuilder.BfcVariable, 
                        metaBuilder.Result
                    );

                    RubyMethodGroupInfo.RuleControlFlowBuilder(metaBuilder, args);
                } else {
                    metaBuilder.BuildControlFlow(args);
                }
            });
        }

        [Emitted]
        public static object InvokeOpenBlock(RubyContext/*!*/ context, BlockParam/*!*/ block, object obj) {
            RubyIO io;
            if (!RubyOps.IsRetrySingleton(obj) && block != null && (io = obj as RubyIO) != null) {
                try {
                    block.Yield(io, out obj);
                } finally {
                    io.Close();
                }
            }
            return obj;
        }

        #endregion

        #region pipe, popen
#if !SILVERLIGHT

        //pipe

        [RubyMethod("popen", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static object OpenPipe(RubyContext/*!*/ context, BlockParam block, RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ command, [DefaultProtocol, Optional, NotNull]MutableString modeString) {

            return TryInvokeOpenBlock(context, block, OpenPipe(context, self, command, modeString));
        }

        [RubyMethod("popen", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static RubyIO/*!*/ OpenPipe(RubyContext/*!*/ context, RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ command, [DefaultProtocol, Optional, NotNull]MutableString modeString) {

            bool preserveEndOfLines;
            IOMode mode = RubyIO.ParseIOMode(modeString.ConvertToString(), out preserveEndOfLines);

            ProcessStartInfo startInfo = KernelOps.GetShell(context, command);
            startInfo.UseShellExecute = false;

            if (mode == IOMode.ReadOnlyFromStart) {
                startInfo.RedirectStandardOutput = true;
            } else if (mode == IOMode.WriteOnlyAppend || mode == IOMode.WriteOnlyTruncate) {
                startInfo.RedirectStandardInput = true;
            } else {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
            }

            Process process;
            try {
                process = Process.Start(startInfo);
            } catch (Exception e) {
                throw new Errno.NoEntryError(startInfo.FileName, e);
            }

            context.ChildProcessExitStatus = new RubyProcess.Status(process);

            StreamReader reader = null;
            StreamWriter writer = null;
            if (startInfo.RedirectStandardOutput) {
                reader = process.StandardOutput;
            }

            if (startInfo.RedirectStandardInput) {
                writer = process.StandardInput;
            }

            return new RubyIO(context, reader, writer, modeString.ConvertToString());
        }
#endif
        #endregion

        #region select

        [RubyMethod("select", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray Select(RubyContext/*!*/ context, object self, RubyArray read, [Optional]RubyArray write, [Optional]RubyArray error) {
            return SelectInternal(context, read, write, error, new TimeSpan(0, 0, 0, 0, Timeout.Infinite));
        }

        [RubyMethod("select", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray Select(RubyContext/*!*/ context, object self, RubyArray read, RubyArray write, RubyArray error, int timeoutInSeconds) {
            if (timeoutInSeconds < 0) {
                throw RubyExceptions.CreateArgumentError("time interval must be positive");
            }
            return SelectInternal(context, read, write, error, new TimeSpan(0, 0, timeoutInSeconds));
        }

        [RubyMethod("select", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray Select(RubyContext/*!*/ context, object self, RubyArray read, RubyArray write, RubyArray error, double timeoutInSeconds) {
            if (timeoutInSeconds < 0) {
                throw RubyExceptions.CreateArgumentError("time interval must be positive");
            }
            return SelectInternal(context, read, write, error, TimeSpan.FromSeconds(timeoutInSeconds));
        }

        private static RubyArray SelectInternal(RubyContext/*!*/ context, RubyArray read, RubyArray write, RubyArray error, TimeSpan timeout) {
            WaitHandle[] handles = null;
            RubyArray result;

            if (read == null && write == null && error == null) {
                Thread.Sleep(timeout);
                return null;
            }

            try {
                handles = GetWaitHandles(context, read, write, error);
                int index;
                try {
#if SILVERLIGHT
                    index = WaitHandle.WaitAny(handles, timeout);
#else
                    index = WaitHandle.WaitAny(handles, timeout, false);
#endif
                    if (index == WaitHandle.WaitTimeout) {
                        return null;
                    }
                } catch (Exception e) {
                    throw new Errno.InvalidError(e.Message, e);
                }

                result = new RubyArray();
                int handleIndex = 0;
                result.Add(MakeResult(handles, ref handleIndex, index, read));
                result.Add(MakeResult(handles, ref handleIndex, index, write));
                result.Add(MakeResult(handles, ref handleIndex, index, error));
            } finally {
                // should we close the handles? 
                //if (handles != null) {
                //    for (int i = 0; i < handles.Length; i++) {
                //        if (handles[i] != null) {
                //            handles[i].Close();
                //        }
                //    }
                //}
            }
            return result;
        }

        private static RubyArray/*!*/ MakeResult(WaitHandle/*!*/[]/*!*/ handles, ref int handleIndex, int signaling, RubyArray ioObjects) {
            RubyArray result = new RubyArray();
            if (ioObjects != null) {
                for (int i = 0; i < ioObjects.Count; i++) {
#if SILVERLIGHT
                    if (handleIndex == signaling || handles[handleIndex].WaitOne(0)) {
#else
                    if (handleIndex == signaling || handles[handleIndex].WaitOne(0, false)) {
#endif
                        result.Add(ioObjects[i]);
                    }
                    handleIndex++;
                }
            }
            return result;
        }

        private static WaitHandle/*!*/[]/*!*/ GetWaitHandles(RubyContext/*!*/ context, RubyArray read, RubyArray write, RubyArray error) {
            WaitHandle[] handles = new WaitHandle[
                (read != null ? read.Count : 0) +
                (write != null ? write.Count : 0) +
                (error != null ? error.Count : 0)
            ];

            int i = 0;
            if (read != null) {
                foreach (object obj in read) {
                    handles[i++] = ToIo(context, obj).CreateReadWaitHandle();
                }
            }

            if (write != null) {
                foreach (object obj in write) {
                    handles[i++] = ToIo(context, obj).CreateWriteWaitHandle();
                }
            }

            if (error != null) {
                foreach (object obj in error) {
                    handles[i++] = ToIo(context, obj).CreateErrorWaitHandle();
                }
            }

            return handles;
        }

        private static RubyIO/*!*/ ToIo(RubyContext/*!*/ context, object obj) {
            RubyIO io = obj as RubyIO;
            if (io == null) {
                throw RubyExceptions.CreateTypeConversionError(RubyUtils.GetClassName(context, obj), "IO");
            }
            return io;
        }

        #endregion

        //sysopen

        #endregion

        #region Public instance methods

        [RubyMethod("<<")]
        public static RubyIO Output(BinaryOpStorage/*!*/ writeStorage, RubyContext/*!*/ context, RubyIO/*!*/ self, object value) {
            Protocols.Write(writeStorage, context, self, value);
            return self;
        }

        [RubyMethod("binmode")]
        public static RubyIO/*!*/ Binmode(RubyIO/*!*/ self) {
            if (!self.Closed && self.Position == 0) {
                self.PreserveEndOfLines = true;
            }
            return self;
        }

        #region close, close_read, close_write, closed?, close_on_exec (1.9)

        [RubyMethod("close")]
        public static void Close(RubyIO/*!*/ self) {
            if (self.Closed) {
                throw RubyExceptions.CreateIOError("closed stream");
            }

            self.Close();
        }

        // TODO:
        [RubyMethod("close_read")]
        public static void CloseReader(RubyIO/*!*/ self) {
            self.CloseReader();
        }

        // TODO:
        [RubyMethod("close_write")]
        public static void CloseWriter(RubyIO/*!*/ self) {
            self.CloseWriter();
        }
        
        [RubyMethod("closed?")]
        public static bool Closed(RubyIO/*!*/ self) {
            return self.Closed;
        }

        // TODO: 1.9 only
        // close_on_exec=
        // close_on_exec?

        #endregion

        //reopen
        //stat

        [RubyMethod("eof")]
        [RubyMethod("eof?")]
        public static bool Eof(RubyIO/*!*/ self) {
            self.AssertOpenedForReading();
            return self.IsEndOfStream();
        }

        [RubyMethod("fcntl")]
        public static int FileControl(RubyIO/*!*/ self, [DefaultProtocol]int commandId, [Optional]MutableString arg) {
            return self.FileControl(commandId, (arg != null) ? arg.ConvertToBytes() : null);
        }

        [RubyMethod("fcntl")]
        public static int FileControl(RubyIO/*!*/ self, [DefaultProtocol]int commandId, int arg) {
            return self.FileControl(commandId, arg);
        }

        [RubyMethod("pid")]
        public static object Pid(RubyIO/*!*/ self) {
            return null;  // OK to return null on Windows
        }

        [RubyMethod("fileno")]
        [RubyMethod("to_i")]
        public static int FileNo(RubyIO/*!*/ self) {
            return self.FileDescriptor;
        }

        [RubyMethod("fsync")]
        [RubyMethod("flush")]
        public static void Flush(RubyIO/*!*/ self) {
            self.AssertOpenedForWriting();
            self.Flush();
        }

        //ioctl

        [RubyMethod("isatty")]
        [RubyMethod("tty?")]
        public static bool IsAtty(RubyIO/*!*/ self) {
            return self.IsConsole;
        }

        [RubyMethod("sync")]
        public static bool Sync(RubyIO/*!*/ self) {
            return self.AutoFlush;
        }

        [RubyMethod("sync=")]
        public static bool Sync(RubyIO/*!*/ self, bool sync) {
            self.AutoFlush = sync;
            return sync;
        }

        [RubyMethod("to_io")]
        public static RubyIO/*!*/ ToIO(RubyIO/*!*/ self) {
            return self;
        }

        #region external_encoding (1.9), internal_encoding (1.9), set_encoding (1.9)

        // TODO: 1.9 only
        [RubyMethod("external_encoding")]
        public static RubyEncoding GetExternalEncoding(RubyIO/*!*/ self) {
            return (self.ExternalEncoding != null) ? RubyEncoding.GetRubyEncoding(self.ExternalEncoding) : null;
        }

        // TODO: 1.9 only
        [RubyMethod("internal_encoding")]
        public static RubyEncoding GetInternalEncoding(RubyIO/*!*/ self) {
            return (self.InternalEncoding != null) ? RubyEncoding.GetRubyEncoding(self.InternalEncoding) : null;
        }

        // TODO: 1.9 only
        // set_encoding

        #endregion

        #region rewind, seek, pos, tell, lineno

        [RubyMethod("rewind")]
        public static void Rewind(RubyContext/*!*/ context, RubyIO/*!*/ self) {
            Seek(self, 0, 0);
            SetLineNo(context, self, 0);
        }

        private static void Seek(RubyIO/*!*/ self, long pos, int seekOrigin) {
            if (seekOrigin < 0 || seekOrigin > 2) {
                throw RubyExceptions.CreateArgumentError("Invalid argument");
            }

            if (self.IsConsoleDescriptor()) {
                throw new Errno.BadFileDescriptorError();
            }

            // TODO: make sure we assert stream is not actually closed
            if (self.Closed) {
                throw RubyExceptions.CreateArgumentError("trying to seek on a non-existent stream?");
            }

            SeekOrigin origin = SeekOrigin.Current;
            if (seekOrigin == SEEK_SET) {
                origin = SeekOrigin.Begin;
            } else if (seekOrigin == SEEK_END) {
                origin = SeekOrigin.End;
            }

            self.Seek(pos, origin);
        }

        [RubyMethod("seek")]
        public static int Seek(RubyIO/*!*/ self, [DefaultProtocol]int pos, [DefaultProtocol, DefaultParameterValue(SEEK_SET)]int seekOrigin) {
            Seek(self, (long)pos, seekOrigin);
            return 0;
        }

        [RubyMethod("seek")]
        public static int Seek(RubyIO/*!*/ self, [NotNull]BigInteger/*!*/ pos, [DefaultProtocol, DefaultParameterValue(SEEK_SET)]int seekOrigin) {
            long longPos;
            if (!pos.AsInt64(out longPos)) {
                throw RubyExceptions.CreateRangeError("bignum too big to convert into `long'");
            }
            Seek(self, longPos, seekOrigin);
            return 0;
        }

        [RubyMethod("lineno")]
        public static int GetLineNo(RubyContext/*!*/ context, RubyIO/*!*/ self) {
            return context.InputProvider.LastInputLineNumber;
        }

        [RubyMethod("lineno=")]
        public static void SetLineNo(RubyContext/*!*/ context, RubyIO/*!*/ self, [DefaultProtocol]int value) {
            context.InputProvider.LastInputLineNumber = value;
        }

        [RubyMethod("pos")]
        [RubyMethod("tell")]
        public static object Pos(RubyIO/*!*/ self) {
            if (self.IsConsoleDescriptor()) {
                throw new Errno.BadFileDescriptorError();
            }
            if (self.Closed) {
                throw RubyExceptions.CreateIOError("closed stream");
            }

            if (self.Position <= Int32.MaxValue) {
                return (int)self.Position;
            }

            return (BigInteger)self.Position;
        }

        [RubyMethod("pos=")]
        public static void Pos(RubyIO/*!*/ self, [DefaultProtocol]int value) {
            if (self.IsConsoleDescriptor()) {
                throw new Errno.BadFileDescriptorError();
            }

            self.Seek(value, SeekOrigin.Begin);
        }

        #endregion

        #region print, puts, putc

        // print, puts accept an arbitrary self object (it is called from Kernel#print, puts).
        
        [RubyMethod("print")]
        public static void Print(BinaryOpStorage/*!*/ writeStorage, RubyScope/*!*/ scope, object self) {
            Print(writeStorage, scope.RubyContext, self, scope.GetInnerMostClosureScope().LastInputLine);
        }

        [RubyMethod("print")]
        public static void Print(BinaryOpStorage/*!*/ writeStorage, RubyContext/*!*/ context, object self, object value) {
            Protocols.Write(writeStorage, context, self, value);
        }

        [RubyMethod("print")]
        public static void Print(BinaryOpStorage/*!*/ writeStorage, UnaryOpStorage/*!*/ tosStorage, RubyContext/*!*/ context, object self, 
            [NotNull]params object[]/*!*/ args) {
            MutableString delimiter = context.OutputSeparator;
            for (int i = 0; i < args.Length; i++) {
                MutableString str = ToPrintedString(tosStorage, context, args[i]);
                if (delimiter != null) {
                    str.Append(delimiter);
                }
                Print(writeStorage, context, self, str);
            }
        }

        [RubyMethod("putc")]
        public static MutableString/*!*/ Putc(BinaryOpStorage/*!*/ writeStorage, RubyContext/*!*/ context, object self, [NotNull]MutableString/*!*/ val) {
            if (val.IsEmpty) {
                throw RubyExceptions.CreateTypeError("can't convert String into Integer");
            }

            // writes a single byte into the output stream:
            var c = MutableString.CreateBinary(val.GetBinarySlice(0, 1));
            Protocols.Write(writeStorage, context, self, c);
            return val;
        }

        [RubyMethod("putc")]
        public static int Putc(BinaryOpStorage/*!*/ writeStorage, RubyContext/*!*/ context, object self, [DefaultProtocol]int c) {
            MutableString str = MutableString.CreateBinary(1).Append(unchecked((byte)c));
            Protocols.Write(writeStorage, context, self, str);
            return c;
        }

        private static readonly MutableString NewLine = MutableString.CreateMutable("\n").Freeze();

        public static MutableString/*!*/ ToPrintedString(UnaryOpStorage/*!*/ tosStorage, RubyContext/*!*/ context, object obj) {
            IDictionary<object, object> hash;
            List<object> list;
            MutableString str;

            if ((list = obj as List<object>) != null) {
                return IListOps.Join(tosStorage, context, list, NewLine);
            } else if ((hash = obj as IDictionary<object, object>) != null) {
                return IDictionaryOps.ToString(tosStorage, context, hash);
            } else if (obj == null) {
                return MutableString.Create("nil");
            } else if (obj is bool) {
                return MutableString.Create((bool)obj ? "true" : "false");
            } else if (obj is double) {
                var result = MutableString.Create(obj.ToString());
                if ((double)(int)(double)obj == (double)obj) {
                    result.Append(".0");
                }
                return result;
            } else if ((str = obj as MutableString) != null) {
                return str;
            } else {
                return RubyUtils.ObjectToMutableString(tosStorage, context, obj);
            }
        }

        [RubyMethod("puts")]
        public static void PutsEmptyLine(BinaryOpStorage/*!*/ writeStorage, RubyContext/*!*/ context, object self) {
            Protocols.Write(writeStorage, context, self, MutableString.CreateMutable("\n"));
        }

        [RubyMethod("puts")]
        public static void Puts(BinaryOpStorage/*!*/ writeStorage, RubyContext/*!*/ context, object self, [NotNull]MutableString/*!*/ str) {
            Protocols.Write(writeStorage, context, self, str);

            if (!str.EndsWith('\n')) {
                PutsEmptyLine(writeStorage, context, self);
            }
        }

        [RubyMethod("puts")]
        public static void Puts(BinaryOpStorage/*!*/ writeStorage, UnaryOpStorage/*!*/ tosStorage, RubyContext/*!*/ context, object self, [NotNull]object/*!*/ val) {
            Puts(writeStorage, context, self, ToPrintedString(tosStorage, context, val));
        }

        [RubyMethod("puts")]
        public static void Puts(BinaryOpStorage/*!*/ writeStorage, UnaryOpStorage/*!*/ tosStorage, RubyContext/*!*/ context, object self, [NotNull]params object[]/*!*/ vals) {
            for (int i = 0; i < vals.Length; i++) {
                Puts(writeStorage, tosStorage, context, self, vals[i]);
            }
        }

        [RubyMethod("printf")]
        public static void PrintFormatted(
            StringFormatterSiteStorage/*!*/ storage, 
            ConversionStorage<MutableString>/*!*/ stringCast, 
            BinaryOpStorage/*!*/ writeStorage,
            RubyContext/*!*/ context, RubyIO/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ format, [NotNull]params object[]/*!*/ args) {

            KernelOps.PrintFormatted(storage, stringCast, writeStorage, context, null, self, format, args);
        }

        #endregion

        #region write, write_nonblock

        [RubyMethod("write")]
        public static int Write(RubyIO/*!*/ self, [NotNull]MutableString/*!*/ val) {
            self.AssertOpenedForWriting();
            int bytesWritten = self.Write(val);
            if (self.AutoFlush) {
                self.Flush();
            }
            return bytesWritten;
        }

        [RubyMethod("write")]
        public static int Write(ConversionStorage<MutableString>/*!*/ tosStorage, RubyContext/*!*/ context, RubyIO/*!*/ self, object obj) {
            return Write(self, Protocols.ConvertToString(tosStorage, context, obj));
        }

        //write_nonblock
        
        #endregion

        #region read, read_nonblock

        private static RubyIO/*!*/ OpenFileForRead(RubyContext/*!*/ context, MutableString/*!*/ path) {
            string strPath = path.ConvertToString();
            if (!File.Exists(strPath)) {
                throw new Errno.NoEntryError(String.Format("No such file or directory - {0}", strPath));
            }
            return new RubyIO(context, File.OpenRead(strPath), "r");
        }
        
        private static byte[]/*!*/ ReadAllBytes(RubyIO/*!*/ io) {
            var fixedBuffer = new byte[io.Length];
            io.ReadBytes(fixedBuffer, 0, (int)io.Length);
            return fixedBuffer;
        }

        [RubyMethod("read")]
        public static MutableString/*!*/ Read(RubyIO/*!*/ self) {
            self.AssertOpenedForReading();

            if (!self.PreserveEndOfLines) {
                MutableString result = MutableString.CreateBinary();
                int c;
                while ((c = self.ReadByteNormalizeEoln()) != -1) {
                    result.Append((byte)c);
                }
                return result;
            } else {
                // TODO: change this once Binary mutable string uses resizable byte[] instead of List<byte>
                return MutableString.CreateBinary(ReadAllBytes(self));
            }
        }

        [RubyMethod("read")]
        public static MutableString/*!*/ Read(RubyIO/*!*/ self, [DefaultProtocol]int bytes, [DefaultProtocol, Optional]MutableString buffer) {
            self.AssertOpenedForReading();
            if (self.IsEndOfStream()) {
                return null;
            }

            if (buffer == null) {
                buffer = MutableString.CreateBinary();
            }

            buffer.Clear();
            if (!self.PreserveEndOfLines) {
                for (int i = 0; i < bytes; ++i) {
                    int c = self.ReadByteNormalizeEoln();
                    if (c == -1) {
                        return buffer;
                    } else {
                        buffer.Append((byte)c);
                    }
                }
            } else {
                var fixedBuffer = new byte[bytes];
                bytes = self.ReadBytes(fixedBuffer, 0, bytes);
                buffer.Append(fixedBuffer, 0, bytes);
            }

            return buffer;
        }

        [RubyMethod("read", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ ReadFile(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ path) {

            using (RubyIO io = OpenFileForRead(self.Context, path)) {
                return Read(io);
            }
        }

        [RubyMethod("read", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ Read(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ path, [DefaultProtocol]int length, [DefaultProtocol, Optional]int offset) {

            if (offset < 0) {
                throw new Errno.InvalidError();
            }

            if (length < 0) {
                throw RubyExceptions.CreateArgumentError(String.Format("negative length {0} given", length));
            }

            using (RubyIO io = OpenFileForRead(self.Context, path)) {
                if (offset > 0) {
                    io.Seek(offset, SeekOrigin.Begin);
                }
                return Read(io, length, null);
            }
        }

        //read_nonblock

        #endregion

        #region readchar, readbyte (1.9), readline, readlines

        // returns a string in 1.9
        [RubyMethod("readchar")]
        public static int ReadChar(RubyIO/*!*/ self) {
            self.AssertOpenedForReading();
            int c = self.ReadByteNormalizeEoln();
            
            if (c == -1) {
                throw new EOFError("end of file reached");
            }

            return c;
        }

        // readbyte

        [RubyMethod("readline")]
        public static MutableString/*!*/ ReadLine(RubyScope/*!*/ scope, RubyIO/*!*/ self) {
            return ReadLine(scope, self, scope.RubyContext.InputSeparator);
        }

        [RubyMethod("readline")]
        public static MutableString/*!*/ ReadLine(RubyScope/*!*/ scope, RubyIO/*!*/ self, [DefaultProtocol]MutableString separator) {
            // no dynamic call, modifies $_ scope variable:
            MutableString result = Gets(scope, self);
            if (result == null) {
                throw new EOFError("end of file reached");
            }

            return result;
        }

        [RubyMethod("readlines")]
        public static RubyArray/*!*/ ReadLines(RubyContext/*!*/ context, RubyIO/*!*/ self) {
            return ReadLines(context, self, context.InputSeparator);
        }

        [RubyMethod("readlines")]
        public static RubyArray/*!*/ ReadLines(RubyContext/*!*/ context, RubyIO/*!*/ self, [DefaultProtocol]MutableString separator) {
            RubyArray result = new RubyArray();

            // no dynamic call, doesn't modify $_ scope variable:
            MutableString line;
            while ((line = self.ReadLineOrParagraph(separator)) != null) {
                result.Add(line);
            }

            context.InputProvider.LastInputLineNumber += result.Count;
            return result;
        }

        [RubyMethod("readlines", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ ReadLines(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ path) {

            return ReadLines(self, path, self.Context.InputSeparator);
        }

        [RubyMethod("readlines", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ ReadLines(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString path, [DefaultProtocol]MutableString separator) {

            using (RubyIO io = new RubyIO(self.Context, File.OpenRead(path.ConvertToString()), "r")) {
                return ReadLines(self.Context, io, separator);
            }
        }



        #endregion

        #region getc, gets, ungetc, getbyte (1.9)

        [RubyMethod("getc")]
        public static object Getc(RubyIO/*!*/ self) {
            self.AssertOpenedForReading();
            int c = self.ReadByteNormalizeEoln();
            return (c != -1) ? ScriptingRuntimeHelpers.Int32ToObject(c) : null;
        }

        [RubyMethod("gets")]
        public static MutableString Gets(RubyScope/*!*/ scope, RubyIO/*!*/ self) {
            return Gets(scope, self, scope.RubyContext.InputSeparator);
        }

        [RubyMethod("gets")]
        public static MutableString Gets(RubyScope/*!*/ scope, RubyIO/*!*/ self, [DefaultProtocol]MutableString separator) {

            MutableString result = self.ReadLineOrParagraph(separator);
            KernelOps.Taint(scope.RubyContext, result);

            scope.GetInnerMostClosureScope().LastInputLine = result;
            scope.RubyContext.InputProvider.IncrementLastInputLineNumber();

            return result;
        }

        // TODO: 1.9 only
        // getbyte

        //ungetc

        #endregion

        #region each, each_byte, each_line

        [RubyMethod("each")]
        [RubyMethod("each_line")]
        public static void Each(RubyContext/*!*/ context, BlockParam block, RubyIO/*!*/ self) {
            Each(block, self, context.InputSeparator);
        }

        [RubyMethod("each")]
        [RubyMethod("each_line")]
        public static object Each(BlockParam block, RubyIO/*!*/ self, [DefaultProtocol]MutableString separator) {
            self.AssertOpenedForReading();

            MutableString line;
            while ((line = self.ReadLineOrParagraph(separator)) != null) {
                if (block == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                KernelOps.Taint(block.RubyContext, line);

                object result;
                if (block.Yield(line, out result)) {
                    return result;
                }
            }

            return self;
        }

        [RubyMethod("each_byte")]
        public static object EachByte(BlockParam block, RubyIO/*!*/ self) {
            self.AssertOpenedForReading();
            object aByte;
            while ((aByte = Getc(self)) != null) {
                if (block == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object result;
                if (block.Yield((int)aByte, out result)) {
                    return result;
                }
            }
            return self;
        }

        #endregion

        // TODO: 1.9 only
        // bytes -> Enumerable::Enumerator
        // lines -> Enumerable::Enumerator

        //readpartial

        [RubyMethod("sysread")]
        public static MutableString/*!*/ SystemRead(RubyIO/*!*/ self, [DefaultProtocol]int bytes) {
            var fixedBuffer = new byte[bytes];
            int len = self.ReadBytes(fixedBuffer, 0, bytes);
            if (len == 0) {
                throw new EOFError("end of file reached");
            }

            MutableString result = MutableString.CreateBinary();
            result.Append(fixedBuffer, 0, len);
            return result;
        }

        //sysseek
        //syswrite

        #endregion

        public static IOWrapper/*!*/ CreateIOWrapper(RespondToStorage/*!*/ respondToStorage, 
            RubyContext/*!*/ context, object io, FileAccess access) {

            bool canRead, canWrite, canSeek, canFlush, canBeClosed;

            if (access == FileAccess.Read || access == FileAccess.ReadWrite) {
                canRead = Protocols.RespondTo(respondToStorage, context, io, "read");
            } else {
                canRead = false;
            }

            if (access == FileAccess.Write || access == FileAccess.ReadWrite) {
                canWrite = Protocols.RespondTo(respondToStorage, context, io, "write");
            } else {
                canWrite = false;
            }

            canSeek = Protocols.RespondTo(respondToStorage, context, io, "seek") && Protocols.RespondTo(respondToStorage, context, io, "tell");
            canFlush = Protocols.RespondTo(respondToStorage, context, io, "flush");
            canBeClosed = Protocols.RespondTo(respondToStorage, context, io, "close");

            return new IOWrapper(context, io, canRead, canWrite, canSeek, canFlush, canBeClosed);
        }
    }
}
