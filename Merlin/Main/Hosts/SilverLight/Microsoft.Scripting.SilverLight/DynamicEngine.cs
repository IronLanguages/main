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
using System.Dynamic;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// Responcible for executing all dynamic language code
    /// </summary>
    public class DynamicEngine {

        /// <summary>
        /// Avaliable languages
        /// </summary>
        public DynamicLanguageConfig LangConfig { get; private set; }

        /// <summary>
        /// DLR ScriptRuntime to execute code against
        /// </summary>
        public ScriptRuntime Runtime { get; private set; }

        /// <summary>
        /// DLR ScriptRuntimeSetup to initialize ScriptRuntime from. It's
        /// stored in case the user needs to create another ScriptRuntime.
        /// </summary>
        public ScriptRuntimeSetup RuntimeSetup { get; private set; }

        /// <summary>
        /// Current ScriptEngine for the language executing code.
        /// </summary>
        public ScriptEngine Engine { get; internal set; }

        /// <summary>
        /// The ScriptScope all entry-point code is executed against.
        /// </summary>
        public ScriptScope EntryPointScope { get; private set; }

        /// <summary>
        /// true while the entry-point is running, false otherwise
        /// </summary>
        internal bool RunningEntryPoint { get; set; }

        /// <summary>
        /// Finds the avaliable languages, initializes the ScriptRuntime, 
        /// and initializes the entry-points ScriptScope.
        /// </summary>
        public DynamicEngine() : this(null) { }

        /// <summary>
        /// Initializes the languages, ScriptRuntime, and entry-point ScriptScope.
        /// </summary>
        public DynamicEngine(DynamicLanguageConfig langConfig) {
            RunningEntryPoint = false;
            if (langConfig == null) {
                InitializeLangConfig();
            } else {
                LangConfig = langConfig;
            }
            InitializeRuntime(Settings.Debug);
            InitializeScope();
        }

        /// <summary>
        /// Initializes the language config
        /// </summary>
        private void InitializeLangConfig() {
            if (DynamicApplication.Current == null) {
                LangConfig = CreateLangConfig();
            } else {
                LangConfig = DynamicApplication.Current.LanguagesConfig;
            }
        }

        /// <summary>
        /// Initializes the entry-point scope, adding any convenience 
        /// globals and modules.
        /// </summary>
        private void InitializeScope() {
            EntryPointScope = CreateScope();
        }

        /// <summary>
        /// Creates a new scope, adding any convenience globals and modules.
        /// </summary>
        public ScriptScope CreateScope() {
            var scope = Runtime.CreateScope();
            scope.SetVariable("document", new DynamicHtmlDocument());
            scope.SetVariable("window", new DynamicHtmlObject(HtmlPage.Window));
            scope.SetVariable("me", DynamicApplication.Current.RootVisual);
            return scope;
        }

        /// <summary>
        /// Creates a new ScriptRuntimeSetup.
        /// </summary>
        /// <param name="debugMode">Tells the setup to generate debuggable code</param>
        /// <returns>new ScriptRuntimeSetup</returns>
        public static ScriptRuntimeSetup CreateRuntimeSetup(bool debugMode) {
            DynamicLanguageConfig langConfig = CreateLangConfig();
            return CreateRuntimeSetup(debugMode, langConfig);
        }

        /// <summary>
        /// Creates a new ScriptRuntimeSetup, given a language config.
        /// </summary>
        /// <param name="langConfig">Use this language config to generate the setup</param>
        /// <param name="debugMode">Tells the setup to generate debuggable code</param>
        /// <returns>new ScriptRuntimeSetup</returns>
        public static ScriptRuntimeSetup CreateRuntimeSetup(bool debugMode, DynamicLanguageConfig langConfig) {
            var setup = langConfig.CreateRuntimeSetup();
            setup.HostType = typeof(BrowserScriptHost);
            setup.Options["SearchPaths"] = new string[] { String.Empty };
            setup.DebugMode = debugMode;
            return setup;
        }

        /// <summary>
        /// Create a ScriptRuntimeSetup for generating optimized (non-debuggable) code.
        /// </summary>
        public static ScriptRuntimeSetup CreateRuntimeSetup() {
            return CreateRuntimeSetup(false);
        }

        /// <summary>
        /// Create a new language config
        /// </summary>
        private static DynamicLanguageConfig CreateLangConfig() {
            return DynamicLanguageConfig.Create(new DynamicAppManifest().Assemblies);
        }

        /// <summary>
        /// Load default references into the runtime, including this assembly
        /// and a select set of platform assemblies.
        /// </summary>
        private void LoadDefaultAssemblies() {
            Runtime.LoadAssembly(GetType().Assembly);
            foreach (string name in new string[] { "mscorlib", "System", "System.Windows", "System.Windows.Browser", "System.Net" }) {
                Runtime.LoadAssembly(Runtime.Host.PlatformAdaptationLayer.LoadAssembly(name));
            }
        }

        /// <summary>
        /// Initialize the ScriptRuntime, and load any default assemblies into it.
        /// Also sets the language config's Runtime property.
        /// </summary>
        /// <param name="debugMode">Should this runtime generate debuggable code?</param>
        private void InitializeRuntime(bool debugMode) {
            RuntimeSetup = CreateRuntimeSetup(debugMode, LangConfig);
            Runtime = new ScriptRuntime(RuntimeSetup);
            LangConfig.Runtime = Runtime;
            LoadDefaultAssemblies();
        }

        /// <summary>
        /// Run a script. Get's the contents from the script's path, uses the
        /// script's file extension to find the corresponding ScriptEngine,
        /// setting the "Engine" property, and execute the script against the
        /// entry point scope as a file.
        /// </summary>
        /// <param name="entryPoint">path to the script</param>
        public void Run(string entryPoint) {
            if (entryPoint != null) {
                var vfs = ((BrowserPAL) Runtime.Host.PlatformAdaptationLayer).VirtualFilesystem;
                string code = vfs.GetFileContents(entryPoint);
                Engine = Runtime.GetEngineByFileExtension(Path.GetExtension(entryPoint));
                ScriptSource sourceCode = Engine.CreateScriptSourceFromString(code, entryPoint, SourceCodeKind.File);
                RunningEntryPoint = true;
                sourceCode.Compile(new ErrorFormatter.Sink()).Execute(EntryPointScope);
                RunningEntryPoint = false;
            }
        }
    }
}
