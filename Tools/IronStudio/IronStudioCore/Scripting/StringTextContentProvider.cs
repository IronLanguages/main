/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.IO;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;

namespace Microsoft.Scripting.Library {
    public class StringTextContentProvider : TextContentProvider {
        private readonly string _text;

        public StringTextContentProvider(string text) {
            _text = text;
        }

        public static SourceUnit Make(ScriptEngine engine, string text, SourceCodeKind kind) {
            return Make(engine, text, "None", kind);
        }

        public static SourceUnit Make(ScriptEngine engine, string text, string path, SourceCodeKind kind) {
            var textContent = new StringTextContentProvider(text);
            var languageContext = HostingHelpers.GetLanguageContext(engine);
            return new SourceUnit(languageContext, textContent, path, kind);
        }

        #region TextContentProvider

        public override SourceCodeReader GetReader() {
            return new SourceCodeReader(new StringReader(_text), Encoding.Default);
        }

        #endregion
    }

}
