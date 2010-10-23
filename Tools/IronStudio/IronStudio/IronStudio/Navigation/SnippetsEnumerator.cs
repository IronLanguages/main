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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.TextManager.Interop;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSConstants = Microsoft.VisualStudio.VSConstants;


namespace Microsoft.IronStudio.Navigation {
    public class SnippetsEnumerator : IEnumerable<VsExpansion> {
        private IVsTextManager2 _textManager;
        private Guid _languageGuid;
        private bool _shortcutOnly;

        /// <summary>
        /// This structure is used to facilitate the interop calls to the method
        /// exposed by IVsExpansionEnumeration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ExpansionBuffer {
            public IntPtr pathPtr;
            public IntPtr titlePtr;
            public IntPtr shortcutPtr;
            public IntPtr descriptionPtr;
        }
       
        public SnippetsEnumerator(IVsTextManager2 textManager, Guid languageGuid) {
            if (null == textManager) {
                throw new ArgumentNullException("textManager");
            }
            _textManager = textManager;
            _languageGuid = languageGuid;
        }

        public bool ShortcutOnly {
            get { return _shortcutOnly; }
            set { _shortcutOnly = value; }
        }

        #region IEnumerable<VsExpansion> Members
        public IEnumerator<VsExpansion> GetEnumerator() {
            IVsExpansionManager expansionManager;
            ErrorHandler.ThrowOnFailure(_textManager.GetExpansionManager(out expansionManager));

            IVsExpansionEnumeration enumerator;
            int onlyShortcut = (this.ShortcutOnly ? 1 : 0);
            ErrorHandler.ThrowOnFailure(expansionManager.EnumerateExpansions(_languageGuid, onlyShortcut, null, 0, 0, 0, out enumerator));

            ExpansionBuffer buffer = new ExpansionBuffer();
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try {
                int hr = VSConstants.S_OK;
                uint fetched;
                while (VSConstants.S_OK == (hr = enumerator.Next(1, new IntPtr[] { handle.AddrOfPinnedObject() }, out fetched))) {
                    buffer = (ExpansionBuffer)handle.Target;
                    try {
                        handle.Free();
                        if (IntPtr.Zero != buffer.shortcutPtr) {
                            VsExpansion expansion = new VsExpansion();
                            expansion.shortcut = Marshal.PtrToStringBSTR(buffer.shortcutPtr);
                            if (IntPtr.Zero != buffer.descriptionPtr) {
                                expansion.description = Marshal.PtrToStringBSTR(buffer.descriptionPtr);
                            }
                            if (IntPtr.Zero != buffer.pathPtr) {
                                expansion.path = Marshal.PtrToStringBSTR(buffer.pathPtr);
                            }
                            if (IntPtr.Zero != buffer.titlePtr) {
                                expansion.title = Marshal.PtrToStringBSTR(buffer.titlePtr);
                            }
                            yield return expansion;
                            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        }
                    } finally {
                        if (IntPtr.Zero != buffer.descriptionPtr) {
                            Marshal.FreeBSTR(buffer.descriptionPtr);
                            buffer.descriptionPtr = IntPtr.Zero;
                        }
                        if (IntPtr.Zero != buffer.pathPtr) {
                            Marshal.FreeBSTR(buffer.pathPtr);
                            buffer.pathPtr = IntPtr.Zero;
                        }
                        if (IntPtr.Zero != buffer.shortcutPtr) {
                            Marshal.FreeBSTR(buffer.shortcutPtr);
                            buffer.shortcutPtr = IntPtr.Zero;
                        }
                        if (IntPtr.Zero != buffer.titlePtr) {
                            Marshal.FreeBSTR(buffer.titlePtr);
                            buffer.titlePtr = IntPtr.Zero;
                        }
                    }
                }
                ErrorHandler.ThrowOnFailure(hr);
            } finally {
                if (handle.IsAllocated) {
                    handle.Free();
                }
            }
        }
        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }

        #endregion
    }
}