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
using System.IO;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.IronStudio {
    public static class CommonUtils {
        private static readonly bool isProductionEnvironment = Environment.GetEnvironmentVariable("DLR_ROOT") == null;
        /// <summary>
        /// Ensures that the caller is only used for testing purposes
        /// </summary>
        public static void AssertIsTestOnlyCode() {
            if (isProductionEnvironment) {
                throw new InvalidOperationException("Test-only code called in production environment");
            }
        }

        public static bool IsRecognizedFile(string filename, ScriptEngine engine) {
            string extension = Path.GetExtension(filename);
            foreach (string regext in engine.Setup.FileExtensions) {
                if (string.Compare(extension, regext, StringComparison.OrdinalIgnoreCase) == 0) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns absolute path of a given directory, always
        /// ending with '/'.
        /// </summary>
        public static string NormalizeDirectoryPath(string path) {
            string absPath = new Uri(path).LocalPath;
            if (!absPath.EndsWith("\\")) {
                absPath += "\\";
            }
            return absPath;
        }

        /// <summary>
        /// Return true if both paths represent the same directory.
        /// </summary>
        public static bool AreTheSameDirectories(string path1, string path2) {
            if (path1 == null || path2 == null) {
                return false;
            }
            return NormalizeDirectoryPath(path1) == NormalizeDirectoryPath(path2);
        }

        /// <summary>
        /// Tries to create friendly directory path: '.' if the same as base path,
        /// relative path if short, absolute path otherwise.
        /// </summary>
        public static string CreateFriendlyPath(string basePath, string path) {
            string normalizedBaseDir = NormalizeDirectoryPath(basePath);
            string normalizedDir = NormalizeDirectoryPath(path);
            return normalizedBaseDir == normalizedDir ? " . " :
                new DirectoryInfo(normalizedDir).Name;
        }

        /// <summary>
        /// Returns startup project in the currently open solution (if any).
        /// </summary>
        public static ProjectNode GetStartupProject(IServiceProvider provider) {
            var solutionManager = (IVsSolutionBuildManager)provider.GetService(
                typeof(IVsSolutionBuildManager));
            if (solutionManager == null) {
                //Cannot get solution manager, assume no solution is open
                return null;
            }
            IVsHierarchy startupPrj;
            int hr = solutionManager.get_StartupProject(out startupPrj);
            if (ErrorHandler.Failed(hr) || startupPrj == null) {
                return null;
            }
            object project;
            hr = startupPrj.GetProperty(VSConstants.VSITEMID_ROOT,
                (int)__VSHPROPID.VSHPROPID_ExtObject, out project);
            if (ErrorHandler.Failed(hr)) {
                //Cannot get project object
                return null;
            }
            OAProject oaProject = project as OAProject;
            if (oaProject == null) {
                //Unrecognized startup project type
                return null;
            }
            return oaProject.Project;
        }

        /// <summary>
        /// Returns true is the solution is empty - used to detect the ad-hoc mode.
        /// </summary>
        public static bool IsSolutionEmpty() {
            IVsSolution solution = (IVsSolution)Package.GetGlobalService(typeof(IVsSolution));
            object projectCount = 0;
            solution.GetProperty((int)__VSPROPID.VSPROPID_ProjectCount, out projectCount);
            return (int)projectCount == 0;
        }
    }
}
