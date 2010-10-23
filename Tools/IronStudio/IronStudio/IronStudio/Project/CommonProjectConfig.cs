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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.IronStudio.Project {    
    [CLSCompliant(false)]
    [ComVisible(true)]
    public class CommonProjectConfig : ProjectConfig {
        private readonly CommonProjectNode/*!*/ _project;

        public CommonProjectConfig(CommonProjectNode/*!*/ project, string configuration)
            : base(project, configuration) {
            _project = project;
        }

        public override int DebugLaunch(uint flags) {
            IStarter starter =  _project.Package.GetStarter();

            __VSDBGLAUNCHFLAGS launchFlags = (__VSDBGLAUNCHFLAGS)flags;
            if ((launchFlags & __VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug) == __VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug) {
                //Start project with no debugger
                starter.StartProject(_project, false);
            } else {
                //Start project with debugger 
                starter.StartProject(_project, true);
            }
            return VSConstants.S_OK;
        }

        protected override OutputGroup CreateOutputGroup(ProjectNode project, KeyValuePair<string, string> group) {
            return new CommonOutputGroup(group.Key, group.Value, project, this);
        }
    }
}
