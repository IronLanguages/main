/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.IO;
using System.Reflection;
using System.Dynamic;
using System.Windows;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// PlatformAdaptationLayer for use inside the browser
    /// Overrides FileExists to look in the XAP
    /// 
    /// NOTE: where possible, logic should be placed in the base class in 
    /// #if SILVERLIGHT code paths, rather than as overrides. That makes the
    /// behavior more sane if someone hosts the DLR in their own Silverlight
    /// application. (we had this problem with PlatformAdapationLayer.LoadAssembly)
    /// </summary>
    internal sealed class BrowserPAL : PlatformAdaptationLayer {
        internal static readonly BrowserPAL/*!*/ PAL = new BrowserPAL();
        
        public override bool FileExists(string path) {
            if (!DynamicApplication.InUIThread) {
                return false; // Application.GetResourceStream will throw if called from a non-UI thread
            }
            return Package.GetFile(path) != null;
        }

        public override Assembly LoadAssemblyFromPath(string path) {
            Stream stream = Package.GetFile(path);
            if (stream == null) {
                throw new FileNotFoundException("could not find assembly in XAP: " + path);
            }
            return new AssemblyPart().Load(stream);
        }

        /// <exception cref="ArgumentException">Invalid path.</exception>
        public override string/*!*/ GetFullPath(string/*!*/ path) {
            return Package.NormalizePath(path);
        }

        /// <exception cref="ArgumentException">Invalid path.</exception>
        public override bool IsAbsolutePath(string/*!*/ path) {
            return PathToUri(path).IsAbsoluteUri;
        }

        private Uri/*!*/ PathToUri(string/*!*/ path) {
            try {
                return new Uri(Package.NormalizePath(path), UriKind.RelativeOrAbsolute);
            } catch (UriFormatException e) {
                throw new ArgumentException("The specified path is invalid", e);
            }
        }

        public override Stream OpenInputFileStream(string path) {
            Stream result = Package.GetFile(GetFullPath(path));
            if (result == null)
                throw new IOException(String.Format("file {0} not found in XAP", path));
            return result;
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share) {
            if (mode != FileMode.Open || access != FileAccess.Read) {
                throw new IOException("can only read files from the XAP"); 
            }
            return OpenInputFileStream(path);
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) {
            return OpenInputFileStream(path, mode, access, share);
        }
    }
}
