/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.IronStudio.Core.Repl;
using Microsoft.IronStudio.Repl;
using Microsoft.Scripting.Utils;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronStudio.Library.Repl {

    public class ReplWindow : IReplWindow, IReplWindowCommands, IReplPromptProvider, IMixedBuffer, IDisposable {
        private DockPanel _panel;
        private ComboBox _scopebox;
        private AutoResetEvent _inputEvent = new AutoResetEvent(false);
        private bool _useSmartUpDown;

        private bool _isRunning;
        private bool _showOutput;

        private IClassifier _classifier;
        private IIntellisenseSessionStack _sessionStack;
        private ITrackingPoint _inputPoint;
        private ITrackingPoint _outputPoint;
        private Action<ReplSpan> _processResult;
        private Stopwatch _sw;
        private string _unCommittedInput;
        private DispatcherTimer _executionTimer;
        private Cursor _oldCursor;
        private List<IReplCommand> _commands;
        private IWpfTextViewHost _textViewHost;
        private IEditorOperations _editorOperations;

        private readonly IComponentModel/*!*/ _componentModel;
        private readonly IReplEvaluator/*!*/ _evaluator;
        private readonly IContentType/*!*/ _contentType;
        private readonly string/*!*/ _title;
        private readonly List<ITrackingSpan>/*!*/ _inputSpans;
        private readonly List<int>/*!*/ _errorInputs;
        private readonly List<PendingInput>/*!*/ _pendingInput;
        private readonly List<string>/*!*/ _demoCommands;
        private readonly History/*!*/ _history; // TODO: history provider should be loaded via MEF?

        // We use one or two regions to protect span [0, input start) from modifications:
        private readonly IReadOnlyRegion[]/*!*/ _readOnlyRegions;

        private readonly string/*!*/ _commandPrefix;
        private readonly string/*!*/ _prompt;

        private struct PendingInput {
            public readonly string Text;
            public readonly bool HasNewline;

            public PendingInput(string text, bool hasNewline) {
                Text = text;
                HasNewline = hasNewline;
            }
        }

        #region Construction

        public ReplWindow(IComponentModel/*!*/ model, IReplEvaluator/*!*/ evaluator, IContentType/*!*/ contentType, string/*!*/ title) {
            ContractUtils.RequiresNotNull(evaluator, "evaluator");
            ContractUtils.RequiresNotNull(contentType, "contentType");
            ContractUtils.RequiresNotNull(title, "title");
            ContractUtils.RequiresNotNull(model, "model");

            _componentModel = model;
            _evaluator = evaluator;
            _contentType = contentType;
            _title = title;
            
            _commandPrefix = _evaluator.CommandPrefix;
            _prompt = _evaluator.Prompt;

            ContractUtils.Requires(_commandPrefix != null && _prompt != null);
            
            _readOnlyRegions = new IReadOnlyRegion[2];
            _pendingInput = new List<PendingInput>();
            _inputSpans = new List<ITrackingSpan>();
            
            _errorInputs = new List<int>();

            _history = new History();
            _demoCommands = new List<string>();

            SetupOutput();
        }

        public void Initialize() {
            _textViewHost = CreateTextViewHost();

            var textView = _textViewHost.TextView;

            textView.Options.SetOptionValue(DefaultTextViewHostOptions.HorizontalScrollBarId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.LineNumberMarginId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.OutliningMarginId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.GlyphMarginId, _evaluator.DisplayPromptInMargin);
            textView.Options.SetOptionValue(DefaultTextViewOptions.WordWrapStyleId, WordWrapStyles.WordWrap);
            
            _editorOperations = GetEditorOperations(textView);

            StartEvaluator();

            _commands = CreateCommands();
            CreateUI();

            Evaluator.TextViewCreated(this, textView);

            textView.TextBuffer.Properties.AddProperty(typeof(IReplEvaluator), _evaluator);
            //
            // WARNING: We must set following properties only after everything is inititalized. 
            // If we set them earlier we would expose uninitialized repl window properties.
            //
            textView.TextBuffer.Properties.AddProperty(typeof(IMixedBuffer), this);
            textView.TextBuffer.Properties.AddProperty(typeof(IReplWindowCommands), this);

            // the margin publishes itself in the properties upon creation:
            // textView.TextBuffer.Properties.TryGetProperty(typeof(ReplMargin), out _margin);

            //if (_evaluator.DisplayPromptInMargin) {
            //    _margin = _textViewHost.GetTextViewMargin(PredefinedMarginNames.Glyph);
            //}

            PrepareForInput();
            ApplyProtection();
        }        

        private List<IReplCommand>/*!*/ CreateCommands() {
            var commands = new List<IReplCommand>();
            var commandTypes = new HashSet<Type>();
            foreach (var command in _componentModel.GetExtensions<IReplCommand>()) {
                // avoid duplicate commands
                if (commandTypes.Contains(command.GetType())) {
                    continue;
                } else {
                    commandTypes.Add(command.GetType());
                }

                commands.Add(command);
            }
            return commands;
        }

        private void CreateUI() {
            // background: new SolidColorBrush(Color.FromArgb(0, 188, 199, 216));
            _panel = new DockPanel();

            ToolBarTray tray = new ToolBarTray();
            ToolBar toolBar = new ToolBar();
            tray.ToolBars.Add(toolBar);

            tray.Background = new SolidColorBrush(Color.FromArgb(255, 173, 185, 205));
            toolBar.Background = new SolidColorBrush(Color.FromArgb(255, 188, 199, 216));

            IMultipleScopeEvaluator multiScopeEval = Evaluator as IMultipleScopeEvaluator;
            if (multiScopeEval != null && multiScopeEval.EnableMultipleScopes) {
                AddScopeBox(toolBar, multiScopeEval);
            }

            foreach (var command in _commands) {
                AddToolBarButton(toolBar, command);
            }

            //_toolbar.Children.Add(comboBox);
            DockPanel.SetDock(tray, Dock.Top);

            _panel.Children.Add(tray);

            _panel.Children.Add((UIElement)_textViewHost);
        }

        #endregion

        #region To Be Moved Into Contracts?

        public event Action ExecutionFinished;

        public IEditorOperations EditorOperations {
            get {
                return _editorOperations;
            }
        }

        public IComponentModel ComponentModel {
            get {
                return _componentModel;
            }
        }

        public ITextBuffer/*!*/ TextBuffer {
            get { return CurrentView.TextBuffer; }
        }

        public IClassifier Classifier {
            get {
                if (_classifier == null) {
                    var aggregator = ComponentModel.GetService<IClassifierAggregatorService>();
                    _classifier = aggregator.GetClassifier(TextBuffer);
                }

                return _classifier;
            }
        }

        public ITextSnapshot CurrentSnapshot {
            get { return TextBuffer.CurrentSnapshot; }
        }

        public string CurrentLine {
            get {
                return Caret.Position.BufferPosition.GetContainingLine().GetText();
            }
        }

        protected virtual IWpfTextViewHost CreateTextViewHost() {
            var textBufferFactoryService = ComponentModel.GetService<ITextBufferFactoryService>();
            var textBuffer = textBufferFactoryService.CreateTextBuffer(ContentType);

            // we need to set IReplProptProvider property before TextViewHost is instantiated so that ReplPromptTaggerProvider can bind to it 
            if (Evaluator.DisplayPromptInMargin) {
                textBuffer.Properties.AddProperty(typeof(IReplPromptProvider), this);
            }

            ITextEditorFactoryService textEditorFactoryService = ComponentModel.GetService<ITextEditorFactoryService>();
            ITextViewRoleSet roles =  GetReplRoles();

            var textView = textEditorFactoryService.CreateTextView(textBuffer, roles);
            return textEditorFactoryService.CreateTextViewHost(textView, false);
        }

        protected ITextViewRoleSet/*!*/ GetReplRoles() {
            var textEditorFactoryService = ComponentModel.GetService<ITextEditorFactoryService>();
            return textEditorFactoryService.CreateTextViewRoleSet(
                PredefinedTextViewRoles.Analyzable, 
                PredefinedTextViewRoles.Editable, 
                PredefinedTextViewRoles.Interactive, 
                PredefinedTextViewRoles.Zoomable, 
                PredefinedTextViewRoles.Document, 
                CoreConstants.ReplTextViewRole
            );
        }

        #endregion

        #region IReplWindow

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public ITextCaret Caret {
            get {
                return CurrentView.Caret;
            }
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public FrameworkElement/*!*/ Content {
            get {
                return _panel; 
            }
        }


        /// <summary>
        /// Content type provided by the evaluator.
        /// </summary>
        public IContentType/*!*/ ContentType {
            get {
                return _contentType;
            }
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public IWpfTextView/*!*/ CurrentView {
            get { 
                return _textViewHost.TextView; 
            }
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public IReplEvaluator/*!*/ Evaluator {
            get {
                return _evaluator;
            }
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public bool ShowOutput {
            get {
                return _showOutput;
            }
            set {
                Evaluator.FlushOutput();
                _showOutput = value;
            }
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public string/*!*/ Title {
            get {
                return _title;
            }
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public void ClearScreen() {
            if (!CheckAccess()) {
                Dispatcher.Invoke(new Action(() => ClearScreen()));
                return;
            }

            RemoveProtection();
            
            _inputSpans.Clear();
            _errorInputs.Clear();
            _inputPoint = null;
            
            using (var edit = TextBuffer.CreateEdit()) {
                edit.Delete(0, CurrentSnapshot.Length);
                edit.Apply();
            }

            PrepareForInput();
            ApplyProtection();
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public void Focus() {
            var textView = CurrentView;

            IInputElement input = textView as IInputElement;
            if (input != null) {
                Keyboard.Focus(input);
            }
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public void PasteText(string text) {
            if (!CheckAccess()) {
                Dispatcher.BeginInvoke(new Action(() => PasteText(text)));
                return;
            }

            if (_isRunning) {
                return;
            }

            if (text.IndexOf('\n') >= 0) {
                if (IsCaretProtected) {
                    _editorOperations.MoveToEndOfDocument(false);
                }

                // TODO: Use a language service to parse out individual commands rather
                // than pretending each line has been typed in manually
                _pendingInput.AddRange(SplitLines(text));
                ProcessPendingInput();
            } else {
                if (_editorOperations.SelectedText != "") {
                    _editorOperations.Delete();
                }

                _editorOperations.InsertText(text);
            }
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public void Reset() {
            WriteLine("Resetting execution engine");
            Evaluator.Reset();
        }

        public void AbortCommand() {
            Evaluator.AbortCommand();
            EnsureNewLine();
            PrepareForInput();
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public void WriteLine(string text) {
            Write(text + _textViewHost.TextView.Options.GetNewLineCharacter());
        }

        #endregion

        #region IReplWindowCommands

        /// <summary>
        /// See IReplWindowCommands
        /// </summary>
        public void BreakLine() {
            AutoIndent.HandleReturn(this);
        }

        /// <summary>
        /// See IReplWindowCommands
        /// </summary>
        public void Cancel() {
            ClearCurrentInput();
        }

        /// <summary>
        /// See IReplWindowCommands
        /// </summary>
        public void SmartUpArrow() {
            UIThread(() => {
                if (!((IIntellisenseCommandTarget)this.SessionStack).ExecuteKeyboardCommand(IntellisenseKeyboardCommand.Up)) {
                    // uparrow and downarrow at the end of input or with empty input rotate history
                    // with multi-line input, uparrow and downarrow move around in text
                    if (!_isRunning && CaretAtEnd && UseSmartUpDown) {
                        HistoryPrevious();
                    } else {
                        _editorOperations.MoveLineUp(false);
                    }
                }
            });
        }

        public bool UseSmartUpDown {
            get {
                return _useSmartUpDown;
            }
            set {
                _useSmartUpDown = value;
            }
        }

        public void HistoryPrevious() {
            var found = _history.FindPrevious("");
            if (found != null) {
                StoreUncommittedInput();
                SelectHistoryItem(found);
            }
        }

        private bool IsSingleLineInput {
            get {
                return ActiveInput.Split('\n').Length == 1;
            }
        }

        /// <summary>
        /// See IReplWindowCommands
        /// </summary>
        public void SmartDownArrow() {
            UIThread(() => {
                if (!((IIntellisenseCommandTarget)this.SessionStack).ExecuteKeyboardCommand(IntellisenseKeyboardCommand.Down)) {
                    if (!_isRunning && CaretAtEnd && UseSmartUpDown) {
                        HistoryNext();
                    } else {
                        _editorOperations.MoveLineDown(false);
                    }
                }
            });
        }

        public void HistoryNext() {
            var found = _history.FindNext("");
            if (found != null) {
                StoreUncommittedInput();
                SelectHistoryItem(found);
            } else {
                InsertUncommittedInput();
            }
        }

        /// <summary>
        /// See IReplWindowCommands
        /// </summary>
        public void Home(bool extendSelection) {
            UIThread(() => {
                var caret = Caret;
                var currentInput = MakeInputSpan();

                if (currentInput != null) {
                    var start = currentInput.GetSpan(CurrentSnapshot).Start;
                    int lineNumber = CurrentSnapshot.GetLineNumberFromPosition(start.Position);

                    if (CurrentSnapshot.GetLineNumberFromPosition(caret.Position.BufferPosition) != lineNumber) {
                        _editorOperations.MoveToStartOfLine(extendSelection);
                    } else if (extendSelection) {
                        VirtualSnapshotPoint anchor = CurrentView.Selection.AnchorPoint;
                        caret.MoveTo(start);
                        CurrentView.Selection.Select(anchor.TranslateTo(CurrentView.TextSnapshot), CurrentView.Caret.Position.VirtualBufferPosition);
                    } else {
                        CurrentView.Selection.Clear();
                        caret.MoveTo(start);
                    }
                } else {
                    _editorOperations.MoveToStartOfLine(extendSelection);
                }
            });
        }

        /// <summary>
        /// See IReplWindowCommands
        /// </summary>
        public bool PasteClipboard() {
            return UIThread(() => {
                // TODO:
                //var handler = TryGetSho();
                //if (handler != null) {
                //    var data = handler();
                //    if (data) {
                //        var varname = Evaluator.InsertData(data, "__data");
                //        PasteText(varname);
                //        ExecuteText();
                //        return true;
                //    }
                //}
                if (Clipboard.ContainsText()) {
                    PasteText(Clipboard.GetText());
                } else if (Clipboard.ContainsImage()) {
                    var image = Clipboard.GetImage();
                    var varname = Evaluator.InsertData(image, "__data");
                    PasteText(varname);
                } else {
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// See IReplWindowCommands
        /// </summary>
        public bool SelectAll() {
            return UIThread(() => {
                var ts = CurrentView.Selection;
                var region = GetInputSpanContainingCaret();
                if (region != null && (ts.SelectedSpans.Count == 0 || region != ts.SelectedSpans[0])) {
                    ts.Select(region.Value, true);
                    return true;
                }
                return false;
            });
        }

        #endregion

        #region IMixedBuffer

        /// <summary>
        /// See IMixedBuffer
        /// </summary>
        public SnapshotSpan[]/*!*/ GetLanguageSpans(ITextSnapshot/*!*/ snapshot) {
            var result = new List<SnapshotSpan>();
            foreach (var span in GetAllSpans(snapshot)) {
                if (!span.WasCommand) {
                    // All the input spans have their CRLFs stripped off. Add to the span to compensate.
                    if (span.Input.End.Position + 2 < span.Input.Snapshot.Length) {
                        result.Add(new SnapshotSpan(span.Input.Start, span.Input.End + 2));
                    } else {
                        result.Add(span.Input);
                    }
                }
            }

#if DEBUG
            // we should never include redundant spans
            for (int i = 1; i < result.Count; i++) {
                Debug.Assert(result[i].Start.Position > result[i - 1].Start.Position);
            }
#endif
            return result.ToArray();
        }

        /// <summary>
        /// See IMixedBuffer
        /// </summary>
        public SnapshotSpan? GetLanguageSpanForLine(ITextSnapshot snapshot, int line) {
            return UIThread(() => {
                // check if the current input is a relevant span
                var currentInput = MakeInputSpan(false, snapshot);
                if (currentInput != null) {
                    var res = GetSpanFromInput(snapshot, line, currentInput);
                    if (res != null) {
                        return res;
                    }
                }

                // then check the previous inputs
                foreach (var curInput in _inputSpans) {
                    var res = GetSpanFromInput(snapshot, line, curInput);
                    if (res != null) {
                        return res;
                    }
                }
                return null;
            });
        }

        #endregion

        #region IReplProptProvider Members

        bool IReplPromptProvider.HasPromptForLine(ITextSnapshot/*!*/ snapshot, int lineNumber) {
            return UIThread(() => {
                if (!_isRunning && 
                    (_inputPoint != null && snapshot.GetLineNumberFromPosition(_inputPoint.GetPosition(snapshot)) == lineNumber ||
                    _inputPoint == null && lineNumber == snapshot.LineCount - 1)) {
                    return true;
                }

                // TODO: bin search?
                foreach (var input in _inputSpans) {
                    if (snapshot.GetLineNumberFromPosition(input.GetStartPoint(snapshot)) == lineNumber) {
                        return true;
                    }
                }
                return false;
            });
        }

        public event Action<SnapshotSpan> PromptChanged;

        string/*!*/ IReplPromptProvider.Prompt {
            get { return _prompt; }
        }

        Control/*!*/ IReplPromptProvider.HostControl {
            get { return _textViewHost.HostControl; }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// See IDisposable
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }

        #endregion

        #region Protected Methods

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                Evaluator.Dispose();

                _commands = null;
            }
        }

        #endregion

        #region Internal Methods

        internal bool CanExecuteInput() {
            var input = ActiveInput;
            if (input.Trim().Length == 0) {
                // Always allow "execution" of a blank line.
                // This will just close the current prompt and start a new one
                return true;
            }

            // Ignore any whitespace past the insertion point when determining
            // whether or not we're at the end of the input
            var pt = RelativeInsertionPoint;
            var atEnd = (pt == input.Length) || (pt >= 0 && input.Substring(pt).Trim().Length == 0);
            if (!atEnd) {
                return false;
            }

            // A command is never multi-line, so always try to execute something which looks like a command
            if (input.StartsWith(_commandPrefix)) {
                return true;
            }

            return Evaluator.CanExecuteText(input);
        }

        #endregion

        #region Private Methods

        private Dispatcher Dispatcher {
            get {
                return ((FrameworkElement)CurrentView).Dispatcher;
            }
        }

        private int CurrentInsertionPoint {
            get { return Caret.Position.BufferPosition.Position; }
        }

        private bool CaretAtEnd {
            get { return CurrentInsertionPoint == CurrentSnapshot.Length; }
        }

        private string ActiveInput {
            get {
                var s = CurrentSnapshot;
                var p = _inputPoint.GetPosition(s);
                return s.GetText(p, s.Length - p);
            }
        }

        private int RelativeInsertionPoint {
            get {
                var p = _inputPoint.GetPosition(CurrentSnapshot);
                return CurrentInsertionPoint - p;
            }
        }
        
        private IIntellisenseSessionStack SessionStack {
            get {
                if (_sessionStack == null) {
                    IIntellisenseSessionStackMapService stackMapService = ComponentModel.GetService<IIntellisenseSessionStackMapService>();
                    _sessionStack = stackMapService.GetStackForTextView(CurrentView);
                }

                return _sessionStack;
            }
        }

        private bool IsCaretProtected {
            get {
                return _readOnlyRegions[0] != null &&
                    Caret.Position.BufferPosition.Position < _readOnlyRegions[0].Span.GetStartPoint(CurrentView.TextBuffer.CurrentSnapshot).Position;
            }
        }

        private bool CaretInFirstInputLine {
            get {
                var pos = Caret.Position.BufferPosition.Position;
                var line = Caret.Position.BufferPosition.GetContainingLine().LineNumber;
                var currentInput = MakeInputSpan();
                if (currentInput == null) {
                    return false;
                }
                var span = currentInput.GetSpan(CurrentSnapshot);
                return (span.Start.GetContainingLine().LineNumber == line &&
                        pos >= span.Start.Position);
            }
        }

        private bool CaretInLastInputLine {
            get {
                var pos = Caret.Position.BufferPosition.Position;
                var line = Caret.Position.BufferPosition.GetContainingLine().LineNumber;
                var currentInput = MakeInputSpan();
                if (currentInput == null) {
                    return false;
                }
                var span = currentInput.GetSpan(CurrentSnapshot);
                return (span.End.GetContainingLine().LineNumber == line);
            }
        }

        public bool CaretInInputRegion {
            get {
                var currentInput = MakeInputSpan(false, null);
                if (currentInput == null) {
                    return false;
                }
                var span = currentInput.GetSpan(CurrentSnapshot);
                return Caret.Position.BufferPosition.Position >= span.Start.Position && Caret.Position.BufferPosition.Position <= span.End.Position;
            }
        }

        private string InputTextToCaret {
            get {
                var s = CurrentSnapshot;
                var p = _inputPoint.GetPosition(s);
                var sl = Caret.Position.BufferPosition.Position - p;
                return s.GetText(p, sl);
            }
        }


        private bool CheckAccess() {
            return Dispatcher.CheckAccess();
        }

        public void ExecuteText() {
            ExecuteText(null);
        }

        private void AppendInput(string text) {
            if (!CheckAccess()) {
                Dispatcher.BeginInvoke(new Action(() => AppendInput(text)));
                return;
            }

            using (var edit = TextBuffer.CreateEdit()) {
                edit.Insert(edit.Snapshot.Length, text);
                edit.Apply();
            }

            Caret.EnsureVisible();
        }

        private void EnsureNewLine() {
            if (!CheckAccess()) {
                Dispatcher.BeginInvoke(new Action(() => EnsureNewLine()));
                return;
            }

            AppendInput(_textViewHost.TextView.Options.GetNewLineCharacter());
            if (!CaretAtEnd) {
                _editorOperations.MoveToEndOfDocument(false);
            }
        }

        private ITrackingSpan MakeInputSpan() {
            return MakeInputSpan(false, null);
        }

        private ITrackingSpan MakeInputSpan(bool excludeCrLf, ITextSnapshot snapshot) {
            if (_isRunning || _inputPoint == null) {
                return null;
            }

            snapshot = snapshot ?? CurrentSnapshot;
            int pos = _inputPoint.GetPosition(snapshot);
            int len = snapshot.Length - pos;

            if (excludeCrLf && len > 1) {
                // remove the line feed including any trailing linespace
                var text = snapshot.GetText(pos, len);
                var endTrimmed = text.TrimEnd(' ');
                string newLine = _textViewHost.TextView.Options.GetNewLineCharacter();
                if (endTrimmed.Length > newLine.Length && String.Compare(endTrimmed, endTrimmed.Length - newLine.Length, newLine, 0, newLine.Length) == 0) {
                    len -= newLine.Length + (text.Length - endTrimmed.Length);
                }
            }
            return snapshot.CreateTrackingSpan(pos, len, SpanTrackingMode.EdgeExclusive);
        }

        private void Write(string text) {
            PerformWrite(() => {
                if (_outputPoint == null || !_showOutput) {
                    return;
                }

                using (var edit = TextBuffer.CreateEdit()) {
                    int insertPos = _outputPoint.GetPosition(CurrentSnapshot);
                    edit.Insert(insertPos, text);
                    edit.Apply();
                }
                Caret.EnsureVisible();
            });
        }

        private void WriteOutput(object sender, Output output) {
            if (!CheckAccess()) {
                Dispatcher.BeginInvoke(new Action(() => WriteOutput(sender, output)));
                return;
            }

            if (output.Object != null) {
                if (TryShowObject(output.Object, output.Text)) {
                    return;
                }
                WriteLine(output.Text);
            } else {
                Write(output.Text);
            }
        }

        private string ReadInput() {
            // shouldn't be called on the UI thread because we'll hang
            Debug.Assert(!CheckAccess());

            bool wasRunning = _isRunning;
            // TODO: What do we do if we weren't running?
            if (_isRunning) {
                RemoveProtection();
                _isRunning = false;
            }

            Dispatcher.Invoke(new Action(() => {
                _outputPoint = CurrentSnapshot.CreateTrackingPoint(CurrentSnapshot.Length, PointTrackingMode.Positive);
                _inputPoint = CurrentSnapshot.CreateTrackingPoint(CurrentSnapshot.Length, PointTrackingMode.Negative);
            }));

            _inputEvent.WaitOne();

            if (wasRunning) {
                ApplyProtection();
                _isRunning = true;
            }

            return _textViewHost.TextView.Options.GetNewLineCharacter();
        }

        private List<ITrackingSpan> GetInputSpans() {
            var currentInput = MakeInputSpan();
            if (currentInput == null) {
                return _inputSpans;
            }
            var result = new List<ITrackingSpan>(_inputSpans);
            result.Add(currentInput);
            return result;
        }

        private SnapshotSpan? GetInputSpanContainingCaret() {
            var bufferPos = Caret.Position.BufferPosition;
            var pos = bufferPos.Position;
            var s = bufferPos.Snapshot;
            foreach (var inputSpan in GetInputSpans()) {
                var span = inputSpan.GetSpan(s);
                if (pos >= span.Start.Position && pos <= span.End.Position) {
                    return span;
                }
            }
            return null;
        }
        
        private bool ProcessPendingInput() {
            if (!CheckAccess()) {
                return (bool)(Dispatcher.Invoke(new Action(() => ProcessPendingInput())));
            }

            while (_pendingInput.Count > 0) {
                var line = _pendingInput[0];
                _pendingInput.RemoveAt(0);
                AppendInput(line.Text);
                _editorOperations.MoveToEndOfDocument(false);
                if (line.HasNewline) {
                    if (TryExecuteInput()) {
                        return true;
                    }
                    EnsureNewLine();
                }
            }
            return false;
        }

        private bool RemoveProtection() {
            bool wasProtected = false;
            using (var edit = TextBuffer.CreateReadOnlyRegionEdit()) {
                foreach (var region in _readOnlyRegions) {
                    if (region != null) {
                        edit.RemoveReadOnlyRegion(region);
                        wasProtected = true;
                    }
                }
                edit.Apply();
                _readOnlyRegions[0] = _readOnlyRegions[1] = null;
            }

            return wasProtected;
        }

        private void ApplyProtection() {
            SpanTrackingMode trackingMode;
            EdgeInsertionMode insertionMode;
            int end;
            if (_isRunning) {
                trackingMode = SpanTrackingMode.EdgeInclusive;
                insertionMode = EdgeInsertionMode.Deny;
                end = CurrentSnapshot.Length;
            } else {
                trackingMode = SpanTrackingMode.EdgeExclusive;
                insertionMode = EdgeInsertionMode.Allow;
                end = _inputPoint.GetPosition(CurrentSnapshot);
            }

            using (var edit = TextBuffer.CreateReadOnlyRegionEdit()) {
                IReadOnlyRegion region0 = edit.CreateReadOnlyRegion(new Span(0, end), trackingMode, insertionMode);

                // Create a second read-only region to prevent insert at start of buffer:
                IReadOnlyRegion region1 = (end > 0) ? edit.CreateReadOnlyRegion(new Span(0, 0), SpanTrackingMode.EdgeExclusive, EdgeInsertionMode.Deny) : null;
                
                edit.Apply();
                _readOnlyRegions[0] = region0;
                _readOnlyRegions[1] = region1;
            }
        }

        private bool ExecuteReplCommand(string commandLine) {
            commandLine = commandLine.Trim();
            IReplCommand commandFn = null;
            string args, command = null;
            if (commandLine.Length == 0 || commandLine == "help") {
                ShowReplHelp();
                return true;
            } else if (commandLine.Substring(0, 1) == _commandPrefix) { // TODO ??
                // REPL-level comment; do nothing
                return true;
            } else {
                command = commandLine.Split(' ')[0];
                args = commandLine.Substring(command.Length).Trim();
                commandFn = _commands.Find(x => x.Command == command);
            }

            if (commandFn == null) {
                WriteLine(String.Format("Unknown command '{0}', use \"{1}help\" for help", command, _commandPrefix));
                return false;
            }
            try {
                // commandFn is either an Action or Action<string>
                commandFn.Execute(this, args);
                return true;
            } catch (Exception e) {
                WriteLine(Evaluator.FormatException(new ObjectHandle(e)));
                return false;
            }
        }

        public bool TryExecuteInput() {
            bool tryIt = CanExecuteInput();
            if (tryIt) {
                ExecuteText();
            }
            return tryIt;
        }

        private void PerformWrite(Action action) {
            if (!CheckAccess()) {
                Dispatcher.Invoke(new Action(() => PerformWrite(action)));
                return;
            }

            bool wasProtected = RemoveProtection();
            try {
                action();
            } finally {
                if (wasProtected) {
                    ApplyProtection();
                }
            }
        }

        /// <summary>
        /// Execute and then call the callback function with the result text.
        /// </summary>
        /// <param name="processResult"></param>
        internal void ExecuteText(Action<ReplSpan> processResult) {
            PerformWrite(() => {
                // Ensure that the REPL doesn't try to execute if it is already
                // executing.  If this invariant can no longer be maintained more of
                // the code in this method will need to be bullet-proofed
                if (_isRunning) {
                    return;
                }

                var text = ActiveInput;
                var span = CreateInputSpan(text);

                _isRunning = true;
                Debug.Assert(_processResult == null);
                _processResult = processResult;

                // Following method assumes that _isRunning will be cleared before 
                // the following method is called again.
                StartCursorTimer();

                _sw = Stopwatch.StartNew();
                if (text.Trim().Length == 0) {
                    // Special case to avoid round-trip when remoting
                    FinishExecute(true, null);
                } else if (text.StartsWith(_commandPrefix)) {
                    _history.Last.Command = true;
                    var status = ExecuteReplCommand(text.Substring(_commandPrefix.Length));
                    FinishExecute(status, null);
                } else if (!Evaluator.ExecuteText(ActiveInput, FinishExecute)) {
                    _inputSpans.Add(span);
                    FinishExecute(false, null);
                } else {
                    _inputSpans.Add(span);
                }
            });
        }

        private ITrackingSpan CreateInputSpan(string text) {
            CurrentView.Selection.Clear();
            var s = CurrentSnapshot;
            _outputPoint = s.CreateTrackingPoint(s.Length, PointTrackingMode.Positive);
            var span = MakeInputSpan(true, s);

            if (text.Length > 0) {
                _history.Add(text.TrimEnd());
            }
            return span;
        }

        private void FinishExecute(bool success, ObjectHandle exception) {
            PerformWrite(() => {
                _sw.Stop();
                var handler = ExecutionFinished;
                if (handler != null) {
                    handler();
                }
                if (_history.Last != null) {
                    _history.Last.Duration = _sw.Elapsed.Seconds;
                }
                if (!success) {
                    if (exception != null) {
                        AddFileHyperlink(exception);
                    }
                    if (_history.Last != null) {
                        _history.Last.Failed = true;
                    }
                    _errorInputs.Add(_inputSpans.Count);
                }
                PrepareForInput();
                var processResult = _processResult;
                if (processResult != null) {
                    _processResult = null;
                    var spans = GetAllSpans(CurrentSnapshot);
                    var lastSpan = spans[spans.Length - 2];
                    processResult(lastSpan);
                }
            });
        }

        private static SnapshotSpan? GetSpanFromInput(ITextSnapshot snapshot, int line, ITrackingSpan curInput) {
            var span = curInput.GetSpan(snapshot);
            var start = span.Start;
            var startLine = start.GetContainingLine();

            if (startLine.LineNumber == line) {
                // we are the 1st line of a user entered code block
                return span.Intersection(snapshot.GetLineFromPosition(span.Start.Position).Extent);
            } else if (startLine.LineNumber < line) {
                // we may be part of a multi-line block

                var end = span.End;
                var endLine = end.GetContainingLine();

                if (endLine.LineNumber == line) {
                    // we are the last line of a user entered code block
                    return span.Overlap(snapshot.GetLineFromPosition(end.Position).Extent);
                } else if (endLine.LineNumber > line) {
                    // we are in the middle of a user entered block, return the entire line
                    return snapshot.GetLineFromLineNumber(line).Extent;
                }
            }
            return null;
        }


        public void ExecuteOrPasteSelected() {
            if (CaretInInputRegion) {
                EnsureNewLine();
                ExecuteText();
            } else {
                string input = GetCurrentSelectedInput(TextBuffer.CurrentSnapshot);
                if (input != null) {
                    _editorOperations.MoveToEndOfDocument(false);
                    PasteText(input);
                }
            }
        }

        private string GetCurrentSelectedInput(ITextSnapshot snapshot) {
            List<ITrackingSpan> inputs = GetInputSpans(snapshot);

            var curPosition = Caret.Position.BufferPosition.Position;
            foreach (var input in inputs) {
                if (curPosition >= input.GetStartPoint(snapshot).Position &&
                    curPosition <= input.GetEndPoint(snapshot).Position) {
                        return input.GetText(snapshot);
                }
            }

            return null;
        }
        
        private ReplSpan[] GetAllSpans(ITextSnapshot s) {
            // For thread safety, clone _inputSpans on the UI thread.
            // TODO: review the rest of the intellisense interface for threadsafety
            List<ITrackingSpan> inputs = GetInputSpans(s);

            SnapshotSpan? last = null;
            var result = new List<ReplSpan>();
            for (int i = 0; i < inputs.Count; i++) {
                var input = inputs[i].GetSpan(s);
                if (last != null) {
                    var eoo = input.Start - 1 - (_evaluator.DisplayPromptInMargin ? 0 : _prompt.Length);
                    var start = eoo;
                    var end = last.Value.End;
                    end += Math.Min(2, end.Snapshot.Length - end.Position);
                    if (start.Position > end.Position) {
                        start = end;
                        end = eoo;
                    }
                    var output = new SnapshotSpan(start, end);
                    bool wasCommand = last.Value.GetText().StartsWith(_commandPrefix); // TODO: get less text?
                    bool wasException = _errorInputs.Contains(i);
                    result.Add(new ReplSpan(wasCommand, wasException, last.Value, output));
                }
                last = input;
            }
            if (last != null) {
                bool finalWasCommand = last.Value.GetText().StartsWith(_commandPrefix);
                result.Add(new ReplSpan(finalWasCommand, false, last.Value, null));
            }

            return result.ToArray();
        }

        private List<ITrackingSpan> GetInputSpans(ITextSnapshot s) {
            List<ITrackingSpan> inputs = null;

            UIThread(() => {
                inputs = new List<ITrackingSpan>(_inputSpans);
                var currentInput = MakeInputSpan(false, s);
                if (currentInput != null) {
                    inputs.Add(currentInput);
                }
            });
            return inputs;
        }

        private void SetupOutput() {
            _outputPoint = null;
            _showOutput = true;
        }

        private bool TryShowObject(object obj, string textRepr) {
            Image image = null;
            var imageSource = obj as ImageSource;
            if (imageSource != null) {
                image = new Image();
                image.Source = imageSource;
            }
            return false;
        }

        private void ShowReplHelp() {
            var cmdnames = new List<IReplCommand>(_commands.Where(x => x.Command != null));
            cmdnames.Sort((x, y) => String.Compare(x.Command, y.Command));

            const string helpFmt = "  {0,-16}  {1}";
            WriteLine(string.Format(helpFmt, "help", "Show a list of REPL commands"));

            foreach (var cmd in cmdnames) {
                WriteLine(string.Format(helpFmt, cmd.Command, cmd.Description));
            }
        }

        private void AddFileHyperlink(ObjectHandle exception) {
            // TODO:
        }

        private void PrepareForInput() {            
            Evaluator.FlushOutput();            

            int saved = CurrentSnapshot.Length;
            if (!_evaluator.DisplayPromptInMargin) {
                using (var edit = TextBuffer.CreateEdit()) {
                    // remove any leading white space which may have been inserted by auto-indent support
                    var containingLine = Caret.Position.BufferPosition.GetContainingLine();
                    if (Caret.Position.BufferPosition.Position > containingLine.Start) {
                        int length = Caret.Position.BufferPosition.Position - containingLine.Start.Position;
                        edit.Delete(new Span(containingLine.Start.Position, length));
                        saved -= length;
                    }
                    edit.Insert(saved, _prompt + " ");
                    edit.Apply();
                }
            }

            Caret.EnsureVisible();
                
            _outputPoint = CurrentSnapshot.CreateTrackingPoint(saved, PointTrackingMode.Positive);
            _inputPoint = CurrentSnapshot.CreateTrackingPoint(CurrentSnapshot.Length, PointTrackingMode.Negative);

            ResetCursor();
            _isRunning = false;
            _unCommittedInput = null;
            ProcessPendingInput();

            // we need to update margin prompt after the new _inputPoint is set:
            if (_evaluator.DisplayPromptInMargin) {
                var promptChanged = PromptChanged;
                if (promptChanged != null) {
                    promptChanged(new SnapshotSpan(CurrentSnapshot, new Span(CurrentSnapshot.Length, 0)));
                }
            }
        }

        private void ResetCursor() {
            if (_executionTimer != null) {
                _executionTimer.Stop();
            }
            if (_oldCursor != null) {
                ((ContentControl)CurrentView).Cursor = _oldCursor;
            }
            /*if (_oldCaretBrush != null) {
                CurrentView.Caret.RegularBrush = _oldCaretBrush;
            }*/

            _oldCursor = null;
            //_oldCaretBrush = null;
            _executionTimer = null;
        }

        private void StartCursorTimer() {
            // Save the old value of the caret brush so it can be restored
            // after execution has finished
            //_oldCaretBrush = CurrentView.Caret.RegularBrush;

            // Set the caret's brush to transparent so it isn't shown blinking
            // while code is executing in the REPL
            //CurrentView.Caret.RegularBrush = Brushes.Transparent;

            var timer = new DispatcherTimer();
            timer.Tick += SetRunningCursor;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            _executionTimer = timer;
            timer.Start();
        }

        private void SetRunningCursor(object sender, EventArgs e) {
            var view = (ContentControl)CurrentView;

            // Save the old value of the cursor so it can be restored
            // after execution has finished
            _oldCursor = view.Cursor;

            // TODO: Design work to come up with the correct cursor to use
            // Set the repl's cursor to the "executing" cursor
            view.Cursor = Cursors.Wait;

            // Stop the timeer so it doesn't fire again
            _executionTimer.Stop();
        }

        private void StoreUncommittedInput() {
            if (_unCommittedInput == null && !string.IsNullOrEmpty(ActiveInput)) {
                _unCommittedInput = ActiveInput;
            }
        }

        private void InsertUncommittedInput() {
            if (_unCommittedInput != null) {
                using (var edit = TextBuffer.CreateEdit()) {
                    var snapshot = CurrentSnapshot;
                    var span = MakeInputSpan().GetSpan(snapshot);
                    edit.Replace(span, _unCommittedInput);
                    edit.Apply();
                }
                _unCommittedInput = null;
            }
        }

        private void StartEvaluator() {
            Evaluator.SetIO(WriteOutput, ReadInput);
            Evaluator.Start();
        }

        /// <summary>
        /// Clear the current input region and move the caret to the right
        /// place for entering new text
        /// </summary>
        private void ClearCurrentInput() {
            // if there's an intellisense session we cancel that, otherwise we clear the current input.
            if (!((IIntellisenseCommandTarget)this.SessionStack).ExecuteKeyboardCommand(IntellisenseKeyboardCommand.Escape)) {
                var currentInput = MakeInputSpan();
                if (currentInput != null) {
                    using (var edit = TextBuffer.CreateEdit()) {
                        var span = currentInput.GetSpan(CurrentSnapshot);
                        edit.Delete(span);
                        edit.Apply();
                    }
                }
                _editorOperations.MoveToEndOfDocument(false);
                _unCommittedInput = null;
            }
        }

        private void SelectHistoryItem(string text) {
            string newLine = _textViewHost.TextView.Options.GetNewLineCharacter();
            while (text.EndsWith(newLine)) {
                text = text.Substring(0, text.Length - newLine.Length);
            }

            var position = Caret.Position.BufferPosition.Position;
            using (var edit = TextBuffer.CreateEdit()) {
                var snapshot = CurrentSnapshot;
                var span = MakeInputSpan().GetSpan(snapshot);
                edit.Replace(span, text);
                edit.Apply();
            }

            //Caret.MoveTo(Math.Min(position, CurrentSnapshot.Length));
        }
        
        private T UIThread<T>(Func<T> func) {
            if (!CheckAccess()) {
                return (T)Dispatcher.Invoke(func);
            }
            return func();
        }

        private void UIThread(Action action) {
            if (!CheckAccess()) {
                Dispatcher.Invoke(action);
                return;
            }
            action();
        }

        /// <summary>
        /// Pump events until condition returns true or time runs out.
        /// TODO: move to a utilities class.
        /// </summary>
        private static bool PumpEvents(Func<bool> condition, int msToWait) {
            var d = Dispatcher.CurrentDispatcher;
            var end = DateTime.Now.Ticks + (msToWait * 10000);
            while (!condition() && DateTime.Now.Ticks < end && !d.HasShutdownStarted) {
                DoEvents();
            }
            return condition();
        }

        private static void DoEvents() {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action<DispatcherFrame>(f => f.Continue = false),
                frame
                );
            Dispatcher.PushFrame(frame);
        }

        private static List<PendingInput> SplitLines(string text) {
            List<PendingInput> lines = new List<PendingInput>();
            int curStart = 0;
            for (int i = 0; i < text.Length; i++) {
                if (text[i] == '\r') {
                    if (i < text.Length - 1 && text[i + 1] == '\n') {
                        lines.Add(new PendingInput(text.Substring(curStart, i - curStart), true));
                        curStart = i + 2;
                        i++; // skip \n
                    } else {
                        lines.Add(new PendingInput(text.Substring(curStart, i - curStart), true));
                        curStart = i + 1;
                    }
                } else if (text[i] == '\n') {
                    lines.Add(new PendingInput(text.Substring(curStart, i - curStart), true));
                    curStart = i + 1;
                }
            }
            if (curStart < text.Length) {
                lines.Add(new PendingInput(text.Substring(curStart, text.Length - curStart), false));
            }

            return lines;
        }

        private void AddScopeBox(ToolBar/*!*/ tb, IMultipleScopeEvaluator/*!*/ multiScopeEval) {
            var label = new Label();
            label.Content = "File scope: ";
            label.ToolTip = "Selects the current file scope that the REPL is executing against.";

            var comboBox = _scopebox = new ComboBox();
            comboBox.MouseEnter += new MouseEventHandler(ComboBoxMouseEnter);

            tb.Items.Add(label);
            tb.Items.Add(comboBox);
            UpdateScopeList(this, EventArgs.Empty);
            multiScopeEval.AvailableScopesChanged += new EventHandler<EventArgs>(UpdateScopeList);
            multiScopeEval.CurrentScopeChanged += new EventHandler<EventArgs>(UpdateScopeList);

            comboBox.SelectionChanged += (sender, args) => {
                if (!_updatingList && comboBox.SelectedItem != null) {
                    StoreUncommittedInput();
                    WriteLine(String.Format("Current scope changed to {0}", comboBox.SelectedItem));
                    multiScopeEval.SetScope((string)comboBox.SelectedItem);
                    InsertUncommittedInput();
                }
            };
        }

        void ComboBoxMouseEnter(object sender, MouseEventArgs e) {
            UpdateScopeList(sender, e);
        }

        [ThreadStatic]
        private static bool _updatingList;

        private void UpdateScopeList(object sender, EventArgs e) {
            if (!CheckAccess()) {
                Dispatcher.BeginInvoke(new Action(() => UpdateScopeList(sender, e)));
                return;
            }

            string currentScope = ((IMultipleScopeEvaluator)Evaluator).CurrentScopeName;
            _updatingList = true;
            _scopebox.Items.Clear();            
            int index = 0;
            bool found = false;
            foreach (var scope in ((IMultipleScopeEvaluator)Evaluator).GetAvailableScopes()) {
                _scopebox.Items.Add(scope);
                if (!found && scope == currentScope) {
                    found = true;
                } else if (!found) {
                    index++;
                }
            }
            if (found) {                                
                _scopebox.SelectedIndex = index;
            }
            _updatingList = false;
        }

        private void AddToolBarButton(ToolBar tb, IReplCommand command) {
            if (command.ButtonContent != null) {
                var button = new Button();
                button.Content = command.ButtonContent;
                button.ToolTip = command.Description;
                button.Click += (sender, args) => {
                    if (command.Command != null) {
                        // show the keyboard command to the user
                        ClearCurrentInput();
                        PasteText(_commandPrefix + command.Command);
                        EnsureNewLine();
                        ExecuteText();
                    } else {
                        // no keyboard command, just execute it.
                        command.Execute(this, String.Empty);
                    }
                };
                tb.Items.Add(button);
            }
        }

        private IEditorOperations GetEditorOperations(IWpfTextView textView) {
            IEditorOperationsFactoryService factory = ComponentModel.GetService<IEditorOperationsFactoryService>();
            return factory.GetEditorOperations(textView);
        }

        #endregion
    }
}
