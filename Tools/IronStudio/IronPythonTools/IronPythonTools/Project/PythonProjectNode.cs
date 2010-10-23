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
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.IronPythonTools.Designer;
using Microsoft.IronPythonTools.Navigation;
using Microsoft.IronStudio.Project;
using Microsoft.VisualStudio.Project;
using Microsoft.Windows.Design.Host;

namespace Microsoft.IronPythonTools.Project {
    [Guid(PythonConstants.ProjectNodeGuid)]
    public class PythonProjectNode : CommonProjectNode {
        private DesignerContext _designerContext;
        public PythonProjectNode(CommonProjectPackage package) 
            : base(package, Utilities.GetImageList(typeof(PythonProjectNode).Assembly.GetManifestResourceStream(PythonConstants.ProjectImageList))) { 
        }

        public override CommonFileNode CreateCodeFileNode(ProjectElement item) {
            return new PythonFileNode(this, item);
        }

        public override CommonFileNode CreateNonCodeFileNode(ProjectElement item) {
            return new PythonNonCodeFileNode(this, item);
        }

        public override Type GetProjectFactoryType() {
            return typeof(PythonProjectFactory);
        }

        public override string GetProjectName() {
            return "PythonProject";
        }

        public override string GetCodeFileExtension() {
            return PythonConstants.FileExtension;
        }

        public override string GetFormatList() {
            return String.Format(CultureInfo.CurrentCulture, ".py"/*Resources.ProjectFileExtensionFilter*/, "\0", "\0");
        }

        public override Type GetGeneralPropertyPageType() {
            return typeof(PythonGeneralPropertyPage);
        }

        public override Type GetEditorFactoryType() {
            return typeof(PythonEditorFactory);
        }

        public override Type GetLibraryManagerType() {
            return typeof(IPythonLibraryManager);
        }

        public override string GetProjectFileExtension() {
            return ".pyproj";
        }

        protected internal override FolderNode CreateFolderNode(string path, ProjectElement element) {
            return new PythonFolderNode(this, path, element);
        }

        protected override internal Microsoft.Windows.Design.Host.DesignerContext DesignerContext {
            get {
                if (_designerContext == null) {
                    _designerContext = new DesignerContext();
                    //Set the RuntimeNameProvider so the XAML designer will call it when items are added to
                    //a design surface. Since the provider does not depend on an item context, we provide it at 
                    //the project level.
                    // This is currently disabled because we don't successfully serialize to the remote domain
                    // and the default name provider seems to work fine.  Likely installing our assembly into
                    // the GAC or implementing an IsolationProvider would solve this.
                    //designerContext.RuntimeNameProvider = new PythonRuntimeNameProvider();
                }
                return _designerContext;
            }
        }

    }
}