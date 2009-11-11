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
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Interpreter;

namespace Microsoft.Scripting.Hosting.Shell {

    [Serializable]
    public class InvalidOptionException : Exception {
        public InvalidOptionException() { }
        public InvalidOptionException(string message) : base(message) { }
        public InvalidOptionException(string message, Exception inner) : base(message, inner) { }

#if !SILVERLIGHT // SerializationInfo
        protected InvalidOptionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

    public abstract class OptionsParser {
        private ScriptRuntimeSetup _runtimeSetup;
        private LanguageSetup _languageSetup;
        private PlatformAdaptationLayer _platform;

        private List<string> _ignoredArgs = new List<string>();
        private string[] _args;
        private int _current = -1;

        protected OptionsParser() {
        }

        public ScriptRuntimeSetup RuntimeSetup {
            get { return _runtimeSetup; }
        }

        public LanguageSetup LanguageSetup {
            get { return _languageSetup; }
        }

        public PlatformAdaptationLayer Platform {
            get { return _platform; }
        }

        public abstract ConsoleOptions CommonConsoleOptions { 
            get; 
        }

        public IList<string> IgnoredArgs {
            get { return _ignoredArgs; }
        } 

        /// <exception cref="InvalidOptionException">On error.</exception>
        public void Parse(string[] args, ScriptRuntimeSetup setup, LanguageSetup languageSetup, PlatformAdaptationLayer platform) {
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.RequiresNotNull(setup, "setup");
            ContractUtils.RequiresNotNull(languageSetup, "languageSetup");
            ContractUtils.RequiresNotNull(platform, "platform");

            _args = args;
            _runtimeSetup = setup;
            _languageSetup = languageSetup;
            _platform = platform;
            _current = 0;
            try {
                BeforeParse();
                while (_current < args.Length) {
                    ParseArgument(args[_current++]);
                }
                AfterParse();
            } finally {
                _args = null;
                _runtimeSetup = null;
                _languageSetup = null;
                _platform = null;
                _current = -1;
            }
        }

        protected virtual void BeforeParse() {
            // nop
        }

        protected virtual void AfterParse() {
        }

        protected abstract void ParseArgument(string arg);

        protected void IgnoreRemainingArgs() {
            while (_current < _args.Length) {
                _ignoredArgs.Add(_args[_current++]);
            }
        }

        protected string[] PopRemainingArgs() {
            string[] result = ArrayUtils.ShiftLeft(_args, _current);
            _current = _args.Length;
            return result;
        }

        protected string PeekNextArg() {
            if (_current < _args.Length)
                return _args[_current];
            else
                throw new InvalidOptionException(String.Format(CultureInfo.CurrentCulture, "Argument expected for the {0} option.", _current > 0 ? _args[_current - 1] : ""));
        }

        protected string PopNextArg() {
            string result = PeekNextArg();
            _current++;
            return result;
        }

        protected void PushArgBack() {
            _current--;
        }

        protected static Exception InvalidOptionValue(string option, string value) {
            return new InvalidOptionException(String.Format("'{0}' is not a valid value for option '{1}'", value, option));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")] // TODO: fix
        public abstract void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments);
    }

