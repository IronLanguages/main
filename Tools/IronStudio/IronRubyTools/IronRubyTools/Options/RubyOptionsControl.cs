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
using System.Windows.Forms;
using Microsoft.IronStudio.Core.Repl;

namespace Microsoft.IronRubyTools.Options {
    public partial class RubyOptionsControl : UserControl {        
        public RubyOptionsControl() {
            InitializeComponent();
            _outliningOnOpen.Checked = IronRubyToolsPackage.Instance.OptionsPage.EnterOutliningModeOnOpen;
            _autoIndent.Checked = IronRubyToolsPackage.Instance.OptionsPage.AutoIndent;
            _enterCommits.Checked = IronRubyToolsPackage.Instance.OptionsPage.EnterCommitsIntellisense;
            _intersectMembers.Checked = IronRubyToolsPackage.Instance.OptionsPage.IntersectMembers;
            _smartReplHistory.Checked = IronRubyToolsPackage.Instance.OptionsPage.ReplSmartHistory;
            switch (IronRubyToolsPackage.Instance.OptionsPage.ReplIntellisenseMode) {
                case ReplIntellisenseMode.AlwaysEvaluate: _evalAlways.Checked = true; break;
                case ReplIntellisenseMode.DontEvaluateCalls: _evalNoCalls.Checked = true; break;
                case ReplIntellisenseMode.NeverEvaluate: _evalNever.Checked = true; break;
            }
        }

        private void _outliningOnOpen_CheckedChanged(object sender, EventArgs e) {
            IronRubyToolsPackage.Instance.OptionsPage.EnterOutliningModeOnOpen = _outliningOnOpen.Checked;
        }

        private void _autoIndent_CheckedChanged(object sender, EventArgs e) {
            IronRubyToolsPackage.Instance.OptionsPage.AutoIndent = _autoIndent.Checked;
        }

        private void _enterCommits_CheckedChanged(object sender, EventArgs e) {
            IronRubyToolsPackage.Instance.OptionsPage.EnterCommitsIntellisense = _enterCommits.Checked;
        }

        private void _intersectMembers_CheckedChanged(object sender, EventArgs e) {
            IronRubyToolsPackage.Instance.OptionsPage.IntersectMembers = _intersectMembers.Checked;
        }

        private void _smartReplHistory_CheckedChanged(object sender, EventArgs e) {
            IronRubyToolsPackage.Instance.OptionsPage.ReplSmartHistory = _smartReplHistory.Checked;
        }

        private void _evalNever_CheckedChanged(object sender, EventArgs e) {
            IronRubyToolsPackage.Instance.OptionsPage.ReplIntellisenseMode = ReplIntellisenseMode.NeverEvaluate;
        }

        private void _evalNoCalls_CheckedChanged(object sender, EventArgs e) {
            IronRubyToolsPackage.Instance.OptionsPage.ReplIntellisenseMode = ReplIntellisenseMode.DontEvaluateCalls;
        }

        private void _evalAlways_CheckedChanged(object sender, EventArgs e) {
            IronRubyToolsPackage.Instance.OptionsPage.ReplIntellisenseMode = ReplIntellisenseMode.AlwaysEvaluate;
        }
    }
}
