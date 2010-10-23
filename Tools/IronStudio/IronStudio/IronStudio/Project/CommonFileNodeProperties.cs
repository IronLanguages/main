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
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;

namespace Microsoft.IronStudio.Project {
    [ComVisible(true), CLSCompliant(false)]
    [Guid(CommonConstants.FileNodePropertiesGuid)]
    public class CommonFileNodeProperties : SingleFileGeneratorNodeProperties {

        public CommonFileNodeProperties(HierarchyNode node)
            : base(node) { }        

        #region properties
        
        //Hide Build Action property from the property inspector
        [Browsable(false)]
        public override BuildAction BuildAction {
            get {
                return base.BuildAction;
            }
            set {
                base.BuildAction = value;
            }
        }

        //Hide Custom Tool property from the property inspector
        [Browsable(false)]
        public override string CustomTool {
            get {
                return base.CustomTool;
            }
            set {
                base.CustomTool = value;
            }
        }

        //Hide Custom Tool Namespace property from the property inspector
        [Browsable(false)]
        public override string CustomToolNamespace {
            get {
                return base.CustomToolNamespace;
            }
            set {
                base.CustomToolNamespace = value;
            }
        }        

        #endregion
    }    
}
