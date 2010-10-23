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

using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.IronPythonTools.Commands;
using Microsoft.IronStudio.Core.Repl;

namespace Microsoft.IronPythonTools.Options {
    class PythonIntellisenseOptionsPage : PythonDialogPage {
        private bool _enterCommitsIntellisense, _intersectMembers, _addNewLineAtEndOfFullyTypedWord;
        private PythonIntellisenseOptionsControl _window;
        private string _completionCommittedBy;
        private const string _defaultCompletionChars = "{}[]().,:;+-*/%&|^~=<>#'\"\\";

        public PythonIntellisenseOptionsPage()
            : base("Intellisense") {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new PythonIntellisenseOptionsControl();
                }
                return _window;
            }
        }

        #region Intellisense Options

        public bool EnterCommitsIntellisense {
            get { return _enterCommitsIntellisense; }
            set { _enterCommitsIntellisense = value; }
        }

        public bool IntersectMembers {
            get { return _intersectMembers; }
            set {
                _intersectMembers = value;

                IronPythonToolsPackage.Instance.RuntimeHost.IntersectMembers = IntersectMembers;
            }
        }

        public bool AddNewLineAtEndOfFullyTypedWord {
            get { return _addNewLineAtEndOfFullyTypedWord; }
            set { _addNewLineAtEndOfFullyTypedWord = value; }
        }

        public string CompletionCommittedBy { 
            get { return _completionCommittedBy; } 
            set { _completionCommittedBy = value; } 
        }

        #endregion

        public override void ResetSettings() {
            _enterCommitsIntellisense = true;
            _intersectMembers = true;
            _addNewLineAtEndOfFullyTypedWord = false;
            _completionCommittedBy = _defaultCompletionChars;  
        }

        private const string EnterCommitsSetting = "EnterCommits";
        private const string IntersectMembersSetting = "IntersectMembers";
        private const string NewLineAtEndOfWordSetting = "NewLineAtEndOfWord";
        private const string CompletionCommittedBySetting = "CompletionCommittedBy";

        public override void LoadSettingsFromStorage() {
            _enterCommitsIntellisense = LoadBool(EnterCommitsSetting) ?? true;
            _intersectMembers = LoadBool(IntersectMembersSetting) ?? true;
            _addNewLineAtEndOfFullyTypedWord = LoadBool(NewLineAtEndOfWordSetting) ?? false;
            _completionCommittedBy = LoadString("CompletionCommittedBy") ?? _defaultCompletionChars;
        }

        public override void SaveSettingsToStorage() {
            SaveBool(EnterCommitsSetting, _enterCommitsIntellisense);
            SaveBool(IntersectMembersSetting, _intersectMembers);
            SaveBool(NewLineAtEndOfWordSetting, _addNewLineAtEndOfFullyTypedWord);
            SaveString(CompletionCommittedBySetting, _defaultCompletionChars);
        }
    }
}
