#if WIN8
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace IronRuby.Tests
{
	// Neither Win8 profile nor WinRT expose Console API, so we need to use Win32 :(
	// This is very rough implementation, do not use for anything but test logs.
    internal sealed class ConsoleOutputStream : Stream
    {
        internal static readonly Stream Instance = new ConsoleOutputStream();

        private const int STDOUT = -11;
        private readonly IntPtr _handle;

        private ConsoleOutputStream()
        {
            this._handle = GetStdHandle(STDOUT);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern unsafe bool WriteConsoleW(IntPtr hConsoleOutput, char* lpBuffer, int nNumberOfCharsToWrite, out int lpNumberOfCharsWritten, IntPtr lpReserved);

        public override unsafe void Write(byte[] buffer, int offset, int count)
        {
            if (buffer.Length > 0)
            {
                char[] text = Encoding.UTF8.GetChars(buffer, offset, count);
                fixed (char* ptr = text)
                {
                    Int32 charsWritten;
                    WriteConsoleW(_handle, ptr, text.Length, out charsWritten, IntPtr.Zero);
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
        }

        public override void Flush()
        {
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}
#endif