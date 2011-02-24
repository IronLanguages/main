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
using System.ComponentModel.Composition;
using System.IO;
using IronPython.Hosting;
using Microsoft.IronStudio.Core;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;

namespace Microsoft.IronPythonTools {
    [Export(typeof(IPythonRuntimeHost))]
    public sealed class PythonRuntimeHost : IPythonRuntimeHost {
        private readonly IContentType _contentType;
        private readonly ScriptEngine _engine;
        private bool _enterOutliningOnOpen, _intersectMembers, _hideAdvancedMembers;

        [ImportingConstructor]
        internal PythonRuntimeHost(IContentTypeRegistryService/*!*/ contentTypeRegistryService, IFileExtensionRegistryService/*!*/ fileExtensionRegistryService) {
            _engine = Python.CreateEngine(new Dictionary<string, object> { { "NoAssemblyResolveHook", true } });
            _contentType = contentTypeRegistryService.GetContentType(PythonCoreConstants.ContentType);
            CoreUtils.RegisterExtensions(contentTypeRegistryService, fileExtensionRegistryService, _contentType, _engine.Setup.FileExtensions);   
        }

        public ScriptEngine ScriptEngine {
            get {
                return _engine;
            }
        }

        public IContentType ContentType {
            get { 
                return _contentType; 
            }
        }

        public bool EnterOutliningModeOnOpen {
            get {
                return _enterOutliningOnOpen;
            }
            set {
                _enterOutliningOnOpen = value;
            }
        }

        public bool IntersectMembers {
            get {
                return _intersectMembers;
            }
            set {
                _intersectMembers = value;
            }
        }

        public bool HideAdvancedMembers {
            get {
                return _hideAdvancedMembers;
            }
            set {
                _hideAdvancedMembers = value;
            }
        }

        internal static string GetPythonInstallDir() {
#if DEBUG
            string result = Environment.GetEnvironmentVariable("DLR_ROOT");
            if (result != null) {
                result = Path.Combine(result, @"Bin\Debug");
                if (PythonRuntimeHost.IronPythonExistsIn(result)) {
                    return result;
                }
            }
#endif
            
            using (var installPath = Registry.LocalMachine.OpenSubKey("SOFTWARE\\IronPython\\2.7\\InstallPath")) {
                if (installPath != null) {
                    var path = installPath.GetValue("") as string;
                    if (path != null) {
                        return path;
                    }
                }
            }

            var paths = Environment.GetEnvironmentVariable("PATH");
            if (paths != null) {
                foreach (string dir in paths.Split(Path.PathSeparator)) {
                    try {
                        if (IronPythonExistsIn(dir)) {
                            return dir;
                        }
                    } catch {
                        // ignore
                    }
                }
            }

            string extensionDir = Path.GetDirectoryName(typeof(PythonRuntimeHost).Assembly.GetFiles()[0].Name);
            if (PythonRuntimeHost.IronPythonExistsIn(extensionDir)) {
                return extensionDir;
            }

            return null;
        }

        private static bool IronPythonExistsIn(string/*!*/ dir) {
            return File.Exists(Path.Combine(dir, "ipy.exe"));
        }
    }
}
