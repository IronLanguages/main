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
using Microsoft.Scripting.Hosting;
using System.Reflection;
using System.IO;
using Microsoft.Scripting.Runtime;
using System.Xml;
using System.Windows;
using System.Windows.Browser;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Silverlight {
    public class DynamicEngine {
        public ScriptRuntime Runtime { get; private set; }
        public ScriptRuntimeSetup RuntimeSetup { get; private set; }
        public ScriptEngine Engine { get; private set; }
        public ScriptScope EntryPointScope { get; private set; }

        public DynamicEngine() {
            InitializeRuntime(Settings.Debug);
        }

        public static ScriptRuntimeSetup CreateRuntimeSetup(bool debugMode) {
            ScriptRuntimeSetup setup = TryParseFile();
            if (setup == null) {
                setup = LoadFromAssemblies(DynamicApplication.Current != null ?
                    DynamicApplication.Current.AppManifest.Assemblies :
                    new DynamicAppManifest().Assemblies);
            }
            setup.HostType = typeof(BrowserScriptHost);
            setup.Options["SearchPaths"] = new string[] { String.Empty };
            setup.DebugMode = debugMode;
            return setup;
        }

        public static ScriptRuntimeSetup CreateRuntimeSetup() {
            return CreateRuntimeSetup(false);
        }

        private void LoadDefaultAssemblies() {
            Runtime.LoadAssembly(GetType().Assembly);

            // Add default references to Silverlight platform DLLs
            // (Currently we auto reference CoreCLR, UI controls, browser interop, and networking stack.)
            foreach (string name in new string[] { "mscorlib", "System", "System.Windows", "System.Windows.Browser", "System.Net" }) {
                Runtime.LoadAssembly(Runtime.Host.PlatformAdaptationLayer.LoadAssembly(name));
            }
        }

        private void InitializeRuntime(bool debugMode) {
            RuntimeSetup = CreateRuntimeSetup(debugMode);
            Runtime = new ScriptRuntime(RuntimeSetup);
            LoadDefaultAssemblies();
        }


        public void Run(string entryPoint) {
            ContractUtils.RequiresNotNull(entryPoint, "entryPoint");
            
            string code = ((BrowserPAL)Runtime.Host.PlatformAdaptationLayer).VirtualFilesystem.GetFileContents(entryPoint);
            Engine = Runtime.GetEngineByFileExtension(Path.GetExtension(entryPoint));
            EntryPointScope = Engine.CreateScope();

            ScriptSource sourceCode = Engine.CreateScriptSourceFromString(code, entryPoint, SourceCodeKind.File);
            sourceCode.Compile(new ErrorFormatter.Sink()).Execute(EntryPointScope);
        }   

        internal IEnumerable<string> LanguageExtensions() {
            foreach (var language in Runtime.Setup.LanguageSetups) {
                foreach (var ext in language.FileExtensions) {
                    yield return ext;
                }
            }
        }

        public static ScriptRuntimeSetup LoadFromAssemblies(IEnumerable<Assembly> assemblies) {
            var setup = new ScriptRuntimeSetup();
            foreach (var assembly in assemblies) {
                foreach (DynamicLanguageProviderAttribute attribute in assembly.GetCustomAttributes(typeof(DynamicLanguageProviderAttribute), false)) {
                    setup.LanguageSetups.Add(new LanguageSetup(
                        attribute.LanguageContextType.AssemblyQualifiedName,
                        attribute.DisplayName,
                        attribute.Names,
                        attribute.FileExtensions
                    ));
                }
            }
            return setup;
        }

        public static ScriptRuntimeSetup TryParseFile() {
            Stream configFile = BrowserPAL.PAL.VirtualFilesystem.GetFile(Settings.LanguagesConfigFile);
            if (configFile == null) return null;

            var result = new ScriptRuntimeSetup();
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
                    string context = null, assembly = null, exts = null;
                    while (reader.MoveToNextAttribute()) {
                        switch (reader.Name) {
                            case "languageContext":
                                context = reader.Value;
                                break;
                            case "assembly":
                                assembly = reader.Value;
                                break;
                            case "extensions":
                                exts = reader.Value;
                                break;
                        }
                    }

                    if (context == null || assembly == null || exts == null) {
                        throw new ConfigFileException("expected 'Language' element to have attributes 'languageContext', 'assembly', 'extensions'", Settings.LanguagesConfigFile);
                    }

                    string[] extensions = exts.Split(',');
                    result.LanguageSetups.Add(new LanguageSetup(context + ", " + assembly, String.Empty, extensions, extensions));
                }
            } catch (ConfigFileException cfe) {
                throw cfe;
            } catch (Exception ex) {
                throw new ConfigFileException(ex.Message, Settings.LanguagesConfigFile, ex);
            }

            return result;
        }

        // an exception parsing the host configuration file
        public class ConfigFileException : Exception {
            public ConfigFileException(string msg, string configFile)
                : this(msg, configFile, null) {
            }
            public ConfigFileException(string msg, string configFile, Exception inner)
                : base("Invalid configuration file " + configFile + ": " + msg, inner) {
            }
        }
    }
}
