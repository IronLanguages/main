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
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.IronRubyTools.Intellisense;
using Microsoft.IronRubyTools.Navigation;
using Microsoft.IronRubyTools.Options;
using Microsoft.IronRubyTools.Project;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Navigation;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.Windows;
using Microsoft.IronStudio.Project;

namespace Microsoft.IronRubyTools {
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
    [ProvideLanguageEditorOptionPage(typeof(RubyOptionsPage), RubyConstants.LanguageName, "Advanced", "", "113")]
    [Guid(GuidList.guidIronRubyToolsPkgString)]              // our packages GUID        
    [ProvideLanguageService(typeof(RubyLanguageInfo), RubyConstants.LanguageName, 106, RequestStockColors = true, DefaultToInsertSpaces = true, EnableCommenting = true)]
    [ProvideLanguageExtension(typeof(RubyLanguageInfo), ".rb")]
    [ProvideLanguageExtension(typeof(RubyLanguageInfo), ".ru")]
    [ProvideLanguageExtension(typeof(RubyLanguageInfo), ".gemspec")]
    public sealed class IronRubyToolsPackage : CommonPackage {
        public static IronRubyToolsPackage Instance;

        internal readonly string IronRubyBinPath;
        internal readonly string IronRubyExecutable;
        internal readonly string IronRubyWindowsExecutable;
        internal readonly string IronRubyToolsPath;
        internal readonly string GemsBinPath;

        internal bool IronRubyInstalled {
            get { return IronRubyBinPath != IronRubyToolsPath; }
        }

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public IronRubyToolsPackage() {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            Instance = this;

            LoadAssemblies();
            InitializeRegistrySettings();
            IronRubyToolsPath = Path.GetDirectoryName(typeof(IronRubyToolsPackage).Assembly.Location);
            IronRubyBinPath = GetIronRubyBinPath() ?? IronRubyToolsPath;
            IronRubyExecutable = Path.Combine(IronRubyBinPath, RubyConstants.IronRubyExecutable);
            IronRubyWindowsExecutable = Path.Combine(IronRubyBinPath, RubyConstants.IronRubyWindowsExecutable);
            GemsBinPath = FindGemBinPath(IronRubyBinPath);
        }

        private static string GetIronRubyBinPath() {
            string result;

#if DEBUG
            result = Environment.GetEnvironmentVariable("DLR_ROOT");
            if (result != null) {
                result = Path.Combine(result, @"Bin\Debug");
                if (IronRubyExistsIn(result)) {
                    return result;
                }
            }
#endif
            result = Environment.GetEnvironmentVariable(RubyContext.BinDirEnvironmentVariable);
            if (result != null && IronRubyExistsIn(result)) {
                return result;
            }

            var paths = Environment.GetEnvironmentVariable("PATH");
            if (paths != null) {
                foreach (string dir in paths.Split(Path.PathSeparator)) {
                    try {
                        if (IronRubyExistsIn(dir)) {
                            return dir;
                        }
                    } catch {
                        // ignore
                    }
                }
            }

            return null;
        }

        private static bool IronRubyExistsIn(string/*!*/ dir) {
            return File.Exists(Path.Combine(dir, RubyConstants.IronRubyExecutable));
        }

        private static string FindGemBinPath(string/*!*/ ironRubyBinPath) {
            string result;
            result = Environment.GetEnvironmentVariable("GEM_PATH");
            if (Directory.Exists(result)) {
                return result;
            }

            try {
                result = Path.Combine(RubyUtils.GetHomeDirectory(PlatformAdaptationLayer.Default), @"ironruby\1.9.1\bin");
                if (Directory.Exists(result)) {
                    return result;
                }
            } catch {
                // ignore
            }

            result = Path.Combine(ironRubyBinPath, @"Lib\ironruby\gems\1.9.1");
            if (Directory.Exists(result)) {
                return result;
            }

            result = Path.Combine(ironRubyBinPath, @"bin");
            if (Directory.Exists(result)) {
                return result;
            }

            return null;
        }

