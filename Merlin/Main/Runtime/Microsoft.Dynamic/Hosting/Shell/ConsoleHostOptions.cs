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

using System.Collections.Generic;
using Microsoft.Scripting.Utils;
using System.Reflection;

namespace Microsoft.Scripting.Hosting.Shell {

    public class ConsoleHostOptions {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public enum Action {
            None,
            RunConsole,
            RunFile,
            DisplayHelp
        }

        private readonly List<string> _ignoredArgs = new List<string>();
        private readonly List<string> _environmentVars = new List<string>();

        public List<string> IgnoredArgs { get { return _ignoredArgs; } }
        public string RunFile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // TODO: fix
        public string[] SourceUnitSearchPaths { get; set; }
        public Action RunAction { get; set; }
        public List<string> EnvironmentVars { get { return _environmentVars; } }
        public string LanguageProvider { get; set; }
        public bool HasLanguageProvider { get; set; }

        public ConsoleHostOptions() {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")] // TODO: fix
        public string[,] GetHelp() {
            return new string[,] {
                { "/help",                     "Displays this help." },
                { "/lang:<extension>",         "Specify language by the associated extension (py, js, vb, rb). Determined by an extension of the first file. Defaults to IronPython." },
                { "/paths:<file-path-list>",   "Semicolon separated list of import paths (/run only)." },
                { "/setenv:<var1=value1;...>", "Sets specified environment variables for the console process. Not available on Silverlight." },
            };
        }
    }
}
