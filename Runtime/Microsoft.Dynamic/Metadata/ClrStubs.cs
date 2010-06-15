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

using System.Text;
using System;

namespace Microsoft.Scripting.Metadata {
    internal static class ClrStubs {
        internal unsafe static int GetCharCount(this Encoding encoding, byte* bytes, int byteCount, object nls) {
            return encoding.GetCharCount(bytes, byteCount);
        }

        internal unsafe static void GetChars(this Encoding encoding, byte* bytes, int byteCount, char* chars, int charCount, object nls) {
            encoding.GetChars(bytes, byteCount, chars, charCount);
        }
    }
}

#endif