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
#if !SILVERLIGHT

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
#if MONO_TODO
using Mono.Unix.Native;
#endif

namespace Microsoft.Scripting.Metadata {
    /// <summary>
    /// Represents a memory mapped file. Can only be used by trusted code.
    /// </summary>
    public unsafe sealed class MemoryMapping : CriticalFinalizerObject {
        internal byte* _pointer;

        private int _capacity;

        public int Capacity {
            get { return _capacity; }
        }

        public MemoryBlock GetRange(int start, int length) {
            if (_pointer == null) {
                throw new ObjectDisposedException("MemoryMapping");
            }
            if (start < 0) {
                throw new ArgumentOutOfRangeException("start");
            }
            if (length < 0 || length > _capacity - start) {
                throw new ArgumentOutOfRangeException("length");
            }
            return new MemoryBlock(this, _pointer + start, length);
        }

        private MemoryMapping() {
        }

        /// <summary>
        /// Creates an empty mapping for given file. Only first 2GB the file are mapped if the file is greater.
        /// </summary>
        public static MemoryMapping Create(string path) {
            if (path == null) {
                throw new ArgumentNullException("path");
            }
            if (IsWindows) {
                return WindowsCreate(path);
            } else {
                return UnixCreate(path);
            }
        }

        private static MemoryMapping WindowsCreate(string path) {
            int size;
            IntPtr mappingHandle = IntPtr.Zero;
            MemoryMapping mapping = null;

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite)) {
                size = unchecked((int)stream.Length);
                mapping = new MemoryMapping();
                mapping._capacity = size;
                
                // make sure we don't get interrupted before we save the handle and the pointer:
                RuntimeHelpers.PrepareConstrainedRegions();
                try { } finally {
                    mappingHandle = UnsafeNativeMethods.CreateFileMapping(stream.SafeFileHandle, null, PAGE_READONLY, 0, size, null);
                    if (mappingHandle != IntPtr.Zero) {
                        mapping._pointer = UnsafeNativeMethods.MapViewOfFile(mappingHandle, FILE_MAP_READ, 0, 0, (IntPtr)size);
                        UnsafeNativeMethods.CloseHandle(mappingHandle);
                    }
                }

                if (mapping._pointer == null) {
                    throw new IOException("Unable to create memory map: " + path, Marshal.GetLastWin32Error());
                }
            }

            return mapping;
        }

        private static MemoryMapping UnixCreate(string path) {
#if MONO_TODO
            int size;
            int fileDescriptor = 0;
            MemoryMapping mapping = null;
            IntPtr ptr = IntPtr.Zero;

            // make sure we don't get interrupted before we save the handle and the pointer:
            RuntimeHelpers.PrepareConstrainedRegions();
            try { } finally {
                fileDescriptor = Syscall.open(path, OpenFlags.O_RDONLY);
                if (fileDescriptor < 0) {
                    Stat stat;
                    if (Syscall.fstat(fileDescriptor, out stat) >= 0) {
                        size = unchecked((int)stat.st_size);
                        mapping._capacity = size;
                        mapping._pointer = (byte*)Syscall.mmap(IntPtr.Zero, (ulong)size, MmapProts.PROT_READ, MmapFlags.MAP_SHARED, fileDescriptor, 0);
                    }
                    Syscall.close(fileDescriptor);
                }
            }

            if (mapping._pointer == null) {
                throw new IOException("Unable to create memory map: " + path, Marshal.GetLastWin32Error());
            }

            return mapping;
#else
            return null;
#endif
        }

        ~MemoryMapping() {
            if (_pointer == null) {
                // uninitialized:
                return;
            }

            // It is not safe to close the view at this point if there are any MemoryBlocks alive.
            // It's up to the user to ensure not to use them. Since you need unmanaged code priviledge to use them
            // this is not a security issue (it would be if this API was security safe critical).

            if (IsWindows) {
                UnsafeNativeMethods.UnmapViewOfFile(_pointer);
            } else {
#if MONO_TODO
                Syscall.munmap(new IntPtr(_pointer), (ulong)_capacity);
#endif
            }

            _pointer = null;
        }

        private const int PAGE_READONLY = 0x02;
        private const int FILE_MAP_READ = 0x0004;

        private static bool IsWindows {
            get {
                switch (Environment.OSVersion.Platform) {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                        return true;

                    default:
                        return false;
                }
            }
        }
    }

    internal static class UnsafeNativeMethods {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern unsafe IntPtr CreateFileMapping(
            SafeFileHandle hFile, void* lpAttributes, int fProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, String lpName
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern unsafe byte* MapViewOfFile(IntPtr hFileMappingObject, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, IntPtr dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool UnmapViewOfFile(void* lpBaseAddress);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr handle);
    }
}
#endif