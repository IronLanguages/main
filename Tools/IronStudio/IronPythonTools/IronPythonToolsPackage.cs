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
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.IronPythonTools.Editor;
using Microsoft.IronPythonTools.Intellisense;
using Microsoft.IronPythonTools.Navigation;
using Microsoft.IronPythonTools.Options;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Navigation;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronPythonTools {
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
    [InstalledProductRegistration("#110", "#112", "1.0",        // This attribute is used to register the informations needed to show the this package in the Help/About dialog of Visual Studio.
        IconResourceID = 400)]
    [ProvideMenuResource(1000, 1)]                              // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideAutoLoad(CommonConstants.UIContextNoSolution)]
    [ProvideAutoLoad(CommonConstants.UIContextSolutionExists)]
    [ProvideLanguageEditorOptionPage(typeof(PythonOptionsPage), PythonConstants.LanguageName, "Advanced", "", "113")]
    [ProvideLanguageEditorOptionPage(typeof(PythonIntellisenseOptionsPage), PythonConstants.LanguageName, "Intellisense", "", "113")]
    [Guid(GuidList.guidIronPythonToolsPkgString)]              // our packages GUID        
    [ProvideLanguageService(typeof(PythonLanguageInfo), PythonConstants.LanguageName, 106, RequestStockColors = true, ShowSmartIndent=true, ShowCompletion=true, DefaultToInsertSpaces=true, HideAdvancedMembersByDefault=false, EnableAdvancedMembersOption=true, ShowDropDownOptions=true)]
    [ProvideLanguageExtension(typeof(PythonLanguageInfo), ".py")]
    public sealed class IronPythonToolsPackage : CommonPackage {
        private LanguagePreferences _langPrefs;
        public static IronPythonToolsPackage Instance;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public IronPythonToolsPackage() {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            Instance = this;

            LoadAssemblies();
        }

        /// <summary>
        /// VS seems to load extensions via Assembly.LoadFrom. When an assembly is being loaded via Assembly.Load the CLR fusion probes privatePath 
        /// set in App.config (devenv.exe.config) first and then tries the code base of the assembly that called Assembly.Load if it was itself loaded via LoadFrom. 
        /// In order to locate IronPython.Modules correctly, the call to Assembly.Load must originate from an assembly in IronPythonTools installation folder. 
        /// Although Microsoft.Scripting is also in that folder it can be loaded first by IronRuby and that causes the Assembly.Load to search in IronRuby's 
        /// installation folder. Adding a reference to IronPython.Modules also makes sure that the assembly is loaded from the same location as IronPythonToolsCore.
        /// </summary>
        private static void LoadAssemblies() {
            GC.KeepAlive(typeof(IronPython.Modules.ArrayModule)); // IronPython.Modules
        }
       
        internal static void NavigateTo(string view, Guid docViewGuidType, int line, int col) {
            IVsTextManager textMgr = (IVsTextManager)Instance.GetService(typeof(SVsTextManager));                        
            var model = Instance.GetService(typeof(SComponentModel)) as IComponentModel;
            var adapter = model.GetService<IVsEditorAdaptersFactoryService>();

            IVsTextView viewAdapter;
            IVsUIShellOpenDocument uiShellOpenDocument = Instance.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
            IVsUIHierarchy hierarchy;
            uint itemid;
            IVsWindowFrame pWindowFrame;

            VsShellUtilities.OpenDocument(
                Instance,
                view,
                Guid.Empty,
                out hierarchy,
                out itemid,
                out pWindowFrame,
                out viewAdapter);

            ErrorHandler.ThrowOnFailure(pWindowFrame.Show());

            // Set the cursor at the beginning of the declaration.
            ErrorHandler.ThrowOnFailure(viewAdapter.SetCaretPos(line, col));
            // Make sure that the text is visible.
            viewAdapter.CenterLines(line, 1);
        }

        public override ScriptEngine CreateScriptEngine() {
            return RuntimeHost.ScriptEngine;
        }

        public override Type GetLibraryManagerType() {
            return typeof(IPythonLibraryManager);
        }

        internal PythonOptionsPage OptionsPage {
            get {
                return (PythonOptionsPage)GetDialogPage(typeof(PythonOptionsPage));
            }
        }

        internal PythonIntellisenseOptionsPage IntellisenseOptionsPage {
            get {
                return (PythonIntellisenseOptionsPage)GetDialogPage(typeof(PythonIntellisenseOptionsPage));
            }
        }

        private IPythonAnalyzer _analyzer;

        internal IPythonAnalyzer Analyzer {
            get {
                if (_analyzer == null) {
                    var model = GetService(typeof(SComponentModel)) as IComponentModel;
                    _analyzer = model.GetService<IPythonAnalyzer>();
                    //_analyzer = new PythonAnalyzer(model);
                }
                return _analyzer;
            }
        }

        public override LibraryManager CreateLibraryManager(CommonPackage package) {
            return new PythonLibraryManager((IronPythonToolsPackage)package);
        }

        public IVsSolution Solution {
            get {
                return GetService(typeof(SVsSolution)) as IVsSolution;
            }
        }

        public IPythonRuntimeHost RuntimeHost {
            get {
                return ComponentModel.GetService<IPythonRuntimeHost>();
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize() {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs) {
                foreach (var command in Commands.CommandTable.Commands) {
                    var beforeQueryStatus = command.BeforeQueryStatus;
                    CommandID toolwndCommandID = new CommandID(GuidList.guidIronPythonToolsCmdSet, command.CommandId);
                    if (beforeQueryStatus == null) {
                        MenuCommand menuToolWin = new MenuCommand(command.DoCommand, toolwndCommandID);
                        mcs.AddCommand(menuToolWin);
                    } else {
                        OleMenuCommand menuToolWin = new OleMenuCommand(command.DoCommand, toolwndCommandID);
                        menuToolWin.BeforeQueryStatus += beforeQueryStatus;
                        mcs.AddCommand(menuToolWin);
                    }
                }
            }

            // This is just to force the MEF creation of the DLR runtime hosting layer, including the content type and runtime.
            if (RuntimeHost == null) {
                throw new InvalidOperationException("Unable to obtain the DLR Runtime hosting component");
            }

            RuntimeHost.EnterOutliningModeOnOpen = OptionsPage.EnterOutliningModeOnOpen;

            // register our language service so that we can support features like the navigation bar
            var langService = new PythonLanguageInfo(this);
            ((IServiceContainer)this).AddService(langService.GetType(), langService, true);

            var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            uint cookie;
            ErrorHandler.ThrowOnFailure(solution.AdviseSolutionEvents(new SolutionAdvisor(), out cookie));

            IVsTextManager textMgr = (IVsTextManager)Instance.GetService(typeof(SVsTextManager));
            var langPrefs = new LANGPREFERENCES[1];
            langPrefs[0].guidLang = typeof(PythonLanguageInfo).GUID;
            ErrorHandler.ThrowOnFailure(textMgr.GetUserPreferences(null, null, langPrefs, null));
            _langPrefs = new LanguagePreferences(langPrefs[0]);

            Guid guid = typeof(IVsTextManagerEvents2).GUID;
            IConnectionPoint connectionPoint;
            ((IConnectionPointContainer)textMgr).FindConnectionPoint(ref guid, out connectionPoint);
            connectionPoint.Advise(_langPrefs, out cookie);

            // propagate options to our runtime host
            RuntimeHost.IntersectMembers = IntellisenseOptionsPage.IntersectMembers;
            RuntimeHost.EnterOutliningModeOnOpen = OptionsPage.EnterOutliningModeOnOpen;
            RuntimeHost.HideAdvancedMembers = Instance.LangPrefs.HideAdvancedMembers;
        }

        internal LanguagePreferences LangPrefs {
            get {
                return _langPrefs;
            }
        }

        public EnvDTE.DTE DTE {
            get {
                return (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            }
        }

        public IContentType ContentType {
            get {
                return RuntimeHost.ContentType;
            }
        }

    }
}
