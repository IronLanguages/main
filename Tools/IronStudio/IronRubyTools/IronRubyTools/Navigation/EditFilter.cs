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
using System.Windows;
using Microsoft.IronRubyTools.Commands;
using Microsoft.IronRubyTools.Editor.Core;
using Microsoft.IronRubyTools.Navigation;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Core.Repl;
using Microsoft.IronStudio.Navigation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.IronRubyTools.Intellisense;
using IronRuby.Compiler;

namespace Microsoft.IronRubyTools.Language {
    /// <summary>
    /// IOleCommandTarget implementation for interacting with various editor commands.  This enables
    /// wiring up most of our features to the VisualStudio editor.  We currently support:
    ///     Goto Definition
    ///     Find All References
    ///     Show Member List
    ///     Complete Word
    ///     Enable/Disable Outlining
    ///     Comment/Uncomment block
    /// 
    /// We also support IronRuby specific commands via this class.  Currently these commands are
    /// added by updating our CommandTable class to contain a new command.  These commands also need
    /// to be registered in our .vsct file so that VS knows about them.
    /// </summary>
    class EditFilter : IOleCommandTarget {
        private readonly IWpfTextView _textView;
        private readonly IOleCommandTarget _next;

        public EditFilter(IWpfTextView textView, IVsTextView vsTextView) {
            _textView = textView;
            ErrorHandler.ThrowOnFailure(vsTextView.AddCommandFilter(this, out _next));
        }

        /// <summary>
        /// Implements Goto Definition.  Called when the user selects Goto Definition from the 
        /// context menu or hits the hotkey associated with Goto Definition.
        /// 
        /// If there is 1 and only one definition immediately navigates to it.  If there are
        /// no references displays a dialog box to the user.  Otherwise it opens the find
        /// symbols dialog with the list of results.
        /// </summary>
        private int GotoDefinition() {
#if FEATURE_INTELLISENSE
            UpdateStatusForIncompleteAnalysis();

            var analysis = GetExpressionAnalysis();

            StandardGlyphGroup type;
            List<LocationInfo> locations = GetDefinitions(analysis, out type);

            if (locations != null && locations.Count == 1) {
                var location = locations[0];

                location.GotoSource();
            } else if (locations == null || locations.Count == 0) {
                MessageBox.Show(String.Format("Cannot go to definition \"{0}\"", analysis.Expression));
            } else {
                ShowFindSymbolsDialog(analysis, new SymbolList(analysis.Expression, locations, type));
            }

            return VSConstants.S_OK;
#endif
            return VSConstants.S_FALSE;
        }

#if FEATURE_INTELLISENSE
        private static List<LocationInfo> GetDefinitions(ExpressionAnalysis provider, out StandardGlyphGroup type) {
            var vars = provider.Variables;

            ObjectType? finalType = null;

            List<LocationInfo> locations = new List<LocationInfo>();
            foreach (VariableResult result in vars) {
                if (finalType == null) {
                    finalType = result.Type;
                } else if (finalType != result.Type) {
                    finalType = ObjectType.Multiple;
                }

                if (result.Location != null) {
                    locations.Add(result.Location);
                }
            }

            if (finalType != null) {
                type = finalType.Value.ToGlyphGroup();
            } else {
                type = StandardGlyphGroup.GlyphGroupClass;
            }

            return locations;
        }
#endif

        /// <summary>
        /// Implements Find All References.  Called when the user selects Find All References from
        /// the context menu or hits the hotkey associated with find all references.
        /// 
        /// Always opens the Find Symbol Results box to display the results.
        /// </summary>
        private int FindAllReferences() {
#if FEATURE_INTELLISENSE
            UpdateStatusForIncompleteAnalysis();

            var provider = GetExpressionAnalysis();

            StandardGlyphGroup type;
            List<LocationInfo> definitions = GetDefinitions(provider, out type);

            IEnumerable<VariableResult> vars = provider.Variables;

            var references = new List<LocationInfo>();
            foreach (var v in vars) {
                if (v.References != null) {
                    references.AddRange(v.References);
                }
            }

            ShowFindSymbolsDialog(provider, new SymbolList(provider.Expression, definitions, references, type));

            return VSConstants.S_OK;
#endif
            return VSConstants.S_FALSE;
        }

#if FEATURE_INTELLISENSE
        private static bool IsIdentifierChar(char curChar) {
            return Tokenizer.IsIdentifier(curChar, 0x07f);
        }

        private void UpdateStatusForIncompleteAnalysis() {
            var statusBar = (IVsStatusbar)CommonPackage.GetGlobalService(typeof(SVsStatusbar));
            if (!IronRubyToolsPackage.Instance.Analyzer.IsAnalyzing) {
                statusBar.SetText("Ruby source analysis is not up to date");
            }
        }
      
