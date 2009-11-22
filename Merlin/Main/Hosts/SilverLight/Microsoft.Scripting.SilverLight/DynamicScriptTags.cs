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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// Manages script tags that hold DLR-based code.
    /// </summary>
    public class DynamicScriptTags {

        /// <summary>
        /// Inline strings and external URIs for script-tag registered 
        /// to this control.
        /// </summary>
        public class ScriptCode {
            public string Language { get; private set; }
            public bool Defer { get; private set; }
            public string Inline { get; set; }
            public Uri External { get; set; }

            public ScriptCode(string lang, bool defer) {
                Language = lang;
                Defer = defer;
            }
        }

        /// <summary>
        /// Script code registered for this Silverlight control
        /// </summary>
        public List<ScriptCode> Code { get; private set; }

        /// <summary>
        /// Holds onto the language config
        /// </summary>
        private DynamicLanguageConfig _LangConfig;

        /// <summary>
        /// true while the script-tag is running, false otherwise
        /// </summary>
        internal bool RunningScriptTags;

        /// <summary>
        /// Given a language config it processes the script-tags on the HTML
        /// page (see "GetScriptTags").
        /// </summary>
        public DynamicScriptTags(DynamicLanguageConfig langConfig) {
            RunningScriptTags = false;
            _LangConfig = langConfig;
            Code = new List<ScriptCode>();
            GetScriptTags();
        }

        /// <summary>
        /// Scrapes the HTML page and populates the "Code" structure.
        /// </summary>
        public void GetScriptTags() {
            var scriptTags = HtmlPage.Document.GetElementsByTagName("script");
            foreach (ScriptObject scriptTag in scriptTags) {
                var e = (HtmlElement)scriptTag;
                var type = (string)e.GetAttribute("type");

                if (type == null || !LanguageFound(GetLanguageNameFrom(type)))
                    continue;

                if (DynamicApplication.Current.InitParams.ContainsKey("xamlid")) {
                    if (e.CssClass == null || !e.CssClass.Contains(DynamicApplication.Current.InitParams["xamlid"]))
                        continue;
                } else if (e.CssClass != string.Empty)
                    continue;

                var src = (string)e.GetAttribute("src");
                bool defer = (bool)e.GetProperty("defer");

                string language = GetLanguageNameFrom(type).ToLower();

                _LangConfig.LanguagesUsed[language] = true;

                var sc = new ScriptCode(language, defer);
                if (src != null) {
                    sc.External = DynamicApplication.MakeUri(src);
                } else {
                    var innerHtml = (string)e.GetProperty("innerHTML");
                    if (innerHtml != null)
                        sc.Inline = RemoveMargin(innerHtml);
                }
                Code.Add(sc);
            }
        }

        /// <summary>
        /// Gathers the set of external script URIs, and asks the 
        /// HttpVirtualFilesystem to download and cache the URIs. Calls the
        /// delegate when all downloads are complete.
        /// </summary>
        public void DownloadExternalCode(Action onComplete) {
            var externalUris = new List<Uri>();
            foreach (var sc in Code)
                if(sc.External != null)
                    externalUris.Add(sc.External);
            ((HttpVirtualFilesystem)HttpPAL.PAL.VirtualFilesystem).
                DownloadAndCache(externalUris, onComplete);
        }

        /// <summary>
        /// Are there any registered script tags?
        /// </summary>
        public bool HasScriptTags {
            get {
                foreach (var i in Code)
                    if (i.External != null && i.Inline != null)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Runs the registered script tags against the DynamicEngine.
        /// </summary>
        public void Run(DynamicEngine engine) {
            var inlineScope = engine.CreateScope();
            foreach (var sc in Code) {
                engine.Engine = _LangConfig.GetEngine(sc.Language);
                if (engine.Engine != null && !sc.Defer) {
                    ScriptSource sourceCode = null;
                    ScriptScope scope = null;
                    if(sc.External != null) {
                        var code = BrowserPAL.PAL.VirtualFilesystem.GetFileContents(sc.External);
                        if (code == null) continue;
                        sourceCode =
                            engine.Engine.CreateScriptSourceFromString(
                                code,
                                sc.External.AbsoluteUri,
                                SourceCodeKind.File
                            );
                        scope = engine.CreateScope();
                    } else if (sc.Inline != null) {
                        sourceCode =
                            engine.Engine.CreateScriptSourceFromString(
                                sc.Inline,
                                "main" + engine.LangConfig.GetLanguageByName(sc.Language).Extensions[0],
                                SourceCodeKind.File
                            );
                        scope = inlineScope;
                    }
                    if (sourceCode != null && scope != null) {
                        RunningScriptTags = true;
                        sourceCode.Compile(new ErrorFormatter.Sink()).Execute(scope);
                        RunningScriptTags = false;
                    }
                }
            }
        }

        /// <summary>
        /// Removes as much of a margin as possible from "text".
        /// </summary>
        public static string RemoveMargin(string text) {
            return RemoveMargin(text, -1, true);
        }

        
        /// <summary>
        /// Removes as much of a margin as possible from "text".
        /// </summary>
        /// <param name="text">"text" to remove margin from</param>
        /// <param name="firstLineMargin">What is the first line's margin. Set to "-1" to assume no margin.</param>
        /// <param name="firstLine">has the "firstLine" already been processed?</param>
        /// <returns>the de-margined text</returns>
        private static string RemoveMargin(string text, int firstLineMargin, bool firstLine) {
            var reader = new StringReader(text);
            var writer = new StringWriter();
            string line;
            var processedWriter = new StringWriter();
            while ((line = reader.ReadLine()) != null) {
                if (firstLine) {
                    // skips all blank lines at the beginning of the string
                    if(line == string.Empty)
                        continue;

                    // get the first real line's margin as a reference
                    if(firstLineMargin == -1)
                        firstLineMargin = GetMarginSize(line);
                    firstLine = false;
                }

                if (line.Trim().Length != 0) {
                    // if any line does not have any margin spaces to
                    // remove, stop removing margins and reprocess previously
                    // processed lines without removing margins as well; if any
                    // line's margins are less than , then there is no
                    // margin.
                    var currentLineMargin = GetMarginSize(line);
                    if (currentLineMargin < firstLineMargin) {
                        var processedText = RemoveMargin(processedWriter.ToString(), currentLineMargin, true);
                        writer.Close();
                        writer = null;
                        writer = new StringWriter(new StringBuilder(processedText));
                        firstLineMargin = currentLineMargin;
                    }
                } else if (reader.Peek() == -1) {
                    // skip if it's the last line
                    continue;
                }

                var newLine = line;
                newLine = RemoveSpacesFromStart(firstLineMargin, line);

                writer.Write(newLine + "\n");
                processedWriter.Write(line + "\n");
            }

            string result = writer.ToString();
            reader.Close();
            writer.Close();
            return result;
        }

        /// <summary>
        /// returns the number of spaces in the beginning of "line"
        /// </summary>
        /// <param name="line">a string to find the number of spaces in</param>
        /// <returns>the number of spaces at the beginning of "line"</returns>
        private static int GetMarginSize(string line) {
            var count = 0;
            foreach(char c in line) {
                if(c == ' ') {
                    count++;
                } else {
                    return count;
                }
            }
            return count;
        }

        /// <summary>
        /// Removes "n" spaces from the start of "line". If not all those chars
        /// spaces, then "line" is returned in it's entirety.
        /// </summary>
        /// <param name="n">Number of spaces to remove from "line"</param>
        /// <param name="line">The string to remove spaces from</param>
        /// <returns>
        /// A string with "n" spaces removed, or a copy of "line" if there exists
        /// a non-space character in the first "n" spaces.
        /// </returns>
        private static string RemoveSpacesFromStart(int n, string line) {
            n = line.Length < n ? line.Length : n;
            for (int i = 0; i < n; i++)
                if (line[i] != ' ')
                    return line;
            return line.Remove(0, n);
        }

        /// <summary>
        /// Is the given "languageName" represented by a avaliable language?
        /// </summary>
        public bool LanguageFound(string languageName) {
            if (languageName == null) return false;
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

        /// <summary>
        /// Given a mime-type, return the language name
        /// </summary>
        public string GetLanguageNameFrom(string type) {
            if (!type.StartsWith("application/x-") && 
                !type.StartsWith("text/") && 
                !type.StartsWith("application/")) return null;
            string lang = type.Substring(type.LastIndexOf('/') + 1);
            if (lang.StartsWith("x-")) {
                lang = lang.Substring(2);
            }
            return lang;
        }
    }
}
