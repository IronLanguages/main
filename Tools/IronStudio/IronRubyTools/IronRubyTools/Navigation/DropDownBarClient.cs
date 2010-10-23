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
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;
using IronRuby.Compiler.Ast;
using Microsoft.Scripting;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.IronRubyTools.Intellisense;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.IronRubyTools.Navigation {
    /// <summary>
    /// Implements the navigation bar which appears above a source file in the editor.
    /// 
    /// The navigation bar consists of two drop-down boxes.  On the left hand side is a list
    /// of top level constructs.  On the right hand side are list of nested constructs for the
    /// currently selected top-level construct.
    /// 
    /// When the user moves the caret the current selections are automatically updated.  If the
    /// user is inside of a top level construct but not inside any of the available nested 
    /// constructs then the first element of the nested construct list is selected and displayed
    /// grayed out.  If the user is inside of no top level constructs then the 1st top-level
    /// construct is selected and displayed as grayed out.  It's first top-level construct is
    /// also displayed as being grayed out.
    /// 
    /// The most difficult part of this is handling the transitions from one state to another.
    /// We need to change the current selections due to events from two sources:  The first is selections
    /// in the drop down and the 2nd is the user navigating within the source code.  When a change
    /// occurs we may need to update the left hand side (along w/ a corresponding update to the right
    /// hand side) or we may need to update the right hand side.  If we are transitioning from
    /// being outside of a known element to being in a known element we also need to refresh 
    /// the drop down to remove grayed out elements.
    /// </summary>
    class DropDownBarClient : IVsDropdownBarClient {
        private readonly AnalysisItem _classifier;                      // classifier which is used for providing entries
        private readonly Dispatcher _dispatcher;                        // current dispatcher so we can get back to our thread
        private readonly IWpfTextView _textView;                        // text view we're drop downs for
        private Model _entries;
        private IVsDropdownBar _dropDownBar;                            // drop down bar - used to refresh when changes occur
        
        private static readonly ImageList _imageList = GetImageList();
        
        private const int ModuleComboBoxId = 0;
        private const int MethodComboBoxId = 1;

        public DropDownBarClient(IWpfTextView textView, AnalysisItem classifier) {
            Assert.NotNull(textView, classifier);

            _classifier = classifier;
            _classifier.OnNewParseTree += ParserOnNewParseTree;
            _textView = textView;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _textView.Caret.PositionChanged += CaretPositionChanged;
            BuildModel();
        }
        
        internal void Unregister() {
            _classifier.OnNewParseTree -= ParserOnNewParseTree;
            _textView.Caret.PositionChanged -= CaretPositionChanged;
        }

        #region Synchronization

        /// <summary>
        /// Calculates the members of the drop down for top-level members.
        /// </summary>
        private void BuildModel() {
            var tree = _classifier.CurrentTree;
            if (tree != null) {
                _entries = new ModelBuilder(tree).Build();
            }
        }

        /// <summary>
        /// Wired to parser event for when the parser has completed parsing a new tree and we need
        /// to update the navigation bar with the new data.
        /// </summary>
        private void ParserOnNewParseTree(object sender, EventArgs e) {
            if (_dropDownBar != null) {
                Action callback = () => {
                    BuildModel();
                    int position = _textView.Caret.Position.BufferPosition.Position;

                    ModuleEntry newModule = _entries.LocateModule(position);
                    MethodEntry newMethod = newModule.LocateMethod(position);
                    _dropDownBar.RefreshCombo(ModuleComboBoxId, newModule.Index);
                    _dropDownBar.RefreshCombo(MethodComboBoxId, newMethod != null ? newMethod.Index : -1);
                };

                _dispatcher.BeginInvoke(callback, DispatcherPriority.Background);
            }
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            ActivateEntries(e.NewPosition.BufferPosition.Position);
        }

        private void ActivateEntries(int position) {
            if (_entries == null) {
                return;
            }

            int currentModuleIndex;
            _dropDownBar.GetCurrentSelection(ModuleComboBoxId, out currentModuleIndex);

            ModuleEntry newModule = _entries.LocateModule(position);
            Debug.Assert(newModule != null);

            MethodEntry newMethod = newModule.LocateMethod(position);
            int newMethodIndex = newMethod != null ? newMethod.Index : -1;

            if (newModule.Index != currentModuleIndex) {
                _dropDownBar.SetCurrentSelection(ModuleComboBoxId, newModule.Index);
                _dropDownBar.RefreshCombo(MethodComboBoxId, newMethodIndex);
            } else {
                int currentMethodIndex;
                _dropDownBar.GetCurrentSelection(MethodComboBoxId, out currentMethodIndex);
                if (newMethodIndex != currentMethodIndex) {
                    _dropDownBar.SetCurrentSelection(MethodComboBoxId, newMethodIndex);
                }
            }
        }

        #endregion

        #region IVsDropdownBarClient Members

        /// <summary>
        /// Gets the attributes for the specified combo box.  We return the number of elements that we will
        /// display, the various attributes that VS should query for next (text, image, and attributes of
        /// the text such as being grayed out), along with the appropriate image list.
        /// 
        /// We always return the # of entries based off our entries list, the exact same image list, and
        /// we have VS query for text, image, and text attributes all the time.
        /// </summary>
        public int GetComboAttributes(int iCombo, out uint pcEntries, out uint puEntryType, out IntPtr phImageList) {
            uint count = 0;

            if (_entries != null) {
                switch (iCombo) {
                    case ModuleComboBoxId:
                        count = (uint)_entries.ModuleCount;
                        break;

                    case MethodComboBoxId:
                        int currentModule;
                        _dropDownBar.GetCurrentSelection(ModuleComboBoxId, out currentModule);
                        count = (uint)_entries.GetMethodCount(currentModule);
                        break;
                }
            }

            pcEntries = count;
            puEntryType = (uint)(DROPDOWNENTRYTYPE.ENTRY_TEXT | DROPDOWNENTRYTYPE.ENTRY_IMAGE | DROPDOWNENTRYTYPE.ENTRY_ATTR);
            phImageList = _imageList.Handle;
            return VSConstants.S_OK;
        }
        
        public int GetComboTipText(int iCombo, out string pbstrText) {
            pbstrText = null;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the entry attributes for the given combo box and index.
        /// 
        /// We always use plain text unless we are not inside of a valid entry
        /// for the given combo box.  In that case we ensure the 1st item
        /// is selected and we gray out the 1st entry.
        /// </summary>
        public int GetEntryAttributes(int iCombo, int iIndex, out uint pAttr) {
            pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_PLAIN;
            
            if (iIndex == 0) {
                switch (iCombo) {
                    case ModuleComboBoxId:
                        //if (_currentModuleIndex == -1) {
                        //    pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_GRAY;
                        //}
                        break;

                    case MethodComboBoxId:
                        //if (_currentMethodIndex == -1) {
                        //    pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_GRAY;
                        //}
                        break;
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the image which is associated with the given index for the
        /// given combo box.
        /// </summary>
        public int GetEntryImage(int iCombo, int iIndex, out int piImageIndex) {
            piImageIndex = 0;
            Debug.Assert(_entries != null);

            switch (iCombo) {
                case ModuleComboBoxId:
                    piImageIndex = _entries.GetModuleImageIndex(iIndex);
                    break;

                case MethodComboBoxId:
                    int currentModule;
                    _dropDownBar.GetCurrentSelection(ModuleComboBoxId, out currentModule);
                    piImageIndex = _entries.GetMethodImageIndex(currentModule, iIndex);
                    break;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the text which is displayed for the given index for the
        /// given combo box.
        /// </summary>
        public int GetEntryText(int iCombo, int iIndex, out string ppszText) {
            ppszText = String.Empty;
            switch (iCombo) {
                case ModuleComboBoxId:
                    ppszText = _entries.GetModuleName(iIndex);
                    break;

                case MethodComboBoxId:
                    int currentModule;
                    _dropDownBar.GetCurrentSelection(ModuleComboBoxId, out currentModule);
                    ppszText = _entries.GetMethodName(currentModule, iIndex);
                    break;
            }

            return VSConstants.S_OK;
        }

        public int OnComboGetFocus(int iCombo) {
            return VSConstants.S_OK;
        }

        public int OnItemChosen(int iCombo, int iIndex) {
            int position;
            switch (iCombo) {
                case ModuleComboBoxId:
                    position = _entries.GetModuleStart(iIndex);
                    if (position != -1) {
                        _dropDownBar.RefreshCombo(MethodComboBoxId, -1);
                        CenterAndFocus(position);
                    }
                    break;

                case MethodComboBoxId:
                    int currentModule;
                    _dropDownBar.GetCurrentSelection(ModuleComboBoxId, out currentModule);
                    position = _entries.GetMethodStart(currentModule, iIndex);
                    if (position != -1) {
                        CenterAndFocus(position);
                    }
                    break;
            }
            return VSConstants.S_OK;
        }
        
        public int OnItemSelected(int iCombo, int iIndex) {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by VS to provide us with the drop down bar.  We can call back
        /// on the drop down bar to force VS to refresh the combo box or change
        /// the current selection.
        /// </summary>
        public int SetDropdownBar(IVsDropdownBar pDropdownBar) {
            _dropDownBar = pDropdownBar;
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation Details

        /// <summary>
        /// Reads our image list from our DLLs resource stream.
        /// </summary>
        private static ImageList GetImageList() {
            ImageList list = new ImageList();
            list.ImageSize = new Size(0x10, 0x10);
            list.TransparentColor = Color.FromArgb(0xff, 0, 0xff);
            Stream manifestResourceStream = typeof(Microsoft.IronStudio.Repl.VsReplWindow).Assembly.GetManifestResourceStream("Microsoft.Resources.completionset.bmp");
            list.Images.AddStrip(new Bitmap(manifestResourceStream));
            return list;
        }

        /// <summary>
        /// Moves the caret to the specified index in the current snapshot.  Then updates the view port
        /// so that caret will be centered.  Finally moves focus to the text view so the user can 
        /// continue typing.
        /// </summary>
        private void CenterAndFocus(int index) {
            _textView.Caret.MoveTo(new SnapshotPoint(_textView.TextBuffer.CurrentSnapshot, index));

            _textView.ViewScroller.EnsureSpanVisible(
                new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, index, 1),
                EnsureSpanVisibleOptions.AlwaysCenter
            );

            ((System.Windows.Controls.Control)_textView).Focus();
        }

        #endregion
    }
}
