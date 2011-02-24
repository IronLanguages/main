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
    public partial class PythonOptionsControl : UserControl {  
        
        public PythonOptionsControl() {
            InitializeComponent();
            _outliningOnOpen.Checked = IronPythonToolsPackage.Instance.OptionsPage.EnterOutliningModeOnOpen;
            _smartReplHistory.Checked = IronPythonToolsPackage.Instance.OptionsPage.ReplSmartHistory;
            _fillParagraphText.Text = IronPythonToolsPackage.Instance.OptionsPage.FillParagraphColumns.ToString();
            _interactiveOptionsValue.Text = IronPythonToolsPackage.Instance.OptionsPage.InteractiveOptions;

            switch (IronPythonToolsPackage.Instance.OptionsPage.ReplIntellisenseMode) {
                case ReplIntellisenseMode.AlwaysEvaluate: _evalAlways.Checked = true; break;
                case ReplIntellisenseMode.DontEvaluateCalls: _evalNoCalls.Checked = true; break;
                case ReplIntellisenseMode.NeverEvaluate: _evalNever.Checked = true; break;
            }

            const string optionsToolTip = "Options for the interactive window process.  For example: Frames=True;RecursionLimit=1001\r\n\r\nChanges take effect after interactive window reset.";
            _toolTips.SetToolTip(_interactiveOptionsValue, optionsToolTip);
            _toolTips.SetToolTip(_interactiveOptions, optionsToolTip);
        }

        private void _outliningOnOpen_CheckedChanged(object sender, EventArgs e) {
            IronPythonToolsPackage.Instance.OptionsPage.EnterOutliningModeOnOpen = _outliningOnOpen.Checked;
        }

        private void _smartReplHistory_CheckedChanged(object sender, EventArgs e) {
            IronPythonToolsPackage.Instance.OptionsPage.ReplSmartHistory = _smartReplHistory.Checked;
        }

        private void _evalNever_CheckedChanged(object sender, EventArgs e) {
            IronPythonToolsPackage.Instance.OptionsPage.ReplIntellisenseMode = ReplIntellisenseMode.NeverEvaluate;
        }

        private void _evalNoCalls_CheckedChanged(object sender, EventArgs e) {
            IronPythonToolsPackage.Instance.OptionsPage.ReplIntellisenseMode = ReplIntellisenseMode.DontEvaluateCalls;
        }

        private void _evalAlways_CheckedChanged(object sender, EventArgs e) {
            IronPythonToolsPackage.Instance.OptionsPage.ReplIntellisenseMode = ReplIntellisenseMode.AlwaysEvaluate;
        }

        private void _interactiveOptionsValue_TextChanged(object sender, EventArgs e) {
            IronPythonToolsPackage.Instance.OptionsPage.InteractiveOptions = _interactiveOptionsValue.Text;
        }

        private void _fillParagraphText_TextChanged(object sender, EventArgs e) {
            for (int i = 0; i < _fillParagraphText.Text.Length; i++) {
                if (!Char.IsDigit(_fillParagraphText.Text[i])) {
                    _fillParagraphText.Text = IronPythonToolsPackage.Instance.OptionsPage.FillParagraphColumns.ToString();
                    return;
                }
            }
            if (_fillParagraphText.Text.Length != 0) {
                IronPythonToolsPackage.Instance.OptionsPage.FillParagraphColumns = Convert.ToInt32(_fillParagraphText.Text);
            }
        }
    }
}
