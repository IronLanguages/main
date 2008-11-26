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
using System.Diagnostics;
using System.Globalization;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Hosting.Shell {

    public class ConsoleHostOptionsParser {
        private readonly ConsoleHostOptions _options;
        private readonly ScriptRuntimeSetup _runtimeSetup;

        public ConsoleHostOptions Options { get { return _options; } }
        public ScriptRuntimeSetup RuntimeSetup { get { return _runtimeSetup; } }

        public ConsoleHostOptionsParser(ConsoleHostOptions options, ScriptRuntimeSetup runtimeSetup) {
            ContractUtils.RequiresNotNull(options, "options");
            ContractUtils.RequiresNotNull(runtimeSetup, "runtimeSetup");

            _options = options;
            _runtimeSetup = runtimeSetup;
        }

        /// <exception cref="InvalidOptionException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Parse(string[] args) {
            ContractUtils.RequiresNotNull(args, "args");

            int i = 0;
            while (i < args.Length) {
                string name, value;
                string current = args[i++];
                ParseOption(current, out name, out value);

                switch (name) {
                    case "console":
                        _options.RunAction = ConsoleHostOptions.Action.RunConsole;
                        break;

                    case "run":
                        OptionValueRequired(name, value);

                        _options.RunAction = ConsoleHostOptions.Action.RunFile;
                        _options.RunFile = value;
                        break;

                    case "lang":
                        OptionValueRequired(name, value);

                        string provider = null;
                        foreach (var language in _runtimeSetup.LanguageSetups) {
                            if (language.Names.Any(n => DlrConfiguration.LanguageNameComparer.Equals(n, value))) {
                                provider = language.TypeName;
                                break;
                            }
                        }
                        if (provider == null) {
                            throw new InvalidOptionException(String.Format("Unknown language id '{0}'.", value));
                        }

                        _options.LanguageProvider = provider;
                        _options.HasLanguageProvider = true;
                        break;

                    case "path":
                    case "paths":
                        OptionValueRequired(name, value);
                        _options.SourceUnitSearchPaths = value.Split(';');
                        break;

                    case "setenv":
                        OptionNotAvailableOnSilverlight(name);
                        _options.EnvironmentVars.AddRange(value.Split(';'));
                        break;

                    // first unknown/non-option:
                    case null:
                    default:
                        _options.IgnoredArgs.Add(current);
                        goto case "";

                    // host/passthru argument separator
                    case "/":
                    case "":
                        // ignore all arguments starting with the next one (arguments are not parsed):
                        while (i < args.Length) {
                            _options.IgnoredArgs.Add(args[i++]);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// name == null means that the argument doesn't specify an option; the value contains the entire argument
        /// name == "" means that the option name is empty (argument separator); the value is null then
        /// </summary>
        private void ParseOption(string arg, out string name, out string value) {
            Debug.Assert(arg != null);

            int colon = arg.IndexOf(':');

            if (colon >= 0) {
                name = arg.Substring(0, colon);
                value = arg.Substring(colon + 1);
            } else {
                name = arg;
                value = null;
            }

            if (name.StartsWith("--")) name = name.Substring("--".Length);
            else if (name.StartsWith("-") && name.Length > 1) name = name.Substring("-".Length);
            else if (name.StartsWith("/") && name.Length > 1) name = name.Substring("/".Length);
            else {
                value = name;
                name = null;
            }

            if (name != null) {
                name = name.ToLower(CultureInfo.InvariantCulture);
            }
        }

        protected void OptionValueRequired(string optionName, string value) {
            if (value == null) {
                throw new InvalidOptionException(String.Format(CultureInfo.CurrentCulture, "Argument expected for the {0} option.", optionName));
            }
        }

        [Conditional("SILVERLIGHT")]
        private void OptionNotAvailableOnSilverlight(string optionName) {
            throw new InvalidOptionException(String.Format("Option '{0}' is not available on Silverlight.", optionName));
        }
    }
}
