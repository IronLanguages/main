using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Browser;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.Silverlight {
    internal class DynamicScriptTags {
        internal Dictionary<string, string> InlineCode { get; private set; }

        internal DynamicScriptTags() {
            InlineCode = new Dictionary<string, string>();
        }

        internal void Run(DynamicEngine engine) {
            ScriptEngine scriptTagEngine = null;
            ScriptScope scriptTagScope = null;
            foreach (var pair in InlineCode) {
                scriptTagEngine = engine.Runtime.GetEngine(GetLanguageNameFrom(pair.Key));
                if (scriptTagEngine != null) {
                   scriptTagScope = scriptTagEngine.CreateScope();
                   ScriptSource inlineSourceCode = scriptTagEngine.CreateScriptSourceFromString(pair.Value, HtmlPage.Document.DocumentUri.LocalPath.Remove(0,1), SourceCodeKind.File);
                   inlineSourceCode.Compile(new ErrorFormatter.Sink()).Execute(scriptTagScope);
                }
            }
        }

        internal void GetScriptTags(Action onComplete) {
            var tags = new Dictionary<string, string>();
            List<Uri> toDownload = new List<Uri>();
            foreach(ScriptObject scriptTag in HtmlPage.Document.GetElementsByTagName("script")) {
                var e = (HtmlElement) scriptTag;
                var type = (string) e.GetAttribute("type");

                if (type == null || !type.ToLower().StartsWith("application") ||
                    !LanguageFound(GetLanguageNameFrom(type)))
                    continue;

                var src = (string) e.GetAttribute("src");
                if (src != null) {
                    toDownload.Add(new Uri(src));
                } else {
                    var innerHTML = (string) e.GetProperty("innerHTML");
                    InlineCode.Add(type, innerHTML);
                }
            }
            ((HttpVirtualFilesystem)HttpPAL.PAL.VirtualFilesystem).DownloadAndCache(toDownload, onComplete);
        }

        internal bool LanguageFound(string languageName) {
            bool languageNameFound = false;
            foreach (var l in DynamicApplication.Current.Engine.Runtime.Setup.LanguageSetups) {
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
