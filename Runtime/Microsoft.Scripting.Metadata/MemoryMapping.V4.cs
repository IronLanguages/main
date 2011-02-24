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
using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Scripting.Metadata {
    [SecurityCritical]
    public unsafe sealed class MemoryMapping : CriticalFinalizerObject {
        [SecurityCritical]
        internal byte* _pointer;
        
        private SafeMemoryMappedViewHandle _handle;
        internal long _capacity;

        [CLSCompliant(false)]
        public byte* Pointer {
            [SecurityCritical]
            get {
                if (_pointer == null) {
                    throw new ObjectDisposedException("MemoryMapping");
                }
                return _pointer;
            }
        }

        public long Capacity {
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

        [SecuritySafeCritical]
        public static MemoryMapping Create(string path) {
            MemoryMappedFile file = null;
            MemoryMappedViewAccessor accessor = null;
            SafeMemoryMappedViewHandle handle = null;
            MemoryMapping mapping = null;
            FileStream stream = null;
            byte* ptr = null;
            
            try {
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
                file = MemoryMappedFile.CreateFromFile(stream, null, 0, MemoryMappedFileAccess.Read, null, HandleInheritability.None, true);
                accessor = file.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
                mapping = new MemoryMapping();

                // we need to make sure that the handle and the acquired pointer get stored to MemoryMapping:
                RuntimeHelpers.PrepareConstrainedRegions();
                try { } finally {
                    handle = accessor.SafeMemoryMappedViewHandle;
                    handle.AcquirePointer(ref ptr);
                    if (ptr == null) {
                        throw new IOException("Cannot create a file mapping");
                    }
                    mapping._handle = handle;
                    mapping._pointer = ptr;
                    mapping._capacity = accessor.Capacity;
                }
            } finally {
                if (stream != null) {
                    stream.Dispose();
                }
                if (accessor != null) {
                    accessor.Dispose();
                }
                if (file != null) {
                    file.Dispose();
                }
            }
            return mapping;
        }

        [SecuritySafeCritical]
        ~MemoryMapping() {
            if (_pointer == null) {
                // uninitialized:
                return;
            }

            // It is not safe to close the view at this point if there are any MemoryBlocks alive.
            // It's up to the user to ensure not to use them. Since you need unmanaged code priviledge to use them
            // this is not a security issue (it would be if this API was security safe critical).
            _handle.ReleasePointer();
            _pointer = null;
        }
    }
}
#endif