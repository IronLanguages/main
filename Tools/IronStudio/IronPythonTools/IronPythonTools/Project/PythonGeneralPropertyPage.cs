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
using System.Windows.Forms;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Project;

namespace Microsoft.IronPythonTools.Project {
    [Guid(PythonConstants.GeneralPropertyPageGuid)]
    public class PythonGeneralPropertyPage : CommonPropertyPage {
        private readonly PythonGeneralyPropertyPageControl _control;
        public PythonGeneralPropertyPage() {
            _control = new PythonGeneralyPropertyPageControl(this);
        }

        public override Control Control {
            get {
                return _control;
            }
        }

        public override string Name {
            get { return "General"; }
        }

        public override void Apply() {
            ProjectMgr.SetProjectProperty(CommonConstants.StartupFile, _control.StartupFile);
            ProjectMgr.SetProjectProperty(CommonConstants.SearchPath, _control.SearchPaths);
            ProjectMgr.SetProjectProperty(CommonConstants.InterpreterPath, _control.InterpreterPath);
            ProjectMgr.SetProjectProperty(CommonConstants.WorkingDirectory, _control.WorkingDirectory);
            ProjectMgr.SetProjectProperty(CommonConstants.CommandLineArguments, _control.Arguments);
            ProjectMgr.SetProjectProperty(CommonConstants.IsWindowsApplication, _control.IsWindowsApplication.ToString());
            ProjectMgr.SetProjectProperty(PythonConstants.DebugStandardLibrary, _control.DebugStandardLibrary.ToString());
            IsDirty = false;
        }

        public override void LoadSettings() {
            _control.StartupFile = this.ProjectMgr.GetProjectProperty(CommonConstants.StartupFile, false);
            _control.SearchPaths = this.ProjectMgr.GetProjectProperty(CommonConstants.SearchPath, false);
            _control.InterpreterPath = this.ProjectMgr.GetProjectProperty(CommonConstants.InterpreterPath, false);
            _control.WorkingDirectory = this.ProjectMgr.GetProjectProperty(CommonConstants.WorkingDirectory, false);
            if (string.IsNullOrEmpty(_control.WorkingDirectory)) {
                _control.WorkingDirectory = ".";
            }
            _control.Arguments = this.ProjectMgr.GetProjectProperty(CommonConstants.CommandLineArguments, false);
            _control.IsWindowsApplication = Convert.ToBoolean(this.ProjectMgr.GetProjectProperty(CommonConstants.IsWindowsApplication, false));
            _control.DebugStandardLibrary = Convert.ToBoolean(this.ProjectMgr.GetProjectProperty(PythonConstants.DebugStandardLibrary, false));
            IsDirty = false;
        }
    }
}
