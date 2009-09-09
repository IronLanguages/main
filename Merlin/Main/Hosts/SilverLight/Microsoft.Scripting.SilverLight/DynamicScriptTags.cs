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
using System.IO;

namespace Microsoft.Scripting.Silverlight {
    public class DynamicScriptTags {

        public class ScriptCode {
            public string Language { get; private set; }
            public bool Defer { get; private set; }
            public List<string> Inline { get; private set; }
            public List<Uri> External { get; private set; }

            public ScriptCode(string lang, bool defer) {
                Language = lang;
                Defer = defer;
                Inline = new List<string>();
                External = new List<Uri>();
            }
        }

        public Dictionary<string, Dictionary<bool, ScriptCode>> Code { get; private set; }

        private DynamicLanguageConfig _LangConfig;

        public DynamicScriptTags(DynamicLanguageConfig langConfig) {
            _LangConfig = langConfig;
            Code = new Dictionary<string, Dictionary<bool, ScriptCode>>();
            GetScriptTags();
        }

        public void Run(DynamicEngine engine) {
            ScriptEngine scriptTagEngine = null;
            ScriptScope scriptTagScope = null;
            foreach (var pair in Code) {
                var lang = pair.Key;
                scriptTagEngine = _LangConfig.GetEngine(GetLanguageNameFrom(pair.Key));
                if (scriptTagEngine != null) {
                    scriptTagScope = engine.EntryPointScope;
                    foreach (var pair2 in pair.Value) {
                        var defer = pair2.Key;
                        var scripts = pair2.Value;
                        if (!defer) {
                            foreach (var uri in scripts.External) {
                                var code = BrowserPAL.PAL.VirtualFilesystem.GetFileContents(uri);
                                if (code == null) continue;
                                ScriptSource externalSourceCode =
                                    scriptTagEngine.CreateScriptSourceFromString(
                                        code,
                                        HtmlPage.Document.DocumentUri.LocalPath.Remove(0, 1),
                                        SourceCodeKind.File
                                    );
                                externalSourceCode.Compile(new ErrorFormatter.Sink()).Execute(scriptTagScope);
                            }
                            foreach (var code in scripts.Inline) {
                                ScriptSource inlineSourceCode =
                                    scriptTagEngine.CreateScriptSourceFromString(
                                        code,
                                        HtmlPage.Document.DocumentUri.LocalPath.Remove(0, 1),
                                        SourceCodeKind.File
                                    );
                                inlineSourceCode.Compile(new ErrorFormatter.Sink()).Execute(scriptTagScope);
                            }
                        }
                    }
                }
            }
        }

        public void DownloadExternalCode(Action onComplete) {
            var externalUris = new List<Uri>();
            foreach (var pair1 in Code)
                foreach(var pair2 in pair1.Value)
                    externalUris.AddRange(pair2.Value.External);
            ((HttpVirtualFilesystem)HttpPAL.PAL.VirtualFilesystem).
                DownloadAndCache(externalUris, onComplete);
        }

        public bool HasScriptTags {
            get {
                foreach (var i in Code) {
                    foreach (var j in i.Value) {
                        if (j.Value.Inline.Count > 0) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public void GetScriptTags() {
            var scriptTags = HtmlPage.Document.GetElementsByTagName("script");
            foreach(ScriptObject scriptTag in scriptTags) {
                var e = (HtmlElement) scriptTag;
                var type = (string) e.GetAttribute("type");

                if (type == null || !LanguageFound(GetLanguageNameFrom(type)))
                    continue;

                _LangConfig.LanguagesUsed[GetLanguageNameFrom(type).ToLower()] = true;

                var src = (string) e.GetAttribute("src");

                bool defer = (bool) e.GetProperty("defer");
                bool deferDefault = src != null;
                defer = defer ^ deferDefault;

                if (!Code.ContainsKey(type) || Code[type] == null) {
                    Code[type] = new Dictionary<bool, ScriptCode>();
                }
                if (!Code[type].ContainsKey(defer) || Code[type][defer] == null) {
                    var sc = new ScriptCode(GetLanguageNameFrom(type).ToLower(), defer);
                    Code[type][defer] = sc;
                }

                if (src != null) {
                    Code[type][defer].External.Add(MakeUriAbsolute(new Uri(src, UriKind.RelativeOrAbsolute)));
                } else {
                    var innerHtml = (string)e.GetProperty("innerHTML");
                    if(innerHtml != null)
                        Code[type][defer].Inline.Add(innerHtml);
                }
            }
        }

        private Uri MakeUriAbsolute(Uri uri) {
            Uri referenceUri = HtmlPage.Document.DocumentUri;
            if (!uri.IsAbsoluteUri) {

                // Is this a direcory name?
                Uri baseUri = null;
                if (Path.GetExtension(referenceUri.AbsoluteUri) == "") {
                    // yes, so just use the directory
                    baseUri = referenceUri;
                } else {
                    // no, so strip off the filename
                    var slashIndex = referenceUri.AbsoluteUri.LastIndexOf('/');
                    baseUri = new Uri(referenceUri.AbsoluteUri.Substring(0, slashIndex + 1), UriKind.Absolute);
                }

                return new Uri(baseUri, uri);
            } else {
                return uri;
            }
        }

        public bool LanguageFound(string languageName) {
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

        public string GetLanguageNameFrom(string type) {
            string lang = type.Substring(type.LastIndexOf('/') + 1);
            if (lang.StartsWith("x-")) {
                lang = lang.Substring(2);
            }
            return lang;
        }
    }
}
