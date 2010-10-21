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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Reflection;

namespace Microsoft.IronStudio.Project {
    class CommonOutputGroup : OutputGroup {
        public CommonOutputGroup(string outputName, string msBuildTargetName, ProjectNode projectManager, ProjectConfig configuration)
            : base(outputName, msBuildTargetName, projectManager, configuration) {
        }

        public override int get_KeyOutput(out string pbstrCanonicalName) {
            pbstrCanonicalName = "Foo";
            return VSConstants.S_OK;
        }

        public override int get_KeyOutputObject(out IVsOutput2 ppKeyOutput) {
            ppKeyOutput = new DummyOutput();
            return VSConstants.S_OK;
        }

        class DummyOutput : IVsOutput2 {
            #region IVsOutput2 Members

            public int get_CanonicalName(out string pbstrCanonicalName) {
                pbstrCanonicalName = null;
                return VSConstants.E_FAIL;
            }

            public int get_DeploySourceURL(out string pbstrDeploySourceURL) {
                pbstrDeploySourceURL = null;
                return VSConstants.E_FAIL;
            }

            public int get_DisplayName(out string pbstrDisplayName) {
                pbstrDisplayName = null;
                return VSConstants.E_FAIL;
            }

            public int get_Property(string szProperty, out object pvar) {
                if (szProperty == "FinalOutputPath") {
                    pvar = Assembly.GetExecutingAssembly().CodeBase;
                    return VSConstants.S_OK;
                }
                pvar = null;
                return VSConstants.E_FAIL;
            }

            public int get_RootRelativeURL(out string pbstrRelativePath) {
                pbstrRelativePath = null;
                return VSConstants.E_FAIL;
            }

            public int get_Type(out Guid pguidType) {
                pguidType = Guid.Empty;
                return VSConstants.E_FAIL;
            }

            #endregion
        }
    }
}
