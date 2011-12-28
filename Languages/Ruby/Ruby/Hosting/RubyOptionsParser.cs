/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
#if FEATURE_FULL_CONSOLE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Hosting {
    public sealed class RubyConsoleOptions : ConsoleOptions {
        public string ChangeDirectory;
        public bool DisplayVersion;
    }

    public sealed class RubyOptionsParser : OptionsParser<RubyConsoleOptions> {
        private readonly List<string>/*!*/ _loadPaths = new List<string>();
        private readonly List<string>/*!*/ _requiredPaths = new List<string>();
        private bool _disableRubyGems;

#if DEBUG
        private ConsoleTraceListener _debugListener;

        private sealed class CustomTraceFilter : TraceFilter {
            public readonly Dictionary<string, bool>/*!*/ Categories = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            public bool EnableAll { get; set; }

            public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string category, object[] args, object data1, object[] data) {
                string message = data1 as string;
                if (message == null) return true;

                bool enabled;
                if (Categories.TryGetValue(category, out enabled)) {
                    return enabled;
                } else {
                    return EnableAll;
                }
            }
        }

        private void SetTraceFilter(string/*!*/ arg, bool enable) {
            string[] categories = arg.Split(new[] { ';', ','}, StringSplitOptions.RemoveEmptyEntries);

            if (categories.Length == 0 && !enable) {
                Debug.Listeners.Clear();
                return;
            }

            if (_debugListener == null) {
                _debugListener = new ConsoleTraceListener { IndentSize = 4, Filter = new CustomTraceFilter { EnableAll = categories.Length == 0 } };
                Debug.Listeners.Add(_debugListener);
            } 
         
            foreach (var category in categories) {
                ((CustomTraceFilter)_debugListener.Filter).Categories[category] = enable;
            }
        }
