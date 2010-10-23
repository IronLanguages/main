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
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.IronRubyTools.Library.Repl;
using Microsoft.IronRubyTools.Navigation;
using Microsoft.IronRubyTools.Project;
using Microsoft.IronRubyTools.Repl;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Repl;
using Microsoft.Scripting.Utils;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronRubyTools.Commands {
    /// <summary>
    /// Provides the command for starting a file or the start item of a project in the REPL window.
    /// </summary>
    internal sealed class ExecuteInReplCommand : Command {
        public static ExecuteInReplCommand Instance;
        private static Guid _replGuid = new Guid("d9b67029-3a2b-49d1-814b-db21f733c16a");

        public ExecuteInReplCommand() {
            Instance = this;
        }

        internal static ToolWindowPane/*!*/ EnsureReplWindow() {
            var compModel = IronRubyToolsPackage.ComponentModel;
            var provider = compModel.GetExtensions<IReplWindowProvider>().First();

            var window = (VsReplWindow)provider.FindReplWindow(_replGuid);
            if (window == null) {
                var evaluator = new RemoteRubyVsEvaluator();
                evaluator.Initialize();
                window = (VsReplWindow)provider.CreateReplWindow(
                    evaluator, 
                    IronRubyToolsPackage.Instance.ContentType,
                    "IronRuby Interactive", 
                    typeof(RubyLanguageInfo).GUID,
                    _replGuid
                );
                window.UseSmartUpDown = IronRubyToolsPackage.Instance.OptionsPage.ReplSmartHistory;
            }
            return window;
        }

        internal static VsReplWindow TryGetReplWindow() {
            var compModel = IronRubyToolsPackage.ComponentModel;
            var provider = compModel.GetExtensions<IReplWindowProvider>().First();

            return (VsReplWindow)provider.FindReplWindow(_replGuid);
        }
        
        public override EventHandler BeforeQueryStatus {
            get {
                return QueryStatusMethod;
            }
        }

        private void QueryStatusMethod(object sender, EventArgs args) {
            var oleMenu = sender as OleMenuCommand;

            IWpfTextView textView;
            var rbProj = CommonPackage.GetStartupProject() as RubyProjectNode;
            if (rbProj != null) {
                // startup project, enabled in Start in REPL mode.
                oleMenu.Visible = true;
                oleMenu.Enabled = true;
                oleMenu.Supported = true;
                oleMenu.Text = "Execute Project in IronRuby Interactive";
            } else if ((textView = CommonPackage.GetActiveTextView()) != null &&
                textView.TextBuffer.ContentType == IronRubyToolsPackage.Instance.ContentType) {
                // enabled in Execute File mode...
                oleMenu.Visible = true;
                oleMenu.Enabled = true;
                oleMenu.Supported = true;
                oleMenu.Text = "Execute File in IronRuby Interactive";
            } else {
                oleMenu.Visible = false;
                oleMenu.Enabled = false;
                oleMenu.Supported = false;
            }
        }

        public override void DoCommand(object sender, EventArgs args) {
            var window = (IReplWindow)EnsureReplWindow();
            IVsWindowFrame windowFrame = (IVsWindowFrame)((ToolWindowPane)window).Frame;

            string filename, dir;
            if (!CommonPackage.TryGetStartupFileAndDirectory(out filename, out dir)) {
                // TODO: Error reporting
                return;
            }

            ErrorHandler.ThrowOnFailure(windowFrame.Show());
            window.Focus();

            ((RemoteRubyEvaluator)window.Evaluator).Reset();

            window.WriteLine(String.Format("Running {0}", filename));
            string scopeName = Path.GetFileNameWithoutExtension(filename);
            // now execute the current file in the REPL
            var engine = ((RemoteRubyEvaluator)window.Evaluator).Engine;
            ThreadPool.QueueUserWorkItem(
                _ => {
                    try {
                        var src = engine.CreateScriptSourceFromFile(filename, StringUtils.DefaultEncoding, Scripting.SourceCodeKind.Statements);
                        src.Compile().Execute(((RemoteRubyEvaluator)window.Evaluator).CurrentScope);
                    } catch (Exception e) {
                        window.WriteLine(String.Format("Exception: {0}", e));
                    }
                }
            );
        }

        private RubyProjectNode GetRubyStartupProject() {
            var buildMgr = (IVsSolutionBuildManager)Package.GetGlobalService(typeof(IVsSolutionBuildManager));
            IVsHierarchy hierarchy;
            if (ErrorHandler.Succeeded(buildMgr.get_StartupProject(out hierarchy)) && hierarchy != null) {
                return hierarchy.GetProject().GetRubyProject();
            }
            return null;
        }

        public override int CommandId {
            get { return (int)PkgCmdIDList.cmdidExecuteFileInRepl; }
        }
    }
}
