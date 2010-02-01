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
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting.Shell {
    /// <summary>
    /// Command line hosting service.
    /// </summary>
    public class CommandLine {
        private LanguageContext _language;
        private IConsole _console;
        private ConsoleOptions _options;
        private ScriptScope _scope;
        private ScriptEngine _engine;
        private ICommandDispatcher _commandDispatcher;
        private int? _terminatingExitCode;
        private int _exitCode = 1;

        protected IConsole Console { get { return _console; } }
        protected ConsoleOptions Options { get { return _options; } }
        protected ScriptEngine Engine { get { return _engine; } }
        public ScriptScope ScriptScope { get { return _scope; } protected set { _scope = value; } }
        public int ExitCode { get { return _exitCode; } protected set { _exitCode = value; } }

        /// <summary>
        /// Scope is not remotable, and this only works in the same AppDomain.
        /// </summary>
        protected Scope Scope { 
            get {
                if (_scope == null) {
                    return null;
                }
                return HostingHelpers.GetScope(_scope); 
            }
            set {
                _scope = HostingHelpers.CreateScriptScope(_engine, value);
            }
        }
        
        protected LanguageContext Language {
            get {
                // LanguageContext is not remotable, and this only works in the same AppDomain.
                if (_language == null) {
                    _language = HostingHelpers.GetLanguageContext(_engine);
                }
                return _language;
            }
        }

        protected virtual string Prompt { get { return ">>> "; } }
        public virtual string PromptContinuation { get { return "... "; } }
        protected virtual string Logo { get { return null; } }

        public CommandLine() {
        }

        protected virtual void Initialize() {
            if (_commandDispatcher == null) {
                _commandDispatcher = CreateCommandDispatcher();
            }
        }

        protected virtual Scope CreateScope() {
            return new Scope();
        }

        protected virtual ICommandDispatcher CreateCommandDispatcher() {
            return new SimpleCommandDispatcher();
        }

        public virtual void Terminate(int exitCode) {
            // The default implementation just sets a flag. Derived types can support better termination
            _terminatingExitCode = exitCode;
        }

        /// <summary>
        /// Executes the comand line - depending upon the options provided we will
        /// either run a single file, a single command, or enter the interactive loop.
        /// </summary>
        public void Run(ScriptEngine engine, IConsole console, ConsoleOptions options) {
            ContractUtils.RequiresNotNull(engine, "engine");
            ContractUtils.RequiresNotNull(console, "console");
            ContractUtils.RequiresNotNull(options, "options");

            _engine = engine;
            _options = options;
            _console = console;

            Initialize();

            try {
                _exitCode = Run();

#if !SILVERLIGHT // ThreadAbortException.ExceptionState
            } catch (System.Threading.ThreadAbortException tae) {
                if (tae.ExceptionState is KeyboardInterruptException) {
                    Thread.ResetAbort();
                    _exitCode = -1;
                } else {
                    throw;
                }
#endif
            } finally {
                Shutdown();
                _engine = null;
                _options = null;
                _console = null;
            }
        }

        /// <summary>
        /// Runs the command line.  Languages can override this to provide custom behavior other than:
        ///     1. Running a single command
        ///     2. Running a file
        ///     3. Entering the interactive console loop.
        /// </summary>
        /// <returns></returns>
        protected virtual int Run() {
            int result;

            if (_options.Command != null) {
                result = RunCommand(_options.Command);
            } else if (_options.FileName != null) {
                result = RunFile(_options.FileName);
            } else {
                return RunInteractive();
            }

            if (_options.Introspection) {
                return RunInteractiveLoop();
            }

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void Shutdown() {
            try {
                _engine.Runtime.Shutdown();
            } catch (Exception e) {
                UnhandledException(e);
            }
        }

        protected virtual int RunFile(string fileName) {
            return RunFile(_engine.CreateScriptSourceFromFile(fileName));
        }

        protected virtual int RunCommand(string command) {
            return RunFile(_engine.CreateScriptSourceFromString(command, SourceCodeKind.Statements));
        }

        /// <summary>
        /// Runs the specified filename
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual int RunFile(ScriptSource source) {
            int result = 1;

            if (Options.HandleExceptions) {
                try {
                    result = source.ExecuteProgram();
                } catch (Exception e) {
                    UnhandledException(e);
                }
            } else {
                result = source.ExecuteProgram();
            }

            return result;
        }

        protected void PrintLogo() {
            if (Logo != null) {
                _console.Write(Logo, Style.Out);
            }
        }

        #region Interactivity

        /// <summary>
        /// Starts the interactive loop.  Performs any initialization necessary before
        /// starting the loop and then calls RunInteractiveLoop to start the loop.
        /// 
        /// Returns the exit code when the interactive loop is completed.
        /// </summary>
        protected virtual int RunInteractive() {
            PrintLogo();
            return RunInteractiveLoop();
        }

        /// <summary>
        /// Runs the interactive loop.  Repeatedly parse and run interactive actions
        /// until an exit code is received.  If any exceptions are unhandled displays
        /// them to the console
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected int RunInteractiveLoop() {
            if (_scope == null) {
                _scope = _engine.CreateScope();
            }

#if !SILVERLIGHT // Remote console
            string remoteRuntimeChannel = _options.RemoteRuntimeChannel;
            if (remoteRuntimeChannel != null) {
                // Publish the ScriptScope so that the host can use it
                Remote.RemoteRuntimeServer.StartServer(remoteRuntimeChannel, _scope);
                return 0;
            }
#endif
            int? res = null;

            do {
                if (Options.HandleExceptions) {
                    try {
                        res = TryInteractiveAction();
#if SILVERLIGHT 
                    } catch (ExitProcessException e) {
                        res = e.ExitCode;
#endif
                    }  catch (Exception e) {
                        if (CommandLine.IsFatalException(e)) {
                            // Some exceptions are too dangerous to try to catch
                            throw;
                        }

                        // There should be no unhandled exceptions in the interactive session
                        // We catch all (most) exceptions here, and just display it,
                        // and keep on going
                        UnhandledException(e);
                    }
                } else {
                    res = TryInteractiveAction();
                }

            } while (res == null);

            return res.Value;
        }

        internal static bool IsFatalException(Exception e) {
            ThreadAbortException tae = e as ThreadAbortException;
            if (tae != null) {
#if SILVERLIGHT // ThreadAbortException.ExceptionState
                return true;
#else
                if ((tae.ExceptionState as KeyboardInterruptException) == null) {
                    return true;
                }
#endif
            }
            return false;
        }

        protected virtual void UnhandledException(Exception e) {
            ExceptionOperations exceptionOperations = _engine.GetService<ExceptionOperations>();
            _console.WriteLine(exceptionOperations.FormatException(e), Style.Error);
        }

        /// <summary>
        /// Attempts to run a single interaction and handle any language-specific
        /// exceptions.  Base classes can override this and call the base implementation
        /// surrounded with their own exception handling.
        /// 
        /// Returns null if successful and execution should continue, or an exit code.
        /// </summary>
        protected virtual int? TryInteractiveAction() {
            int? result = null;

            try {
                result = RunOneInteraction();
#if SILVERLIGHT // ThreadAbortException.ExceptionState
            } catch (ThreadAbortException) {
#else
            } catch (ThreadAbortException tae) {
                KeyboardInterruptException pki = tae.ExceptionState as KeyboardInterruptException;
                if (pki != null) {
                    UnhandledException(tae);
                    Thread.ResetAbort();
                } else {
                    throw;
                }
#endif
            }

            return result;
        }

        /// <summary>
        /// Parses a single interactive command or a set of statements and executes it.  
        /// 
        /// Returns null if successful and execution should continue, or the appropiate exit code.
        /// 
        /// We check if the code read is an interactive command or statements is by checking for NewLine
        /// If the code contains NewLine, it's a set of statements (most probably from SendToConsole)
        /// If the code does not contain a NewLine, it's an interactive command typed by the user at the prompt
        /// </summary>
        private int? RunOneInteraction() {
            bool continueInteraction;
            string s = ReadStatement(out continueInteraction);

            if (continueInteraction == false) {
                return (_terminatingExitCode == null) ? 0 : _terminatingExitCode;
            }

            if (String.IsNullOrEmpty(s)) {
                // Is it an empty line?
                _console.Write(String.Empty, Style.Out);
                return null;
            }

            ExecuteCommand(s);
            return null;
        }

        protected virtual void ExecuteCommand(string command) {
            ExecuteCommand(_engine.CreateScriptSourceFromString(command, SourceCodeKind.InteractiveCode));
        }

        protected object ExecuteCommand(ScriptSource source) {
            ErrorListener errorListener = new ErrorSinkProxyListener(ErrorSink);
            CompiledCode compiledCode = source.Compile(_engine.GetCompilerOptions(_scope), errorListener);
            return _commandDispatcher.Execute(compiledCode, _scope);
        }

        protected virtual ErrorSink ErrorSink {
            get { return ErrorSink.Default; }
        }

        /// <summary>
        /// Private helper function to see if we should treat the current input as a blank link.
        /// 
        /// We do this if we only have auto-indent text.
        /// </summary>
        private static bool TreatAsBlankLine(string line, int autoIndentSize) {
            if (line.Length == 0) return true;
            if (autoIndentSize != 0 && line.Trim().Length == 0 && line.Length == autoIndentSize) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Read a statement, which can potentially be a multiple-line statement suite (like a class declaration).
        /// </summary>
        /// <param name="continueInteraction">Should the console session continue, or did the user indicate 
        /// that it should be terminated?</param>
        /// <returns>Expression to evaluate. null for empty input</returns>
        protected string ReadStatement(out bool continueInteraction) {
            StringBuilder b = new StringBuilder();
            int autoIndentSize = 0;

            _console.Write(Prompt, Style.Prompt);

            while (true) {
                string line = ReadLine(autoIndentSize);
                continueInteraction = true;

                if (line == null || (_terminatingExitCode != null)) {
                    continueInteraction = false;
                    return null;
                }

                bool allowIncompleteStatement = TreatAsBlankLine(line, autoIndentSize);
                b.Append(line);
                // Note that this does not use Environment.NewLine because some languages (eg. Python) only
                // recognize \n as a line terminator.
                b.Append("\n");

                string code = b.ToString();

                var props = GetCommandProperties(code);
                if (SourceCodePropertiesUtils.IsCompleteOrInvalid(props, allowIncompleteStatement)) {
                    return props != ScriptCodeParseResult.Empty ? code : null;
                }

                if (_options.AutoIndent && _options.AutoIndentSize != 0) {
                    autoIndentSize = GetNextAutoIndentSize(code);
                }

                // Keep on reading input
                _console.Write(PromptContinuation, Style.Prompt);
            }
        }

        protected virtual ScriptCodeParseResult GetCommandProperties(string code) {
            ScriptSource command = _engine.CreateScriptSourceFromString(code, SourceCodeKind.InteractiveCode);
            return command.GetCodeProperties(_engine.GetCompilerOptions(_scope));
        }

        /// <summary>
        /// Gets the next level for auto-indentation
        /// </summary>
        protected virtual int GetNextAutoIndentSize(string text) {
            return 0;
        }

        protected virtual string ReadLine(int autoIndentSize) {
            return _console.ReadLine(autoIndentSize);
        }

        internal protected virtual TextWriter GetOutputWriter(bool isErrorOutput) {
            return isErrorOutput ? System.Console.Error : System.Console.Out;
        }

        //private static DynamicSite<object, IList<string>>  _memberCompletionSite =
        //    new DynamicSite<object, IList<string>>(OldDoOperationAction.Make(Operators.GetMemberNames));

        public IList<string> GetMemberNames(string code) {
            object value = _engine.CreateScriptSourceFromString(code, SourceCodeKind.Expression).Execute(_scope);
            return _engine.Operations.GetMemberNames(value);
            // TODO: why doesn't this work ???
            //return _memberCompletionSite.Invoke(new CodeContext(_scope, _engine), value);
        }

        public virtual IList<string> GetGlobals(string name) {
            List<string> res = new List<string>();
            foreach (string scopeName in _scope.GetVariableNames()) {
                if (scopeName.StartsWith(name)) {
                    res.Add(scopeName);
                }
            }

            return res;
        }

        #endregion

        class SimpleCommandDispatcher : ICommandDispatcher {
            public object Execute(CompiledCode compiledCode, ScriptScope scope) {
                return compiledCode.Execute(scope);
            }
        }
    }

}
