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
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Library {
    public class FileTextContentProvider : TextContentProvider {
        private readonly string _path;

        public FileTextContentProvider(string path) {
            _path = path;
        }

        public static SourceUnit Make(ScriptEngine engine, string path) {
            var textContent = new FileTextContentProvider(path);
            var languageContext = HostingHelpers.GetLanguageContext(engine);
            return new SourceUnit(languageContext, textContent, path, SourceCodeKind.File);
        }

        public static SourceUnit MakeEmpty(ScriptEngine engine) {
            return MakeEmpty(engine, "None");
        }

        public static SourceUnit MakeEmpty(ScriptEngine engine, string path) {
            var textContent = NullTextContentProvider.Null;
            var languageContext = HostingHelpers.GetLanguageContext(engine);
            return new SourceUnit(languageContext, textContent, path, SourceCodeKind.File);
        }

        #region TextContentProvider

        public override SourceCodeReader GetReader() {
            var stream = new FileStream(_path, FileMode.Open, FileAccess.Read);
            return new SourceCodeReader(new StreamReader(stream), Encoding.Default);
        }

        #endregion

        public string Path {
            get {
                return _path;
            }
        }
    }
}
