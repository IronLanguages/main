/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.IronStudio.Repl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.IronRubyTools.Intellisense {

    class IntellisenseController : IIntellisenseController, IOleCommandTarget {
        private readonly ITextView _textView;
        private readonly IList<ITextBuffer> _subjectBuffers;
        private readonly IntellisenseControllerProvider _provider;
#if FEATURE_INTELLISENSE
        private ICompletionSession _activeSession;
        private ISignatureHelpSession _sigHelpSession;
        private IQuickInfoSession _quickInfoSession;
#endif
        private IOleCommandTarget _oldTarget;
        private IEditorOperations _editOps;
        private IntellisensePreKeyProcessor _preProcessor;

        /// <summary>
        /// Attaches events for invoking Statement completion 
        /// </summary>
        /// <param name="subjectBuffers"></param>
        /// <param name="textView"></param>
        /// <param name="completionBrokerMap"></param>
        public IntellisenseController(IntellisenseControllerProvider provider, IList<ITextBuffer> subjectBuffers, ITextView textView) {
            _subjectBuffers = subjectBuffers;
            _textView = textView;
            _provider = provider;
            _editOps = provider._EditOperationsFactory.GetEditorOperations(textView);
#if FEATURE_INTELLISENSE
            _textView.MouseHover += new EventHandler<MouseHoverEventArgs>(TextViewMouseHover);
#endif
            textView.Properties.AddProperty(typeof(IntellisenseController), this);  // added so our key processors can get back to us
        }

#if FEATURE_INTELLISENSE
        private void TextViewMouseHover(object sender, MouseHoverEventArgs e) {
            if (_quickInfoSession != null && !_quickInfoSession.IsDismissed) {
                _quickInfoSession.Dismiss();
            }
            
            _quickInfoSession = _provider._QuickInfoBroker.TriggerQuickInfo(_textView, _textView.TextSnapshot.CreateTrackingPoint(e.Position, PointTrackingMode.Negative), true);
        }
#endif

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer) {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer) {
        }

        /// <summary>
        /// Detaches the events
        /// </summary>
        /// <param name="textView"></param>
        public void Detach(ITextView textView) {
            if (_textView == null) {
                throw new InvalidOperationException("Already detached from text view");
            }
            if (textView != _textView) {
                throw new ArgumentException("Not attached to specified text view", "textView");
            }

            DetachKeyboardFilter();

#if FEATURE_INTELLISENSE
            if (_preProcessor != null) {
                _preProcessor.PreprocessTextInput -= OnPreprocessKeyDown;
            }
#endif
        }

#if FEATURE_INTELLISENSE

        /// <summary>
        /// Triggers Statement completion when appropriate keys are pressed
        /// The key combination is CTRL-J or "."
        /// The intellisense window is dismissed when one presses ESC key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreprocessKeyDown(object sender, TextCompositionEventArgs e) {
            // We should only receive pre-process events from our text view
            Debug.Assert(sender == _textView);

            // TODO: We should handle = for signature completion of keyword arguments

            // We trigger completions when the user types . or space.  Our EditFilter will
            // also trigger completions when we receive the VSConstants.VSStd2KCmdID.SHOWMEMBERLIST
            // command or the VSConstants.VSStd2KCmdID.COMPLETEWORD command.
            //
            // We trigger signature help when we receive a "(".  We update our current sig when 
            // we receive a "," and we close sig help when we receive a ")".

            switch (e.Text) {
                case ".":
                case " ":
                    //DeleteSelectedSpans();
                    TriggerCompletionSession(false);
                    break;
                case "(": OpenParenStartSignatureSession(); break;
                case ")":
                    if (_sigHelpSession != null) {
                        _sigHelpSession.Dismiss();
                        _sigHelpSession = null;
                    }
                    break;
                case ",":
                    if (_sigHelpSession == null) {
                        CommaStartSignatureSession();
                    } else {
                        CommaAdvanceParameter();
                    }
                    break;
            }
        }

        private void Backspace() {
            if (_sigHelpSession != null) {                
                SnapshotPoint? caretPoint = GetCaretPoint();
                if (caretPoint != null && caretPoint.Value.Position != 0) {
                    string deleting = _textView.TextSnapshot.GetText(caretPoint.Value.Position - 1, 1);
                    if (deleting == ",") {
                        PythonSignature sig = _sigHelpSession.SelectedSignature as PythonSignature;
                        if (sig != null) {
                            int curParam = sig.Parameters.IndexOf(sig.CurrentParameter);                            

                            if (curParam > 0) {
                                sig.SetCurrentParameter(sig.Parameters[curParam - 1]);
                            }
                        }
                    } else if (deleting == "(") {
                        // TODO: See if we should pop to an outer nesting of signature help
                        _sigHelpSession.Dismiss();
                    }
                }
            }
        }

        private void OpenParenStartSignatureSession() {
            if (_activeSession != null) {
                // TODO: Should we complete here instead?
                _activeSession.Dismiss();
            }

            SnapshotPoint? caretPoint = GetCaretPoint();

            if (_sigHelpSession != null) {
                _sigHelpSession.Dismiss();
            }

            TriggerSignatureHelp();
        }

        private void CommaStartSignatureSession() {
            TriggerSignatureHelp();
        }

        private void CommaAdvanceParameter() {
            // we advance to the next parameter
            // TODO: Take into account params arrays
            // TODO: need to parse and see if we have keyword arguments entered into the current signature yet
            PythonSignature sig = _sigHelpSession.SelectedSignature as PythonSignature;
            if (sig != null) {
                int curParam = sig.Parameters.IndexOf(sig.CurrentParameter);
                if (curParam != -1) {
                    if (curParam < sig.Parameters.Count - 1) {
                        sig.SetCurrentParameter(sig.Parameters[curParam + 1]);
                    } else {
                        CommaFindBestSignature(curParam);
                    }
                }
            }
        }

        private void CommaFindBestSignature(int curParam) {
            // see if we have a signature which accomodates this...

            // TODO: We should also take into account param arrays
            // TODO: We should also get the types of the arguments and use that to
            // pick the best signature when the signature includes types.
            foreach (var availableSig in _sigHelpSession.Signatures) {
                if (availableSig.Parameters.Count > (curParam + 1)) {
                    _sigHelpSession.SelectedSignature = availableSig;

                    PythonSignature sig = availableSig as PythonSignature;
                    if (sig != null) {
                        sig.SetCurrentParameter(sig.Parameters[curParam + 1]);
                    }
                    break;
                }
            }
        }

        internal void TriggerCompletionSession(bool completeWord) {
            Dismiss();

            _activeSession = CompletionBroker.TriggerCompletion(_textView);

            if (_activeSession != null) {
                if (completeWord &&
                    _activeSession.CompletionSets.Count == 1 &&
                    _activeSession.CompletionSets[0].Completions.Count == 1) {
                    _activeSession.Commit();
                } else {
                    AttachKeyboardFilter();
                    _activeSession.Dismissed += new EventHandler(OnSessionDismissedOrCommitted);
                    _activeSession.Committed += new EventHandler(OnSessionDismissedOrCommitted);
                }
            }
        }

        private void TriggerSignatureHelp() {
            _sigHelpSession = SignatureBroker.TriggerSignatureHelp(_textView);

            if (_sigHelpSession != null) {
                AttachKeyboardFilter();
                _sigHelpSession.Dismissed += new EventHandler(OnSessionDismissedOrCommitted);
            }
        }

        private void OnSessionDismissedOrCommitted(object sender, System.EventArgs e) {
            // We've just been told that our active session was dismissed.  We should remove all references to it.
            _activeSession = null;
            _sigHelpSession = null;
        }

        private SnapshotPoint? GetCaretPoint() {
            return _textView.Caret.Position.Point.GetPoint(
                textBuffer => _subjectBuffers.Contains(textBuffer),
                PositionAffinity.Predecessor
            );
        }

        private void DeleteSelectedSpans() {            
            if (!_textView.Selection.IsEmpty) {
                _editOps.Delete();
            }
        }

        private void Dismiss() {
            if (_activeSession != null) {
                _activeSession.Dismiss();
            }
        }
