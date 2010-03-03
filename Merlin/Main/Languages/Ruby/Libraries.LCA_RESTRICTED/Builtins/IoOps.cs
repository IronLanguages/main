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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    using Ast = Expression;
    using Utils = IronRuby.Runtime.Utils;

    /// <summary>
    /// Implementation of IO builtin class. 
    /// </summary>
    [RubyClass("IO", Extends = typeof(RubyIO)), Includes(typeof(RubyFileOps.Constants), typeof(Enumerable))]
    [Includes(typeof(PrintOps), Copy = true)]
    public class RubyIOOps {
        internal static Stream/*!*/ GetDescriptorStream(RubyContext/*!*/ context, int descriptor) {
            Stream stream = context.GetStream(descriptor);
            if (stream == null) {
                throw RubyExceptions.CreateEBADF();
            }
            return stream;
        }

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
        public static RubyIO/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol]int descriptor, 
            [DefaultProtocol, Optional, NotNull]MutableString mode) {
            return Create(self, descriptor, (int)IOModeEnum.Parse(mode));
        }

        [RubyConstructor]
        public static RubyIO/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol]int descriptor, int mode) {
            return new RubyIO(self.Context, GetDescriptorStream(self.Context, descriptor), descriptor, (IOMode)mode);
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyIO/*!*/ Reinitialize(RubyIO/*!*/ self, [DefaultProtocol]int descriptor,
            [DefaultProtocol, Optional, NotNull]MutableString mode) {
            return Reinitialize(self, descriptor, (int)IOModeEnum.Parse(mode));
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyIO/*!*/ Reinitialize(RubyIO/*!*/ self, [DefaultProtocol]int descriptor, int mode) {
            self.Mode = (IOMode)mode;
            self.SetStream(GetDescriptorStream(self.Context, descriptor));
            self.SetFileDescriptor(descriptor);
            return self;
        }

        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static RubyIO/*!*/ InitializeCopy(RubyIO/*!*/ self, [NotNull]RubyIO/*!*/ source) {
            Stream stream = source.GetStream();
            int descriptor = self.Context.DuplicateFileDescriptor(source.GetFileDescriptor());

            self.SetStream(stream);
            self.SetFileDescriptor(descriptor);
            self.Mode = source.Mode;
            return self;
        }

        [RubyMethod("for_fd", RubyMethodAttributes.PublicSingleton)]
        public static RuleGenerator/*!*/ ForFileDescriptor() {
            return new RuleGenerator(RuleGenerators.InstanceConstructor);
        }

        [RubyMethod("reopen")]
        public static RubyIO/*!*/ Reopen(RubyIO/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path,
            [DefaultProtocol, Optional, NotNull]MutableString mode) {
            return Reopen(self, path, (int)IOModeEnum.Parse(mode, self.Mode));
        }

        [RubyMethod("reopen")]
        public static RubyIO/*!*/ Reopen(RubyIO/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path, int mode) {
            Stream newStream = RubyFile.OpenFileStream(self.Context, path.ConvertToString(), (IOMode)mode);
            self.Context.SetStream(self.GetFileDescriptor(), newStream);
            self.SetStream(newStream);
            self.Mode = (IOMode)mode;
            return self;
        }

        [RubyMethod("reopen")]
        public static RubyIO/*!*/ Reopen(RubyIO/*!*/ self, [NotNull]RubyIO/*!*/ source) {
            self.Context.RedirectFileDescriptor(self.GetFileDescriptor(), source.GetFileDescriptor());
            self.SetStream(source.GetStream());
            self.Mode = source.Mode;
            return self;
        }

        #endregion

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

        #region open

        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static RuleGenerator/*!*/ Open() {
            return new RuleGenerator((metaBuilder, args, name) => {
                var targetClass = (RubyClass)args.Target;
                targetClass.BuildObjectConstructionNoFlow(metaBuilder, args, name);

                // TODO: initialize yields the block?
                if (args.Signature.HasBlock) {
                    // ignore flow builder set up so far, we need one that creates a BlockParam for library calls:
                    metaBuilder.ControlFlowBuilder = null;

                    if (metaBuilder.BfcVariable == null) {
                        metaBuilder.BfcVariable = metaBuilder.GetTemporary(typeof(BlockParam), "#bfc");
                    }

                    metaBuilder.Result = Ast.Call(new Func<UnaryOpStorage, BlockParam, object, object>(InvokeOpenBlock).Method, 
                        Ast.Constant(new UnaryOpStorage(args.RubyContext)),
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
        public static object InvokeOpenBlock(UnaryOpStorage/*!*/ closeStorage, BlockParam block, object obj) {
            object result = obj;
            if (!RubyOps.IsRetrySingleton(obj) && block != null) {
                try {
                    block.Yield(obj, out result);
                } finally {
                    try {
                        var site = closeStorage.GetCallSite("close");
                        site.Target(site, obj);                        
                    } catch (SystemException) {
                        // MRI: nop
                    }
                }
            }
            return result;
        }

        #endregion

        #region pipe, popen

        [RubyMethod("pipe", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static RubyArray/*!*/ OpenPipe(RubyClass/*!*/ self) {
            Stream reader, writer;
            RubyPipe.CreatePipe(out reader, out writer);
            RubyArray result = new RubyArray(2);
            result.Add(new RubyIO(self.Context, reader, IOMode.ReadOnly));
            result.Add(new RubyIO(self.Context, writer, IOMode.WriteOnly));
            return result;
        }

#if !SILVERLIGHT
        [RubyMethod("popen", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static object OpenPipe(RubyContext/*!*/ context, BlockParam block, RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ command, [DefaultProtocol, Optional, NotNull]MutableString modeString) {

            return TryInvokeOpenBlock(context, block, OpenPipe(context, self, command, modeString));
        }

        [RubyMethod("popen", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static RubyIO/*!*/ OpenPipe(RubyContext/*!*/ context, RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ command, [DefaultProtocol, Optional, NotNull]MutableString modeString) {
            return OpenPipe(context, command, IOModeEnum.Parse(modeString));
        }

        public static RubyIO/*!*/ OpenPipe(
            RubyContext/*!*/ context, 
            MutableString/*!*/ command, 
            IOMode mode) {

            bool redirectStandardInput = mode.CanWrite();
            bool redirectStandardOutput = mode.CanRead();

            Process process = RubyProcess.CreateProcess(context, command, redirectStandardInput, redirectStandardOutput, false);

            StreamReader reader = null;
            StreamWriter writer = null;
            if (redirectStandardOutput) {
                reader = process.StandardOutput;
            }

            if (redirectStandardInput) {
                writer = process.StandardInput;
            }

            return new RubyIO(context, reader, writer, mode);
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
                    throw RubyExceptions.CreateEINVAL(e.Message, e);
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
                throw RubyExceptions.CreateTypeConversionError(context.GetClassDisplayName(obj), "IO");
            }
            return io;
        }

        #endregion

        #region sysopen
        [RubyMethod("sysopen", RubyMethodAttributes.PublicSingleton)]
        public static int SysOpen(RubyClass/*!*/ self, [NotNull]MutableString path, [Optional]MutableString mode, [Optional]int perm) {
            if (RubyFileOps.DirectoryExists(self.Context, path)) {
                // TODO: What file descriptor should be returned for a directory?
                return -1;
            }
            RubyIO io = new RubyFile(self.Context, path.ToString(), IOModeEnum.Parse(mode));
            int fileDesc = io.GetFileDescriptor();
            io.Close();
            return fileDesc;
        }
        #endregion


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
            if (self.Closed) {
                throw RubyExceptions.CreateIOError("closed stream");
            }
            self.CloseReader();
        }

        // TODO:
        [RubyMethod("close_write")]
        public static void CloseWriter(RubyIO/*!*/ self) {
            if (self.Closed) {
                throw RubyExceptions.CreateIOError("closed stream");
            }
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

        //stat

        [RubyMethod("eof")]
        [RubyMethod("eof?")]
        public static bool Eof(RubyIO/*!*/ self) {
            self.RequireReadable();
            return self.IsEndOfStream();
        }

        [RubyMethod("ioctl")]
        [RubyMethod("fcntl")]
        public static int FileControl(RubyIO/*!*/ self, [DefaultProtocol]int commandId, [Optional]MutableString arg) {
            return self.FileControl(commandId, (arg != null) ? arg.ConvertToBytes() : null);
        }

        [RubyMethod("ioctl")]
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
            return self.GetFileDescriptor();
        }

        [RubyMethod("fsync")]
        [RubyMethod("flush")]
        public static void Flush(RubyIO/*!*/ self) {
            self.Flush();
        }

        #region isatty
        
#if !SILVERLIGHT
        [RubyMethod("isatty", BuildConfig = "!SILVERLIGHT")]
        [RubyMethod("tty?", BuildConfig = "!SILVERLIGHT")]
        public static bool IsAtty(RubyIO/*!*/ self) {
            ConsoleStreamType? console = self.ConsoleStreamType;
            if (console == null) {
                return self.GetStream().BaseStream == Stream.Null;
            }

            int fd = GetStdHandleFd(console.Value);
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    IntPtr handle = GetStdHandle(fd);
                    if (handle == IntPtr.Zero) {
                        throw new Win32Exception();
                    }

                    return GetFileType(handle) == FILE_TYPE_CHAR;

                default:
                    return isatty(fd) == 1;
            }
        }

        private static int GetStdHandleFd(ConsoleStreamType streamType) {
            switch (streamType) {
                case ConsoleStreamType.Input: return STD_INPUT_HANDLE;
                case ConsoleStreamType.Output: return STD_OUTPUT_HANDLE;
                case ConsoleStreamType.ErrorOutput: return STD_ERROR_HANDLE;
                default: throw Assert.Unreachable;
            }
        }

        private const int FILE_TYPE_CHAR = 0x0002;

        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;

        [DllImport("kernel32")]
        private extern static IntPtr GetStdHandle(int nStdHandle);
        
        [DllImport("kernel32")]
        private extern static int GetFileType(IntPtr hFile);

        [DllImport ("libc")]
        private static extern int isatty(int desc);
#endif

        #endregion

        [RubyMethod("sync")]
        public static bool Sync(RubyIO/*!*/ self) {
            self.RequireOpen();
            return self.AutoFlush;
        }

        [RubyMethod("sync=")]
        public static bool Sync(RubyIO/*!*/ self, bool sync) {
            self.RequireOpen();
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
            return self.ExternalEncoding;
        }

        // TODO: 1.9 only
        [RubyMethod("internal_encoding")]
        public static RubyEncoding GetInternalEncoding(RubyIO/*!*/ self) {
            return self.InternalEncoding;
        }

        // TODO: 1.9 only
        // set_encoding

        #endregion

        #region rewind, seek, sysseek, pos, tell, lineno

        [RubyMethod("rewind")]
        public static void Rewind(RubyContext/*!*/ context, RubyIO/*!*/ self) {
            self.Seek(0, SeekOrigin.Begin);
            self.LineNumber = 0;
        }

        [RubyMethod("seek")]
        public static int Seek(RubyIO/*!*/ self, [DefaultProtocol]IntegerValue pos, [DefaultProtocol, DefaultParameterValue(SEEK_SET)]int seekOrigin) {
            self.Seek(pos.ToInt64(), RubyIO.ToSeekOrigin(seekOrigin));
            return 0;
        }

        [RubyMethod("sysseek")]
        public static object SysSeek(RubyIO/*!*/ self, [DefaultProtocol]IntegerValue pos, [DefaultProtocol, DefaultParameterValue(SEEK_SET)]int seekOrigin) {
            self.Flush();
            self.Seek(pos.ToInt64(), RubyIO.ToSeekOrigin(seekOrigin));
            return pos.ToObject();
        }

        [RubyMethod("pos")]
        [RubyMethod("tell")]
        public static object/*!*/ Pos(RubyIO/*!*/ self) {
            if (self.Position <= Int32.MaxValue) {
                return (int)self.Position;
            }

            return (BigInteger)self.Position;
        }

        [RubyMethod("pos=")]
        public static void Pos(RubyIO/*!*/ self, [DefaultProtocol]IntegerValue pos) {
            self.Seek(pos.ToInt64(), SeekOrigin.Begin);
        }

        [RubyMethod("lineno")]
        public static int GetLineNumber(RubyIO/*!*/ self) {
            self.RequireOpen();
            return self.LineNumber;
        }

        [RubyMethod("lineno=")]
        public static void SetLineNumber(RubyContext/*!*/ context, RubyIO/*!*/ self, [DefaultProtocol]int value) {
            self.RequireOpen();
            self.LineNumber = value;
        }

        #endregion

        #region write, syswrite, write_nonblock

        [RubyMethod("write")]
        public static int Write(RubyIO/*!*/ self, [NotNull]MutableString/*!*/ val) {
            int bytesWritten = val.IsEmpty ? 0 : self.WriteBytes(val, 0, val.GetByteCount());
            if (self.AutoFlush) {
                self.Flush();
            }
            return bytesWritten;
        }

        [RubyMethod("write")]
        public static int Write(ConversionStorage<MutableString>/*!*/ tosConversion, RubyIO/*!*/ self, object obj) {
            return Write(self, Protocols.ConvertToString(tosConversion, obj));
        }

        [RubyMethod("syswrite")]
        public static int SysWrite(BinaryOpStorage/*!*/ writeStorage, ConversionStorage<MutableString>/*!*/ tosConversion,
            RubyContext/*!*/ context, RubyIO/*!*/ self, [NotNull]MutableString/*!*/ val) {

            RubyBufferedStream stream = self.GetWritableStream();
            if (stream.DataBuffered) {
                PrintOps.ReportWarning(writeStorage, tosConversion, MutableString.CreateAscii("syswrite for buffered IO"));
            }
            int bytes = Write(self, val);
            self.Flush();
            return bytes;
        }

        [RubyMethod("syswrite")]
        public static int SysWrite(BinaryOpStorage/*!*/ writeStorage, ConversionStorage<MutableString>/*!*/ tosConversion,
            RubyContext/*!*/ context, RubyIO/*!*/ self, object obj) {
            return SysWrite(writeStorage, tosConversion, context, self, Protocols.ConvertToString(tosConversion, obj));
        }

        [RubyMethod("write_nonblock")]
        public static int WriteNoBlock(RubyIO/*!*/ self, [NotNull]MutableString/*!*/ val) {
            var stream = self.GetWritableStream();
            try {
                stream.WriteTimeout = 0;
            } catch (InvalidOperationException) {
                throw RubyExceptions.CreateEBADF();
            }
            return Write(self, val);
        }

        [RubyMethod("write_nonblock")]
        public static int WriteNoBlock(ConversionStorage<MutableString>/*!*/ tosConversion, RubyIO/*!*/ self, object obj) {
            return Write(self, Protocols.ConvertToString(tosConversion, obj));
        }
        
        #endregion

        #region read, sysread, read_nonblock, readpartial

        [RubyMethod("read")]
        public static MutableString/*!*/ Read(RubyIO/*!*/ self) {
            var buffer = MutableString.CreateBinary();
            self.AppendBytes(buffer, Int32.MaxValue);
            return buffer;
        }

        [RubyMethod("read")]
        public static MutableString/*!*/ Read(RubyIO/*!*/ self, DynamicNull bytes, [DefaultProtocol, Optional]MutableString buffer) {
            if (buffer == null) {
                buffer = MutableString.CreateBinary();
            } else {
                buffer.Clear();
            } 
            
            self.AppendBytes(buffer, Int32.MaxValue);
            return buffer;
        }

        [RubyMethod("read")]
        public static MutableString Read(RubyIO/*!*/ self, [DefaultProtocol]int bytes, [DefaultProtocol, Optional]MutableString buffer) {
            self.RequireReadable();
            if (bytes < 0) {
                throw RubyExceptions.CreateArgumentError("negative length -1 given");
            }

            if (buffer == null) {
                buffer = MutableString.CreateBinary();
            } else {
                buffer.Clear();
            }

            int bytesRead = self.AppendBytes(buffer, bytes);
            return (bytesRead == 0 && bytes != 0) ? null : buffer;
        }

        [RubyMethod("sysread")]
        public static MutableString/*!*/ SystemRead(RubyIO/*!*/ self, [DefaultProtocol]int bytes, [DefaultProtocol, Optional]MutableString buffer) {
            var stream = self.GetReadableStream();
            if (stream.DataBuffered) {
                throw RubyExceptions.CreateIOError("sysread for buffered IO");
            }

            // We use Flush to simulate non-buffered IO. 
            // A better approach would be to create a parallel FileStream with 
            // System.IO.FileOptions.WriteThrough (which corresponds to FILE_FLAG_NO_BUFFERING), and also maybe 
            // System.IO.FileOptions.SequentialScan (FILE_FLAG_SEQUENTIAL_SCAN).
            // TODO: sysopen does that?
            stream.Flush();

            var result = Read(self, bytes, buffer);
            if (result == null) {
                throw new EOFError("end of file reached");
            }
            return result;
        }

        [RubyMethod("read_nonblock")]
        public static MutableString/*!*/ ReadNoBlock(RubyIO/*!*/ self, [DefaultProtocol]int bytes, [DefaultProtocol, Optional]MutableString buffer) {
            var stream = self.GetReadableStream();

            try {
                stream.ReadTimeout = 0;
            } catch (InvalidOperationException) {
                throw RubyExceptions.CreateEBADF();
            }

            return Read(self, bytes, buffer);
        }

        // TODO: use Nullable<int> as parameters:

        [RubyMethod("read", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ Read(ConversionStorage<int>/*!*/ fixnumCast, RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ path, [DefaultParameterValue(null)]object lengthObj, 
            [DefaultParameterValue(0)]object offsetObj) {

            var site = fixnumCast.GetSite(ConvertToFixnumAction.Make(fixnumCast.Context));
            int length = (lengthObj != null) ? site.Target(site, lengthObj) : 0;
            int offset = (offsetObj != null) ? site.Target(site, offsetObj) : 0;

            if (offset < 0) {
                throw RubyExceptions.CreateEINVAL();
            }

            if (length < 0) {
                throw RubyExceptions.CreateArgumentError("negative length {0} given", length);
            }

            using (RubyIO io = new RubyFile(self.Context, path.ConvertToString(), IOMode.ReadOnly)) {
                if (offset > 0) {
                    io.Seek(offset, SeekOrigin.Begin);
                }

                if (lengthObj == null) {
                    return Read(io);
                } else {
                    return Read(io, length, null);
                }
            }
        }

        //readpartial

        #endregion

        #region readchar, readbyte (1.9), readline, readlines

        // returns a string in 1.9
        [RubyMethod("readchar")]
        public static int ReadChar(RubyIO/*!*/ self) {
            self.RequireReadable();
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

            self.LineNumber += result.Count;
            context.InputProvider.LastInputLineNumber = self.LineNumber;
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

            using (RubyIO io = new RubyIO(self.Context, File.OpenRead(path.ConvertToString()), IOMode.ReadOnly)) {
                return ReadLines(self.Context, io, separator);
            }
        }

        #endregion

        #region getc, gets, ungetc, getbyte (1.9)

        [RubyMethod("getc")]
        public static object Getc(RubyIO/*!*/ self) {
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
            if (result != null) {
                result.IsTainted = true;
            }

            scope.GetInnerMostClosureScope().LastInputLine = result;
            scope.RubyContext.InputProvider.LastInputLineNumber = ++self.LineNumber;

            return result;
        }

        [RubyMethod("ungetc")]
        public static void SetPreviousByte(RubyIO/*!*/ self, [DefaultProtocol]int b) {
            self.PushBack(unchecked((byte)b));
        }

        // TODO: 1.9 only
        // getbyte

        #endregion

        #region foreach, each, each_byte, each_line

        [RubyMethod("foreach", RubyMethodAttributes.PublicSingleton)]
        public static void ForEach(BlockParam block, RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            ForEach(block, self, path, self.Context.InputSeparator);
        }

        [RubyMethod("foreach", RubyMethodAttributes.PublicSingleton)]
        public static void ForEach(BlockParam block, RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path, [DefaultProtocol]MutableString separator) {
            using (RubyIO io = new RubyIO(self.Context, File.OpenRead(path.ConvertToString()), IOMode.ReadOnly)) {
                Each(self.Context, block, io, separator);
            }
        }

        [RubyMethod("each")]
        [RubyMethod("each_line")]
        public static object Each(RubyContext/*!*/ context, BlockParam block, RubyIO/*!*/ self) {
            return Each(context, block, self, context.InputSeparator);
        }

        [RubyMethod("each")]
        [RubyMethod("each_line")]
        public static object Each(RubyContext/*!*/ context, BlockParam block, RubyIO/*!*/ self, [DefaultProtocol]MutableString separator) {
            self.RequireReadable();

            MutableString line;
            while ((line = self.ReadLineOrParagraph(separator)) != null) {
                if (block == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                line.IsTainted = true;
                context.InputProvider.LastInputLineNumber = ++self.LineNumber;

                object result;
                if (block.Yield(line, out result)) {
                    return result;
                }
            }

            return self;
        }

        [RubyMethod("each_byte")]
        public static object EachByte(BlockParam block, RubyIO/*!*/ self) {
            self.RequireReadable();
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

        public static IOWrapper/*!*/ CreateIOWrapper(RespondToStorage/*!*/ respondToStorage, object io, FileAccess access) {
            bool canRead, canWrite, canSeek, canFlush, canBeClosed;

            if (access == FileAccess.Read || access == FileAccess.ReadWrite) {
                canRead = Protocols.RespondTo(respondToStorage, io, "read");
            } else {
                canRead = false;
            }

            if (access == FileAccess.Write || access == FileAccess.ReadWrite) {
                canWrite = Protocols.RespondTo(respondToStorage, io, "write");
            } else {
                canWrite = false;
            }

            canSeek = Protocols.RespondTo(respondToStorage, io, "seek") && Protocols.RespondTo(respondToStorage, io, "tell");
            canFlush = Protocols.RespondTo(respondToStorage, io, "flush");
            canBeClosed = Protocols.RespondTo(respondToStorage, io, "close");

            return new IOWrapper(respondToStorage.Context, io, canRead, canWrite, canSeek, canFlush, canBeClosed);
        }
    }
}
