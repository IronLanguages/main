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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.IronStudio.Utils;
using System.ComponentModel;
using System.Threading;

namespace Microsoft.IronRubyTools.Project {
    /// <summary>
    /// Creates Ruby Projects
    /// </summary>
    [Guid(RubyConstants.ProjectFactoryGuid)]
    public class RubyProjectFactory : ProjectFactory {

        public RubyProjectFactory(RubyProjectPackage/*!*/ package)
            : base(package) {
        }        
                
        protected override ProjectNode/*!*/ CreateProject() {
            RubyProjectNode project = new RubyProjectNode((RubyProjectPackage)Package);
            project.SetSite((IOleServiceProvider)((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider)));
            return project;
        }

        // filename: .rbproj file in temp
        // location: user selected target location of the project
        protected override void CreateProject(string fileName, string location, string name, uint flags, ref Guid projectGuid, out IntPtr project, out int canceled) {
            string tempDir = Path.GetDirectoryName(fileName);

            // TODO: gem checks should not need a script
            string script = Path.GetFullPath(Path.Combine(tempDir, "__Gems.rb"));
            bool isGemLoader = File.Exists(script);
            if (!isGemLoader) {
                script = Path.GetFullPath(Path.Combine(tempDir, "__TemplateScript.rb"));
                if (!File.Exists(script)) {
                    script = null;
                }
            }

            if (script != null) {
                IVsOutputWindowPane outputPane = RubyVsUtils.GetOrCreateOutputPane("Project", RubyConstants.ProjectOutputPaneGuid);
                if (outputPane != null) {
                    RubyVsUtils.ShowWindow(EnvDTE.Constants.vsWindowKindOutput);
                    outputPane.Activate();
                }

                if (!IronRubyToolsPackage.Instance.RequireIronRubyInstalled(allowCancel: true)) {
                    canceled = 1;
                    project = IntPtr.Zero;
                    return;
                }

                var dir = Path.GetFullPath(Path.Combine(location, ".."));
                var cancelled = new ManualResetEvent(false);

                ProgressForm progressForm = new ProgressForm(10);
                progressForm.Text = isGemLoader ? "Checking required Gems" : "Creating Web Application";
                progressForm.CancellationEvent = cancelled;

                bool success = false;

                progressForm.BackgroundWorker.DoWork += new DoWorkEventHandler((s, e) => {
                    success = RubyStarter.ExecuteScriptFile(script, dir, outputPane, progressForm.BackgroundWorker, cancelled);
                });

                progressForm.BackgroundWorker.RunWorkerAsync();
                progressForm.ShowDialog();

                File.Delete(script);

                if (success) {
                    if (!isGemLoader) {
                        try {
                            CopyFilesRecursive(tempDir, location);
                        } catch {
                            Rollback(location);
                            throw;
                        }
                    }
                } else {
                    Rollback(location);

                    if (progressForm.Cancelled) {
                        outputPane.OutputStringThreadSafe("Cancelled.");
                    } else {
                        MessageBox.Show(
                            "An error occured while creating project. See output window for details.",
                            "Error",
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error
                        );
                    }

                    canceled = 1;
                    project = IntPtr.Zero;
                    return;
                }
            }

            base.CreateProject(fileName, location, name, flags, ref projectGuid, out project, out canceled);
        }

        private static void Rollback(string/*!*/ targetDir) {
            Directory.Delete(targetDir, true);
        }

        private static void CopyFilesRecursive(string/*!*/ sourceDir, string/*!*/ targetDir) {
            foreach (string src in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories)) {
                Debug.Assert(src.StartsWith(sourceDir + "\\"));
                string dst = Path.Combine(targetDir, src.Substring(sourceDir.Length + 1));
                File.Copy(src, dst, true);
            }
        }
    }
}