        /// <summary>
        /// Opens the find symbols dialog with a list of results.  This is done by requesting
        /// that VS does a search against our library GUID.  Our library then responds to
        /// that request by extracting the prvoided symbol list out and using that for the
        /// search results.
        /// </summary>
        private static void ShowFindSymbolsDialog(ExpressionAnalysis provider, SymbolList symbols) {
            // ensure our library is loaded so find all references will go to our library
            Package.GetGlobalService(typeof(IRubyLibraryManager));

            var findSym = (IVsFindSymbol)IronRubyToolsPackage.GetGlobalService(typeof(SVsObjectSearch));
            VSOBSEARCHCRITERIA2 searchCriteria = new VSOBSEARCHCRITERIA2();
            searchCriteria.eSrchType = VSOBSEARCHTYPE.SO_ENTIREWORD;
            searchCriteria.pIVsNavInfo = symbols;
            searchCriteria.grfOptions = (uint)_VSOBSEARCHOPTIONS2.VSOBSO_LISTREFERENCES;
            searchCriteria.szName = provider.Expression;

            Guid guid = Guid.Empty;
            ErrorHandler.ThrowOnFailure(findSym.DoSearch(new Guid(CommonConstants.LibraryGuid), new VSOBSEARCHCRITERIA2[] { searchCriteria }));
        }

        private ExpressionAnalysis GetExpressionAnalysis() {
            var textView = _textView;
            var textBuffer = _textView.TextBuffer;
            var snapshot = textBuffer.CurrentSnapshot;
            int caretPos = _textView.Caret.Position.BufferPosition.Position;

            // foo(
            //    ^
            //    +---  Caret here
            //
            // We want to lookup foo, not foo(
            //
            if (caretPos != snapshot.Length) {
                string curChar = snapshot.GetText(caretPos, 1);
                if (!IsIdentifierChar(curChar[0]) && caretPos > 0) {
                    string prevChar = snapshot.GetText(caretPos - 1, 1);
                    if (IsIdentifierChar(prevChar[0])) {
                        caretPos--;
                    }
                }
            }

            var span = snapshot.CreateTrackingSpan(
                caretPos,
                0,
                SpanTrackingMode.EdgeInclusive
            );

            return Analysis.AnalyzeExpression(snapshot, textBuffer, span);
        }
#endif

        class SimpleLocationInfo : SimpleObject, IVsNavInfoNode {
            private readonly LocationInfo _locationInfo;
            private readonly StandardGlyphGroup _glyphType;
            private readonly string _pathText, _lineText;

            public SimpleLocationInfo(string searchText, LocationInfo locInfo, StandardGlyphGroup glyphType) {
                _locationInfo = locInfo;
                _glyphType = glyphType;
                _pathText = GetSearchDisplayText();
                _lineText = _locationInfo.Cookie.GetLine(_locationInfo.Line);
            }

            public override string Name {
                get {
                    return _locationInfo.FilePath;
                }
            }

            public override string GetTextRepresentation(VSTREETEXTOPTIONS options) {
                if (options == VSTREETEXTOPTIONS.TTO_DEFAULT) {
                    return _pathText + _lineText.Trim();
                }
                return String.Empty;
            }

            private string GetSearchDisplayText() {
                return String.Format("{0} - ({1}, {2}): ",
                    _locationInfo.FilePath,
                    _locationInfo.Line,
                    _locationInfo.Column);
            }

            public override string UniqueName {
                get {
                    return _locationInfo.FilePath;
                }
            }

            public override bool CanGoToSource {
                get {
                    return true;
                }
            }

            public override VSTREEDISPLAYDATA DisplayData {
                get {
                    var res = new VSTREEDISPLAYDATA();
                    res.Image = res.SelectedImage = (ushort)_glyphType;
                    res.State = (uint)_VSTREEDISPLAYSTATE.TDS_FORCESELECT;

                    // This code highlights the text but it gets the wrong region.  This should be re-enabled
                    // and highlight the correct region.

                    //res.ForceSelectStart = (ushort)(_pathText.Length + _locationInfo.Column - 1);
                    //res.ForceSelectLength = (ushort)_locationInfo.Length;
                    return res;
                }
            }

            public override void GotoSource(VSOBJGOTOSRCTYPE SrcType) {
                _locationInfo.GotoSource();
            }

            #region IVsNavInfoNode Members

            public int get_Name(out string pbstrName) {
                pbstrName = _locationInfo.FilePath;
                return VSConstants.S_OK;
            }

            public int get_Type(out uint pllt) {
                pllt = 16; // (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion
        }

        class SymbolList : SimpleObjectList<SimpleLocationInfo>, IVsNavInfo, ICustomSearchListProvider {
            public SymbolList(string searchText, List<LocationInfo> locations, StandardGlyphGroup glyphType) {
                foreach (var location in locations) {
                    Children.Add(new SimpleLocationInfo(searchText, location, glyphType));
                }
            }

            public SymbolList(string searchText, List<LocationInfo> locations, List<LocationInfo> references, StandardGlyphGroup glyphType) {
                foreach (var location in locations) {
                    Children.Add(new SimpleLocationInfo(searchText, location, glyphType));
                }
                foreach (var location in references) {
                    Children.Add(new SimpleLocationInfo(searchText, location, StandardGlyphGroup.GlyphReference));
                }
            }

