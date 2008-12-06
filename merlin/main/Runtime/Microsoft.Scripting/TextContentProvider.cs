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

using System;
using System.IO;
using System.Text;

namespace Microsoft.Scripting {

    /// <summary>
    /// Provides a factory to create TextReader's over one source of textual content.
    /// 
    /// TextContentProvider's are used when reading from a source which is already decoded
    /// or has a known specific decoding.  
    /// 
    /// For example a text editor might provide a TextContentProvider whose backing is
    /// an in-memory text buffer that the user can actively edit.
    /// </summary>
    [Serializable]
    public abstract class TextContentProvider {

        /// <summary>
        /// Creates a new TextReader which is backed by the content the TextContentProvider was created for.
        /// 
        /// This method may be called multiple times.  For example once to compile the code and again to get
        /// the source code to display error messages.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract SourceCodeReader GetReader();
    }
}