#endif
        internal ICompletionBroker CompletionBroker {
            get {
                return _provider._CompletionBroker;
            }
        }

        internal IVsEditorAdaptersFactoryService AdaptersFactory {
            get {
                return _provider._adaptersFactory;
            }
        }

        internal ISignatureHelpBroker SignatureBroker {
            get {
                return _provider._SigBroker;
            }
        }

        #region Internal Implementation
       
        /// <summary>
        /// Attaches two events
        /// </summary>
        internal void Attach(IntellisensePreKeyProcessor preProcessor) {
#if FEATURE_INTELLISENSE
            preProcessor.PreprocessTextInput += OnPreprocessKeyDown;
#endif
            _preProcessor = preProcessor;
        }

        #endregion

        #region IOleCommandTarget Members

        // we need this because VS won't give us certain keyboard events as they're handled before our key processor.  These
        // include enter and tab both of which we want to complete.

        private void AttachKeyboardFilter() {
            if (_oldTarget == null) {
                ErrorHandler.ThrowOnFailure(AdaptersFactory.GetViewAdapter(_textView).AddCommandFilter(this, out _oldTarget));
            }
        }

        private void DetachKeyboardFilter() {
            ErrorHandler.ThrowOnFailure(AdaptersFactory.GetViewAdapter(_textView).RemoveCommandFilter(this));
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
#if FEATURE_INTELLISENSE

            if (_activeSession != null) {
                if (pguidCmdGroup == VSConstants.VSStd2K) {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                        case VSConstants.VSStd2KCmdID.RETURN:
                            if (IronPythonToolsPackage.Instance.OptionsPage.EnterCommitsIntellisense) {
                                _activeSession.Commit();
                                return VSConstants.S_OK;
                            } else {
                                _activeSession.Dismiss();
                            }
                        /*
                            // If the user has typed all of the characters as the completion and presses
                            // enter we should dismiss & let the text editor receive the enter.  For example 
                            // when typing "import sys[ENTER]" completion starts after the space.  After typing
                            // sys the user wants a new line and doesn't want to type enter twice.
                            if (EnterOnCompleteText()) {
                                int res = _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                                return res;
                            }*/
                            break;
                        case VSConstants.VSStd2KCmdID.TAB:
                            _activeSession.Commit();
                            return VSConstants.S_OK;
                    }
                }
            } else if (_sigHelpSession != null) {
                if (pguidCmdGroup == VSConstants.VSStd2K) {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                        case VSConstants.VSStd2KCmdID.BACKSPACE:
                            Backspace();
                            break;
                        case VSConstants.VSStd2KCmdID.HOME:
                        case VSConstants.VSStd2KCmdID.BOL:
                        case VSConstants.VSStd2KCmdID.BOL_EXT:
                        case VSConstants.VSStd2KCmdID.END:
                        case VSConstants.VSStd2KCmdID.WORDPREV:
                        case VSConstants.VSStd2KCmdID.WORDPREV_EXT:
                        case VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                            _sigHelpSession.Dismiss();
                            _sigHelpSession = null;
                            break;
                    }
                }
            }
#endif
            return _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }
        /*
        private bool EnterOnCompleteText() {            
            SnapshotPoint? point = _activeSession.GetTriggerPoint(_textView.TextBuffer.CurrentSnapshot);
            if (point.HasValue) {
                int chars = _textView.Caret.Position.BufferPosition.Position - point.Value.Position;
                var selectionStatus = _activeSession.SelectedCompletionSet.SelectionStatus;
                if (chars == selectionStatus.Completion.InsertionText.Length) {
                    string text = _textView.TextSnapshot.GetText(point.Value.Position, chars);
                    // TODO: If we match except for case we should commit & send enter.
                    if (text == selectionStatus.Completion.InsertionText) {
                        return true;
                    }
                }
            }

            return false;
        }*/

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            return _oldTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #endregion
    }
}

