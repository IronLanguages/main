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
using System.Windows.Browser;
using System.IO;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;
using System.Windows.Threading;
#if CLR4
using System.Core;
#endif

namespace Microsoft.Scripting.Silverlight {
    public class Repl {
        
        #region Console Html Template
        private const string _sdlr        = "silverlightDlrRepl";
        private const string _sdlrCode    = "silverlightDlrReplCode";
        private const string _sdlrPrompt  = "silverlightDlrReplPrompt";
        private const string _sdlrLine    = "silverlightDlrReplLine";
        private const string _sdlrOutput  = "silverlightDlrReplOutput";
        private const string _sdlrValue   = "silverlightDlrReplValue";
        private const string _sdlrResult  = "silverlightDlrReplResult";
        private const string _sdlrRunForm = "silverlightDlrReplRunForm";
        private const string _sdlrRun     = "silverlightDlrReplRun";

        // 0 - result id
        // 1 - prompt id/class
        // 2 - run form id
        // 3 - code input id
        // 4 - run id
        private static Func<string> _replHtmlTemplate = delegate() {
            return string.Format(@"
<div id=""{1}"" class=""{0}""></div>
<span id=""{3}"" class=""{2}""></span><form id=""{5}"" class=""{4}"" action=""javascript:void(0)""><input type=""text"" id=""{7}"" class=""{6}"" autocomplete=""off"" /><input type=""submit"" id=""{9}"" class=""{8}"" value=""Run"" /></form>",
                _sdlrResult,  GetId(_sdlrResult),
                _sdlrPrompt,  GetId(_sdlrPrompt),
                _sdlrRunForm, GetId(_sdlrRunForm),
                _sdlrCode,    GetId(_sdlrCode),
                _sdlrRun,     GetId(_sdlrRun)
            );
        };

        private static string GetId(string id) {
            return id + _count;
        }
        #endregion

        #region Private fields
        private string              _code;
        private bool                _multiLine;
        private bool                _multiLinePrompt;
        private bool                _multiLineComplete;
        private ReplOutputBuffer    _outputBuffer;
        private ReplInputBuffer     _inputBuffer;
        private HtmlElement         _silverlightDlrReplCode;
        private HtmlElement         _silverlightDlrReplResult;
        private HtmlElement         _silverlightDlrReplPrompt;
        private ScriptEngine        _engine;
        private ScriptScope         _currentScope;
        private static int          _count;
        private ReplHistory         _history = new ReplHistory();
        #endregion

        #region Public properties
        /// <summary>
        /// The InputBuffer for the Repl
        /// </summary>
        public ReplInputBuffer InputBuffer {
            get { return _inputBuffer; }
        }

        /// <summary>
        /// The OutputBuffer for the Repl
        /// </summary>
        public ReplOutputBuffer OutputBuffer {
            get { return _outputBuffer; }
        }

        /// <summary>
        /// The ScriptEngine to run Repl code against
        /// </summary>
        public ScriptEngine Engine {
            get { return _engine; }
        }

        public ReplHistory History {
            get { return _history; }
        }

        public HtmlElement Input {
            get { return _silverlightDlrReplCode; }
        }
        #endregion

        #region Console management
        /// <summary>
        /// Creates a console and inserts it into the page.
        /// </summary>
        public static Repl Show() {
            if (DynamicApplication.Current == null) {
                throw new Exception("Use the Show(engine, scope) overload, since this is not a dynamic application");
            }

            ScriptEngine engine = null;
            if (DynamicApplication.Current.Engine != null) {
                engine = DynamicApplication.Current.Engine.Engine;    
            }
            if (engine == null) {
                var langCfg = DynamicApplication.Current.LanguagesConfig;
                if (langCfg.LanguagesUsed.Count < 1) {
                    throw new Exception("Use the Show(engine, scope) overload, since there are no languages used");
                }
                foreach(var lang in langCfg.LanguagesUsed) {
                    if (lang.Value) {
                        engine = langCfg.GetEngine(lang.Key);
                        break;
                    }
                }
            }

            ScriptScope scope = DynamicApplication.Current.Engine.EntryPointScope;
            if (scope == null) {
                scope = DynamicApplication.Current.Engine.CreateScope();
            }

            return Show(engine, scope);
        }

