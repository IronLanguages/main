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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Resources;
using System.Windows;
using System.Windows.Browser;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// Interface for a browser virtual filesystem, as is expected
    /// by DLR-languages that run in Silverlight.
    /// </summary>
    public abstract class BrowserVirtualFilesystem {
        
        public abstract string Name();
        public object CurrentStorageUnit { get; set; }
        protected abstract Stream GetFileInternal(object storageUnit, Uri relativeUri);
        
        public void UsingStorageUnit(object storageUnit, Action action) {
            var origStorageUnit = CurrentStorageUnit;
            CurrentStorageUnit = storageUnit;
            action.Invoke();
            CurrentStorageUnit = origStorageUnit;
        }

        public Stream GetFile(string relativePath) {
            return GetFile(CurrentStorageUnit, relativePath);
        }

        public Stream GetFile(Uri relativePath) {
            return GetFile(CurrentStorageUnit, relativePath);
        }

        public Stream GetFile(object storageUnit, string relativePath) {
            return GetFileInternal(storageUnit, relativePath);
        }

        public Stream GetFile(object storageUnit, Uri relativeUri) {
            return GetFileInternal(storageUnit, relativeUri);
        }

        public string GetFileContents(string relativeUri) {
            return GetFileContents(CurrentStorageUnit, relativeUri);
        }

        public string GetFileContents(Uri relativeUri) {
            return GetFileContents(CurrentStorageUnit, relativeUri);
        }

        public string GetFileContents(object storageUnit, string relativePath) {
            return GetFileContents(storageUnit, new Uri(NormalizePath(relativePath), UriKind.Relative));
        }

        public string GetFileContents(object storageUnit, Uri relativeUri) {
            Stream stream = GetFile(storageUnit, relativeUri);
            if (stream == null) {
                return null;
            }

            string result;
            using (StreamReader sr = new StreamReader(stream)) {
                result = sr.ReadToEnd();
            }
            return result;
        }

        public virtual string NormalizePath(string path) {
            return path.Replace('\\', '/');
        }

        protected Stream GetFileInternal(object storageUnit, string relativePath) {
            return GetFileInternal(storageUnit, new Uri(NormalizePath(relativePath), UriKind.Relative));
        }
    }

    /// <summary>
    /// Access the XAP file contents
    /// </summary>
    public class XapVirtualFilesystem : BrowserVirtualFilesystem {

        public override string Name() { return "XAP file"; }

        public override string NormalizePath(string path) {
            var normPath = base.NormalizePath(path);

            // Application.GetResource doesn't like paths that start with ./ 
            // BUG: try to get this fixed in SL
            if (normPath.StartsWith("./")) {
                normPath = normPath.Substring(2);
            }

            return normPath;
        }

        protected override Stream GetFileInternal(object xap, Uri relativeUri) {
            StreamResourceInfo sri = null;
            if (xap == null) {
                sri = Application.GetResourceStream(relativeUri);
            } else {
                sri = Application.GetResourceStream((StreamResourceInfo) xap, relativeUri);
            }
            return (sri != null) ? sri.Stream : null;
        }

        #region Depricated Methods
        [Obsolete("Use DynamicApplication.Current.Engine.GetEntryPointContents() instead")]
        public string GetEntryPointContents() {
            return DynamicApplication.Current.Engine.GetEntryPointContents();
        }

        [Obsolete("Use DynamicApplication.Current.AppManifest.AssemblyParts() instead")]
        public List<Assembly> GetManifestAssemblies() {
            return DynamicApplication.Current.AppManifest.AssemblyParts();
        }
        #endregion
    }

    /// <summary>
    /// Download files synchronously 
    /// </summary>
    public class HttpVirtualFilesystem : BrowserVirtualFilesystem {
        public override string Name() { return "Web server"; }

        private Dictionary<Uri, string> _cache = new Dictionary<Uri, string>();

        protected override Stream GetFileInternal(object baseUri, Uri relativeUri) {
            baseUri = baseUri ?? DefaultBaseUri();
            var fullUri = new Uri(NormalizePath(((Uri)baseUri).AbsoluteUri) + relativeUri.ToString(), UriKind.Absolute);
            string content;

            if (_cache.ContainsKey(fullUri)) {
                content = _cache[fullUri];
                if (content == null) return null;
                return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            }

            var request = HtmlPage.Window.CreateInstance("XMLHttpRequest");
            request.Invoke("open", "GET", fullUri.AbsoluteUri, false);
            request.Invoke("send", "");

            if (request.GetProperty("status").ToString() != "200") {
                _cache[fullUri] = null;
                return null;
            }

            content = request.GetProperty("responseText").ToString();
            _cache[fullUri] = content;
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        }

        private Uri DefaultBaseUri() {
            var uri = Application.Current.Host.Source;
            var server = uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            var path = NormalizePath(Path.GetDirectoryName(uri.LocalPath));
            var defaultBaseUri = new Uri(new Uri(server), path);
            if (Settings.DownloadScripts)
                defaultBaseUri = new Uri(NormalizePath(Path.Combine(defaultBaseUri.AbsoluteUri, Settings.DownloadScriptsFrom)));
            return defaultBaseUri;
        }

        internal void ClearCache() {
            _cache = null;
            _cache = new Dictionary<Uri, string>();
        }
    }

    /// <summary>
    /// Read and write files from Isolated Storage
    /// </summary>
    public class IsolatedStorageVirtualFilesystem : BrowserVirtualFilesystem {
        public override string Name() { return "Isolated Storage"; }

        protected override Stream GetFileInternal(object baseUri, Uri relativeUri) {
            throw new NotImplementedException();
        }
    }
}