#endif

        private string[] GetPaths(string input) {
            string[] paths = StringUtils.Split(input, new char[] { Path.PathSeparator }, Int32.MaxValue, StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < paths.Length; i++) {
                string path = paths[i];

                if (!RubyUtils.IsRelativeToCurrentDirectory(path)) {
                    paths[i] = RubyUtils.ExpandPath(Platform, path, Platform.CurrentDirectory, true);
                }
            }

            return paths;
        }

        protected override void ParseArgument(string arg) {
            ContractUtils.RequiresNotNull(arg, "arg");

            if (ParseRubyOptArgument(arg, PopNextArg)) {
                return;
            }

            if (arg.StartsWith("-C", StringComparison.Ordinal)) {
                ConsoleOptions.ChangeDirectory = arg.Substring(2);
                return;
            }

            if (arg.StartsWith("-e", StringComparison.Ordinal)) {
                string command;
                if (arg == "-e") {
                    command = PopNextArg();
                } else {
                    command = arg.Substring(2);
                }

                LanguageSetup.Options["MainFile"] = "-e";
                if (CommonConsoleOptions.Command == null) {
                    CommonConsoleOptions.Command = String.Empty;
                } else {
                    CommonConsoleOptions.Command += "\n";
                }
                CommonConsoleOptions.Command += command;
                return;
            }

            if (arg.StartsWith("-0", StringComparison.Ordinal) ||
                arg.StartsWith("-F", StringComparison.Ordinal) ||
                arg.StartsWith("-i", StringComparison.Ordinal) ||
                arg.StartsWith("-T", StringComparison.Ordinal) ||
                arg.StartsWith("-x", StringComparison.Ordinal)) {
                throw new InvalidOptionException(String.Format("Option `{0}' not supported", arg));
            }

            string mainFileFromPath = null;
            if (arg.StartsWith("-S", StringComparison.Ordinal)) {
                mainFileFromPath = arg == "-S" ? PopNextArg() : arg.Substring(2);
            }

            switch (arg) {
                #region Ruby Options

                case "-a":
                case "-c":
                case "--copyright":
                case "-l":
                case "-n":
                case "-p":
                case "-s":
                    throw new InvalidOptionException(String.Format("Option `{0}' not supported", arg));

                case "--version":
                    ConsoleOptions.PrintVersion = true;
                    ConsoleOptions.Exit = true;
                    return;

                #endregion

                #region IronRuby Options

#if DEBUG
                case "-DT*":
                    SetTraceFilter(String.Empty, false);
                    return;

                case "-DT":
                    SetTraceFilter(PopNextArg(), false);
                    return;

                case "-ET*":
                    SetTraceFilter(String.Empty, true);
                    return;

                case "-ET":
                    SetTraceFilter(PopNextArg(), true);
                    return;

                case "-ER":
                    RubyOptions.ShowRules = true;
                    return;

                case "-save":
                    LanguageSetup.Options["SavePath"] = PeekNextArg() ?? AppDomain.CurrentDomain.BaseDirectory;
                    return;

                case "-load":
                    LanguageSetup.Options["LoadFromDisk"] = ScriptingRuntimeHelpers.True;
                    return;

                case "-useThreadAbortForSyncRaise":
                    RubyOptions.UseThreadAbortForSyncRaise = true;
                    return;

                case "-compileRegexps":
                    RubyOptions.CompileRegexps = true;
                    return;
#endif
                case "-trace":
                    LanguageSetup.Options["EnableTracing"] = ScriptingRuntimeHelpers.True;
                    return;

                case "-profile":
                    LanguageSetup.Options["Profile"] = ScriptingRuntimeHelpers.True;
                    return;

                case "-1.8.6":
                case "-1.8.7":
                case "-1.9":
                case "-2.0":
                    throw new InvalidOptionException(String.Format("Option `{0}' is no longer supported. The compatible Ruby version is 1.9.", arg));

                case "--disable-gems":
                    _disableRubyGems = true;
                    return;

                case "-X:AutoIndent":
                case "-X:TabCompletion":
                case "-X:ColorfulConsole":
                    throw new InvalidOptionException(String.Format("Option `{0}' not supported", arg));

                #endregion
            }

            // DLR options, file name:
            base.ParseArgument(arg);

            if (ConsoleOptions.FileName != null) {
                if (mainFileFromPath != null) {
                    ConsoleOptions.FileName = FindMainFileFromPath(mainFileFromPath);
                }

                if (ConsoleOptions.Command == null) {
                    SetupOptionsForMainFile();
                } else {
                    SetupOptionsForCommand();
                }
            }
        }

        /// <summary>
        /// Parses arguments available both on command line and in RUBYOPT environment variable.
        /// </summary>
        private bool ParseRubyOptArgument(string arg, Func<string> getNextArg) {
            ContractUtils.RequiresNotNull(arg, "arg");

            if (arg.StartsWith("-I", StringComparison.Ordinal)) {
                string includePaths;
                if (arg == "-I") {
                    includePaths = getNextArg();
                } else {
                    includePaths = arg.Substring(2);
                }

                if (!String.IsNullOrEmpty(includePaths)) {
                    _loadPaths.AddRange(GetPaths(includePaths));
                }

                return true;
            }

            if (arg.StartsWith("-K", StringComparison.Ordinal)) {
                // ignore (1.8 encoding option)
                return true;
            }

            if (arg.StartsWith("-r", StringComparison.Ordinal)) {
                string path = (arg == "-r") ? getNextArg() : arg.Substring(2);
                
                if (!String.IsNullOrEmpty(path)) {
                    _requiredPaths.Add(path);
                }

                return true;
            }

            switch (arg) {
                case "-d":
                    LanguageSetup.Options["DebugVariable"] = true; // $DEBUG = true
                    return true;

                case "-v":
                    ConsoleOptions.DisplayVersion = true;
                    goto case "-W2";

                case "-W0":
                    LanguageSetup.Options["Verbosity"] = 0; // $VERBOSE = nil
                    return true;

                case "-W1":
                    LanguageSetup.Options["Verbosity"] = 1; // $VERBOSE = false
                    return true;

                case "-w":
                case "-W2":
                    LanguageSetup.Options["Verbosity"] = 2; // $VERBOSE = true
                    return true;
            }

            return false;
        }

        private void SetupOptionsForMainFile() {
            LanguageSetup.Options["MainFile"] = RubyUtils.CanonicalizePath(ConsoleOptions.FileName);
            LanguageSetup.Options["Arguments"] = PopRemainingArgs();;
        }

        private void SetupOptionsForCommand() {
            string firstArg = ConsoleOptions.FileName;
            ConsoleOptions.FileName = null;

            List<string> args = new List<string>(new string[] { firstArg });
            args.AddRange(PopRemainingArgs());

            LanguageSetup.Options["MainFile"] = "-e";
            LanguageSetup.Options["Arguments"] = args.ToArray();
        }

        private string FindMainFileFromPath(string mainFileFromPath) {
            string path = Platform.GetEnvironmentVariable("PATH");
            foreach (string p in path.Split(';')) {
                string fullPath = RubyUtils.CombinePaths(p, mainFileFromPath);
                if (Platform.FileExists(fullPath)) {
                    return fullPath;
                }
            }
            return mainFileFromPath;
        }

        protected override void AfterParse() {
            
            // parse ENV["RUBYOPT"] options, they follow command line options:
            // TODO: how do args get split, quoting?
            string rubyopt = Platform.GetEnvironmentVariable("RUBYOPT");
            if (!String.IsNullOrEmpty(rubyopt)) {
                string[] additionalArgs = rubyopt.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                int i = 0;
                while (i < additionalArgs.Length) {
                    ParseRubyOptArgument(additionalArgs[i], () => (i + 1 < additionalArgs.Length) ? additionalArgs[++i] : null);
                    i++;
                }
            }

            var existingSearchPaths =
                LanguageOptions.GetSearchPathsOption(LanguageSetup.Options) ??
                LanguageOptions.GetSearchPathsOption(RuntimeSetup.Options);

            if (existingSearchPaths != null) {
                _loadPaths.InsertRange(0, existingSearchPaths);
            }

            try {
                string rubylib = Platform.GetEnvironmentVariable("RUBYLIB");
                if (rubylib != null) {
                    _loadPaths.AddRange(GetPaths(rubylib));
                }
            } catch (SecurityException) {
                // nop
            }
      
            LanguageSetup.Options["SearchPaths"] = _loadPaths;

            if (!_disableRubyGems) {
                _requiredPaths.Insert(0, "gem_prelude.rb");
            }

            LanguageSetup.Options["RequiredPaths"] = _requiredPaths;

#if DEBUG
            // Can be set to nl-BE, ja-JP, etc
            string culture = Platform.GetEnvironmentVariable("IR_CULTURE");
            if (culture != null) {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture, false);
            }
