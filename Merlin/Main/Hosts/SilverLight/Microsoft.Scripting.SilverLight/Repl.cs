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
        private List<string>        _history;
        private int                 _currentCommand = -1;
        private static Repl         _current;
        private ReplOutputBuffer    _outputBuffer;
        private ReplInputBuffer     _inputBuffer;
        private HtmlElement         _silverlightDlrReplCode;
        private HtmlElement         _silverlightDlrReplResult;
        private HtmlElement         _silverlightDlrReplPrompt;
        private ScriptEngine        _engine;
        private ScriptScope         _currentScope;
        private static int          _count;
        #endregion

        #region Public properties
        public ReplInputBuffer InputBuffer {
            get { return _inputBuffer; }
        }

        public ReplOutputBuffer OutputBuffer {
            get { return _outputBuffer; }
        }

        public static Repl Current {
            get { return _current; }
        }

        public ScriptEngine Engine {
            get { return _engine; }
        }
        #endregion

        #region Console management
        /// <summary>
        /// Creates a console and inserts it into the page.
        /// </summary>
        public static void Show() {
            if (DynamicApplication.Current == null) {
                throw new Exception("Need to give Show() an engine, since this is not a dynamic application");
            }
            var engine = DynamicApplication.Current.LanguagesConfig.GetEngine(
                DynamicApplication.Current.LanguagesConfig.Languages[0].Names[0]);
            Show(engine, DynamicApplication.Current.Engine.EntryPointScope);
        }

        public static void Show(ScriptEngine engine, ScriptScope scope) {
            ContractUtils.RequiresNotNull(engine, "engine");
            ContractUtils.RequiresNotNull(scope, "scope");

            _count++;

            if (DynamicApplication.Current == null) {
                Window.Show();
            } else {
                Window.Show(Settings.ErrorTargetID);
            }

            Window.Current.AddPanel(engine.Setup.Names[0] + " Console", Create(engine, scope));
            Window.Current.Initialize();
            Repl.Current.Start();
        }

        public static HtmlElement Create() {
            var dynEngine = DynamicApplication.Current.Engine;
            return Create(dynEngine.Engine, dynEngine.EntryPointScope);
        }

        public static HtmlElement Create(ScriptEngine engine, ScriptScope scope) {
            HtmlElement replDiv = null;
            if (_current == null) {
                replDiv = HtmlPage.Document.CreateElement("div");
                replDiv.Id = GetId(_sdlr);
                replDiv.CssClass += _sdlr;
                replDiv.SetProperty("innerHTML", _replHtmlTemplate.Invoke());
                _current = new Repl(engine, scope);
            }
            return replDiv;
        }

        private Repl(ScriptEngine engine, ScriptScope scope) {
            _engine = engine;
            _currentScope = scope;
        }

        public void Start() {
            _silverlightDlrReplCode = HtmlPage.Document.GetElementById(GetId(_sdlrCode));
            _silverlightDlrReplResult = HtmlPage.Document.GetElementById(GetId(_sdlrResult));
            _silverlightDlrReplPrompt = HtmlPage.Document.GetElementById(GetId(_sdlrPrompt));
            _inputBuffer = new ReplInputBuffer(_current);
            _outputBuffer = new ReplOutputBuffer(_silverlightDlrReplResult, _sdlrOutput);
            ShowDefaults();
            ShowPrompt();
            _silverlightDlrReplCode.AttachEvent("onkeypress", new EventHandler<HtmlEventArgs>(OnKeyPress));
        }

        private void OnKeyPress(object sender, HtmlEventArgs args) {
            switch(args.CharacterCode) {
            case 13:
                RunCode(args.CtrlKey);
                break;
            case 38:
                ShowPreviousCommand();
                break;
            case 40:
                if (!args.ShiftKey) {
                    ShowNextCommand();
                }
                break;
            default:
                _currentCommand = _history.Count;
                break;
            };
        }

        private void Remember(string line) {
            if (_history == null) {
                _history = new List<string>();
            }
            _history.Add(line);
        }

        private void Reset() {
            _code = null;
            _multiLine = false;
            _multiLinePrompt = false;
            _multiLineComplete = false;
        }
        #endregion

        #region History
        private int CurrentCommand {
            get {
                if (_history == null) {
                    _currentCommand = -1;
                } else if(_currentCommand == -1) {
                    _currentCommand = _history.Count != 0 ? _history.Count - 1 : 0;
                }
                return _currentCommand;
            }
            set {
                if (_history == null) {
                    _currentCommand = -1;
                } else if (value < 0) {
                    _currentCommand = 0;
                } else if (value > _history.Count - 1) {
                    _currentCommand = _history.Count - 1;
                } else {
                    _currentCommand = value;
                }
            }
        }

        private string TryGetFromHistory(int index) {
            if (_history == null)
                return "";
            return _history[index];
        }

        public string GetNextCommand() {
            CurrentCommand++;
            return TryGetFromHistory(CurrentCommand); 
        }

        public string GetPreviousCommand() {
            --CurrentCommand;
            return TryGetFromHistory(CurrentCommand);
        }
        #endregion

        #region Running Code
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

        public void RunCode() {
            RunCode(false);
        }
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

        private void HandleException(Exception e) {
            _outputBuffer.WriteLine(string.Format("{0}: {1}", e.GetType(), e.Message));
            var dfs = Microsoft.Scripting.Runtime.ScriptingRuntimeHelpers.GetDynamicStackFrames(e);
            if(dfs == null || dfs.Length == 0) {
                _outputBuffer.WriteLine(e.StackTrace != null ? e.StackTrace : e.ToString());
            } else {
                foreach(var frame in dfs) { 
                    _outputBuffer.WriteLine("  at {0} in {1}, line {2}",
                        frame.GetMethodName(),
                        frame.GetFileName() != null ? frame.GetFileName() : null,
                        frame.GetFileLineNumber()
                    );
                }
            }
        }

        public bool IsComplete(string text, bool allowIncomplete) {
            var props = _engine.CreateScriptSourceFromString(
                text, SourceCodeKind.InteractiveCode
            ).GetCodeProperties();
            var result = (props != ScriptCodeParseResult.Invalid) &&
                (props != ScriptCodeParseResult.IncompleteToken) &&
                (allowIncomplete || (props != ScriptCodeParseResult.IncompleteStatement));
            return result;
        }

        private bool AllowIncomplete() {
            var lines = _code.Split('\n');
            if(lines.Length == 0) return false;
            return lines[lines.Length - 1] == string.Empty;
        }
        #endregion

        #region Rendering
        internal void ShowLineAndResult(string line, object result) {
            Remember(line);
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

        internal void ShowDefaults() {
            _silverlightDlrReplCode.SetProperty("value", "");
            _silverlightDlrReplPrompt.Focus();
            _silverlightDlrReplCode.Focus();
        }

        #region Code input
        public void AppendCode(string str) {
            string toPrepend = HtmlPage.Document.GetElementById(GetId(_sdlrCode)).GetProperty("value").ToString();
            HtmlPage.Document.GetElementById(GetId(_sdlrCode)).SetProperty("value", toPrepend + str);
        }
        #endregion

        #region Prompt
        internal void ShowSubPrompt() {
            _outputBuffer.PutTextInElement(SubPromptHtml(), _silverlightDlrReplPrompt);
        }

        internal void ShowPrompt() {
            _outputBuffer.PutTextInElement(PromptHtml(), _silverlightDlrReplPrompt);
        }

        internal string PromptHtml() {
            return String.Format("{0}> ", _engine.Setup.FileExtensions[0].Substring(1));
        }

        internal string SubPromptHtml() {
            return "... ";
        }
        #endregion

        #region Pushing stuff into Result Div
        internal void ShowCodeLineInResultDiv(string codeLine) {
            ShowPromptInResultDiv();
            _outputBuffer.ElementClass = _sdlrLine;
            _outputBuffer.ElementName = "div";
            _outputBuffer.Write(codeLine);
            _outputBuffer.Reset();
        }

        internal void ShowPromptInResultDiv() {
            _outputBuffer.ElementClass = _sdlrPrompt;
            _outputBuffer.Write(_multiLinePrompt ? SubPromptHtml() : PromptHtml());
            _outputBuffer.Reset();
            if (_multiLine) {
                _multiLinePrompt = true;
            }
        }

        internal void ShowValueInResultDiv(object result) {
            ScriptScope scope = _engine.CreateScope();
            scope.SetVariable("sdlr_result", result);
            string resultStr;
            // TODO: Need some language specific way of doing this:
            try {
                resultStr = _engine.CreateScriptSourceFromString("sdlr_result.inspect").Execute(scope).ToString();
            } catch {
                resultStr = _engine.CreateScriptSourceFromString("repr(sdlr_result)").Execute(scope).ToString();
            }
            _outputBuffer.ElementClass = _sdlrValue;
            _outputBuffer.ElementName = "div";
            _outputBuffer.Write("=> " + resultStr);
            _outputBuffer.Reset();
        }

        internal void FlushOutputInResultDiv() {
            _outputBuffer.Flush();
        }
        #endregion

        #region History
        public void ShowNextCommand() {
            _silverlightDlrReplCode.SetProperty("value", GetNextCommand());
        }

        public void ShowPreviousCommand() {
            _silverlightDlrReplCode.SetProperty("value", GetPreviousCommand());
        }
        #endregion

        #endregion
    }

    #region Text Buffer
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

        public void write(string str) {
            Write(str);
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
                Replace((new ConsoleWriter()).NewLine, "<br />");
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

    public class ConsoleWriter : TextWriter {
        protected Encoding _encoding;
        public ConsoleWriter() {
            _encoding = new System.Text.UTF8Encoding();
            CoreNewLine = new char[] { '\n' };
        }
        public override Encoding Encoding { get { return _encoding; } }
        public override void WriteLine(string str) {
            Write(str + "\n");
        }
    }
    #endregion
}