        /// <summary>
        /// Creates a console with a language
        /// </summary>
        public static Repl Show(string language) {
            var engine = DynamicApplication.Current.LanguagesConfig.GetEngine(language);
            return Show(engine, DynamicApplication.Current.Engine.EntryPointScope);
        }

        /// <summary>
        /// Creates a console with a engine and scope
        /// </summary>
        public static Repl Show(ScriptEngine engine, ScriptScope scope) {
            ContractUtils.RequiresNotNull(engine, "engine");
            ContractUtils.RequiresNotNull(scope, "scope");

            _count++;

            if (DynamicApplication.Current == null) {
                Window.Show();
            } else {
                Window.Show(Settings.ErrorTargetID);
            }

            HtmlElement replDiv = null;
            var repl = Create(engine, scope, out replDiv);

            Window.Current.AddPanel(engine.Setup.Names[0] + " Console", replDiv);
            Window.Current.Initialize();

            repl.Start();
            return repl;
        }

        /// <summary>
        /// Creates the Repl object and HtmlElement containing the rendered Repl.
        /// </summary>
        public static Repl Create(out HtmlElement element) {
            var dynEngine = DynamicApplication.Current.Engine;
            return Create(dynEngine.Engine, dynEngine.EntryPointScope, out element);
        }

        /// <summary>
        /// Create the Repl object with the ScriptEngine and ScriptScope, and outputs
        /// the HtmlElement containing the rendered Repl.
        /// </summary>
        public static Repl Create(ScriptEngine engine, ScriptScope scope, out HtmlElement replDiv) {
            replDiv = HtmlPage.Document.CreateElement("div");
            replDiv.Id = GetId(_sdlr);
            replDiv.CssClass += _sdlr;
            replDiv.SetProperty("innerHTML", _replHtmlTemplate.Invoke());
            return new Repl(engine, scope);
        }

        private Repl(ScriptEngine engine, ScriptScope scope) {
            _engine = engine;
            _currentScope = scope;
        }

        /// <summary>
        /// Starts the Repl: creates HTML elements, input/output buffer, make
        /// sure the prompt is cleared and focused, show the prompt, and attach
        /// the keypress event.
        /// </summary>
        public void Start() {
            _silverlightDlrReplCode = HtmlPage.Document.GetElementById(GetId(_sdlrCode));
            _silverlightDlrReplResult = HtmlPage.Document.GetElementById(GetId(_sdlrResult));
            _silverlightDlrReplPrompt = HtmlPage.Document.GetElementById(GetId(_sdlrPrompt));
            _inputBuffer = new ReplInputBuffer(this);
            _outputBuffer = new ReplOutputBuffer(_silverlightDlrReplResult, _sdlrOutput);
            ShowDefaults();
            ShowPrompt();
            _silverlightDlrReplCode.AttachEvent("onkeydown", new EventHandler<HtmlEventArgs>(OnKeyDown));
        }

        private void OnKeyDown(object sender, HtmlEventArgs args) {
            SendCharacterCode(args.CharacterCode, args.CtrlKey, args.ShiftKey);
        }

