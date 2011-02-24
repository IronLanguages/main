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
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Microsoft.IronStudio.Library.Repl;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Scripting.Utils;

namespace Microsoft.IronStudio.Repl {
    using OleMenuCommandService = Microsoft.VisualStudio.Shell.OleMenuCommandService;
    using VSConstants = Microsoft.VisualStudio.VSConstants;
    using Microsoft.IronStudio.Core.Repl;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid(VsReplWindow.TypeGuid)]
    public class VsReplWindow : ToolWindowPane, IOleCommandTarget, IMixedBuffer, IReplWindow, IVsFindTarget {
        public const string TypeGuid = "5adb6033-611f-4d39-a193-57a717115c0f";

        // This is the user control hosted by the tool window; it is exposed to the base class 
        // using the Content property. Note that, even if this class implements IDispose, we are
        // not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
        // the object   returned by the Content property.
        private IVsEditorAdaptersFactoryService _adapterFactory;
        private Dictionary<VSConstants.VSStd2KCmdID, Action> _commands2k = new Dictionary<VSConstants.VSStd2KCmdID, Action>();
        private Dictionary<VSConstants.VSStd97CmdID, Action> _commands97 = new Dictionary<VSConstants.VSStd97CmdID, Action>();
        private IVsFindTarget _findTarget;
        private IVsTextView _view;
        private OleMenuCommandService _commandService;
        private readonly Guid _guid, _langSvcGuid;

        private ReplWindow _replWindow; // non-null until disposed
        private object _content;
        
        private static Guid DefaultFileType = new Guid(0x8239bec4, 0xee87, 0x11d0, 0x8c, 0x98, 0x0, 0xc0, 0x4f, 0xc2, 0xab, 0x22);

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public VsReplWindow(IComponentModel/*!*/ model, IReplEvaluator/*!*/ evaluator, IContentType/*!*/ contentType, string/*!*/ title, Guid languageServiceGuid, Guid replGuid, int? id) :
            base(null) {

            if (id != null) {
                IronStudioPackage.Instance.SaveReplInfo(id.Value, evaluator, contentType, title, languageServiceGuid, replGuid);
            }
            _replWindow = new VsReplWindowImpl(model, this, evaluator, contentType, title);

            _guid = replGuid;
            _langSvcGuid = languageServiceGuid;

            // Set the window title reading it from the resources.z
            Caption = _replWindow.Title;

            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;
        }

        #region IReplWindow

        /// <summary>
        /// Gets the ITextCaret in which the REPL window is executing.
        /// </summary>
        public ITextCaret Caret {
            get {
                return _replWindow.Caret;
            }
        }

        /// <summary>
        /// WPF Content of the Repl Window
        /// </summary>
        FrameworkElement IReplWindow.Content {
            get {
                return _replWindow.Content as FrameworkElement;
            }
        }

        /// <summary>
        /// Content type in the Repl Window
        /// </summary>
        public IContentType ContentType {
            get {
                return _replWindow.ContentType;
            }
        }

        /// <summary>
        /// Gets the IWpfTextView in which the REPL window is executing.
        /// </summary>
        public IWpfTextView CurrentView {
            get { 
                return _replWindow.CurrentView; 
            }
        }

        /// <summary>
        /// The language evaluator used in Repl Window
        /// </summary>
        public IReplEvaluator Evaluator {
            get {
                return _replWindow.Evaluator;
            }
        }

        public bool UseSmartUpDown {
            get {
                return _replWindow.UseSmartUpDown;
            }
            set {
                _replWindow.UseSmartUpDown = value;
            }
        }

        /// <summary>
        /// Gets or sets whether output from scripts should be echoed to the window.
        /// </summary>
        public bool ShowOutput {
            get {
                return _replWindow.ShowOutput;
            }
            set {
                _replWindow.ShowOutput = value;
            }
        }

        /// <summary>
        /// Title of the Repl Window
        /// </summary>
        public string Title {
            get {
                return _replWindow.Title;
            }
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public void ClearScreen() {
            _replWindow.ClearScreen();
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public void Focus() {
            _replWindow.Focus();
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public void PasteText(string text) {
            _replWindow.PasteText(text);
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public void Reset() {
            _replWindow.Reset();
        }

        public void AbortCommand() {
            _replWindow.AbortCommand();
        }

        /// <summary>
        /// See IReplWindow
        /// </summary>
        public void WriteLine(string text) {
            _replWindow.WriteLine(text);
        }

        #endregion

        #region IMixedBuffer

        /// <summary>
        /// See IMixedBuffer
        /// </summary>
        public SnapshotSpan[] GetLanguageSpans(ITextSnapshot snapshot) {
            return _replWindow.GetLanguageSpans(snapshot);
        }

        /// <summary>
        /// See IMixedBuffer
        /// </summary>
        public SnapshotSpan? GetLanguageSpanForLine(ITextSnapshot snapshot, int line) {
            return _replWindow.GetLanguageSpanForLine(snapshot, line);
        }

        #endregion

        #region Public Members

        public Guid Guid { get { return _guid; } }

        public Guid LanguageServiceGuid {
            get {
                return _langSvcGuid;
            }
        }

        #endregion

        /// <summary>
        /// A command filter which runs before the text view for all commands used for certain commands we need to intercept.
        /// </summary>
        class EarlyCommandFilter : IOleCommandTarget {
            internal IOleCommandTarget _nextTarget;
            private VsReplWindow _vsReplWindow;

            public EarlyCommandFilter(VsReplWindow vsReplWindow) {
                _vsReplWindow = vsReplWindow;
            }

            #region IOleCommandTarget Members

            public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
                if (pguidCmdGroup == VSConstants.VSStd2K) {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                        case VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU:
                            _vsReplWindow.ShowContextMenu();
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.TYPECHAR:
                            char typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
                            if (!_vsReplWindow._replWindow.CaretInInputRegion) {
                                _vsReplWindow.EditorOperations.MoveToEndOfDocument(false);
                            }
                            _vsReplWindow.EditorOperations.InsertText(typedChar.ToString());
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.PASTE:
                            break;
                    }
                } else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                    switch ((VSConstants.VSStd97CmdID)nCmdID) {
                        case VSConstants.VSStd97CmdID.Paste:
                            // move the cursor into a valid input region and then paste.
                            if (!_vsReplWindow._replWindow.CaretInInputRegion) {
                                _vsReplWindow.EditorOperations.MoveToEndOfDocument(false);
                            }

                            _vsReplWindow.EditorOperations.Paste();
                            return VSConstants.S_OK;
                    }
                }

                return _nextTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
                return _nextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }

            #endregion
        }

        #region IOleCommandTarget Members

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (pguidCmdGroup == GuidList.guidIronStudioCmdSet) {
                switch (nCmdID) {
                    case PkgCmdIDList.cmdidBreakRepl: BreakRepl(this, EventArgs.Empty); return VSConstants.S_OK;
                    case PkgCmdIDList.cmdidSmartExecute: SmartExecute(this, EventArgs.Empty); return VSConstants.S_OK;
                    case PkgCmdIDList.cmdidResetRepl: ResetRepl(this, EventArgs.Empty); return VSConstants.S_OK;
                    case PkgCmdIDList.cmdidReplHistoryNext: HistoryNext(this, EventArgs.Empty); return VSConstants.S_OK;
                    case PkgCmdIDList.cmdidReplHistoryPrevious: HistoryPrevious(this, EventArgs.Empty); return VSConstants.S_OK;
                    case PkgCmdIDList.cmdidReplClearScreen: _replWindow.ClearScreen(); return VSConstants.S_OK;
                    case PkgCmdIDList.cmdidBreakLine: _replWindow.BreakLine(); return VSConstants.S_OK;
                }
            } else if (pguidCmdGroup == VSConstants.VSStd2K) {
                switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                    case VSConstants.VSStd2KCmdID.RETURN:
                        int res = VSConstants.S_OK;
                        var position = _replWindow.CurrentView.Caret.Position.BufferPosition.GetContainingLine();
                        if (_commandService != null) {
                            res = ((IOleCommandTarget)_commandService).Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                        }
                        if (position.LineNumber < _replWindow.CurrentView.Caret.Position.BufferPosition.GetContainingLine().LineNumber) {
                            _replWindow.TryExecuteInput();
                            _replWindow.Caret.EnsureVisible();
                        }
                        return res;
                    default:
                        Action action;
                        if (_commands2k.TryGetValue((VSConstants.VSStd2KCmdID)nCmdID, out action)) {
                            action();
                            return VSConstants.S_OK;
                        }
                        break;
                }
            } else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                Action action;
                if (_commands97.TryGetValue((VSConstants.VSStd97CmdID)nCmdID, out action)) {
                    action();
                    return VSConstants.S_OK;
                }
            }
            if (_commandService != null) {
                return ((IOleCommandTarget)_commandService).Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            return VSConstants.S_OK;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            if (pguidCmdGroup == GuidList.guidIronStudioCmdSet) {
                switch (prgCmds[0].cmdID) {
                    case PkgCmdIDList.cmdidReplHistoryNext: 
                    case PkgCmdIDList.cmdidReplHistoryPrevious: 
                    case PkgCmdIDList.cmdidSmartExecute:
                    case PkgCmdIDList.cmdidBreakLine:
                        prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_DEFHIDEONCTXTMENU);
                        return VSConstants.S_OK;
                }
            }

            int hr = (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;

            if (_commandService != null) {
                hr = ((IOleCommandTarget)_commandService).QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }

            return hr;
        }

        #endregion

        #region VS Initialization

        /// <summary>
        /// Subclass so we can create the editor using the old APIs and get the new APIs from that.
        /// </summary>
        private sealed class VsReplWindowImpl : ReplWindow {
            private readonly VsReplWindow/*!*/ _window;

            public VsReplWindowImpl(IComponentModel/*!*/ model, VsReplWindow/*!*/ window, IReplEvaluator/*!*/ evaluator, IContentType/*!*/ contentType, string/*!*/ title)
                : base(model, evaluator, contentType, title) {
                Assert.NotNull(window);
                _window = window;
            }

            protected override IWpfTextViewHost/*!*/ CreateTextViewHost() {                
                var adapterFactory = ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
                var provider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_window.GetService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider));

                // create the buffer adapter and initialize it
                IVsTextBuffer bufferAdapter = adapterFactory.CreateVsTextBufferAdapter(provider, ContentType);
                int result = bufferAdapter.InitializeContent("", 0);
                bufferAdapter.SetLanguageServiceID(_window.LanguageServiceGuid);
                
                // ITextEditor is available only after InitializeContent was called:
                ITextBuffer textBuffer = adapterFactory.GetDataBuffer(bufferAdapter);

                // we need to set IReplProptProvider property before TextViewHost is instantiated so that ReplPromptTaggerProvider can bind to it 
                if (Evaluator.DisplayPromptInMargin) {
                    textBuffer.Properties.AddProperty(typeof(IReplPromptProvider), this);
                }
                
                // Create and inititalize text view adapter.
                // WARNING: This might trigger various services like IntelliSense, margins, taggers, etc.
                IVsTextView textViewAdapter = adapterFactory.CreateVsTextViewAdapter(provider, GetReplRoles());
                ((IVsWindowPane)textViewAdapter).SetSite(provider);
                
                // make us a code window so we'll have the same colors as a normal code window.
                IVsTextEditorPropertyContainer propContainer;
                ((IVsTextEditorPropertyCategoryContainer)textViewAdapter).GetPropertyCategory(Microsoft.VisualStudio.Editor.DefGuidList.guidEditPropCategoryViewMasterSettings, out propContainer);
                propContainer.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewComposite_AllCodeWindowDefaults, true);

                textViewAdapter.Initialize(
                    (IVsTextLines)bufferAdapter, 
                    IntPtr.Zero, 
                    (uint)TextViewInitFlags.VIF_HSCROLL | (uint)TextViewInitFlags.VIF_VSCROLL, 
                    new[] { new INITVIEW { fSelectionMargin = 0, fWidgetMargin = 0, fVirtualSpace = 0, fDragDropMove = 1 } }
                );

                // disable change tracking because everything will be changed
                var res = adapterFactory.GetWpfTextViewHost(textViewAdapter);
                res.TextView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, false);
                return res;
            }
        }

        protected override void OnCreate() {
            _replWindow.Initialize();

            _adapterFactory = _replWindow.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();

            var textView = _replWindow.CurrentView;

            SetDefaultFontSize(_replWindow.ComponentModel, textView);

            // create adapters so VS is happy...
            var serviceProvider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)GetService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider));

            _view = _adapterFactory.GetViewAdapter(textView);
            _findTarget = _view as IVsFindTarget;

            CurrentView.Closed += OnClose;

            _content = _replWindow.Content;
        }

        /// <summary>
        /// Sets the default font size to match that of a normal editor buffer.
        /// </summary>
        private void SetDefaultFontSize(IComponentModel model, IWpfTextView textView) {
            var formatMapSvc = model.GetService<IClassificationFormatMapService>();
            var fontsAndColorsSvc = model.GetService<IVsFontsAndColorsInformationService>();
            var fontCat = new VisualStudio.Editor.FontsAndColorsCategory(
                    _langSvcGuid,
                    Microsoft.VisualStudio.Editor.DefGuidList.guidTextEditorFontCategory,
                    Microsoft.VisualStudio.Editor.DefGuidList.guidTextEditorFontCategory
                    );

            var fontInfo = fontsAndColorsSvc.GetFontAndColorInformation(fontCat);
            var fontPrefs = fontInfo.GetFontAndColorPreferences();
            var font = System.Drawing.Font.FromHfont(fontPrefs.hRegularViewFont);

            var classMap = formatMapSvc.GetClassificationFormatMap(textView);
            var defaultProps = classMap.DefaultTextProperties;
            defaultProps = defaultProps.SetFontRenderingEmSize(font.Size);
            classMap.DefaultTextProperties = defaultProps;
        }

        public override void OnToolWindowCreated() {
            Guid commandUiGuid = Microsoft.VisualStudio.VSConstants.GUID_TextEditorFactory;
            ((IVsWindowFrame)Frame).SetGuidProperty((int)__VSFPROPID.VSFPROPID_InheritKeyBindings, ref commandUiGuid);

            var earlyFilter = new EarlyCommandFilter(this);
            ErrorHandler.ThrowOnFailure(_view.AddCommandFilter(earlyFilter, out earlyFilter._nextTarget));
            _commandService = new OleMenuCommandService(this, (IOleCommandTarget)_view);

            AddCommand(VSConstants.VSStd2KCmdID.CANCEL, () => _replWindow.Cancel());

            AddCommand(VSConstants.VSStd2KCmdID.UP, _replWindow.SmartUpArrow);
            AddCommand(VSConstants.VSStd2KCmdID.DOWN, _replWindow.SmartDownArrow);
            AddCommand(VSConstants.VSStd2KCmdID.BOL, () => _replWindow.Home(false));
            AddCommand(VSConstants.VSStd2KCmdID.BOL_EXT, () => _replWindow.Home(true));

            AddCommand(VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU, ShowContextMenu);

            var id = new CommandID(GuidList.guidIronStudioCmdSet, (int)PkgCmdIDList.cmdidBreakRepl);
            var cmd = new OleMenuCommand(BreakRepl, id);
            _commandService.AddCommand(cmd);

            id = new CommandID(GuidList.guidIronStudioCmdSet, (int)PkgCmdIDList.cmdidResetRepl);
            cmd = new OleMenuCommand(ResetRepl, id);
            _commandService.AddCommand(cmd);

            id = new CommandID(GuidList.guidIronStudioCmdSet, (int)PkgCmdIDList.cmdidSmartExecute);
            cmd = new OleMenuCommand(SmartExecute, id);
            _commandService.AddCommand(cmd);


            id = new CommandID(GuidList.guidIronStudioCmdSet, (int)PkgCmdIDList.cmdidReplHistoryNext);
            cmd = new OleMenuCommand(SmartExecute, id);
            _commandService.AddCommand(cmd);

            id = new CommandID(GuidList.guidIronStudioCmdSet, (int)PkgCmdIDList.cmdidBreakLine);
            cmd = new OleMenuCommand(SmartExecute, id);
            _commandService.AddCommand(cmd);


            id = new CommandID(GuidList.guidIronStudioCmdSet, (int)PkgCmdIDList.cmdidReplHistoryPrevious);
            cmd = new OleMenuCommand(SmartExecute, id);
            _commandService.AddCommand(cmd);

            base.OnToolWindowCreated();
        }

        public void HistoryNext(object sender, EventArgs e) {
            _replWindow.HistoryNext();
        }

        public void HistoryPrevious(object sender, EventArgs e) {
            _replWindow.HistoryPrevious();
        }

        public void ShowContextMenu() {
            var uishell = (IVsUIShell)GetService(typeof(SVsUIShell));
            if (uishell != null) {
                var pt = System.Windows.Forms.Cursor.Position;
                var pnts = new[] { new POINTS { x = (short)pt.X, y = (short)pt.Y } };
                var guid = GuidList.guidIronStudioCmdSet;
                int hr = uishell.ShowContextMenu(
                    0,
                    ref guid,
                    0x2100,
                    pnts,
                    _replWindow.CurrentView as IOleCommandTarget);

                ErrorHandler.ThrowOnFailure(hr);
            }
        }

        public void BreakRepl(object sender, EventArgs args) {
            _replWindow.AbortCommand();
        }

        public void ResetRepl(object sender, EventArgs args) {
            _replWindow.Reset();
        }

        public void SmartExecute(object sender, EventArgs args) {
            _replWindow.ExecuteOrPasteSelected();
        }

        private void AddCommand(VSConstants.VSStd2KCmdID command, Action action) {
            var id = new CommandID(typeof(VSConstants.VSStd2KCmdID).GUID, (int)command);
            var cmd = new OleMenuCommand(OnReturn, id);
            _commandService.AddCommand(cmd);
            _commands2k[command] = action;
        }

        private void AddCommand(VSConstants.VSStd97CmdID command, Action action) {
            var id = new CommandID(typeof(VSConstants.VSStd97CmdID).GUID, (int)command);
            var cmd = new OleMenuCommand(OnReturn, id);
            _commandService.AddCommand(cmd);
            _commands97[command] = action;
        }

        private void OnReturn(object sender, EventArgs args) {
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// This property returns the control that should be hosted in the Tool Window.
        /// It can be either a FrameworkElement (for easy creation of toolwindows hosting WPF content), 
        /// or it can be an object implementing one of the IVsUIWPFElement or IVsUIWin32Element interfaces.
        /// </summary>
        public override object Content {
            get {
                Debug.Assert(_content != null);
                return _content;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _content = value;
            }
        }

        /// <summary>
        /// Return the service of the given type.
        /// This override is needed to be able to use a different command service from the one
        /// implemented in the base class.
        /// </summary>
        protected override object GetService(Type serviceType) {
            if ((typeof(IOleCommandTarget) == serviceType) ||
                (typeof(IMenuCommandService) == serviceType)) {
                if (null != _commandService) {
                    return _commandService;
                }
            }
            return base.GetService(serviceType);
        }

        #endregion

        #region Private Methods

        private IEditorOperations EditorOperations {
            get {
                return _replWindow.EditorOperations;
            }
        }

        private void OnClose(object sender, EventArgs ea) {
            _replWindow.Dispose();
            _replWindow = null;

            try {
                //File.Delete(FilePath);
            } catch (Exception e) {
                // TODO: Log error
                Debug.WriteLine("Error while deleting temporary REPL file: {0}", e.ToString());
            }
        }
        
        #endregion

        public int Find(string pszSearch, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out uint pResult) {
            if (_findTarget != null) {
                return _findTarget.Find(pszSearch, grfOptions, fResetStartPoint, pHelper, out pResult);
            }
            pResult = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCapabilities(bool[] pfImage, uint[] pgrfOptions) {
            if (_findTarget != null) {
                return _findTarget.GetCapabilities(pfImage, pgrfOptions);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int GetCurrentSpan(TextSpan[] pts) {
            if (_findTarget != null) {
                return _findTarget.GetCurrentSpan(pts);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int GetFindState(out object ppunk) {
            if (_findTarget != null) {
                return _findTarget.GetFindState(out ppunk);
            }
            ppunk = null;
            return VSConstants.E_NOTIMPL;

        }

        public int GetMatchRect(RECT[] prc) {
            if (_findTarget != null) {
                return _findTarget.GetMatchRect(prc);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int GetProperty(uint propid, out object pvar) {
            if (_findTarget != null) {
                return _findTarget.GetProperty(propid, out pvar);
            }
            pvar = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetSearchImage(uint grfOptions, IVsTextSpanSet[] ppSpans, out IVsTextImage ppTextImage) {
            if (_findTarget != null) {
                return _findTarget.GetSearchImage(grfOptions, ppSpans, out ppTextImage);
            }
            ppTextImage = null;
            return VSConstants.E_NOTIMPL;
        }

        public int MarkSpan(TextSpan[] pts) {
            if (_findTarget != null) {
                return _findTarget.MarkSpan(pts);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int NavigateTo(TextSpan[] pts) {
            if (_findTarget != null) {
                return _findTarget.NavigateTo(pts);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int NotifyFindTarget(uint notification) {
            if (_findTarget != null) {
                return _findTarget.NotifyFindTarget(notification);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int Replace(string pszSearch, string pszReplace, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out int pfReplaced) {
            if (_findTarget != null) {
                return _findTarget.Replace(pszSearch, pszReplace, grfOptions, fResetStartPoint, pHelper, out pfReplaced);
            }
            pfReplaced = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int SetFindState(object pUnk) {
            if (_findTarget != null) {
                return _findTarget.SetFindState(pUnk);
            }
            return VSConstants.E_NOTIMPL;
        }

        internal void Cancel() {
            _replWindow.Cancel();
        }
    }
}
