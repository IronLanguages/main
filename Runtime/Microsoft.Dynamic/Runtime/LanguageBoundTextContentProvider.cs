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
using System.Dynamic;
using System.Text;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Internal class which binds a LanguageContext, StreamContentProvider, and Encoding together to produce
    /// a TextContentProvider which reads binary data with the correct language semantics.
    /// </summary>
    internal sealed class LanguageBoundTextContentProvider : TextContentProvider {
        private readonly LanguageContext _context;
        private readonly StreamContentProvider _streamProvider;
        private readonly Encoding _defaultEncoding;
        private readonly string _path;

        public LanguageBoundTextContentProvider(LanguageContext context, StreamContentProvider streamProvider, Encoding defaultEncoding, string path) {
            Assert.NotNull(context, streamProvider, defaultEncoding);
            _context = context;
            _streamProvider = streamProvider;
            _defaultEncoding = defaultEncoding;
            _path = path;
        }

        public override SourceCodeReader GetReader() {
            return _context.GetSourceReader(_streamProvider.GetStream(), _defaultEncoding, _path);
        }
    }
}