            class NodeEnumerator : IVsEnumNavInfoNodes {
                private readonly List<SimpleLocationInfo> _locations;
                private IEnumerator<SimpleLocationInfo> _locationEnum;

                public NodeEnumerator(List<SimpleLocationInfo> locations) {
                    _locations = locations;
                    Reset();
                }

                #region IVsEnumNavInfoNodes Members

                public int Clone(out IVsEnumNavInfoNodes ppEnum) {
                    ppEnum = new NodeEnumerator(_locations);
                    return VSConstants.S_OK;
                }

                public int Next(uint celt, IVsNavInfoNode[] rgelt, out uint pceltFetched) {
                    pceltFetched = 0;
                    while (celt-- != 0 && _locationEnum.MoveNext()) {
                        rgelt[pceltFetched++] = _locationEnum.Current;
                    }
                    return VSConstants.S_OK;
                }

                public int Reset() {
                    _locationEnum = _locations.GetEnumerator();
                    return VSConstants.S_OK;
                }

                public int Skip(uint celt) {
                    while (celt-- != 0) {
                        _locationEnum.MoveNext();
                    }
                    return VSConstants.S_OK;
                }

                #endregion
            }

            #region IVsNavInfo Members

            public int EnumCanonicalNodes(out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator(Children);
                return VSConstants.S_OK;
            }

            public int EnumPresentationNodes(uint dwFlags, out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator(Children);
                return VSConstants.S_OK;
            }

            public int GetLibGuid(out Guid pGuid) {
                pGuid = Guid.Empty;
                return VSConstants.S_OK;
            }

            public int GetSymbolType(out uint pdwType) {
                pdwType = (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion

            #region ICustomSearchListProvider Members

            public IVsSimpleObjectList2 GetSearchList() {
                return this;
            }

            #endregion
        }

        #region IOleCommandTarget Members

        /// <summary>
        /// Called from VS when we should handle a command or pass it on.
        /// </summary>
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            // preprocessing
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                switch ((VSConstants.VSStd97CmdID)nCmdID) {
                    case VSConstants.VSStd97CmdID.GotoDefn: return GotoDefinition();
                    case VSConstants.VSStd97CmdID.FindReferences: return FindAllReferences();
                        
                }
            } else if (pguidCmdGroup == CommonConstants.Std2KCmdGroupGuid) {
                OutliningTaggerProvider.OutliningTagger tagger;
                switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                    case VSConstants.VSStd2KCmdID.RETURN:
                        if (IronRubyToolsPackage.Instance.OptionsPage.AutoIndent) {
                            AutoIndent.HandleReturn(_textView, (IClassifier)_textView.TextBuffer.Properties.GetProperty(typeof(IDlrClassifier)));
                            return VSConstants.S_OK;
                        }
                        break;
#if FEATURE_INTELLISENSE
                    case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                    case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        var controller = _textView.Properties.GetProperty<IntellisenseController>(typeof(IntellisenseController));
                        if (controller != null) {
                            controller.TriggerCompletionSession((VSConstants.VSStd2KCmdID)nCmdID == VSConstants.VSStd2KCmdID.COMPLETEWORD);
                            return VSConstants.S_OK;
                        }
                        break;
#endif

                    case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                        tagger = _textView.TextBuffer.GetOutliningTagger();
                        if (tagger != null) {
                            tagger.Disable();
                        }
                        // let VS get the event as well
                        break;

                    case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                        tagger = _textView.TextBuffer.GetOutliningTagger();
                        if (tagger != null) {
                            tagger.Enable();
                        }
                        // let VS get the event as well
                        break;

                    case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        _textView.CommentBlock();
                        break;

                    case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        _textView.UncommentBlock();
                        break;
                }
            }

            return _next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// Called from VS to see what commands we support.  
        /// </summary>
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd97CmdID.GotoDefn:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd97CmdID.FindReferences:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            } else if (pguidCmdGroup == GuidList.guidIronRubyToolsCmdSet) {
                for (int i = 0; i < cCmds; i++) {
                    foreach (var command in CommandTable.Commands) {
                        if (command.CommandId == prgCmds[i].cmdID) {
                            int? res = command.EditFilterQueryStatus(ref prgCmds[i], pCmdText);
                            if (res != null) {
                                return res.Value;
                            }
                        }
                    }
                }
            } else if (pguidCmdGroup == CommonConstants.Std2KCmdGroupGuid) {                
                OutliningTaggerProvider.OutliningTagger tagger;
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                        case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                            tagger = _textView.TextBuffer.GetOutliningTagger();
                            if (tagger != null && tagger.Enabled) {
                                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            }
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                            tagger = _textView.TextBuffer.GetOutliningTagger();
                            if (tagger != null && !tagger.Enabled) {
                                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            }
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
            return _next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #endregion
    }
}
