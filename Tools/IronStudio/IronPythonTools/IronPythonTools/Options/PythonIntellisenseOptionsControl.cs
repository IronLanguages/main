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
using System.Windows.Forms;
using Microsoft.IronStudio.Core.Repl;

namespace Microsoft.IronPythonTools.Options {
    public partial class PythonIntellisenseOptionsControl : UserControl {        
        public PythonIntellisenseOptionsControl() {
            InitializeComponent();
            _enterCommits.Checked = IronPythonToolsPackage.Instance.IntellisenseOptionsPage.EnterCommitsIntellisense;
            _intersectMembers.Checked = IronPythonToolsPackage.Instance.IntellisenseOptionsPage.IntersectMembers;
            _completionCommitedBy.Text = IronPythonToolsPackage.Instance.IntellisenseOptionsPage.CompletionCommittedBy;
            _newLineAfterCompleteCompletion.Checked = IronPythonToolsPackage.Instance.IntellisenseOptionsPage.AddNewLineAtEndOfFullyTypedWord;
        }

        private void _enterCommits_CheckedChanged(object sender, EventArgs e) {
            IronPythonToolsPackage.Instance.IntellisenseOptionsPage.EnterCommitsIntellisense = _enterCommits.Checked;
        }

        private void _intersectMembers_CheckedChanged(object sender, EventArgs e) {
            IronPythonToolsPackage.Instance.IntellisenseOptionsPage.IntersectMembers = _intersectMembers.Checked;
        }

        private void _completionCommitedBy_TextChanged(object sender, EventArgs e) {
            IronPythonToolsPackage.Instance.IntellisenseOptionsPage.CompletionCommittedBy = _completionCommitedBy.Text;
        }

        private void _newLineAfterCompleteCompletion_CheckedChanged(object sender, EventArgs e) {
            IronPythonToolsPackage.Instance.IntellisenseOptionsPage.AddNewLineAtEndOfFullyTypedWord = _newLineAfterCompleteCompletion.Checked;
        }
    }
}
