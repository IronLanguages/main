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
using System.IO;
using System.Threading;
using System.Windows.Threading;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Microsoft.IronStudio.Project {
    public abstract class DirectoryBasedProjectNode : CommonProjectNode {
        private Dispatcher _dispatcher;
        private FileSystemWatcher _projectWatcher;

        private static readonly TimeSpan _fileChangeThrottleInterval = TimeSpan.FromSeconds(0.2);
        private DispatcherTimer _timer = null;
        private readonly Dictionary<string, Action> _pendingFileUpdates = new Dictionary<string, Action>();

        public DirectoryBasedProjectNode(CommonProjectPackage package, ImageList imageList)
            : base(package, imageList) {
        }

        protected internal override void ProcessFiles() {
            //No files are registered in project file
        }

        protected internal override void ProcessFolders() {
            //No top-level folders in dynamic project
        }

        protected override void Reload() {
            base.Reload();

            CreateHierarchy(this, GetProjectHomeDir(), false);

            InitializeFileSystemWatcher();
        }

        internal void CreateHierarchy(HierarchyNode parent, string dir, bool isSearchPath) {
            string projFileExt = GetProjectFileExtension();
            if (Directory.Exists(dir)) {
                try {
                    foreach (string subfolder in Directory.GetDirectories(dir)) {
                        AddDirectory(parent, isSearchPath, subfolder);
                    }
                    foreach (string file in Directory.GetFiles(dir)) {
                        if (ShouldIncludeFileInProject(projFileExt, file)) {
                            AddFile(parent, isSearchPath, file);
                        }
                    }
                } catch (UnauthorizedAccessException) {
                }
            }
        }

        /// <summary>
        /// Adds a directory to the project hierarchy with the specified parent.
        /// </summary>
        protected void AddDirectory(HierarchyNode parent, bool isSearchPath, string subfolder) {
            var existing = parent.FindChild(Path.GetFileName(subfolder));
            if (existing == null) {
                FolderNode folderNode = CreateFolderNode(subfolder);
                parent.AddChild(folderNode);
                CreateHierarchy(folderNode, subfolder, isSearchPath);
            }
        }

        /// <summary>
        /// Adds a file to the project hierarchy with the specified parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="isSearchPath"></param>
        /// <param name="file"></param>
        protected void AddFile(HierarchyNode parent, bool isSearchPath, string file) {
            var existing = parent.FindChild(file);
            if (existing == null) {
                FileNode fileNode = CreateFileNode(file);
                //Files in search path are not considered memebers of the project itself
                fileNode.SetProperty((int)__VSHPROPID.VSHPROPID_IsNonMemberItem, isSearchPath);
                parent.AddChild(fileNode);
            }
        }

        /// <summary>
        /// Initializes the file system watcher so that we automatically track changes to the project system
        /// based upon changes to the underlying file system.
        /// </summary>
        private void InitializeFileSystemWatcher() {
            if (_projectWatcher != null) {
                _projectWatcher.Created -= new FileSystemEventHandler(FileCreated);
                _projectWatcher.Deleted -= new FileSystemEventHandler(FileDeleted);
            }

            if (_dispatcher == null) {
                _dispatcher = Dispatcher.CurrentDispatcher;
            }

            _projectWatcher = new FileSystemWatcher(ProjectDir);
            _projectWatcher.IncludeSubdirectories = true;
            _projectWatcher.SynchronizingObject = new SynchronizingInvoke(_dispatcher);

            _projectWatcher.Created += FileCreated;
            _projectWatcher.Deleted += FileDeleted;
            _projectWatcher.Renamed += FileRenamed;

            _projectWatcher.EnableRaisingEvents = true;
        }

        private Action CreateSafeCallback(FileSystemEventArgs notification, Action callback) {
            Predicate<string> exists = (s) => File.Exists(s) || Directory.Exists(s);

            switch (notification.ChangeType) {
                case WatcherChangeTypes.Created: // ignore rapid add/delete
                    return () => {
                        if (exists(notification.FullPath))
                            callback();
                    };
                case WatcherChangeTypes.Deleted: // ignore rapid delete/re-add
                case WatcherChangeTypes.Renamed: // ignore rapid rename/revert
                    return () => {
                        if (!exists(notification.FullPath))
                            callback();
                    };
                default:
                    throw new NotSupportedException(String.Format("CreateSafeCallback is not supported for change type {0}", notification.ChangeType));
            };
        }

        // Problem: Many applications (including visual studio itself) will save a file by deleting and re-creating it.
        // If we issue a Delete/Recreate, then source control providers will pend a delete on the file.
        // This is particularly nasty when merging changes to files just before a checkin, and can lead to accidentally pending
        // deletes for files you're trying to edit.
        // Solution: Throttle file system notifications, and only delete the file if it is actually gone some interval (say 0.2 seconds) later
        private void ThrottleFileUpdate(FileSystemEventArgs notification, Action callback) {
            _dispatcher.VerifyAccess();

            // we only worry about keeping the single last operation for any given file
            _pendingFileUpdates[notification.FullPath] = CreateSafeCallback(notification, callback);

            if(_timer != null) {
                _timer.Stop();
            }

            _timer = new DispatcherTimer(_fileChangeThrottleInterval, DispatcherPriority.Normal, (s, e) => {
                _dispatcher.VerifyAccess();
                _timer.Stop(); // don't repeat

                // flush all the pending updates.
                var copy = new List<Action>(_pendingFileUpdates.Values);
                _pendingFileUpdates.Clear();

                foreach (var t in copy) {
                    callback();
                }
            }, _dispatcher);
            _timer.Start();
        }

        private void FileDeleted(object sender, FileSystemEventArgs e) {
            Debug.Assert(e.ChangeType == WatcherChangeTypes.Deleted);
            ThrottleFileUpdate(e, () => {
                HierarchyNode child = FindChild(e.FullPath);
                if (child != null) {
                    // TODO: We shouldn't be closing any documents, we probably need to pass a flag in here.
                    // Unfortunately it's not really simple because when we remove the child from the parent
                    // the file is no longer savable if it's already open.  So for now the file just simply
                    // disappears - deleting it from the file system means you better want it gone from
                    // the editor as well.
                    child.Remove(false);
                }
            });
        }

        private void FileRenamed(object sender, RenamedEventArgs e) {
            Debug.Assert(e.ChangeType == WatcherChangeTypes.Renamed);

            ThrottleFileUpdate(e, () => {

                var child = FindChild(e.OldFullPath);
                if (child != null) {
                    FileNode fileNode = child as FileNode;
                    if (fileNode != null) {
                        // file nodes could be open so we'll need to update any
                        // state such as the file caption
                        try {
                            fileNode.RenameDocument(e.OldFullPath, e.FullPath);
                        } catch (Exception) {
                        }
                    }

                    FolderNode folderNode = child as FolderNode;
                    if (folderNode != null) {
                        try {
                            folderNode.RenameFolder(e.FullPath);
                        } catch (Exception) {
                        }
                    }

                    child.ItemNode.Rename(e.FullPath);
                    child.OnInvalidateItems(child.Parent);
                }
            });
        }

        private void FileCreated(object sender, FileSystemEventArgs e) {
            if (sender != _projectWatcher) {
                // we've switched directories, ignore the old lingering events.
                return;
            }

            Debug.Assert(e.ChangeType == WatcherChangeTypes.Created);

            ThrottleFileUpdate(e, () => {

                // find the parent where the new node will be inserted
                HierarchyNode parent;
                string path = e.FullPath;
                for (; ; ) {
                    string dir = Path.GetDirectoryName(path);
                    if (NativeMethods.IsSamePath(dir, ProjectDir)) {
                        parent = this;
                        break;
                    }

                    parent = FindChild(dir);
                    if (parent == null) {
                        path = dir;
                    } else {
                        break;
                    }
                }

                // and then insert either a file or directory node
                FileInfo fi = new FileInfo(e.FullPath);
                if ((fi.Attributes & FileAttributes.Directory) != 0) {
                    AddDirectory(parent, false, path);
                } else if (ShouldIncludeFileInProject(GetProjectFileExtension(), e.FullPath)) {
                    AddFile(parent, false, e.FullPath);
                }
            });
        }

        /// <summary>
        /// Checks if we should include the specified file in the project.
        /// 
        /// Currently we only exclude project files for the current project type.
        /// </summary>
        protected static bool ShouldIncludeFileInProject(string projFileExt, string file) {
            string extension = Path.GetExtension(file);
            return
                String.Compare(extension, projFileExt, StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(extension, ".sln", StringComparison.OrdinalIgnoreCase) != 0 &&
                (new FileInfo(file).Attributes & FileAttributes.Hidden) == 0;
        }

        /// <summary>
        /// Create a folder node based on absolute folder path.
        /// </summary>
        public new virtual FolderNode CreateFolderNode(string absFolderPath) {
            //This code builds folder node in such a way that it won't be added to the
            //project as build item and the project won't be go dirty.
            var prjItem = GetExistingItem(absFolderPath) ?? BuildProject.AddItem("None", absFolderPath)[0];
            ProjectElement prjElem = new ProjectElement(this, prjItem, false);
            return new CommonFolderNode(this, absFolderPath, prjElem);
        }

        /// <summary>
        /// Marshals any events fired by the file system watcher back onto the UI thread
        /// where we can safely process the event.
        /// </summary>
        class SynchronizingInvoke : ISynchronizeInvoke {
            private readonly Dispatcher _dispatcher;

            public SynchronizingInvoke(Dispatcher dispatcher) {
                _dispatcher = dispatcher;
            }

            #region ISynchronizeInvoke Members

            public IAsyncResult BeginInvoke(Delegate method, object[] args) {
                _dispatcher.BeginInvoke(method, args);
                return null;
            }

            public object EndInvoke(IAsyncResult result) {
                throw new NotImplementedException();
            }

            public object Invoke(Delegate method, object[] args) {
                throw new NotImplementedException();
            }

            public bool InvokeRequired {
                get { return Thread.CurrentThread != _dispatcher.Thread; }
            }

            #endregion
        }
    }
}
