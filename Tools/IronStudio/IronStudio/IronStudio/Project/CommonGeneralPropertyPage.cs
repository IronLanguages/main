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
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.IronStudio.Project {
    [ComVisible(true)]
    public abstract class CommonGeneralPropertyPage : SettingsPage, EnvDTE80.IInternalExtenderProvider {
        private string _startupFile;
        private string _searchPath, _interpreterPath;
        private string _workingDirectory;
        private string _commandLineArguments;
        private bool _isWindowsApplication;

        public CommonGeneralPropertyPage() {
            this.Name = DynamicProjectSR.GetString(DynamicProjectSR.GeneralCaption);
        }

        #region overriden methods

        public override string GetClassName() {
            return this.GetType().FullName;
        }

        protected override void BindProperties() {
            if (this.ProjectMgr == null) {
                Debug.Assert(false);
                return;
            }
            _startupFile = this.ProjectMgr.GetProjectProperty(CommonConstants.StartupFile, false);
            _searchPath = this.ProjectMgr.GetProjectProperty(CommonConstants.SearchPath, false);
            _interpreterPath = this.ProjectMgr.GetProjectProperty(CommonConstants.InterpreterPath, false);
            _workingDirectory = this.ProjectMgr.GetProjectProperty(CommonConstants.WorkingDirectory, false);
            //By default working directory is project directory
            if (string.IsNullOrEmpty(_workingDirectory)) {
                _workingDirectory = ".";
            }
            _commandLineArguments = this.ProjectMgr.GetProjectProperty(CommonConstants.CommandLineArguments, false);
            _isWindowsApplication = Convert.ToBoolean(this.ProjectMgr.GetProjectProperty(CommonConstants.IsWindowsApplication, false));
        }

        protected override int ApplyChanges() {
            if (this.ProjectMgr == null) {
                Debug.Assert(false);
                return VSConstants.E_INVALIDARG;
            }

            this.ProjectMgr.SetProjectProperty(CommonConstants.StartupFile, _startupFile);
            this.ProjectMgr.SetProjectProperty(CommonConstants.SearchPath, _searchPath);
            this.ProjectMgr.SetProjectProperty(CommonConstants.InterpreterPath, _interpreterPath);
            this.ProjectMgr.SetProjectProperty(CommonConstants.WorkingDirectory, _workingDirectory);
            this.ProjectMgr.SetProjectProperty(CommonConstants.CommandLineArguments, _commandLineArguments);
            this.ProjectMgr.SetProjectProperty(CommonConstants.IsWindowsApplication, _isWindowsApplication.ToString());
            this.IsDirty = false;

            return VSConstants.S_OK;
        }

        #endregion

        #region exposed properties

        [SRCategoryAttribute(DynamicProjectSR.Application)]
        [LocalizableDisplayName(DynamicProjectSR.StartupFile)]
        [SRDescriptionAttribute(DynamicProjectSR.StartupFileDescription)]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string StartupFile {
            get { return _startupFile; }
            set { _startupFile = value; this.IsDirty = true; }
        }

        [SRCategoryAttribute(DynamicProjectSR.Application)]
        [LocalizableDisplayName(DynamicProjectSR.SearchPaths)]
        [SRDescriptionAttribute(DynamicProjectSR.SearchPathsDescription)]
        public string SearchPath {
            get { return _searchPath; }
            set { _searchPath = value; this.IsDirty = true; }
        }

        [SRCategoryAttribute(DynamicProjectSR.Application)]
        [LocalizableDisplayName(DynamicProjectSR.InterpreterPath)]
        [SRDescriptionAttribute(DynamicProjectSR.InterpreterPathDescription)]
        public string InterpreterPath {
            get { return _interpreterPath; }
            set { _interpreterPath = value; this.IsDirty = true; }
        }

        [SRCategoryAttribute(DynamicProjectSR.Application)]
        [LocalizableDisplayName(DynamicProjectSR.WorkingDirectory)]
        [SRDescriptionAttribute(DynamicProjectSR.WorkingDirectoryDescription)]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string WorkingDirectory {
            get { return _workingDirectory; }
            set { _workingDirectory = value; this.IsDirty = true; }
        }

        [SRCategoryAttribute(DynamicProjectSR.Application)]
        [LocalizableDisplayName(DynamicProjectSR.CommandLineArguments)]
        [SRDescriptionAttribute(DynamicProjectSR.CommandLineArgumentsDescription)]
        public string CommandLineArguments {
            get { return _commandLineArguments; }
            set { _commandLineArguments = value; this.IsDirty = true; }
        }

        [SRCategoryAttribute(DynamicProjectSR.Project)]
        [LocalizableDisplayName(DynamicProjectSR.ProjectFile)]
        [SRDescriptionAttribute(DynamicProjectSR.ProjectFileDescription)]
        [AutomationBrowsable(false)]
        public string ProjectFile {
            get { return Path.GetFileName(this.ProjectMgr.ProjectFile); }
        }

        [SRCategoryAttribute(DynamicProjectSR.Project)]
        [LocalizableDisplayName(DynamicProjectSR.ProjectFolder)]
        [SRDescriptionAttribute(DynamicProjectSR.ProjectFolderDescription)]
        [AutomationBrowsable(false)]
        public string ProjectFolder {
            get { return Path.GetDirectoryName(this.ProjectMgr.ProjectFolder); }
        }

        [SRCategoryAttribute(DynamicProjectSR.Application)]
        [LocalizableDisplayName(DynamicProjectSR.IsWindowsApplication)]
        [SRDescriptionAttribute(DynamicProjectSR.IsWindowsApplicationDescription)]
        public bool IsWindowsApplication {
            get { return _isWindowsApplication; }
            set { _isWindowsApplication = value; this.IsDirty = true; }
        }


        #endregion

        #region IInternalExtenderProvider Members

        bool EnvDTE80.IInternalExtenderProvider.CanExtend(string extenderCATID, string extenderName, object extendeeObject) {
            IVsHierarchy outerHierarchy = HierarchyNode.GetOuterHierarchy(this.ProjectMgr);
            if (outerHierarchy is EnvDTE80.IInternalExtenderProvider)
                return ((EnvDTE80.IInternalExtenderProvider)outerHierarchy).CanExtend(extenderCATID, extenderName, extendeeObject);
            return false;
        }

        object EnvDTE80.IInternalExtenderProvider.GetExtender(string extenderCATID, string extenderName, object extendeeObject, EnvDTE.IExtenderSite extenderSite, int cookie) {
            IVsHierarchy outerHierarchy = HierarchyNode.GetOuterHierarchy(this.ProjectMgr);
            if (outerHierarchy is EnvDTE80.IInternalExtenderProvider)
                return ((EnvDTE80.IInternalExtenderProvider)outerHierarchy).GetExtender(extenderCATID, extenderName, extendeeObject, extenderSite, cookie);
            return null;
        }

        object EnvDTE80.IInternalExtenderProvider.GetExtenderNames(string extenderCATID, object extendeeObject) {
            IVsHierarchy outerHierarchy = HierarchyNode.GetOuterHierarchy(this.ProjectMgr);
            if (outerHierarchy is EnvDTE80.IInternalExtenderProvider)
                return ((EnvDTE80.IInternalExtenderProvider)outerHierarchy).GetExtenderNames(extenderCATID, extendeeObject);
            return null;
        }

        #endregion

        #region ExtenderSupport

        [Browsable(false)]
        [AutomationBrowsable(false)]
        public virtual string ExtenderCATID {
            get {
                Guid catid = this.ProjectMgr.ProjectMgr.GetCATIDForType(this.GetType());
                if (Guid.Empty.CompareTo(catid) == 0)
                    throw new NotImplementedException();
                return catid.ToString("B");
            }
        }

        [Browsable(false)]
        [AutomationBrowsable(false)]
        public object ExtenderNames {
            get {
                EnvDTE.ObjectExtenders extenderService = (EnvDTE.ObjectExtenders)this.ProjectMgr.GetService(typeof(EnvDTE.ObjectExtenders));
                return extenderService.GetExtenderNames(this.ExtenderCATID, this);
            }
        }
        public object get_Extender(string extenderName) {
            EnvDTE.ObjectExtenders extenderService = (EnvDTE.ObjectExtenders)this.ProjectMgr.GetService(typeof(EnvDTE.ObjectExtenders));
            return extenderService.GetExtender(this.ExtenderCATID, extenderName, this);
        }

        #endregion
    }
}