        /// <summary>
        /// VS seems to load extensions via Assembly.LoadFrom. When an assembly is being loaded via Assembly.Load the CLR fusion probes privatePath 
        /// set in App.config (devenv.exe.config) first and then tries the code base of the assembly that called Assembly.Load if it was itself loaded via LoadFrom. 
        /// In order to locate our assemblies correctly, the call to Assembly.Load must originate from an assembly in IronRubyTools installation folder. 
        /// </summary>
        private static void LoadAssemblies() {
            GC.KeepAlive(typeof(IronRuby.Builtins.HashOps)); // IronRuby.Libraries
        }

        private bool _ironRubyInstallationRequirementShown = false;

        internal bool RequireIronRubyInstalled(bool allowCancel) {
            if (!IronRubyInstalled && !_ironRubyInstallationRequirementShown) {
                _ironRubyInstallationRequirementShown = true;

                return MessageBox.Show(
                    "IronRuby installation not found. Although basic scripts should work standard libraries and gems won't be available.\r\n" +
                    "Please download and install the latest release of IronRuby from http://ironruby.codeplex.com.",
                    "IronRuby not installed",
                    allowCancel ? MessageBoxButton.OKCancel : MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK
                ) == MessageBoxResult.OK;
            } 

            return true;
        }

        private void InitializeRegistrySettings() {
            using (var textEditor = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey("Text Editor")) {
                using (var ironRubyKey = textEditor.CreateSubKey(RubyConstants.TextEditorSettingsRegistryKey)) {
                    object curValue;
                    curValue = ironRubyKey.GetValue("Insert Tabs");
                    if (curValue == null) {
                        ironRubyKey.SetValue("Insert Tabs", 0);
                    }

                    curValue = ironRubyKey.GetValue("Indent Size");
                    if (curValue == null) {
                        ironRubyKey.SetValue("Indent Size", 2);
                    }
                }
            }
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
            /*TextSpan visibleSpan = new TextSpan();
            visibleSpan.iStartLine = line;
            visibleSpan.iStartIndex = col;
            visibleSpan.iEndLine = line;
            visibleSpan.iEndIndex = col + 1;*/
            //ErrorHandler.ThrowOnFailure(viewAdapter.EnsureSpanVisible(visibleSpan));
        }
        
        public override ScriptEngine CreateScriptEngine() {
            return RuntimeHost.RubyScriptEngine;
        }

        public override Type GetLibraryManagerType() {
            return typeof(IRubyLibraryManager);
        }

        internal RubyOptionsPage OptionsPage {
            get {
                return (RubyOptionsPage)GetDialogPage(typeof(RubyOptionsPage));
            }
        }

        private RubyAnalyzer _analyzer;

        internal RubyAnalyzer Analyzer {
            get {
                if (_analyzer == null) {
                    var model = GetService(typeof(SComponentModel)) as IComponentModel;
                    _analyzer = new RubyAnalyzer(model);
                }
                return _analyzer;
            }
        }

        public override LibraryManager CreateLibraryManager(CommonPackage package) {
            return new RubyLibraryManager((IronRubyToolsPackage)package);
        }

        public IVsSolution Solution {
            get {
                return GetService(typeof(SVsSolution)) as IVsSolution;
            }
        }

        public IRubyRuntimeHost RuntimeHost {
            get {
                return ComponentModel.GetService<IRubyRuntimeHost>();
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
                    CommandID toolwndCommandID = new CommandID(GuidList.guidIronRubyToolsCmdSet, command.CommandId);                    
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
            var langService = new RubyLanguageInfo(this);
            ((IServiceContainer)this).AddService(langService.GetType(), langService, true);

            var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            uint cookie;
            ErrorHandler.ThrowOnFailure(solution.AdviseSolutionEvents(new SolutionAdvisor(), out cookie));
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
