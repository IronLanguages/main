/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2
using Microsoft.Scripting.Utils;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// ScriptHost for use inside the browser.
    /// </summary>
    public sealed class BrowserScriptHost : ScriptHost {

        public BrowserScriptHost() {
        }

        public override PlatformAdaptationLayer/*!*/ PlatformAdaptationLayer {
            get {
                return BrowserPAL.PAL;
            }
        }
    }

    /// <summary>
    /// Base class for all browser-based PlatformAdaptationLayers. 
    /// Delegates compatible operations to a BrowserVirtualFileSystem.
    /// </summary>
    // BUG: should be internal, but Ruby is refusing to call members if so
    public abstract class BrowserPAL : PlatformAdaptationLayer {
        public BrowserVirtualFilesystem VirtualFilesystem { get; internal set; }

        private string _currentDirectory = "";

        protected static BrowserPAL _PAL;
        internal static BrowserPAL PAL {
            get {
                if (_PAL == null) PAL = HttpPAL.PAL;
                return _PAL;
            }
            set { _PAL = value; }
        }

        /// <summary>
        /// Get the virtual filesystem's current storage unit. It is "object"
        /// based since the CurrentStorageUnit can be different types.
        /// </summary>
        public object CurrentStorageUnit {
            get { return VirtualFilesystem.CurrentStorageUnit; }
            set { VirtualFilesystem.CurrentStorageUnit = value; }
        }

        public override string CurrentDirectory {
            get {
                return _currentDirectory;
            }
            set {
                _currentDirectory = value ?? "";
            }
        }

        /// <summary>
        /// Executes "action" in the context of the "storageUnit". 
        /// </summary>
        /// <param name="xapFile"></param>
        /// <param name="action"></param>
        public void UsingStorageUnit(object storageUnit, Action action) {
            VirtualFilesystem.UsingStorageUnit(storageUnit, action);
        }

        public override bool FileExists(string path) {
            return VirtualFilesystem.GetFile(path) != null;
        }

        public override Assembly LoadAssemblyFromPath(string path) {
            Stream stream = VirtualFilesystem.GetFile(path);
            if (stream == null) {
                throw new FileNotFoundException(
                    string.Format("could not find assembly: {0} (check the {1})", 
                        path, VirtualFilesystem.Name()
                    )
                );
            }
            return new AssemblyPart().Load(stream);
        }

        /// <exception cref="ArgumentException">Invalid path.</exception>
        public override string/*!*/ GetFullPath(string/*!*/ path) {
            return VirtualFilesystem.NormalizePath(path);
        }

        /// <exception cref="ArgumentException">Invalid path.</exception>
        public override bool IsAbsolutePath(string/*!*/ path) {
            return PathToUri(path).IsAbsoluteUri;
        }

        public override Stream OpenInputFileStream(string path) {
            Stream result = VirtualFilesystem.GetFile(GetFullPath(path));
            if (result == null)
                throw new IOException(
                    String.Format("file {0} not found (check the {1})", 
                        path, VirtualFilesystem.Name()
                    )
                );
            return result;
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share) {
            if (mode != FileMode.Open || access != FileAccess.Read) {
                throw new IOException(
                    string.Format("can only read files from the {0}",
                        VirtualFilesystem.Name()
                    )
                );
            }
            return OpenInputFileStream(path);
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) {
            return OpenInputFileStream(path, mode, access, share);
        }

        /// <summary>
        /// Convert a string path to a Uri
        /// </summary>
        /// <param name="path">relative or absolute path</param>
        /// <returns>normalized URI</returns>
        private Uri/*!*/ PathToUri(string/*!*/ path) {
            try {
                return new Uri(VirtualFilesystem.NormalizePath(path), UriKind.RelativeOrAbsolute);
            } catch (UriFormatException e) {
                throw new ArgumentException("The specified path is invalid", e);
            }
        }
    }

    /// <summary>
    /// PlatformAdaptationLayer for use with a read-only XAP file.
    /// </summary>
    internal sealed class XapPAL : BrowserPAL {
        internal static new readonly BrowserPAL PAL = new XapPAL();

        private XapPAL() {
            VirtualFilesystem = new XapVirtualFilesystem();
        }
    }

    /// <summary>
    /// PlatformAdaptationLayer to download and cache files over HTTP.
    /// </summary>
    internal sealed class HttpPAL : BrowserPAL {
        internal static new readonly BrowserPAL PAL = new HttpPAL();

        private HttpPAL() {
            VirtualFilesystem = new HttpVirtualFilesystem();
        }
    }
}

