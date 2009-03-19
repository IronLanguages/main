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
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Hosting {

    public sealed class PythonOptionsParser : OptionsParser<PythonConsoleOptions> {
        private List<string> _warningFilters;

        public PythonOptionsParser() {
        }

        /// <exception cref="Exception">On error.</exception>
        protected override void ParseArgument(string/*!*/ arg) {
            ContractUtils.RequiresNotNull(arg, "arg");

            switch (arg) {
                case "-B": break; // dont_write_bytecode always true in IronPython
                case "-U": break; // unicode always true in IronPython

                case "-b":
                    LanguageSetup.Options["BytesWarning"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-c":
                    ConsoleOptions.Command = PeekNextArg();
                    string[] arguments = PopRemainingArgs();
                    arguments[0] = arg;
                    LanguageSetup.Options["Arguments"] = arguments;
                    break;

                case "-?":
                    ConsoleOptions.PrintUsage = true;
                    ConsoleOptions.Exit = true;
                    break;

                case "-i":
                    ConsoleOptions.Introspection = true;
                    LanguageSetup.Options["Inspect"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-m":
                    ConsoleOptions.ModuleToRun = PeekNextArg();
                    LanguageSetup.Options["Arguments"] = PopRemainingArgs(); 
                    break;

                case "-x":
                    ConsoleOptions.SkipFirstSourceLine = true;
                    break;
                
                // TODO: unbuffered stdout?
                case "-u": break;

                // TODO: create a trace listener?
                case "-v":
                    LanguageSetup.Options["Verbose"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-S":
                    ConsoleOptions.SkipImportSite = true;
                    LanguageSetup.Options["NoSite"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-s":
                    LanguageSetup.Options["NoUserSite"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-E":
                    ConsoleOptions.IgnoreEnvironmentVariables = true;
                    LanguageSetup.Options["IgnoreEnvironment"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-t": LanguageSetup.Options["IndentationInconsistencySeverity"] = Severity.Warning; break;
                case "-tt": LanguageSetup.Options["IndentationInconsistencySeverity"] = Severity.Error; break;

                case "-O":
                    LanguageSetup.Options["Optimize"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-OO":
                    LanguageSetup.Options["Optimize"] = ScriptingRuntimeHelpers.True;
                    LanguageSetup.Options["StripDocStrings"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-Q":
                    LanguageSetup.Options["DivisionOptions"] = ToDivisionOptions(PopNextArg());
                    break;

                case "-Qold": 
                case "-Qnew": 
                case "-Qwarn": 
                case "-Qwarnall":
                    LanguageSetup.Options["DivisionOptions"] = ToDivisionOptions(arg.Substring(2));
                    break;

                case "-V":
                    ConsoleOptions.PrintVersion = true;
                    ConsoleOptions.Exit = true;
                    IgnoreRemainingArgs();
                    break;

                case "-W":
                    if (_warningFilters == null) {
                        _warningFilters = new List<string>();
                    }

                    _warningFilters.Add(PopNextArg());
                    break;
                
                case "-3":
                    LanguageSetup.Options["WarnPy3k"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-":
                    PushArgBack();
                    LanguageSetup.Options["Arguments"] = PopRemainingArgs();
                    break;

                case "-X:MaxRecursion":
                    int limit;
                    if (!StringUtils.TryParseInt32(PopNextArg(), out limit)) {
                        throw new InvalidOptionException(String.Format("The argument for the {0} option must be an integer.", arg));
                    }

                    LanguageSetup.Options["RecursionLimit"] = limit;
                    break;

                case "-X:EnableProfiler":
                    LanguageSetup.Options["EnableProfiler"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-X:MTA":
                    ConsoleOptions.IsMta = true;
                    break;
                case "-X:Python25":
                    LanguageSetup.Options["PythonVersion"] = new Version(2, 5);
                    break;

                case "-d":
                case "-X:Debug":
                    RuntimeSetup.DebugMode = true;
                    LanguageSetup.Options["Debug"] = ScriptingRuntimeHelpers.True;
                    break;

                default:
                    base.ParseArgument(arg);

                    if (ConsoleOptions.FileName != null) {
                        PushArgBack();
                        LanguageSetup.Options["Arguments"] = PopRemainingArgs();
                    }
                    break;
            }            
        }

        protected override void AfterParse() {
            if (_warningFilters != null) {
                LanguageSetup.Options["WarningFilters"] = _warningFilters.ToArray();
            }
        }

        private static PythonDivisionOptions ToDivisionOptions(string/*!*/ value) {
            switch (value) {
                case "old": return PythonDivisionOptions.Old; 
                case "new": return PythonDivisionOptions.New; 
                case "warn": return PythonDivisionOptions.Warn;
                case "warnall": return PythonDivisionOptions.WarnAll; 
                default:
                    throw InvalidOptionValue("-Q", value);
            }
        }

        public override void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments) {
            string [,] standardOptions;
            base.GetHelp(out commandLine, out standardOptions, out environmentVariables, out comments);
#if !IRONPYTHON_WINDOW
            commandLine = "Usage: ipy [options] [file.py|- [arguments]]";
#else
            commandLine = "Usage: ipyw [options] [file.py|- [arguments]]";
#endif

            string [,] pythonOptions = new string[,] {
#if !IRONPYTHON_WINDOW
                { "-v",                     "Verbose (trace import statements) (also PYTHONVERBOSE=x)" },
#endif
                { "-m module",              "run library module as a script"},
                { "-x",                     "Skip first line of the source" },
                { "-u",                     "Unbuffered stdout & stderr" },
                { "-E",                     "Ignore environment variables" },
                { "-Q arg",                 "Division options: -Qold (default), -Qwarn, -Qwarnall, -Qnew" },
                { "-S",                     "Don't imply 'import site' on initialization" },
                { "-t",                     "Issue warnings about inconsistent tab usage" },
                { "-tt",                    "Issue errors for inconsistent tab usage" },
                { "-W arg",                 "Warning control (arg is action:message:category:module:lineno)" },
                { "-3",                     "Warn about Python 3.x incompatibilities" },

                { "-X:MaxRecursion",        "Set the maximum recursion level" },
                { "-X:MTA",                 "Run in multithreaded apartment" },
                { "-X:Python26",            "Enable Python 2.6 features" },
                { "-X:EnableProfiler",      "Enables profiling support in the compiler" },
            };

            // Append the Python-specific options and the standard options
            options = ArrayUtils.Concatenate(pythonOptions, standardOptions);

            Debug.Assert(environmentVariables.GetLength(0) == 0); // No need to append if the default is empty
            environmentVariables = new string[,] {
                { "IRONPYTHONPATH",        "Path to search for module" },
                { "IRONPYTHONSTARTUP",     "Startup module" }
            };

        }
    }
}
