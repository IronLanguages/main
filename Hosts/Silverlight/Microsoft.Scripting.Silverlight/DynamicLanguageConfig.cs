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
using System.Reflection;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using System.Xml;
using System.IO;
using System.Net;
using Microsoft.Scripting.Utils;
using System.Windows.Resources;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// Manages configuration information for languages
    /// </summary>
    public class DynamicLanguageConfig {

        /// <summary>
        /// List of avaliable languages
        /// </summary>
        public List<DynamicLanguageInfo> Languages { get; private set; }

        /// <summary>
        /// Keeps track of the language's that have been used
        /// </summary>
        internal Dictionary<string, bool> LanguagesUsed { get; set; }

        /// <summary>
        /// Holds onto the ScriptRuntime
        /// </summary>
        internal ScriptRuntime Runtime { get; set; }

        private DynamicLanguageConfig() {
            Languages = new List<DynamicLanguageInfo>();
            LanguagesUsed = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Finds the language by name from the loaded languages.
        /// </summary>
        /// <param name="name">name of the language</param>
        /// <returns>configuration information for the language</returns>
        public DynamicLanguageInfo GetLanguageByName(string name) {
            foreach (var lang in Languages)
                foreach (var n in lang.Names)
                    if (n.ToLower() == name.ToLower())
                        return lang;
            return null;
        }

        /// <summary>
        /// Gets the ScriptEngine from used languages. If the used language is
        /// found, it creates the engine and stores it on the configuration
        /// information.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ScriptEngine GetEngine(string name) {
            var lang = GetLanguageByName(name);
            if (lang != null) {
                if (lang.Engine == null)
                    foreach (var n in lang.Names)
                        foreach (var used in LanguagesUsed)
                            if (used.Value && n.ToLower() == used.Key)
                                return lang.Engine = Runtime.GetEngine(n);
                return lang.Engine;
            }
            return null;
        }

        private static readonly object _lock = new object();

        /// <summary>
        /// Downloads languages. If there is a missing language assembly in the
        /// XAP, then download the language's external package and load the
        /// package's assemblies, updating the AppManifest.Assemblies list. 
        /// Calls the provided delegate when the downloadQueue is empty or if 
        /// the language assemblies are all in the XAP.
        /// </summary>
        public void DownloadLanguages(DynamicAppManifest appManifest, Action onComplete) {
            var downloadQueue = new List<DynamicLanguageInfo>();
            foreach(var used in LanguagesUsed) {
                var lang = GetLanguageByName(used.Key);
                if (used.Value && lang != null && lang.External != null) {
                    downloadQueue.Add(lang);
                }
            }
            if (downloadQueue.Count == 0) {
                onComplete.Invoke();
                return;
            }
            foreach (var lang in downloadQueue) {
                var xapvfs = (XapVirtualFilesystem)XapPAL.PAL.VirtualFilesystem;
                bool inXAP = true;
                foreach(var assembly in lang.Assemblies) {
                    if(xapvfs.GetFile(assembly) != null) {
                        BrowserPAL.PAL.LoadAssemblyFromPath(assembly);
                    } else {
                        inXAP = false;
                        break;
                    }
                }
                if(inXAP) {
                    onComplete.Invoke();
                } else {
                    WebClient wc = new WebClient();
                    var uri = new Uri(lang.External, UriKind.RelativeOrAbsolute);
                    wc.OpenReadCompleted += (sender, e) => {
                        // Make sure two handlers never step on eachother (could this even happen?)
                        lock (_lock) {
                            var sri = new StreamResourceInfo(e.Result, null);
                            var alang = ((DynamicLanguageInfo) e.UserState);
                            bool first = true;
                            foreach (var assembly in alang.Assemblies) {
                                if (xapvfs.GetFile(sri, assembly) != null) {
                                    xapvfs.UsingStorageUnit(sri, () => {
                                        var asm = BrowserPAL.PAL.LoadAssemblyFromPath(assembly);
                                        if (first) {
                                            GetLanguageByName(alang.Names[0].ToLower()).LanguageContext = 
                                                alang.LanguageContext.Split(',')[0] + ", " + asm.FullName;
                                            first = false;
                                        }
                                        appManifest.Assemblies.Add(asm);
                                    });
                                }
                            }
                            downloadQueue.Remove(alang);
                            if (downloadQueue.Count == 0) {
                                onComplete.Invoke();
                            }
                        }
                    };
                    wc.OpenReadAsync(uri, lang);
                }
            }
        }

        /// <returns>IEumerable for each language file extension</returns>
        public IEnumerable<string> Extensions() {
            foreach (var language in Languages)
                foreach (var ext in language.Extensions)
                    yield return ext;
        }

        /// <summary>
        /// Creates a ScriptRuntimeSetup from the language configuration
        /// information.
        /// </summary>
        public ScriptRuntimeSetup CreateRuntimeSetup() {
            var setup = new ScriptRuntimeSetup();
            foreach (var language in Languages) {
                setup.LanguageSetups.Add(new LanguageSetup(
                    language.LanguageContext,
                    language.Names[0],
                    language.Names,
                    language.Extensions
                ));
            }
            return setup;
        }

        /// <summary>
        /// Creats a DynamicLanguageConfig by first trying to load it from a
        /// configuration file. If that fails, try to load from a list of 
        /// assemblies.
        /// </summary>
        public static DynamicLanguageConfig Create(IEnumerable<Assembly> assemblies) {
            var dl = LoadFromConfiguration();
            if (dl == null) dl = LoadFromAssemblies(assemblies);
            return dl;
        }

        /// <summary>
        /// Loads the configuration from a list of assemblies
        /// </summary>
        public static DynamicLanguageConfig LoadFromAssemblies(IEnumerable<Assembly> assemblies) {
            var dl = new DynamicLanguageConfig();
            foreach (var assembly in assemblies) {
                foreach (DynamicLanguageProviderAttribute attribute in assembly.GetCustomAttributes(typeof(DynamicLanguageProviderAttribute), false)) {
                    dl.Languages.Add(new DynamicLanguageInfo(
                        attribute.Names,
                        attribute.LanguageContextType.AssemblyQualifiedName,
                        new string[] { assembly.ManifestModule.ToString() },
                        attribute.FileExtensions,
                        null
                    ));
                }
            }
            return dl;
        }

        /// <summary>
        /// Loads the configuration from the languages config file.
        /// </summary>
        public static DynamicLanguageConfig LoadFromConfiguration() {
            Stream configFile = BrowserPAL.PAL.VirtualFilesystem.GetFile(Settings.LanguagesConfigFile);
            if (configFile == null) return null;

            var dl = new DynamicLanguageConfig();
            try {
                XmlReader reader = XmlReader.Create(configFile);
                reader.MoveToContent();
                if (!reader.IsStartElement("Languages")) {
                    throw new ConfigFileException("expected 'Configuration' root element", Settings.LanguagesConfigFile);
                }

                while (reader.Read()) {
                    if (reader.NodeType != XmlNodeType.Element || reader.Name != "Language") {
                        continue;
                    }
                    string context = null, asms = null, exts = null, names = null, external = null;
                    while (reader.MoveToNextAttribute()) {
                        switch (reader.Name) {
                            case "names":
                                names = reader.Value;
                                break;
                            case "languageContext":
                                context = reader.Value;
                                break;
                            case "assemblies":
                                asms = reader.Value;
                                break;
                            case "extensions":
                                exts = reader.Value;
                                break;
                            case "external":
                                external = reader.Value;
                                break;
                        }
                    }

                    if (context == null || asms == null || exts == null || names == null || external == null) {
                        throw new ConfigFileException("expected 'Language' element to have attributes 'languageContext', 'assemblies', 'extensions', 'names', 'external'", Settings.LanguagesConfigFile);
                    }

                    char[] splitChars = new char[] { ' ', '\t', ',', ';', '\r', '\n' };
                    string[] assemblies = asms.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    string[] splitNames = names.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    string contextAssembly = assemblies[0].Split('.')[0];
                    foreach (Assembly asm in DynamicApplication.Current.AppManifest.Assemblies) {
                        if (asm.FullName.Contains(contextAssembly)) {
                            contextAssembly = asm.FullName;
                            break;
                        }
                    }
                    dl.Languages.Add(new DynamicLanguageInfo(
                        splitNames,
                        context + ", " + contextAssembly,
                        assemblies,
                        exts.Split(splitChars, StringSplitOptions.RemoveEmptyEntries),
                        external
                    ));
                }
            } catch (ConfigFileException cfe) {
                throw cfe;
            } catch (Exception ex) {
                throw new ConfigFileException(ex.Message, Settings.LanguagesConfigFile, ex);
            }

            return dl;
        }
    }

    /// <summary>
    /// Holds onto language configuration info
    /// </summary>
    public class DynamicLanguageInfo {

        /// <summary>
        /// Names the language has.
        /// </summary>
        public string[] Names { get; private set; }

        /// <summary>
        /// LanguageContext type.
        /// </summary>
        public string LanguageContext { get; internal set; }

        /// <summary>
        /// List of assemblies that makes up the language.
        /// </summary>
        public string[] Assemblies { get; private set; }

        /// <summary>
        /// File extensions that are valid for the language.
        /// </summary>
        public string[] Extensions { get; private set; }

        /// <summary>
        /// Uri of the external package (slvx) file containing all the 
        /// language's assemblies.
        /// </summary>
        public string External { get; private set; }

        /// <summary>
        /// ScriptEngine for the language
        /// </summary>
        public ScriptEngine Engine { get; internal set; }

        public DynamicLanguageInfo(string[] names, string languageContext,
            string[] assemblies, string[] extensions, string external) {
            Names = names;
            LanguageContext = languageContext;
            Assemblies = assemblies;
            Extensions = extensions;
            External = external;
        }
    }

    /// <summary>
    /// An exception parsing the host configuration file
    /// </summary>
    public class ConfigFileException : Exception {
        public ConfigFileException(string msg, string configFile)
            : this(msg, configFile, null) {
        }

        public ConfigFileException(string msg, string configFile, Exception inner)
            : base("Invalid configuration file " + configFile + ": " + msg, inner) {
        }
    }
}
