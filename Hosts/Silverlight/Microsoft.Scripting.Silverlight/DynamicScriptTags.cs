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

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Browser;
using System.IO;
using Microsoft.Scripting.Hosting;
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

        public List<Uri> ZipPackages { get; private set; }

        /// <summary>
        /// Holds onto the language config
        /// </summary>
        private DynamicLanguageConfig _LangConfig;

        /// <summary>
        /// true while the script-tag is running, false otherwise
        /// </summary>
        internal bool RunningScriptTags;

        public DynamicScriptTags(DynamicLanguageConfig langConfig) {
            RunningScriptTags = false;
            _LangConfig = langConfig;
            Code = new List<ScriptCode>();
            ZipPackages = new List<Uri>();
        }

        public void DownloadHtmlPage(Action onComplete) {
            if (!HtmlPage.IsEnabled) {
                onComplete();
            } else {
                DownloadAndCache(new List<Uri>() { DynamicApplication.HtmlPageUri }, onComplete);
            }
        }

        /// <summary>
        /// Scrapes the HTML page and populates the "Code" structure.
        /// </summary>
        public void FetchScriptTags() {
            if (!HtmlPage.IsEnabled)
                return;

            var scriptTags = HtmlPage.Document.GetElementsByTagName("script");

            foreach (ScriptObject scriptTag in scriptTags) {
                var e = (HtmlElement)scriptTag;
                var type = (string)e.GetAttribute("type");
                var src = (string)e.GetAttribute("src");

                string language = null;

                // Find the language by either mime-type or script's file extension
                if (type != null)
                    language = GetLanguageByType(type);
                else if (src != null)
                    language = GetLanguageByExtension(Path.GetExtension(src));

                // Only move on if the language was found
                if (language != null) {

                    var initParams = DynamicApplication.Current.InitParams;

                    // Process this script-tag if ...
                    if (
                        // it's class is "*" ... OR
                        (e.CssClass == "*") ||

                        // the xamlid initparam is set and matches this tag's class ... OR
                        (initParams.ContainsKey("xamlid") && initParams["xamlid"] != null &&
                         e.CssClass != null && e.CssClass == initParams["xamlid"]) ||

                        // the xamlid initparam is not set and this tag does not have a class
                        (!initParams.ContainsKey("xamlid") && (e.CssClass == null || e.CssClass.Length == 0))
                    ) {
                        bool defer = (bool)e.GetProperty("defer");

                        _LangConfig.LanguagesUsed[language] = true;

                        var sc = new ScriptCode(language, defer);

                        if (src != null) {
                            sc.External = DynamicApplication.MakeUri(src);
                        } else {

                            var innerHtml = (string)e.GetProperty("innerHTML");
                            if (innerHtml != null) {
                                // IE BUG: inline script-tags have an extra newline at the front,
                                // so remove it ...
                                if (HtmlPage.BrowserInformation.Name == "Microsoft Internet Explorer" && innerHtml.IndexOf("\r\n") == 0) {
                                    innerHtml = innerHtml.Substring(2);
                                }

                                sc.Inline = innerHtml;
                            }
                        }

                        Code.Add(sc);
                    }

                // Lastly, check to see if this is a zip file
                } else if (src != null && ((type != null && type == "application/x-zip-compressed") || Path.GetExtension(src) == ".zip")) {
                    
                    ZipPackages.Add(DynamicApplication.MakeUri(src));

                }
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
                if (sc.External != null)
                    externalUris.Add(sc.External);
            foreach (var zip in ZipPackages)
                externalUris.Add(zip);
            DownloadAndCache(externalUris, onComplete);
        }

        private void DownloadAndCache(List<Uri> uris, Action onComplete) {
            if (!HtmlPage.IsEnabled) {
                onComplete();
            } else {
                ((HttpVirtualFilesystem)HttpPAL.PAL.VirtualFilesystem).
                    DownloadAndCache(uris, onComplete);
            }
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
                                DynamicApplication.BaseUri.MakeRelativeUri(sc.External).ToString(),
                                SourceCodeKind.File
                            );
                        scope = engine.CreateScope();
                    } else if (sc.Inline != null) {
                        Assert.NotNull(DynamicApplication.HtmlPageUri);
                        var page = BrowserPAL.PAL.VirtualFilesystem.GetFileContents(DynamicApplication.HtmlPageUri);
                        var code = AlignSourceLines(sc.Inline, page);
                        sourceCode =
                            engine.Engine.CreateScriptSourceFromString(
                                RemoveMargin(code),
                                DynamicApplication.HtmlPageUri.ToString(),
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

        private string AlignSourceLines(string partialSource, string fullSource) {
            partialSource = partialSource.Replace("\r", "");
            fullSource = fullSource.Replace("\r", "");
            StringBuilder final = new StringBuilder();

            if (fullSource.Contains(partialSource)) {
                int offset = fullSource.IndexOf(partialSource);
                int partialOffset = offset + partialSource.Length;
                for (int i = 0; i < fullSource.Length; i++) {
                    if ((i < offset || i >= partialOffset) ) {
                        if (fullSource[i] == '\n') {
                            final.Append('\n');
                        }
                    } else {
                        final.Append(fullSource[i]);
                    }
                }
            }

            return final.ToString();
        }

        /// <summary>
        /// Removes as much of a margin as possible from "text".
        /// </summary>
        public static string RemoveMargin(string text) {
            return RemoveMargin(text, -1, true, true);
        }

        /// <summary>
        /// Removes as much of a margin as possible from "text". 
        /// </summary>
        /// <param name="text">"text" to remove margin from</param>
        /// <param name="firstLineMargin">What is the first line's margin. Set to "-1" to assume no margin.</param>
        /// <param name="firstLine">does the "firstLine" need to be processed?</param>
        /// <param name="keepLines">Should lines be kept (true) or removed (false)</param>
        /// <returns>the de-margined text</returns>
        private static string RemoveMargin(string text, int firstLineMargin, bool firstLine, bool keepLines) {
            var reader = new StringReader(text);
            var writer = new StringWriter();
            string line;
            var processedWriter = new StringWriter();
            while ((line = reader.ReadLine()) != null) {
                if (firstLine) {
                    // skips all blank lines at the beginning of the string
                    if (line.Length == 0) {
                        if (keepLines) {
                            writer.Write("\n");
                            processedWriter.Write("\n");
                        }
                        continue;
                    }

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
                        var processedText = RemoveMargin(processedWriter.ToString(), currentLineMargin, true, keepLines);
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
        /// Get the language by name; if it exists get back the "main"
        /// language name, otherwise null
        /// </summary>
        public string GetLanguageByType(string mimeType) {
            return GetLanguage(GetLanguageNameFromType(mimeType), (dli) => { return dli.Names; });
        }

        /// <summary>
        /// Get the language by file extension; if it exists get back the 
        /// "main" language name, otherwise null
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public string GetLanguageByExtension(string extension) {
            return GetLanguage(extension, (dli) => { return dli.Extensions; });
        }

        private string GetLanguage(string token, Func<DynamicLanguageInfo, string[]> getProperty) {
            if (token == null) return null;
            foreach (var l in _LangConfig.Languages) {
                foreach (var n in getProperty(l)) {
                    if (n.ToLower() == token.ToLower()) {
                        return (l.Names.Length > 0 ? l.Names[0] : token).ToLower();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Given a mime-type, return the language name
        /// </summary>
        public string GetLanguageNameFromType(string type) {
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
