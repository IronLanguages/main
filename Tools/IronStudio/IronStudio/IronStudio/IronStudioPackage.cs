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
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.IronStudio.Repl;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text.Classification;
using System.ComponentModel;

namespace Microsoft.IronStudio {
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>    
    [PackageRegistration(UseManagedResourcesOnly = true)]       // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
    [ProvideMenuResource(1000, 1)]                              // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideKeyBindingTable(VsReplWindow.TypeGuid, 200)]        // Resource ID: "Interactive Console"
    [ProvideAutoLoad(CommonConstants.UIContextNoSolution)]
    [ProvideAutoLoad(CommonConstants.UIContextSolutionExists)]
    [ProvideToolWindow(typeof(VsReplWindow), MultiInstances = true)]
    [Guid(GuidList.guidIronStudioPkgString)]              // our packages GUID        
    public sealed class IronStudioPackage : Package, IVsToolWindowFactory {
        public static IronStudioPackage Instance;
        public IronStudioPackage() {
            Instance = this;
        }

        protected override void Initialize() {
            base.Initialize();

            // TODO:
            //OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            //if (null != mcs) {
            //    CommandID menuCommandID = new CommandID(GuidList.guidIronStudioCmdSet, (int)PkgCmdIDList.cmdidBreakRepl);
            //    MenuCommand menuItem = new MenuCommand(Callback, menuCommandID);
            //    mcs.AddCommand(menuItem);
            //}
        }
        
        int IVsToolWindowFactory.CreateToolWindow(ref Guid toolWindowType, uint id) {
            int num = (int)id;
            if (toolWindowType == typeof(VsReplWindow).GUID) {                
                string evalAsm, eval, contentType, title;
                Guid guid, langSvcGuid;
                if (GetReplInfo(num, out evalAsm, out eval, out contentType, out title, out langSvcGuid, out guid)) {

                    var model = (IComponentModel)GetService(typeof(SComponentModel));
                    var contentTypes = model.GetService<IContentTypeRegistryService>();
                    var contentTypeObj = contentTypes.GetContentType(contentType);
                    var evaluator = (IReplEvaluator)Activator.CreateInstance(evalAsm, eval).Unwrap();

                    var replProvider = model.GetExtensions<IReplWindowProvider>().First();
                    replProvider.CreateReplWindow(evaluator, contentTypeObj, num, title, langSvcGuid, guid);
                    
                    return VSConstants.S_OK;
                }   

                return VSConstants.E_FAIL;
            }

            foreach (Attribute attribute in Attribute.GetCustomAttributes(base.GetType())) {
                if (attribute is ProvideToolWindowAttribute) {
                    ProvideToolWindowAttribute tool = (ProvideToolWindowAttribute)attribute;
                    if (tool.ToolType.GUID == toolWindowType) {
                        this.FindToolWindow(tool.ToolType, num, true);
                        break;
                    }
                }
            }
            return 0;
        }

        const string ActiveReplsKey = "ActiveRepls";

        public void SaveReplInfo(int id, IReplEvaluator evaluator, IContentType contentType, string title, Guid languageServiceGuid, Guid replId) {
            using (var repl = UserRegistryRoot.CreateSubKey(ActiveReplsKey)) {
                using (var curRepl = repl.CreateSubKey(id.ToString())) {
                    curRepl.SetValue("EvaluatorType", evaluator.GetType().FullName);
                    curRepl.SetValue("EvaluatorAssembly", evaluator.GetType().Assembly.FullName);
                    curRepl.SetValue("ContentType", contentType.TypeName);
                    curRepl.SetValue("Title", title);
                    curRepl.SetValue("Guid", replId.ToString());
                    curRepl.SetValue("LanguageServiceGuid", languageServiceGuid.ToString());
                }
            }
        }

        public bool GetReplInfo(int id, out string evaluator, out string evalType, out string contentType, out string title, out Guid languageServiceGuid, out Guid guid) {
            using (var repl = UserRegistryRoot.OpenSubKey(ActiveReplsKey)) {                
                if (repl != null) {
                    using (var curRepl = repl.OpenSubKey(id.ToString())) {
                        if (curRepl != null) {
                            evaluator = (string)curRepl.GetValue("EvaluatorAssembly");
                            evalType = (string)curRepl.GetValue("EvaluatorType");
                            contentType = (string)curRepl.GetValue("ContentType");
                            title = (string)curRepl.GetValue("Title");
                            guid = Guid.Parse((string)curRepl.GetValue("Guid"));
                            languageServiceGuid = Guid.Parse((string)curRepl.GetValue("LanguageServiceGuid"));
                            return true;
                        }
                    }
                }
            }
            evaluator = null;
            contentType = null;
            title = null;
            evalType = null;
            guid = Guid.Empty;
            languageServiceGuid = Guid.Empty;
            return false;
        }
    }
}
