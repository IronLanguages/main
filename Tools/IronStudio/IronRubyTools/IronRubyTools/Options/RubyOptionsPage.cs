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

using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.IronRubyTools.Commands;
using Microsoft.IronStudio.Core.Repl;

namespace Microsoft.IronRubyTools.Options {
    class RubyOptionsPage : DialogPage {
        private bool _noEnterCommitsIntellisense, _noEnterOutliningMode, _noAutoIndent, _noIntersectMembers, _noSmartHistory;
        private ReplIntellisenseMode _replIntellisenseMode;
        private RubyOptionsControl _window;

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new RubyOptionsControl();
                }
                return _window;
            }
        }

        #region Editor Options

        public bool EnterOutliningModeOnOpen {
            get { return !_noEnterOutliningMode; }
            set {
                _noEnterOutliningMode = !value;

                IronRubyToolsPackage.Instance.RuntimeHost.EnterOutliningModeOnOpen = EnterOutliningModeOnOpen;
            }
        }

        public bool AutoIndent { get { return !_noAutoIndent; } set { _noAutoIndent = !value; } }

        #endregion

        #region Repl Options

        public bool ReplSmartHistory {
            get { return !_noSmartHistory; }
            set {
                _noSmartHistory = !value;

                // propagate changes
                var repl = ExecuteInReplCommand.TryGetReplWindow();
                if (repl != null) {
                    repl.UseSmartUpDown = ReplSmartHistory;
                }
            }
        }

        public ReplIntellisenseMode ReplIntellisenseMode {
            get { return _replIntellisenseMode; }
            set { _replIntellisenseMode = value; }
        }

        #endregion

        #region Intellisense Options

        public bool EnterCommitsIntellisense {
            get { return !_noEnterCommitsIntellisense; }
            set { _noEnterCommitsIntellisense = !value; }
        }

        public bool IntersectMembers {
            get { return !_noIntersectMembers; }
            set {
                _noIntersectMembers = !value;
#if FEATURE_INTELLISENSE
                IronRubyToolsPackage.Instance.RuntimeHost.IntersectMembers = IntersectMembers;
#endif
            }
        }

        #endregion

        public override void ResetSettings() {
            _noEnterCommitsIntellisense = false;
            _noEnterOutliningMode = false;
            _noAutoIndent = false;
            _noIntersectMembers = false;
            _noSmartHistory = false;
            _replIntellisenseMode = ReplIntellisenseMode.DontEvaluateCalls;
        }
    }
}