    public class OptionsParser<TConsoleOptions> : OptionsParser
        where TConsoleOptions : ConsoleOptions, new() {
       
        private TConsoleOptions _consoleOptions;

        private bool _saveAssemblies = false;
        private string _assembliesDir = null;

        public OptionsParser() {
        }

        public TConsoleOptions ConsoleOptions {
            get {
                if (_consoleOptions == null) {
                    _consoleOptions = new TConsoleOptions();
                } 
                
                return _consoleOptions; 
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _consoleOptions = value; 
            }
        }

        public sealed override ConsoleOptions CommonConsoleOptions {
            get { return ConsoleOptions; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected override void ParseArgument(string arg) {
            ContractUtils.RequiresNotNull(arg, "arg");

            // the following extension switches are in alphabetic order
            switch (arg) {
                case "-h":
                case "-help":
                case "-?":
                case "/?":
                    ConsoleOptions.PrintUsage = true;
                    ConsoleOptions.Exit = true;
                    IgnoreRemainingArgs();
                    break;

                case "-D": 
                    RuntimeSetup.DebugMode = true; 
                    break;
                
                case "-X:PrivateBinding": 
                    RuntimeSetup.PrivateBinding = true; 
                    break;

                case "-X:PassExceptions": ConsoleOptions.HandleExceptions = false; break;
                // TODO: #if !IRONPYTHON_WINDOW
                case "-X:ColorfulConsole": ConsoleOptions.ColorfulConsole = true; break;
                case "-X:TabCompletion": ConsoleOptions.TabCompletion = true; break;
                case "-X:AutoIndent": ConsoleOptions.AutoIndent = true; break;
                //#endif

#if DEBUG
                case "-X:AssembliesDir":
                    _assembliesDir = PopNextArg();
                    break;

                case "-X:SaveAssemblies":
                    _saveAssemblies = true;
                    break;

                case "-X:TrackPerformance":
                    SetDlrOption(arg.Substring(3));
                    break;
#endif
                // TODO: remove
                case "-X:Interpret":
                    LanguageSetup.Options["InterpretedMode"] = ScriptingRuntimeHelpers.True;
                    break;

                case "-X:NoAdaptiveCompilation":
                    LanguageSetup.Options["NoAdaptiveCompilation"] = true;
                    break;

                case "-X:CompilationThreshold":
                    LanguageSetup.Options["CompilationThreshold"] = Int32.Parse(PopNextArg());
                    break;

                case "-X:ExceptionDetail":
                case "-X:ShowClrExceptions":
#if DEBUG
                case "-X:PerfStats":
#endif
                    // TODO: separate options dictionary?
                    LanguageSetup.Options[arg.Substring(3)] = ScriptingRuntimeHelpers.True; 
                    break;

#if !SILVERLIGHT // Remote console
                case Remote.RemoteRuntimeServer.RemoteRuntimeArg:
                    ConsoleOptions.RemoteRuntimeChannel = PopNextArg();
                    break;
#endif
                default:
                    ConsoleOptions.FileName = arg.Trim();
                    break;
            }

            if (_saveAssemblies) {
                Snippets.SetSaveAssemblies(true, _assembliesDir);
            }
        }

        internal static void SetDlrOption(string option) {
            SetDlrOption(option, "true");
        }

        // Note: this works because it runs before the compiler picks up the
        // environment variable
        internal static void SetDlrOption(string option, string value) {
#if !SILVERLIGHT
            Environment.SetEnvironmentVariable("DLR_" + option, value);
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")] // TODO: fix
        public override void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments) {

            commandLine = "[options] [file|- [arguments]]";

            options = new string[,] {
                { "-c cmd",                      "Program passed in as string (terminates option list)" },
                { "-h",                          "Display usage" },
#if !IRONPYTHON_WINDOW                      
                { "-i",                          "Inspect interactively after running script" },
#endif                                      
                { "-V",                          "Print the version number and exit" },
                { "-D",                          "Enable application debugging" },

                { "-X:AutoIndent",               "Enable auto-indenting in the REPL loop" },
                { "-X:ExceptionDetail",          "Enable ExceptionDetail mode" },
                { "-X:NoAdaptiveCompilation",    "Disable adaptive compilation" },
                { "-X:CompilationThreshold",     "The number of iterations before the interpreter starts compiling" },
                { "-X:PassExceptions",           "Do not catch exceptions that are unhandled by script code" },
                { "-X:PrivateBinding",           "Enable binding to private members" },
                { "-X:ShowClrExceptions",        "Display CLS Exception information" },

#if !SILVERLIGHT
                { "-X:TabCompletion",            "Enable TabCompletion mode" },
                { "-X:ColorfulConsole",          "Enable ColorfulConsole" },
#endif
#if DEBUG
                { "-X:AssembliesDir <dir>",      "Set the directory for saving generated assemblies [debug only]" },
                { "-X:SaveAssemblies",           "Save generated assemblies [debug only]" },
                { "-X:TrackPerformance",         "Track performance sensitive areas [debug only]" },
                { "-X:PerfStats",                "Print performance stats when the process exists [debug only]" },
#if !SILVERLIGHT // Remote console
                { Remote.RemoteRuntimeServer.RemoteRuntimeArg + " <channel_name>", 
                                                 "Start a remoting server for a remote console session." },
#endif
#endif
           };

            environmentVariables = new string[0, 0];

            comments = null;
        }
    }
}
