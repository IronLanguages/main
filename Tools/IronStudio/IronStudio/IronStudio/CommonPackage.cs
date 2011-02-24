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
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.IronStudio.Navigation;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.IronStudio.Project;

namespace Microsoft.IronStudio {
    public abstract class CommonPackage : Package, IOleComponent {
        private uint _componentID;
        private LibraryManager _libraryManager;
        private ScriptEngine/*!*/ _engine;        

        public ScriptEngine/*!*/ Engine {
            get {
                if (_engine == null) {
                    _engine = CreateScriptEngine();
                }

                return _engine; 
            }
        }

        #region Language-specific abstracts

        public abstract ScriptEngine CreateScriptEngine();
        public abstract Type GetLibraryManagerType();
        public abstract LibraryManager CreateLibraryManager(CommonPackage package);

        // TODO:
        // public abstract bool TryGetStartupFileAndDirectory(out string filename, out string dir);

        #endregion

        public CommonPackage() {
            IServiceContainer container = this as IServiceContainer;
            ServiceCreatorCallback callback = new ServiceCreatorCallback(CreateService);
            //container.AddService(GetLanguageServiceType(), callback, true);
            container.AddService(GetLibraryManagerType(), callback, true);
        }

        private void RegisterForIdleTime() {
            IOleComponentManager mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
            if (_componentID == 0 && mgr != null) {
                OLECRINFO[] crinfo = new OLECRINFO[1];
                crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime |
                                              (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal |
                                              (uint)_OLECADVF.olecadvfRedrawOff |
                                              (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 1000;
                int hr = mgr.FRegisterComponent(this, crinfo, out _componentID);
            }
        }

        protected override void Dispose(bool disposing) {
            try {
                if (_componentID != 0) {
                    IOleComponentManager mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
                    if (mgr != null) {
                        mgr.FRevokeComponent(_componentID);
                    }
                    _componentID = 0;
                }
                if (null != _libraryManager) {
                    _libraryManager.Dispose();
                    _libraryManager = null;
                }
            } finally {
                base.Dispose(disposing);
            }
        }

        private object CreateService(IServiceContainer container, Type serviceType) {
            if (GetLibraryManagerType() == serviceType) {
                return _libraryManager = CreateLibraryManager(this);
            }
            return null;
        }

        /// <summary>
        /// Gets the current IWpfTextView that is the active document.
        /// </summary>
        /// <returns></returns>
        public static IWpfTextView GetActiveTextView() {
            var monitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
            object curDocument;
            if (ErrorHandler.Failed(monitorSelection.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out curDocument))) {
                // TODO: Report error
                return null;
            }

            IVsWindowFrame frame = curDocument as IVsWindowFrame;
            if (frame == null) {
                // TODO: Report error
                return null;
            }

            object docView = null;
            if (ErrorHandler.Failed(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView))) {
                // TODO: Report error
                return null;
            }

            if (docView is IVsCodeWindow) {
                IVsTextView textView;
                if (ErrorHandler.Failed(((IVsCodeWindow)docView).GetPrimaryView(out textView))) {
                    // TODO: Report error
                    return null;
                }

                var model = (IComponentModel)GetGlobalService(typeof(SComponentModel));
                var adapterFactory = model.GetService<IVsEditorAdaptersFactoryService>();
                var wpfTextView = adapterFactory.GetWpfTextView(textView);
                return wpfTextView;
            }
            return null;
        }

        public static IComponentModel ComponentModel {
            get {
                return (IComponentModel)GetGlobalService(typeof(SComponentModel));
            }
        }

        public static bool TryGetStartupFileAndDirectory(out string filename, out string dir) {
            var startupProject = GetStartupProject();
            if (startupProject != null) {
                filename = startupProject.GetStartupFile();
                dir = startupProject.GetWorkingDirectory();
            } else {
                var textView = CommonPackage.GetActiveTextView();
                if (textView == null) {
                    filename = null;
                    dir = null;
                    return false;
                }
                filename = textView.GetFilePath();
                dir = Path.GetDirectoryName(filename);
            }
            return true;
        }

        internal static CommonProjectNode GetStartupProject() {
            var buildMgr = (IVsSolutionBuildManager)Package.GetGlobalService(typeof(IVsSolutionBuildManager));
            IVsHierarchy hierarchy;
            if (ErrorHandler.Succeeded(buildMgr.get_StartupProject(out hierarchy)) && hierarchy != null) {
                return hierarchy.GetProject().GetCommonProject();
            }
            return null;
        }

        #region IOleComponent Members

        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked) {
            return 1;
        }

        public int FDoIdle(uint grfidlef) {
            if (null != _libraryManager) {
                _libraryManager.OnIdle();
            }
            return 0;
        }

        public int FPreTranslateMessage(MSG[] pMsg) {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser) {
            return 1;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam) {
            return 1;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved) {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved) {
        }

        public void OnAppActivate(int fActive, uint dwOtherThreadID) {
        }

        public void OnEnterState(uint uStateID, int fEnter) {
        }

        public void OnLoseActivation() {
        }

        public void Terminate() {
        }

        #endregion
    }
}