        /// <summary>
        /// Processes a key press:
        /// - On enter, run the code in the input buffer. Pass the ctrl-key
        ///   to decide whether to force execution, even if the expression is
        ///   incomplete.
        /// - On up arrow, show the previous command.
        /// - On down arrow (and the shift key is not pressed), show the next 
        ///   command.
        /// - Otherwise, just remeber the char for history purposes.
        /// </summary>
        /// <param name="c">int representation of the char to handle</param>
        /// <param name="ctrlKey">Was the ctrl key pressed?</param>
        /// <param name="shiftKey">Was the shift key pressed?</param>
        public void SendCharacterCode(int c, bool ctrlKey, bool shiftKey) {
            switch(c) {
            case 13:
                Store();
                RunCode(ctrlKey);
                break;
            case 38:
                ShowPreviousCommand();
                break;
            case 40:
                if (!shiftKey) {
                    ShowNextCommand();
                }
                break;
            };
        }

        /// <summary>
        /// Reset the REPL
        /// </summary>
        private void Reset() {
            _code = null;
            _multiLine = false;
            _multiLinePrompt = false;
            _multiLineComplete = false;
        }
        #endregion

        #region History

        private string Remember() {
            return _history.ReplaceWriteCommand(Input.GetProperty("value").ToString());
        }

        private void Store() {
            Remember();
            _history.StartNewCommand();
        }

        /// <summary>
        /// Get the next command
        /// </summary>
        public string GetNextCommand() {
            _history.ReadNext();
            return _history.CurrentReadCommand;
        }

        /// <summary>
        /// Get the previous command
        /// </summary>
        public string GetPreviousCommand() {
            Remember();
            _history.ReadPrev();
            return _history.CurrentReadCommand;
        }

        public class ReplHistory {

            private List<string> _commands;
            private int _readPointer;
            private int _writePointer;
            private Func<bool> ResetReadPointerBehavior { get; set; }

            public List<string> Commands { get { return _commands; } }
            
            public ReplHistory() : this(true) {
                Clear();
            }

            public ReplHistory(bool cmdExeBehavior) {
                if (cmdExeBehavior) {
                    ResetReadPointerBehavior = new Func<bool>(() => _readPointer == _writePointer);
                } else {
                    ResetReadPointerBehavior = new Func<bool>(() => true);
                }
            }

            public string CurrentReadCommand {
                get {
                    return GetCommand(_readPointer);
                }
            }

            public string CurrentWriteCommand {
                get {
                    return GetCommand(_writePointer);
                }
            }

            public int ReadPrev() {
                if (_readPointer > 0)
                    _readPointer -= 1;
                return _readPointer;
            }

            public int ReadNext() {
                if (_readPointer < (_commands.Count - 1))
                    _readPointer += 1;
                return _readPointer;
            }

            public string GetCommand(int index) {
                if (_commands.Count == 0) return "";
                return _commands[PointerValue(index)];
            }

            public string SetCommand(int index, string cmd) {
                if (_commands.Count == 0) StartNewCommand();
                return _commands[PointerValue(_writePointer)] = cmd;
            }

            public int ResetReadPointer() {
                return _readPointer = PointerValue(_commands.Count - 1);
            }

            public int ResetWritePointer() {
                return _writePointer = PointerValue(_commands.Count - 1);
            }

            private int PointerValue(int p) {
                if (p <= 0) return 0;
                else if (p > _commands.Count - 1) return _commands.Count - 1;
                return p;
            }

            public string AppendWriteCommand(string cmd) {
                return ReplaceWriteCommand(CurrentWriteCommand + cmd);
            }

            public string AppendWriteCommand(char c) {
                return ReplaceWriteCommand(CurrentWriteCommand + c);
            }

            public string ReplaceWriteCommand(string cmd) {
                return SetCommand(_writePointer, cmd);
            }

            public void StartNewCommand() {
                _commands.Add("");
                if (ResetReadPointerBehavior())
                    ResetReadPointer();
                ResetWritePointer();
            }

            public void Clear() {
                _commands = new List<string>(new string[] { "" });
                _readPointer = 0;
                _writePointer = 0;
            }
        }

        #endregion

