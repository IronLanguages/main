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

using System.IO;

namespace Microsoft.Scripting {
    /// <summary>
    /// A custom Stream class that forwards calls to nothing.
    /// </summary>
    internal sealed class NulStream : Stream {
        private static NulStream g_Instance = new NulStream();
        
        public static NulStream Instance {
            get {
                return g_Instance;
            }
        }

        private NulStream() {
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get { return true; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override void Flush() {
        }

        public override long Length {
            get { return 0; }
        }

        public override long Position {
            get { return 0; }
            set { }
        }

        public override int Read(byte[]/*!*/ buffer, int offset, int count) {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return 0;
        }

        public override void SetLength(long value) {
        }

        public override void Write(byte[]/*!*/ buffer, int offset, int count) {
        }
    }
}
