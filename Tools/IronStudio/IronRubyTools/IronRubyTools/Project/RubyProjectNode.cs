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
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.IronRubyTools.Navigation;
using Microsoft.IronStudio.Project;
using Microsoft.VisualStudio.Project;

namespace Microsoft.IronRubyTools.Project {
    [Guid(RubyConstants.ProjectNodeGuid)]
    public class RubyProjectNode : DirectoryBasedProjectNode {
        public RubyProjectNode(RubyProjectPackage/*!*/ package)
            : base(package, Utilities.GetImageList(typeof(RubyProjectNode).Assembly.GetManifestResourceStream(RubyConstants.ProjectImageList))) { 
        }

        public override Type GetProjectFactoryType() {
            return typeof(RubyProjectFactory);
        }

        public override string GetProjectName() {
            return "RubyProject";
        }

        public override string GetCodeFileExtension() {
            return RubyConstants.FileExtension;
        }

        public override string GetFormatList() {
            return String.Format(CultureInfo.CurrentCulture, ".rb"/*Resources.ProjectFileExtensionFilter*/, "\0", "\0");
        }

        public override Type GetGeneralPropertyPageType() {
            return typeof(RubyGeneralPropertyPage);
        }

        public override Type GetEditorFactoryType() {
            return typeof(RubyEditorFactory);
        }

        public override Type GetLibraryManagerType() {
            return typeof(IRubyLibraryManager);
        }

        public override string GetProjectFileExtension() {
            return ".rbproj";
        }

        public override FolderNode CreateFolderNode(string absFolderPath) {
            return base.CreateFolderNode(absFolderPath);
        }
    }
}