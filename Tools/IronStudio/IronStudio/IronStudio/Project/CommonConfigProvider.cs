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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;

namespace Microsoft.IronStudio.Project {
    /// <summary>
    /// Enables the Any CPU Platform form name for Dynamic Projects.
    /// Hooks language specific project config.
    /// </summary>
    public class CommonConfigProvider : ConfigProvider {
        private CommonProjectNode _project;        

        #region ctors
        public CommonConfigProvider(CommonProjectNode project)
            : base(project) {
            _project = project;
        }
        #endregion

        #region overridden methods

        protected override ProjectConfig CreateProjectConfiguration(string configName) {
            return new CommonProjectConfig(_project, configName);
        }

        public override int GetPlatformNames(uint celt, string[] names, uint[] actual) {
            if (names != null) {
                names[0] = "Any CPU";
            }

            if (actual != null) {
                actual[0] = 1;
            }

            return VSConstants.S_OK;
        }

        public override int GetSupportedPlatformNames(uint celt, string[] names, uint[] actual) {
            if (names != null) {
                names[0] = "Any CPU";
            }

            if (actual != null) {
                actual[0] = 1;
            }

            return VSConstants.S_OK;
        }
        #endregion
    }
}
