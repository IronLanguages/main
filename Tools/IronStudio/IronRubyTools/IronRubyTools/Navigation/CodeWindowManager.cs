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

using System.Collections.Generic;
using Microsoft.IronRubyTools.Intellisense;
using Microsoft.IronRubyTools.Language;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.IronRubyTools.Navigation {
    class CodeWindowManager : IVsCodeWindowManager {
        private readonly IVsCodeWindow _window;
        private readonly IWpfTextView _textView;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private static readonly Dictionary<IWpfTextView, CodeWindowManager> _windows = new Dictionary<IWpfTextView, CodeWindowManager>();
        private DropDownBarClient _client;

        public CodeWindowManager(IVsCodeWindow codeWindow, IWpfTextView textView, IComponentModel componentModel) {
            _window = codeWindow;
            _textView = textView;
            _editorOperationsFactory = componentModel.GetService<IEditorOperationsFactoryService>();
            _textView.Properties.AddProperty(typeof(CodeWindowManager), this);
        }

        #region IVsCodeWindowManager Members

        public int AddAdornments() {
            IVsTextView textView;
            _windows[_textView] = this;

            if (ErrorHandler.Succeeded(_window.GetPrimaryView(out textView))) {
                OnNewView(textView);
            }

            if (ErrorHandler.Succeeded(_window.GetSecondaryView(out textView))) {
                OnNewView(textView);
            }

            //if (IronPythonToolsPackage.Instance.LangPrefs.NavigationBar) {
                return AddDropDownBar();
            //}

            //return VSConstants.S_OK;
        }

        private int AddDropDownBar() {
            DropDownBarClient dropDown = _client = new DropDownBarClient(
                _textView,
                AnalysisItem.GetAnalysis(_textView.TextBuffer)
            );

            IVsDropdownBarManager manager = (IVsDropdownBarManager)_window;

            IVsDropdownBar dropDownBar;
            int hr = manager.GetDropdownBar(out dropDownBar);
            if (ErrorHandler.Succeeded(hr) && dropDownBar != null) {
                hr = manager.RemoveDropdownBar();
                if (!ErrorHandler.Succeeded(hr)) {
                    return hr;
                }
            }
            hr = manager.AddDropdownBar(2, dropDown);

            return hr;
        }

        private int RemoveDropDownBar() {
            if (_client != null) {
                IVsDropdownBarManager manager = (IVsDropdownBarManager)_window;
                _client.Unregister();
                _client = null;
                return manager.RemoveDropdownBar();
            }
            return VSConstants.S_OK;
        }

        public int OnNewView(IVsTextView pView) {
            // TODO: We pass _textView which may not be right for split buffers, we need
            // to test the case where we split a text file and save it as an existing file?
            new EditFilter(_textView, pView);
            return VSConstants.S_OK;
        }

        public int RemoveAdornments() {
            _windows.Remove(_textView);
            return RemoveDropDownBar();
        }

        public static void ToggleNavigationBar(bool fEnable) {
            foreach (var keyValue in _windows) {
                if (fEnable) {
                    ErrorHandler.ThrowOnFailure(keyValue.Value.AddDropDownBar());
                } else {
                    ErrorHandler.ThrowOnFailure(keyValue.Value.RemoveDropDownBar());
                }
            }
        }

        #endregion
    }

}
