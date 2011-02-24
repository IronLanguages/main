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
using Microsoft.IronPythonTools.Commands;
using Microsoft.IronStudio.Core.Repl;

namespace Microsoft.IronPythonTools.Options {
    class PythonOptionsPage : PythonDialogPage {
        private bool _enterOutliningMode, _smartHistory;
        private ReplIntellisenseMode _replIntellisenseMode;
        private PythonOptionsControl _window;
        private int _fillParagraphColumns;
        private string _interactiveOptions;

        public PythonOptionsPage()
            : base("Advanced") {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new PythonOptionsControl();
                }
                return _window;
            }
        }

        #region Editor Options

        public bool EnterOutliningModeOnOpen {
            get { return _enterOutliningMode; }
            set {
                _enterOutliningMode = value;

                IronPythonToolsPackage.Instance.RuntimeHost.EnterOutliningModeOnOpen = EnterOutliningModeOnOpen;
            }
        }

        #endregion

        #region Repl Options

        public bool ReplSmartHistory {
            get { return _smartHistory; }
            set {
                _smartHistory = value;
                
                // propagate changes
                var repl = ExecuteInReplCommand.TryGetReplWindow();
                if (repl != null) {
                    repl.UseSmartUpDown = ReplSmartHistory;
                }
            }
        }

        public ReplIntellisenseMode ReplIntellisenseMode {
            get { return _replIntellisenseMode; }
            set { _replIntellisenseMode  = value; }
        }

        public int FillParagraphColumns {
            get { return _fillParagraphColumns; }
            set { _fillParagraphColumns = value; }
        }

        public string InteractiveOptions {
            get { return _interactiveOptions; }
            set { _interactiveOptions = value; }
        }

        #endregion

        public override void ResetSettings() {
            _enterOutliningMode = true;
            _smartHistory = true;
            _replIntellisenseMode = ReplIntellisenseMode.DontEvaluateCalls;            
        }

        private const string EnterOutlingModeOnOpenSetting = "EnterOutlingModeOnOpen";
        private const string SmartHistorySetting = "InteractiveSmartHistory";
        private const string ReplIntellisenseModeSetting = "InteractiveIntellisenseMode";
        private const string FillParagraphColumnsSetting = "FillParagraphColumns";
        private const string InteractiveOptionsSetting = "InteractiveOptions";

        public override void LoadSettingsFromStorage() {
            _enterOutliningMode = LoadBool(EnterOutlingModeOnOpenSetting) ?? true;
            _smartHistory = LoadBool(SmartHistorySetting) ?? true;
            _replIntellisenseMode = LoadEnum<ReplIntellisenseMode>(ReplIntellisenseModeSetting) ?? ReplIntellisenseMode.DontEvaluateCalls;
            _fillParagraphColumns = LoadInt(FillParagraphColumnsSetting) ?? 80;
            _interactiveOptions = LoadString(InteractiveOptionsSetting) ?? String.Empty;
        }

        public override void SaveSettingsToStorage() {
            SaveBool(EnterOutlingModeOnOpenSetting, _enterOutliningMode);
            SaveBool(SmartHistorySetting, _smartHistory);
            SaveEnum<ReplIntellisenseMode>(ReplIntellisenseModeSetting, _replIntellisenseMode);
            SaveInt(FillParagraphColumnsSetting, _fillParagraphColumns);
            SaveString(InteractiveOptionsSetting, _interactiveOptions);
        }
    }
}
