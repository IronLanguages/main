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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting.Shell {
    /// <summary>
    /// Core functionality to implement an interactive console. This should be derived for concrete implementations
    /// </summary>
    public abstract class ConsoleHost {
        private int _exitCode;
        private ConsoleHostOptionsParser _optionsParser;
        private ScriptRuntime _runtime;
        private ScriptEngine _engine;
        internal OptionsParser _languageOptionsParser;
        private IConsole _console;
        private CommandLine _commandLine;

        public ConsoleHostOptions Options { get { return _optionsParser.Options; } }
        public ScriptRuntimeSetup RuntimeSetup { get { return _optionsParser.RuntimeSetup; } }

        public ScriptEngine Engine { get { return _engine; } protected set { _engine = value; } }
        public ScriptRuntime Runtime { get { return _runtime; } protected set { _runtime = value; } }
        protected int ExitCode { get { return _exitCode; } set { _exitCode = value; } }
        protected ConsoleHostOptionsParser ConsoleHostOptionsParser { get { return _optionsParser; } set { _optionsParser = value; } }
        protected IConsole ConsoleIO { get { return _console; } set { _console = value; } }
        protected CommandLine CommandLine { get { return _commandLine; } }

        protected ConsoleHost() {
        }


        /// <summary>
        /// Console Host entry-point .exe name.
        /// </summary>
        protected virtual string ExeName {
            get {
#if !SILVERLIGHT
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                //Can be null if called from unmanaged code (VS integration scenario)
                return entryAssembly != null ? entryAssembly.GetName().Name : "ConsoleHost";
#else
                return "ConsoleHost";
#endif
            }
        }

        #region Customization

        protected virtual void ParseHostOptions(string[] args) {
            _optionsParser.Parse(args);
        }

        protected virtual ScriptRuntimeSetup CreateRuntimeSetup() {
            var setup = ScriptRuntimeSetup.ReadConfiguration();

            string provider = Provider.AssemblyQualifiedName;

            if (!setup.LanguageSetups.Any(s => s.TypeName == provider)) {
                var languageSetup = CreateLanguageSetup();
                if (languageSetup != null) {
                    setup.LanguageSetups.Add(languageSetup);
                }
            }

            return setup;
        }

        protected virtual LanguageSetup CreateLanguageSetup() {
            return null;
        }

        protected virtual PlatformAdaptationLayer PlatformAdaptationLayer {
            get { return PlatformAdaptationLayer.Default; }
        }

        protected virtual Type Provider {
            get { return null; }
        }

        private string GetLanguageProvider(ScriptRuntimeSetup setup) {
            var providerType = Provider;
            if (providerType != null) {
                return providerType.AssemblyQualifiedName;
            }
            
            if (Options.HasLanguageProvider) {
                return Options.LanguageProvider;
            }

            if (Options.RunFile != null) {
                string ext = Path.GetExtension(Options.RunFile);
                foreach (var lang in setup.LanguageSetups) {
                    if (lang.FileExtensions.Any(e => DlrConfiguration.FileExtensionComparer.Equals(e, ext))) {
                        return lang.TypeName;
                    }
                }
            }

            throw new InvalidOptionException("No language specified.");
        }

        protected virtual CommandLine CreateCommandLine() {
            return new CommandLine();
        }

        protected virtual OptionsParser CreateOptionsParser() {
            return new OptionsParser<ConsoleOptions>();
        }

        protected virtual IConsole CreateConsole(ScriptEngine engine, CommandLine commandLine, ConsoleOptions options) {
            ContractUtils.RequiresNotNull(options, "options");

            if (options.TabCompletion) {
                return CreateSuperConsole(commandLine, options.ColorfulConsole);
            } else {
                return new BasicConsole(options.ColorfulConsole);
            }
        }

        // The advanced console functions are in a special non-inlined function so that 
        // dependencies are pulled in only if necessary.
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static IConsole CreateSuperConsole(CommandLine commandLine, bool isColorful) {
            return new SuperConsole(commandLine, isColorful);
        }

        #endregion

        /// <summary>
        /// Request (from another thread) the console REPL loop to terminate
        /// </summary>
        /// <param name="exitCode">The caller can specify the exitCode corresponding to the event triggering
        /// the termination. This will be returned from CommandLine.Run</param>
        public virtual void Terminate(int exitCode) {
            _commandLine.Terminate(exitCode);
        }

        /// <summary>
        /// To be called from entry point.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual int Run(string[] args) {
            var runtimeSetup = CreateRuntimeSetup();
            var options = new ConsoleHostOptions();
            _optionsParser = new ConsoleHostOptionsParser(options, runtimeSetup);

            try {
                ParseHostOptions(args);
            } catch (InvalidOptionException e) {
                Console.Error.WriteLine("Invalid argument: " + e.Message);
                return _exitCode = 1;
            }

            SetEnvironment();

            string provider = GetLanguageProvider(runtimeSetup);

            LanguageSetup languageSetup = null;
            foreach (var language in runtimeSetup.LanguageSetups) {
                if (language.TypeName == provider) {
                    languageSetup = language;
                }
            }
            if (languageSetup == null) {
                // the language doesn't have a setup -> create a default one:
                languageSetup = new LanguageSetup(Provider.AssemblyQualifiedName, Provider.Name);
                runtimeSetup.LanguageSetups.Add(languageSetup);
            }

            // inserts search paths for all languages (/paths option):
            InsertSearchPaths(runtimeSetup.Options, Options.SourceUnitSearchPaths);

            _languageOptionsParser = CreateOptionsParser();

            try {
                _languageOptionsParser.Parse(Options.IgnoredArgs.ToArray(), runtimeSetup, languageSetup, PlatformAdaptationLayer);
            } catch (InvalidOptionException e) {
                Console.Error.WriteLine(e.Message);
                return _exitCode = -1;
            }

#if !SILVERLIGHT
            if (typeof(DynamicMethod).GetConstructor(new Type[] { typeof(string), typeof(Type), typeof(Type[]), typeof(bool) }) == null) {
                Console.WriteLine(string.Format("{0} requires .NET 2.0 SP1 or later to run.", languageSetup.DisplayName));
                Environment.Exit(1);
            }
#endif

            _runtime = new ScriptRuntime(runtimeSetup);

            try {
                _engine = _runtime.GetEngineByTypeName(provider);
            } catch (Exception e) {
                Console.Error.WriteLine(e.Message);
                return _exitCode = 1;
            }

            Execute();
            return _exitCode;
        }

        private static void InsertSearchPaths(IDictionary<string, object> options, ICollection<string> paths) {
            if (options != null && paths != null && paths.Count > 0) {
                var existingPaths = new List<string>(LanguageOptions.GetSearchPathsOption(options) ?? (IEnumerable<string>)ArrayUtils.EmptyStrings);
                existingPaths.InsertRange(0, paths);
                options["SearchPaths"] = existingPaths;
            }
        }

        #region Printing help

        protected virtual void PrintHelp() {
            Console.WriteLine(GetHelp());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual string GetHelp() {
            StringBuilder sb = new StringBuilder();

            string[,] optionsHelp = Options.GetHelp();

            sb.AppendLine(String.Format("Usage: {0}.exe [<dlr-options>] [--] [<language-specific-command-line>]", ExeName));
            sb.AppendLine();

            sb.AppendLine("DLR options (both slash or dash could be used to prefix options):");
            ArrayUtils.PrintTable(sb, optionsHelp);
            sb.AppendLine();

            sb.AppendLine("Language specific command line:");
            PrintLanguageHelp(sb);
            sb.AppendLine();

            return sb.ToString();
        }

        public void PrintLanguageHelp(StringBuilder output) {
            ContractUtils.RequiresNotNull(output, "output");

            string commandLine, comments;
            string[,] options, environmentVariables;

            CreateOptionsParser().GetHelp(out commandLine, out options, out environmentVariables, out comments);

            // only display language specific options if one or more optinos exists.
            if (commandLine != null || options != null || environmentVariables != null || comments != null) {
                if (commandLine != null) {
                    output.AppendLine(commandLine);
                    output.AppendLine();
                }

                if (options != null) {
                    output.AppendLine("Options:");
                    ArrayUtils.PrintTable(output, options);
                    output.AppendLine();
                }

                if (environmentVariables != null) {
                    output.AppendLine("Environment variables:");
                    ArrayUtils.PrintTable(output, environmentVariables);
                    output.AppendLine();
                }

                if (comments != null) {
                    output.Append(comments);
                    output.AppendLine();
                }

                output.AppendLine();
            }
        }

        #endregion

        private void Execute() {
#if !SILVERLIGHT
            if (_languageOptionsParser.CommonConsoleOptions.IsMta) {
                Thread thread = new Thread(ExecuteInternal);
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();
                thread.Join();
                return;
            }
#endif
            ExecuteInternal();
        }

        protected virtual void ExecuteInternal() {
            Debug.Assert(_engine != null);

            ConsoleOptions consoleOptions = _languageOptionsParser.CommonConsoleOptions;

            if (consoleOptions.PrintVersion){
                PrintVersion();
            }

            if (consoleOptions.PrintUsage){
                PrintUsage();
            }

            if (consoleOptions.Exit){
                _exitCode = 0;
                return;
            }

            switch (Options.RunAction) {
                case ConsoleHostOptions.Action.None:
                case ConsoleHostOptions.Action.RunConsole:
                    _exitCode = RunCommandLine();
                    break;

                case ConsoleHostOptions.Action.RunFile:
                    _exitCode = RunFile();
                    break;

                default:
                    throw Assert.Unreachable;
            }
        }

        private void SetEnvironment() {
            Debug.Assert(Options.EnvironmentVars != null);

#if !SILVERLIGHT
            foreach (string env in Options.EnvironmentVars) {
                if (!String.IsNullOrEmpty(env)) {
                    string[] var_def = env.Split('=');
                    System.Environment.SetEnvironmentVariable(var_def[0], (var_def.Length > 1) ? var_def[1] : "");
                }
            }
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private int RunFile() {
            Debug.Assert(_engine != null);

            int result = 0;
            try {
                return _engine.CreateScriptSourceFromFile(Options.RunFile).ExecuteProgram();
#if SILVERLIGHT 
            } catch (ExitProcessException e) {
                result = e.ExitCode;
#endif
            } catch (Exception e) {
                UnhandledException(Engine, e);
                result = 1;
            } finally {
                try {
                    Snippets.SaveAndVerifyAssemblies();
                } catch (Exception) {
                    result = 1;
                }
            }

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private int RunCommandLine() {
            Debug.Assert(_engine != null);

            _commandLine = CreateCommandLine();
            ConsoleOptions consoleOptions = _languageOptionsParser.CommonConsoleOptions;

            if (_console == null) {
                _console = CreateConsole(Engine, _commandLine, consoleOptions);
            }

            int? exitCodeOverride = null;

            try {
                if (consoleOptions.HandleExceptions) {
                    try {
                        _commandLine.Run(Engine, _console, consoleOptions);
                    } catch (Exception e) {
                        if (CommandLine.IsFatalException(e)) {
                            // Some exceptions are too dangerous to try to catch
                            throw;
                        }
                        UnhandledException(Engine, e);
                    }
                } else {
                    _commandLine.Run(Engine, _console, consoleOptions);
                }
            } finally {
                try {
                    Snippets.SaveAndVerifyAssemblies();
                } catch (Exception) {
                    exitCodeOverride = 1;
                }
            }

            if (exitCodeOverride == null) {
                return _commandLine.ExitCode;
            } else {
                return exitCodeOverride.Value;
            }
        }

        private void PrintUsage()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Usage: {0}.exe ", ExeName);
            PrintLanguageHelp(sb);
            Console.Write(sb.ToString());
        }

        protected  void PrintVersion() {
            Console.WriteLine("{0} {1} on {2}", Engine.Setup.DisplayName, Engine.LanguageVersion, GetRuntime());
        }

        private static string GetRuntime() {
            Type mono = typeof(object).Assembly.GetType("Mono.Runtime");
            return mono != null ?
                (string)mono.GetMethod("GetDisplayName", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null)
                : String.Format(CultureInfo.InvariantCulture, ".NET {0}", Environment.Version);
        }

        protected virtual void UnhandledException(ScriptEngine engine, Exception e) {
            Console.Error.Write("Unhandled exception");
            Console.Error.WriteLine(':');

            ExceptionOperations es = engine.GetService<ExceptionOperations>();
            Console.Error.WriteLine(es.FormatException(e));
        }

        protected static void PrintException(TextWriter output, Exception e) {
            Debug.Assert(output != null);
            ContractUtils.RequiresNotNull(e, "e");

            while (e != null) {
                output.WriteLine(e);
                e = e.InnerException;
            }
        }
    }
}

