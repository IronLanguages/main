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
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronStudio.Repl {
    [Export(typeof(IReplWindowProvider))]
    internal sealed class ReplWindowProvider : IReplWindowProvider {
        public ReplWindowProvider() {
        }

        #region IReplWindowProvider Members

        public ToolWindowPane CreateReplWindow(IReplEvaluator/*!*/ evaluator, IContentType contentType, string/*!*/ title, Guid languageServiceGuid, Guid replGuid) {
            int curId = 0;

            ToolWindowPane window;
            do {
                curId++;
                window = FindReplWindow(curId);
            } while (window != null);

            window = CreateReplWindow(evaluator, contentType, curId, title, languageServiceGuid, replGuid);
            if ((null == window) || (null == window.Frame)) {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }

            return window;
        }

        private Dictionary<int, VsReplWindow> _windows = new Dictionary<int, VsReplWindow>();

        public VsReplWindow FindReplWindow(int id) {
            VsReplWindow res;
            if (_windows.TryGetValue(id, out res)) {
                return res;
            }
            return null;
        }

        public ToolWindowPane FindReplWindow(Guid replId) {
            foreach (var idAndWindow in _windows) {
                var window = idAndWindow.Value;
                if (window.Guid == replId) {
                    return window;
                }
            }
            return null;
        }

        public ToolWindowPane CreateReplWindow(IReplEvaluator/*!*/ evaluator, IContentType/*!*/ contentType, int id, string/*!*/ title, Guid languageServiceGuid, Guid replGuid) {
            var service = (IVsUIShell)ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell));
            var model = (IComponentModel)ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel));

            var replWindow = new VsReplWindow(model, evaluator, contentType, title, languageServiceGuid, replGuid, id);

            Guid clsId = replWindow.ToolClsid;
            Guid toolType = typeof(VsReplWindow).GUID;
            Guid empty = Guid.Empty;
            IVsWindowFrame frame;

            // we don't pass __VSCREATETOOLWIN.CTW_fMultiInstance because multi instance panes are
            // destroyed when closed.  We are really multi instance but we don't want to be closed.  This
            // seems to work fine.
            ErrorHandler.ThrowOnFailure(
                service.CreateToolWindow(
                    (uint)(__VSCREATETOOLWIN.CTW_fInitNew | __VSCREATETOOLWIN.CTW_fForceCreate),
                    (uint)id,
                    replWindow.GetIVsWindowPane(),
                    ref clsId,
                    ref toolType,
                    ref empty,
                    null,
                    title,
                    null,
                    out frame
                )
            );

            replWindow.Frame = frame;

            replWindow.OnToolBarAdded();
            _windows[id] = replWindow;

            return replWindow;
        }

        #endregion
    }
}