        #region Running Code
        /// <summary>
        /// Returns null if "text" is not a complete expression, otherwise
        /// return the "text".
        /// </summary>
        public string TryExpression(string text) {
            var props = _engine.CreateScriptSourceFromString(
                text, SourceCodeKind.Expression
            ).GetCodeProperties();
            string result;
            if (props == ScriptCodeParseResult.Complete || props == ScriptCodeParseResult.Empty) {
                result = text;
            } else {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Run the REPL input, but do not force the execution.
        /// </summary>
        public void RunCode() {
            RunCode(false);
        }

        /// <summary>
        /// Run single and multiple lines from the REPL input, and render the
        /// result to the REPL.
        /// </summary>
        /// <param name="forceExecute">Forces the statement to execute, regardless of it's validity</param>
        public void RunCode(bool forceExecute) {
            var line = _silverlightDlrReplCode.GetProperty("value").ToString();
            _code = (_code == null ? "" : _code + "\n") + line;

            if (_code != null) {
                var multiLine = _code.Split('\n').Length > 1;
                
                object result = null;
                _outputBuffer.UserOutput = true;
                if (_code != string.Empty && !multiLine) {
                    result = DoSingleLine(forceExecute);
                } else {
                    result = DoMultiLine(forceExecute);
                }
                _outputBuffer.UserOutput = false;

                ShowLineAndResult(line, result);
            }
        }

        /// <summary>
        /// If _code is a valid expression, try running it. Otherwise, run the 
        /// code as a multi-line expression.
        /// </summary>
        /// <param name="forceExecute">Forces the statement to execute, regardless of it's validity</param>
        /// <returns>The result of the _code execution</returns>
        private object DoSingleLine(bool forceExecute) {

            var valid = TryExpression(_code);
            if (valid != null) {
                var source = _engine.CreateScriptSourceFromString(_code, SourceCodeKind.Expression);
                return ExecuteCode(source);
            } else {
                DoMultiLine(forceExecute);
            }
            return null;
        }

        /// <summary>
        /// Runs a multi-line expression. If it's not complete (or not forced
        /// to execute, return null.
        /// </summary>
        /// <param name="forceExecute">Forces the statement to execute, regardless of it's validity</param>
        /// <returns>result of the _code execution</returns>
        private object DoMultiLine(bool forceExecute) {
            if (forceExecute || IsComplete(_code, AllowIncomplete())) {
                _multiLineComplete = true;
                var source = _engine.CreateScriptSourceFromString(_code, SourceCodeKind.InteractiveCode);
                return ExecuteCode(source);
            } else {
                _multiLine = true;
                _multiLineComplete = false;
            }
            return null;
        }

        /// <summary>
        /// Execute code against the currentScope, or create one if it doesn't 
        /// exist. Handles the exception if one occured.
        /// </summary>
        /// <param name="source">ScriptSource to execute</param>
        /// <returns>the result of the execution, or null if an exception occured</returns>
        public object ExecuteCode(ScriptSource source) {
            object result;
            try {
                if (_currentScope == null) {
                    _currentScope = _engine.CreateScope();
                }
                result = source.Compile(new ErrorFormatter.Sink()).Execute(_currentScope);
            } catch (Exception e) {
                HandleException(e);
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Handle the exception by writing the stack trace to the output buffer.
        /// </summary>
        /// <param name="e"></param>
        private void HandleException(Exception e) {
            _outputBuffer.WriteLine(_engine.GetService<ExceptionOperations>().FormatException(e));
        }

        /// <summary>
        /// Is the interactive code complete?
        /// </summary>
        public bool IsComplete(string text, bool allowIncomplete) {
            var props = _engine.CreateScriptSourceFromString(
                text, SourceCodeKind.InteractiveCode
            ).GetCodeProperties();
            var result = (props != ScriptCodeParseResult.Invalid) &&
                (props != ScriptCodeParseResult.IncompleteToken) &&
                (allowIncomplete || (props != ScriptCodeParseResult.IncompleteStatement));
            return result;
        }

        /// <summary>
        /// Allow incomplete if there is more than 0 lines and the last line is blank.
        /// </summary>
        /// <returns></returns>
        private bool AllowIncomplete() {
            var lines = _code.Split('\n');
            if(lines.Length == 0) return false;
            return lines[lines.Length - 1] == string.Empty;
        }
        #endregion

        #region Rendering
        /// <summary>
        /// Given a line of code and it's result, render it to the REPL.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="result"></param>
        internal void ShowLineAndResult(string line, object result) {
            ShowCodeLineInResultDiv(line);

            if (!_multiLine || _multiLineComplete) {
                FlushOutputInResultDiv();
                ShowValueInResultDiv(result);
                ShowPrompt();
                Reset();
            } else {
                ShowSubPrompt();
            }

            ShowDefaults();
        }

        /// <summary>
        /// Defaults of the REPL
        /// </summary>
        internal void ShowDefaults() {
            _silverlightDlrReplCode.SetProperty("value", "");
            try {
                _silverlightDlrReplPrompt.Focus();
                _silverlightDlrReplCode.Focus();
            } catch {}
        }

        #region Code input
        /// <summary>
        /// Append code to the input.
        /// </summary>
        /// <param name="str"></param>
        public void AppendCode(string str) {
            string toPrepend = HtmlPage.Document.GetElementById(GetId(_sdlrCode)).GetProperty("value").ToString();
            HtmlPage.Document.GetElementById(GetId(_sdlrCode)).SetProperty("value", toPrepend + str);
        }
        #endregion

        #region Prompt
        /// <summary>
        /// Render what happens on multi-line input.
        /// </summary>
        internal void ShowSubPrompt() {
            _outputBuffer.PutTextInElement(SubPromptHtml(), _silverlightDlrReplPrompt);
        }

        /// <summary>
        /// Render what happens on normal input.
        /// </summary>
        internal void ShowPrompt() {
            _outputBuffer.PutTextInElement(PromptHtml(), _silverlightDlrReplPrompt);
        }

        /// <summary>
        /// Normal prompt
        /// </summary>
        internal string PromptHtml() {
            return String.Format("{0}> ", _engine.Setup.FileExtensions[0].Substring(1));
        }

        /// <summary>
        /// Multi-line prompt
        /// </summary>
        internal string SubPromptHtml() {
            return "... ";
        }
        #endregion

        #region Pushing stuff into Result Div
        /// <summary>
        /// Render code line in the results section of the Repl
        /// </summary>
        /// <param name="codeLine"></param>
        internal void ShowCodeLineInResultDiv(string codeLine) {
            ShowPromptInResultDiv();
            _outputBuffer.ElementClass = _sdlrLine;
            _outputBuffer.ElementName = "div";
            _outputBuffer.Write(codeLine);
            _outputBuffer.Reset();
        }

        /// <summary>
        /// Render the prompt in the results section of the Repl.
        /// </summary>
        internal void ShowPromptInResultDiv() {
            _outputBuffer.ElementClass = _sdlrPrompt;
            _outputBuffer.Write(_multiLinePrompt ? SubPromptHtml() : PromptHtml());
            _outputBuffer.Reset();
            if (_multiLine) {
                _multiLinePrompt = true;
            }
        }

        /// <summary>
        /// Render the language-specific result representation in the results
        /// section of the Repl.
        /// </summary>
        /// <param name="result"></param>
        internal void ShowValueInResultDiv(object result) {
            _outputBuffer.ElementClass = _sdlrValue;
            _outputBuffer.ElementName = "div";
            _outputBuffer.Write("=> " + _engine.Operations.Format(result));
            _outputBuffer.Reset();
        }

        /// <summary>
        /// Flush the contents of the OutputBuffer.
        /// </summary>
        internal void FlushOutputInResultDiv() {
            _outputBuffer.Flush();
        }
        #endregion

        #region History
        /// <summary>
        /// Render the next command
        /// </summary>
        public void ShowNextCommand() {
            _silverlightDlrReplCode.SetProperty("value", GetNextCommand());
        }

        /// <summary>
        /// Render the previous command
        /// </summary>
        public void ShowPreviousCommand() {
            _silverlightDlrReplCode.SetProperty("value", GetPreviousCommand());
        }
        #endregion

        #endregion
    }

    #region Text Buffer
    /// <summary>
    /// Repl's output buffer
    /// </summary>
    public class ReplOutputBuffer : ConsoleWriter {

        public string ElementClass;
        public string ElementName;
        public bool UserOutput;

        private HtmlElement _results;
        private string _outputClass;
        private string _queue;

        public ReplOutputBuffer(HtmlElement results, string outputClass) {
            _results = results;
            _outputClass = outputClass;
            _queue = "";
            Reset();
        }

        public string Queue { get { return _queue; } }

        public override void Write(string str) {
            if (UserOutput) {
                _queue += str;
            } else {
                AppendToResults(str);
            }
        }

        public void flush() {
            Flush();
        }

        public void Reset() {
            ElementClass = null;
            ElementName = "span";
            UserOutput = false;
        }

        public override void Flush() {
            if (_queue != string.Empty) {
                AppendToResults(_queue, _outputClass);
                _queue = string.Empty;
            }
        }

        #region HTML Helpers
        private void AppendToResults(string str) {
            AppendToResults(str, null);
        }

        private void AppendToResults(string str, string outputClass) {
            str = str == String.Empty ? " " : str;
            _results.AppendChild(PutTextInNewElement(str, ElementName, outputClass ?? ElementClass));
        }

        // TODO any library I can use to do this?
        private static string EscapeHtml(string text) {
            return text.Replace("\t", "  ").
                Replace("&", "&amp;").
                Replace(" ", "&nbsp;").
                Replace("<", "&lt;").
                Replace(">", "&gt;").
                Replace("\"", "&quot;").
                Replace(ConsoleWriter.NewLineChar.ToString(), "<br />");
        }

        private HtmlElement PutTextInNewElement(string str, string tagName, string className) {
            var element = HtmlPage.Document.CreateElement(tagName == null ? "span" : tagName);
            if (className != null) {
                element.CssClass = className;
            }
            PutTextInElement(str, element);
            return element;
        }

        internal void PutTextInElement(string str, HtmlElement e) {
            e.SetProperty("innerHTML", EscapeHtml(str));
        }

        internal void AppendTextInElement(string str, HtmlElement e) {
            var toPrepend = e.GetProperty("innerHTML").ToString();
            e.SetProperty("innerHTML", toPrepend + EscapeHtml(str));
        }
        #endregion
    }

    /// <summary>
    /// Input console buffer 
    /// </summary>
    public class ReplInputBuffer : ConsoleWriter {
        
        private Repl _console;
        
        public ReplInputBuffer(Repl console) {
            _console = console;
        }
        
        public override void Write(string str) {
            string[] lines = str.Split(CoreNewLine);
            if (lines.Length > 1) {
                foreach (var line in lines) {
                    _console.AppendCode(line);
                    _console.RunCode();
                }
            } else if (lines.Length != 0) {
                _console.AppendCode(lines[lines.Length - 1]);
            }
        }
    }

    /// <summary>
    /// Base console writer. Subclasses implement "Write"
    /// </summary>
    public abstract class ConsoleWriter : TextWriter {

        public readonly static char NewLineChar = '\n';
        
        protected Encoding _encoding;
        
        public ConsoleWriter() {
            _encoding = new System.Text.UTF8Encoding();
            CoreNewLine = new char[] { NewLineChar };
        }
        
        public override Encoding Encoding { get { return _encoding; } }
        
        public override void WriteLine(string str) {
            Write(str + "\n");
        }
        
        public abstract override void Write(string str);

        public void write(string str) {
            Write(str);
        }
    }
    #endregion
}
