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
        public DynamicLanguageConfig LangConfig { get; private set; }
        public ScriptRuntime Runtime { get; private set; }
        public ScriptRuntimeSetup RuntimeSetup { get; private set; }
        public ScriptEngine Engine { get; private set; }
        public ScriptScope EntryPointScope { get; private set; }

        public DynamicEngine() {
            if (DynamicApplication.Current == null) {
                LangConfig = CreateLangConfig();
            } else {
                LangConfig = DynamicApplication.Current.LanguagesConfig;
            }
            InitializeRuntime(Settings.Debug);
            EntryPointScope = Runtime.CreateScope();
        }

        public DynamicEngine(DynamicLanguageConfig langConfig) {
            LangConfig = langConfig;
        }

        public static ScriptRuntimeSetup CreateRuntimeSetup(bool debugMode) {
            DynamicLanguageConfig langConfig = CreateLangConfig();
            return CreateRuntimeSetup(debugMode, langConfig);
        }

        public static ScriptRuntimeSetup CreateRuntimeSetup(bool debugMode, DynamicLanguageConfig langConfig) {
            var setup = langConfig.CreateRuntimeSetup();
            setup.HostType = typeof(BrowserScriptHost);
            setup.Options["SearchPaths"] = new string[] { String.Empty };
            setup.DebugMode = debugMode;
            return setup;
        }

        public static ScriptRuntimeSetup CreateRuntimeSetup() {
            return CreateRuntimeSetup(false);
        }

        private static DynamicLanguageConfig CreateLangConfig() {
            return DynamicLanguageConfig.Create(new DynamicAppManifest().Assemblies);
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
            RuntimeSetup = CreateRuntimeSetup(debugMode, LangConfig);
            Runtime = new ScriptRuntime(RuntimeSetup);
            LangConfig.Runtime = Runtime;
            LoadDefaultAssemblies();
        }

        public void Run(string entryPoint) {
            if (Settings.EntryPoint != null) {
                var vfs = ((BrowserPAL) Runtime.Host.PlatformAdaptationLayer).VirtualFilesystem;
                string code = vfs.GetFileContents(entryPoint);
                Engine = Runtime.GetEngineByFileExtension(Path.GetExtension(Settings.EntryPoint));

                ScriptSource sourceCode = Engine.CreateScriptSourceFromString(code, entryPoint, SourceCodeKind.File);
                sourceCode.Compile(new ErrorFormatter.Sink()).Execute(EntryPointScope);
            }
        }
    }
}
