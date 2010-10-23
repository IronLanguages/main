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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.IronStudio.Navigation {
    public class TextViewWrapper : IVsTextView, IConnectionPointContainer {
        private IVsIntellisenseHost _intellisenseHost;
        private IVsContainedLanguageHost _languageHost;
        private IVsTextBufferCoordinator _bufferCoordinator;
        private IOleCommandTarget _nextTarget;
        private IOleCommandTarget _installedFilter;

        // This class is an helper class that provides an empty implementation of
        // IConnectionPoint. Is is needed only because some clients of the text view
        // need to subscribe for events, but this implementation does not raise
        // any event.
        private class TextViewConnectionPoint : IConnectionPoint {
            private uint _nextCoockie;
            
            #region IConnectionPoint Members
            public void Advise(object pUnkSink, out uint pdwCookie) {
                pdwCookie = ++_nextCoockie;
            }

            public void EnumConnections(out IEnumConnections ppEnum) {
                throw new NotImplementedException();
            }

            public void GetConnectionInterface(out Guid pIID) {
                throw new NotImplementedException();
            }

            public void GetConnectionPointContainer(out IConnectionPointContainer ppCPC) {
                throw new NotImplementedException();
            }

            public void Unadvise(uint dwCookie) {
                // Do Nothing
            }

            #endregion
        }

        public TextViewWrapper(IVsContainedLanguageHost languageHost, IVsIntellisenseHost intellisenseHost, IVsTextBufferCoordinator coordinator, IOleCommandTarget nextTarget) {
            if (null == intellisenseHost) {
                throw new ArgumentNullException("buffer");
            }
            _intellisenseHost = intellisenseHost;
            _bufferCoordinator = coordinator;
            _languageHost = languageHost;
            _nextTarget = nextTarget;
        }

        public IVsTextViewFilter InstalledFilter {
            get {
                return _installedFilter as IVsTextViewFilter;
            }
        }

        public TextSpan GetPrimarySpan(TextSpan secondary) {
            TextSpan[] primary = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(_bufferCoordinator.MapSecondaryToPrimarySpan(secondary, primary));
            return primary[0];
        }

        #region IVsTextView Members
        public int AddCommandFilter(IOleCommandTarget pNewCmdTarg, out IOleCommandTarget ppNextCmdTarg) {
            ppNextCmdTarg = _nextTarget;
            _installedFilter = pNewCmdTarg;
            return VSConstants.S_OK;
        }

        public int CenterColumns(int iLine, int iLeftCol, int iColCount) {
            return VSConstants.S_OK;
        }

        public int CenterLines(int iTopLine, int iCount) {
            return VSConstants.S_OK;
        }

        public int ClearSelection(int fMoveToAnchor) {
            throw new NotImplementedException();
        }

        public int CloseView() {
            _intellisenseHost = null;
            _bufferCoordinator = null;
            _languageHost = null;
            return VSConstants.S_OK;
        }

        public int EnsureSpanVisible(TextSpan span) {
            if (null == _languageHost) {
                return VSConstants.S_OK;
            }
            return _languageHost.EnsureSpanVisible(span);
        }

        public int GetBuffer(out IVsTextLines ppBuffer) {
            return _bufferCoordinator.GetSecondaryBuffer(out ppBuffer);
        }

        public int GetCaretPos(out int piLine, out int piColumn) {
            TextSpan originalSpan = new TextSpan();
            ErrorHandler.ThrowOnFailure(
                _intellisenseHost.GetContextCaretPos(out originalSpan.iStartLine, out originalSpan.iStartIndex));
            originalSpan.iEndLine = originalSpan.iStartLine;
            originalSpan.iEndIndex = originalSpan.iStartIndex;
            TextSpan[] convertedSpan = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(_bufferCoordinator.MapPrimaryToSecondarySpan(originalSpan, convertedSpan));
            piLine = convertedSpan[0].iStartLine;
            piColumn = convertedSpan[0].iStartIndex;
            return VSConstants.S_OK;
        }

        public int GetLineAndColumn(int iPos, out int piLine, out int piIndex) {
            IVsTextLines buffer;
            ErrorHandler.ThrowOnFailure(_bufferCoordinator.GetSecondaryBuffer(out buffer));
            return buffer.GetLineIndexOfPosition(iPos, out piLine, out piIndex);
        }

        public int GetLineHeight(out int piLineHeight) {
            throw new NotImplementedException();
        }

        public int GetNearestPosition(int iLine, int iCol, out int piPos, out int piVirtualSpaces) {
            throw new NotImplementedException();
        }

        public int GetPointOfLineColumn(int iLine, int iCol, Microsoft.VisualStudio.OLE.Interop.POINT[] ppt) {
            throw new NotImplementedException();
        }

        public int GetScrollInfo(int iBar, out int piMinUnit, out int piMaxUnit, out int piVisibleUnits, out int piFirstVisibleUnit) {
            throw new NotImplementedException();
        }

        public int GetSelectedText(out string pbstrText) {
            TextSpan[] span = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(_intellisenseHost.GetContextSelection(span));
            TextSpan[] convertedSpan = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(_bufferCoordinator.MapPrimaryToSecondarySpan(span[0], convertedSpan));
            IVsTextLines buffer;
            ErrorHandler.ThrowOnFailure(_bufferCoordinator.GetSecondaryBuffer(out buffer));
            return buffer.GetLineText(convertedSpan[0].iStartLine, convertedSpan[0].iStartIndex, convertedSpan[0].iEndLine, convertedSpan[0].iEndIndex, out pbstrText);
        }

        public int GetSelection(out int piAnchorLine, out int piAnchorCol, out int piEndLine, out int piEndCol) {
            TextSpan[] span = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(_intellisenseHost.GetContextSelection(span));
            TextSpan[] convertedSpan = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(_bufferCoordinator.MapPrimaryToSecondarySpan(span[0], convertedSpan));
            piAnchorLine = convertedSpan[0].iStartLine;
            piAnchorCol = convertedSpan[0].iStartIndex;
            piEndLine = convertedSpan[0].iEndLine;
            piEndCol = convertedSpan[0].iEndIndex;
            return VSConstants.S_OK;
        }

        public int GetSelectionDataObject(out Microsoft.VisualStudio.OLE.Interop.IDataObject ppIDataObject) {
            throw new NotImplementedException();
        }

        public TextSelMode GetSelectionMode() {
            throw new NotImplementedException();
        }

        public int GetSelectionSpan(TextSpan[] pSpan) {
            TextSpan[] primarySpan = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(_intellisenseHost.GetContextSelection(primarySpan));
            return _bufferCoordinator.MapPrimaryToSecondarySpan(primarySpan[0], pSpan);
        }

        public int GetTextStream(int iTopLine, int iTopCol, int iBottomLine, int iBottomCol, out string pbstrText) {
            throw new NotImplementedException();
        }

        public IntPtr GetWindowHandle() {
            IntPtr hWnd;
            ErrorHandler.ThrowOnFailure(_intellisenseHost.GetHostWindow(out hWnd));
            return hWnd;
        }

        public int GetWordExtent(int iLine, int iCol, uint dwFlags, TextSpan[] pSpan) {
            throw new NotImplementedException();
        }

        public int HighlightMatchingBrace(uint dwFlags, uint cSpans, TextSpan[] rgBaseSpans) {
            if ((null == rgBaseSpans) || (rgBaseSpans.Length == 0)) {
                throw new ArgumentNullException("rgBaseSpans");
            }
            if ((uint)rgBaseSpans.Length != cSpans) {
                throw new System.ArgumentOutOfRangeException("cSpans");
            }
            TextSpan[] convertedSpans = new TextSpan[cSpans];
            TextSpan[] workingCopy = new TextSpan[1];
            for (int i = 0; i < cSpans; ++i) {
                ErrorHandler.ThrowOnFailure(_bufferCoordinator.MapSecondaryToPrimarySpan(rgBaseSpans[i], workingCopy));
                convertedSpans[i] = workingCopy[0];
            }
            return _intellisenseHost.HighlightMatchingBrace(dwFlags, cSpans, convertedSpans);
        }

        public int Initialize(IVsTextLines pBuffer, IntPtr hwndParent, uint InitFlags, INITVIEW[] pInitView) {
            return VSConstants.S_OK;
        }

        public int PositionCaretForEditing(int iLine, int cIndentLevels) {
            throw new NotImplementedException();
        }

        public int RemoveCommandFilter(Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget pCmdTarg) {
            _installedFilter = null;
            return VSConstants.S_OK;
        }

        public int ReplaceTextOnLine(int iLine, int iStartCol, int iCharsToReplace, string pszNewText, int iNewLen) {
            IVsTextLines buffer;
            ErrorHandler.ThrowOnFailure(_bufferCoordinator.GetSecondaryBuffer(out buffer));
            GCHandle handle = GCHandle.Alloc(pszNewText, GCHandleType.Pinned);
            try {
                TextSpan[] span = new TextSpan[1];
                IntPtr textPtr = handle.AddrOfPinnedObject();
                int textLen = string.IsNullOrEmpty(pszNewText) ? 0 : pszNewText.Length;
                ErrorHandler.ThrowOnFailure(
                    buffer.ReplaceLines(iLine, iStartCol, iLine, iStartCol + iCharsToReplace, textPtr, textLen, span));
            } finally {
                if ((null != handle) && (handle.IsAllocated)) {
                    handle.Free();
                }
            }
            return VSConstants.S_OK;
        }

        public int RestrictViewRange(int iMinLine, int iMaxLine, IVsViewRangeClient pClient) {
            throw new NotImplementedException();
        }

        public int SendExplicitFocus() {
            throw new NotImplementedException();
        }

        public int SetBuffer(IVsTextLines pBuffer) {
            return VSConstants.S_OK;
        }

        public int SetCaretPos(int iLine, int iColumn) {
            TextSpan original = new TextSpan();
            original.iStartLine = original.iEndLine = iLine;
            original.iStartIndex = original.iEndIndex = iColumn;
            TextSpan[] converted = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(_bufferCoordinator.MapSecondaryToPrimarySpan(original, converted));
            return _intellisenseHost.SetContextCaretPos(converted[0].iStartLine, converted[0].iStartIndex);
        }

        public int SetScrollPosition(int iBar, int iFirstVisibleUnit) {
            throw new NotImplementedException();
        }

        public int SetSelection(int iAnchorLine, int iAnchorCol, int iEndLine, int iEndCol) {
            TextSpan original = new TextSpan();
            original.iStartLine = iAnchorLine;
            original.iStartIndex = iAnchorCol;
            original.iEndLine = iEndLine;
            original.iEndIndex = iEndCol;
            TextSpan[] converted = new TextSpan[1];
            ErrorHandler.ThrowOnFailure(_bufferCoordinator.MapSecondaryToPrimarySpan(original, converted));
            return _intellisenseHost.SetContextSelection(converted[0].iStartLine, converted[0].iStartIndex, converted[0].iEndLine, converted[0].iEndIndex);
        }

        public int SetSelectionMode(TextSelMode iSelMode) {
            throw new NotImplementedException();
        }

        public int SetTopLine(int iBaseLine) {
            throw new NotImplementedException();
        }

        public int UpdateCompletionStatus(IVsCompletionSet pCompSet, uint dwFlags) {
            return _intellisenseHost.UpdateCompletionStatus(pCompSet, dwFlags);
        }

        public int UpdateTipWindow(IVsTipWindow pTipWindow, uint dwFlags) {
            return _intellisenseHost.UpdateTipWindow(pTipWindow, dwFlags);
        }

        public int UpdateViewFrameCaption() {
            return VSConstants.S_OK;
        }
        #endregion

        #region IConnectionPointContainer Members
        void IConnectionPointContainer.EnumConnectionPoints(out IEnumConnectionPoints ppEnum) {
            throw new NotImplementedException();
        }

        private TextViewConnectionPoint connectionPoint;
        void IConnectionPointContainer.FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP) {
            ppCP = null;
            if (typeof(IVsTextViewEvents).GUID == riid) {
                if (null == connectionPoint) {
                    connectionPoint = new TextViewConnectionPoint();
                }
                ppCP = connectionPoint;
                return;
            }
            throw new ArgumentOutOfRangeException("riid");
        }
        #endregion
    }
}
