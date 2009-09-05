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
using System.Collections.Generic;
using System.Text;
using System.Windows.Browser;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.Silverlight {
    internal class DynamicScriptTags {
        internal Dictionary<string, string> InlineCode { get; private set; }
        internal Dictionary<string, Uri> ExternalCode { get; private set; }

        private DynamicLanguageConfig _LangConfig;

        internal DynamicScriptTags(DynamicLanguageConfig langConfig) {
            _LangConfig = langConfig;
            InlineCode = new Dictionary<string, string>();
            ExternalCode = new Dictionary<string, Uri>();
            GetScriptTags();
        }

        internal void Run(DynamicEngine engine) {
            ScriptEngine scriptTagEngine = null;
            ScriptScope scriptTagScope = null;
            foreach (var pair in InlineCode) {
                scriptTagEngine = _LangConfig.GetEngine(GetLanguageNameFrom(pair.Key));
                if (scriptTagEngine != null) {
                    scriptTagScope = engine.EntryPointScope;
                    ScriptSource inlineSourceCode =
                        scriptTagEngine.CreateScriptSourceFromString(
                            pair.Value,
                            HtmlPage.Document.DocumentUri.LocalPath.Remove(0,1),
                            SourceCodeKind.File
                        );
                    inlineSourceCode.Compile(new ErrorFormatter.Sink()).Execute(scriptTagScope);
                }
            }
        }

        internal void DownloadExternalCode(Action onComplete) {
            ((HttpVirtualFilesystem)HttpPAL.PAL.VirtualFilesystem).
                DownloadAndCache(ExternalCode, onComplete);
        }

        internal void GetScriptTags() {
            var scriptTags = HtmlPage.Document.GetElementsByTagName("script");
            foreach(ScriptObject scriptTag in scriptTags) {
                var e = (HtmlElement) scriptTag;
                var type = (string) e.GetAttribute("type");

                if (type == null || !LanguageFound(GetLanguageNameFrom(type)))
                    continue;

                _LangConfig.LanguagesUsed[GetLanguageNameFrom(type).ToLower()] = true;

                var src = (string) e.GetAttribute("src");
                if (src != null) {
                    ExternalCode.Add(type, new Uri(src));
                } else {
                    var innerHTML = (string) e.GetProperty("innerHTML");
                    InlineCode.Add(type, innerHTML);
                }
            }
        }

        internal bool LanguageFound(string languageName) {
            bool languageNameFound = false;
            foreach (var l in _LangConfig.Languages) {
                foreach (var n in l.Names) {
                    if (n.ToLower() == languageName) {
                        languageNameFound = true;
                        break;
                    }
                }
                if (languageNameFound)
                    break;
            }
            return languageNameFound;
        }

        internal string GetLanguageNameFrom(string type) {
            return type.Substring(type.LastIndexOf('/') + 1);
        }
    }
}
