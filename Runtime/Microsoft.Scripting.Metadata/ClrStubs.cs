/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System.Text;
using System;
using System.Diagnostics;

namespace Microsoft.Scripting.Metadata {
    internal static class ClrStubs {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        internal unsafe static int GetCharCount(this Encoding encoding, byte* bytes, int byteCount, object nls) {
            return encoding.GetCharCount(bytes, byteCount);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        internal unsafe static void GetChars(this Encoding encoding, byte* bytes, int byteCount, char* chars, int charCount, object nls) {
            encoding.GetChars(bytes, byteCount, chars, charCount);
        }
    }
}

#if CLR2

namespace System.Diagnostics.Contracts {
    internal static class Contract {
        [Conditional("DEBUG")]
        public static void Assert(bool precondition) {
            Debug.Assert(precondition);
        }

        public static void Requires(bool precondition) {
            if (!precondition) {
                throw new ArgumentException("Method precondition violated");
            }
        }
    }
}

#endif
