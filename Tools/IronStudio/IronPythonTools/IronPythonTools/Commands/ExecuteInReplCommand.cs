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
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.IronPythonTools.Library.Repl;
using Microsoft.IronPythonTools.Navigation;
using Microsoft.IronPythonTools.Project;
using Microsoft.IronPythonTools.Repl;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Repl;
using Microsoft.Scripting.Utils;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronPythonTools.Commands {
    /// <summary>
    /// Provides the command for starting a file or the start item of a project in the REPL window.
    /// </summary>
    internal sealed class ExecuteInReplCommand : Command {
        public static ExecuteInReplCommand Instance;
        private static Guid _replGuid = new Guid("{FAEC7F47-85D8-4899-8D7B-0B855B732CC8}");

        public ExecuteInReplCommand() {
            Instance = this;
        }

        internal static VsReplWindow/*!*/ EnsureReplWindow() {
            var compModel = IronPythonToolsPackage.ComponentModel;
            var provider = compModel.GetExtensions<IReplWindowProvider>().First();

            var window = (VsReplWindow)provider.FindReplWindow(_replGuid);
            if (window == null) {
                var evaluator = new RemotePythonVsEvaluator();
                evaluator.Initialize();
                window = (VsReplWindow)provider.CreateReplWindow(
                    evaluator,
                    IronPythonToolsPackage.Instance.ContentType, 
                    "IronPython Interactive", 
                    typeof(PythonLanguageInfo).GUID,
                    _replGuid
                );

                window.UseSmartUpDown = IronPythonToolsPackage.Instance.OptionsPage.ReplSmartHistory;
            }
            return window;
        }

        internal static VsReplWindow TryGetReplWindow() {
            var compModel = IronPythonToolsPackage.ComponentModel;
            var provider = compModel.GetExtensions<IReplWindowProvider>();

            return provider.First().FindReplWindow(_replGuid) as VsReplWindow;
        }
        
        public override EventHandler BeforeQueryStatus {
            get {
                return QueryStatusMethod;
            }
        }

        private void QueryStatusMethod(object sender, EventArgs args) {
            var oleMenu = sender as OleMenuCommand;

            IWpfTextView textView;
            var pyProj = CommonPackage.GetStartupProject() as PythonProjectNode;
            if (pyProj != null) {
                // startup project, enabled in Start in REPL mode.
                oleMenu.Visible = true;
                oleMenu.Enabled = true;
                oleMenu.Supported = true;
                oleMenu.Text = "Execute Project in IronPython Interactive";
            } else if ((textView = CommonPackage.GetActiveTextView()) != null &&
                textView.TextBuffer.ContentType == IronPythonToolsPackage.Instance.ContentType) {
                // enabled in Execute File mode...
                oleMenu.Visible = true;
                oleMenu.Enabled = true;
                oleMenu.Supported = true;
                oleMenu.Text = "Execute File in IronPython Interactive";
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

            ((RemotePythonVsEvaluator)window.Evaluator).Reset();

            ErrorHandler.ThrowOnFailure(windowFrame.Show());
            window.Focus();

            window.WriteLine(String.Format("Running {0}", filename));
            string scopeName = Path.GetFileNameWithoutExtension(filename);
            // now execute the current file in the REPL
            var engine = ((RemotePythonEvaluator)window.Evaluator).Engine;
            ThreadPool.QueueUserWorkItem(
                _ => {
                    try {
                        var src = engine.CreateScriptSourceFromFile(filename, StringUtils.DefaultEncoding, Scripting.SourceCodeKind.Statements);
                        src.Compile().Execute(((RemotePythonEvaluator)window.Evaluator).CurrentScope);
                    } catch (Exception e) {
                        window.WriteLine(String.Format("Exception: {0}", e));
                    }
                }
            );
        }
        
        public override int CommandId {
            get { return (int)PkgCmdIDList.cmdidExecuteFileInRepl; }
        }
    }
}