#endif
            if (ConsoleOptions.DisplayVersion && ConsoleOptions.Command == null && ConsoleOptions.FileName == null) {
                ConsoleOptions.PrintVersion = true;
                ConsoleOptions.Exit = true;
            }
        }

        public override void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments) {
            commandLine = "[options] [file] [arguments]";
            environmentVariables = null;
            comments = null;

            options = new string[,] {
             // { "-0[octal]",                   "specify record separator (\0, if no argument)" },
             // { "-a",                          "autosplit mode with -n or -p (splits $_ into $F)" },
             // { "-c",                          "check syntax only" },
                { "-Cdirectory",                 "cd to directory, before executing your script" },
                { "-d",                          "set debugging flags (set $DEBUG to true)" },
                { "-D",                          "emit debugging information (PDBs) for Visual Studio debugger" },
                { "-e 'command'",                "one line of script. Several -e's allowed. Omit [file]" },
             // { "-Fpattern",                   "split() pattern for autosplit (-a)" },
                { "-h[elp]",                     "Display usage" },
             // { "-i[extension]",               "edit ARGV files in place (make backup if extension supplied)" },
                { "-Idirectory",                 "specify $LOAD_PATH directory (may be used more than once)" },
#if FEATURE_ENCODING
                { "-Kkcode",                     "specifies KANJI (Japanese) code-set" },
#endif
             // { "-l",                          "enable line ending processing" },
             // { "-n",                          "assume 'while gets(); ... end' loop around your script" },
             // { "-p",                          "assume loop like -n but print line also like sed" },
                { "-rlibrary",                   "require the library, before executing your script" },
             // { "-s",                          "enable some switch parsing for switches after script name" },
                { "-S",                          "look for the script using PATH environment variable" },
             // { "-T[level]",                   "turn on tainting checks" },
                { "-v",                          "print version number, then turn on verbose mode" },
                { "-w",                          "turn warnings on for your script" },
                { "-W[level]",                   "set warning level; 0=silence, 1=medium (default), 2=verbose" },
             // { "-x[directory]",               "strip off text before #!ruby line and perhaps cd to directory" },
             // { "--copyright",                 "print the copyright" },
                { "--version",                   "print the version" },

                { "-trace",                      "enable support for set_trace_func" },
                { "-profile",                    "enable support for 'pi = IronRuby::Clr.profile { block_to_profile }'" },
                
                { "-X:ExceptionDetail",          "enable ExceptionDetail mode" },
                { "-X:NoAdaptiveCompilation",    "disable adaptive compilation - all code will be compiled" },
                { "-X:CompilationThreshold",     "the number of iterations before the interpreter starts compiling" },
                { "-X:PassExceptions",           "do not catch exceptions that are unhandled by script code" },
                { "-X:PrivateBinding",           "enable binding to private members" },
                { "-X:ShowClrExceptions",        "display CLS Exception information" },
                { "-X:RemoteRuntimeChannel",     "remote console channel" }, 
             // { "-X:AutoIndent",               "Enable auto-indenting in the REPL loop" },
             // { "-X:TabCompletion",            "Enable TabCompletion mode" },
             // { "-X:ColorfulConsole",          "Enable ColorfulConsole" },

#if DEBUG
                { "-DT",                         "disables tracing of specified events [debug only]" },
                { "-DT*",                        "disables tracing of all events [debug only]" },
                { "-ET",                         "enables tracing of specified events [debug only]" },
                { "-ET*",                        "enables tracing of all events [debug only]" },
                { "-save [path]",                "save generated code to given path [debug only]" },
                { "-load",                       "load pre-compiled code [debug only]" },
                { "-useThreadAbortForSyncRaise", "for testing purposes [debug only]" },
                { "-compileRegexps",             "faster throughput, slower startup [debug only]" },
                { "-X:AssembliesDir <dir>",      "set the directory for saving generated assemblies [debug only]" },
                { "-X:SaveAssemblies",           "save generated assemblies [debug only]" },
                { "-X:TrackPerformance",         "track performance sensitive areas [debug only]" },
                { "-X:PerfStats",                "print performance stats when the process exists [debug only]" },
#endif
            };
        }
    }
}
#endif