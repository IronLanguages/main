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
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Resources;
using System.Windows;
using System.Windows.Browser;
using System.Net;
using System.Windows.Threading;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// Interface for a browser virtual filesystem, as is expected
    /// by DLR-languages that run in Silverlight.
    /// </summary>
    public abstract class BrowserVirtualFilesystem {
        
        /// <summary>
        /// Defines the name of this filesystem.
        /// </summary>
        public abstract string Name();

        /// <summary>
        /// The current "storage-unit", which is left up to the concrete 
        /// classes to decide it's meaning. For a XAP file, the storage-unit
        /// is which XAP file to get files out of. For a web-server, it's
        /// an absolute URI. Basically, it maps to a current-drive for a 
        /// traditional filesystem.
        /// </summary>
        public object CurrentStorageUnit { get; set; }
        
        /// <summary>
        /// Switches the storage unit and executes the given delegate in that 
        /// context.
        /// </summary>
        /// <param name="storageUnit">the storage unit to switch to</param>
        /// <param name="action">delegate to run in the context of the storage unit</param>
        public void UsingStorageUnit(object storageUnit, Action action) {
            var origStorageUnit = CurrentStorageUnit;
            CurrentStorageUnit = storageUnit;
            action.Invoke();
            CurrentStorageUnit = origStorageUnit;
        }

        /// <summary>
        /// Get a file based on a relative path, in the current storage unit
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns>the stream representing the file</returns>
        public Stream GetFile(string relativePath) {
            return GetFile(CurrentStorageUnit, relativePath);
        }

        /// <summary>
        /// Get a file based on a relative Uri, in the current storage unit
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns>the stream representing the file</returns>
        public Stream GetFile(Uri relativePath) {
            return GetFile(CurrentStorageUnit, relativePath);
        }

        /// <summary>
        /// Get a file based on a relative path, in the given storage unit
        /// </summary>
        /// <param name="storageUnit">Looks for the file in this</param>
        /// <param name="relativePath"></param>
        /// <returns>the stream representing the file</returns>
        public Stream GetFile(object storageUnit, string relativePath) {
            return GetFileInternal(storageUnit, relativePath);
        }

        /// <summary>
        /// Get a file based on a relative Uri, in the given storage unit
        /// </summary>
        /// <param name="storageUnit">Looks for the file in this</param>
        /// <param name="relativeUri"></param>
        /// <returns>the stream representing the file</returns>
        public Stream GetFile(object storageUnit, Uri relativeUri) {
            return GetFileInternal(storageUnit, relativeUri);
        }

        /// <summary>
        /// Get a file's contents based on a relative path, in the current
        /// storage-unit.
        /// </summary>
        /// <param name="relativeUri"></param>
        /// <returns>The file's contents as a string</returns>
        public string GetFileContents(string relativeUri) {
            return GetFileContents(CurrentStorageUnit, relativeUri);
        }

        /// <summary>
        /// Get a file's contents based on a relative Uri, in the current
        /// storage-unit.
        /// </summary>
        /// <param name="relativeUri"></param>
        /// <returns>The file's contents as a string</returns>
        public string GetFileContents(Uri relativeUri) {
            return GetFileContents(CurrentStorageUnit, relativeUri);
        }

        /// <summary>
        /// Get a file's contents based on a relative path, in the given
        /// storage-unit.
        /// </summary>
        /// <param name="storageUnit">Looks for the file in this</param>
        /// <param name="relativePath"></param>
        /// <returns>The file's contents as a string</returns>
        public string GetFileContents(object storageUnit, string relativePath) {
            return GetFileContents(storageUnit, new Uri(NormalizePath(relativePath), UriKind.Relative));
        }

        /// <summary>
        /// Get a file's contents based on a relative Uri, in the given
        /// storage-unit.
        /// </summary>
        /// <param name="storageUnit">Looks for the file in this</param>
        /// <param name="relativeUri"></param>
        /// <returns>The file's contents as a string</returns>
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

        /// <summary>
        /// Normalizes a path, which in general means making sure the directory
        /// separators are forward-slashes.
        /// </summary>
        /// <param name="path">a string representing a path</param>
        /// <returns>a normalized version of "path"</returns>
        public static string Normalize(string path) {
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// See (static) BrowserVirtualFilesystem.Normalize
        /// </summary>
        public virtual string NormalizePath(string path) {
            return BrowserVirtualFilesystem.Normalize(path);
        }

        /// <summary>
        /// Gets a file's stream
        /// </summary>
        /// <param name="storageUnit">Looks for the file in this</param>
        /// <param name="relativePath">path of the file</param>
        /// <returns>a Stream for the file's contents</returns>
        protected Stream GetFileInternal(object storageUnit, string relativePath) {
            return GetFileInternal(storageUnit, new Uri(NormalizePath(relativePath), UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Defines how the specific virtual file-system gets a file.
        /// </summary>
        /// <param name="storageUnit"></param>
        /// <param name="relativeUri"></param>
        /// <returns></returns>
        protected abstract Stream GetFileInternal(object storageUnit, Uri relativeUri);
    }

    /// <summary>
    /// Access the XAP file contents
    /// </summary>
    public class XapVirtualFilesystem : BrowserVirtualFilesystem {

        public override string Name() { return "XAP file"; }

        /// <summary>
        /// Normalizes the path by making sure all directory separators are 
        /// forward slashes, and makes sure no path starts with "./", as the
        /// root of the XAP file is an empty string.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public override string NormalizePath(string path) {
            var normPath = base.NormalizePath(path);

            // Application.GetResource doesn't like paths that start with ./ 
            // BUG: try to get this fixed in SL
            if (normPath.StartsWith("./")) {
                normPath = normPath.Substring(2);
            }

            return normPath;
        }

        /// <summary>
        /// Gets a Stream for a file from the given "xap" file.
        /// </summary>
        /// <param name="xap">a StreamResourceInfo representing a XAP file</param>
        /// <param name="relativeUri">a string respresenting a relative URI</param>
        /// <returns>a Stream for the file, or null if it did not find the file</returns>
        protected override Stream GetFileInternal(object xap, Uri relativeUri) {
            relativeUri = new Uri(NormalizePath(relativeUri.ToString()), UriKind.Relative);
            StreamResourceInfo sri = null;

            // TODO: Application.GetResourceStream always returns null when called from
            // a non-UI thread. Should dispatch to the UI thread but block this thread until
            // the call completes.
            if (xap == null) {
                sri = Application.GetResourceStream(relativeUri);
            } else {
                sri = Application.GetResourceStream((StreamResourceInfo) xap, relativeUri);
            }
            return sri == null ? 
                DynamicApplication.GetManifestResourceStream(relativeUri.ToString()) :
                sri.Stream;
        }

        #region Depricated Methods
        /// <summary>
        /// Get the contents of the entry-point script as a string
        /// </summary>
        [Obsolete("This method will be unavaliable in the next version")]
        public string GetEntryPointContents() {
            return BrowserPAL.PAL.VirtualFilesystem.GetFileContents(Settings.EntryPoint);
        }

        /// <summary>
        /// Get a list of the assemblies defined in the AppManifest
        /// </summary>
        [Obsolete("Use DynamicApplication.Current.AppManifest.AssemblyParts() instead")]
        public List<Assembly> GetManifestAssemblies() {
            return DynamicApplication.Current.AppManifest.AssemblyParts();
        }
        #endregion
    }

    /// <summary>
    /// Download and cache files over HTTP
    /// </summary>
    public class HttpVirtualFilesystem : BrowserVirtualFilesystem {

        public override string Name() { return "Web server"; }

        /// <summary>
        /// The cache of files already downloaded
        /// </summary>
        private DownloadCache _cache = new DownloadCache();

        /// <summary>
        /// Gets a file out of the download cache. This does not download the
        /// file if it is not in the cache; use "DownloadAndCache" before
        /// using this method download the file and cache it.
        /// </summary>
        /// <param name="baseUri">
        /// URI to base relative URI's off of. If null, it defaults to the HTML
        /// page's URI.
        /// </param>
        /// <param name="relativeUri">URI relative to the base URI</param>
        /// <returns>A stream for the URI</returns>
        protected override Stream GetFileInternal(object baseUri, Uri relativeUri) {

            if (!relativeUri.IsAbsoluteUri) {

                // TODO: HttpVirtualFilesystem shouldn't manage all these checks,
                // needs to be re-organized.

                // check in the XAP first
                var stream = XapPAL.PAL.VirtualFilesystem.GetFile(relativeUri);
                if (stream != null) return stream;

                // also check any ZIP files
                if (HtmlPage.IsEnabled && (DynamicApplication.Current != null && DynamicApplication.Current.ScriptTags != null)) {
                    foreach (var zip in DynamicApplication.Current.ScriptTags.ZipPackages) {
                        var relUriStr = relativeUri.ToString();
                        var dirname = Path.GetDirectoryName(relUriStr);
                        if (dirname.Length == 0)
                            continue;
                        
                        var dirs = dirname.Split('/');
                        if (dirs.Length == 0)
                            continue;

                        var toplevelDir = dirs[0];
                        if (toplevelDir != Path.GetFileNameWithoutExtension(zip.ToString()))
                            continue;
                        
                        var rest = relUriStr.Split(new string[] { toplevelDir + "/" }, StringSplitOptions.None);
                        if (rest.Length <= 1)
                            continue;

                        var pathToFileInZip = rest[1];
                        var file = XapPAL.PAL.VirtualFilesystem.GetFile(
                            new StreamResourceInfo(GetFileInternal(baseUri, zip), null), pathToFileInZip
                        );
                        
                        if (file != null)
                            return file;
                    }
                }
            }

            var fullUri = DynamicApplication.MakeUri((Uri)baseUri, relativeUri);

            if (_cache.Has(fullUri)) {
                byte[] bytes = _cache.GetBinary(fullUri);
                string strContent = _cache.Get(fullUri);
                return new MemoryStream(
                    strContent != null ?
                        System.Text.Encoding.UTF8.GetBytes(strContent) :
                        bytes
                );
            } else {
                return null;
            }
        }

        /// <summary>
        /// Download and cache a list of URIs, and execute a delegate when it's
        /// complete.
        /// </summary>
        /// <param name="uris">List of URIs to download and cache</param>
        /// <param name="onComplete">
        /// Called when the URI's are successfully downloaded and cached
        /// </param>
        internal void DownloadAndCache(List<Uri> uris, Action onComplete) {
            _cache.Download(uris, onComplete);
        }
    }

    /// <summary>
    /// A cache of Uris mapping to strings.
    /// </summary>
    public class DownloadCache {

        // TODO: shouldn't differentiate between TextContent and BinaryContent,
        // should just have BinaryContent and then convert to UTF8 for TextContent.
        internal struct DownloadContent {
            internal string TextContent;
            internal byte[] BinaryContent;

            internal DownloadContent(string tC, byte[] bC) {
                TextContent = tC;
                BinaryContent = bC;
            }
        }

        private DownloadCacheDownloader _downloader;

        public DownloadCache() {
            _downloader = new DownloadCacheDownloader(this);
        }

        private Dictionary<Uri, DownloadContent> _cache = new Dictionary<Uri, DownloadContent>();

        /// <summary>
        /// Adds a URI/code pair to the cache if the URI doesn't not already
        /// exist.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="code"></param>
        public void Add(Uri uri, string code) {
            if (!Has(uri)) {
                _cache.Add(uri, new DownloadContent(code, null));
            }
        }

        public void Add(Uri uri, byte[] data) {
            if (!Has(uri)) {
                _cache.Add(uri, new DownloadContent(null, data));
            }
        }

        /// <summary>
        /// Gets a string from the cache from a URI. Returns null if the URI is
        /// not in the cache.
        /// </summary>
        /// <param name="uri">A URI to look-up</param>
        /// <returns>Binary content associated with that URI.</returns>
        public string Get(Uri uri) {
            if (!Has(uri)) return null;
            return _cache[uri].TextContent;
        }

        /// <summary>
        /// Get binary content from the cache from a URI. Returns null if the
        /// URI is not in the cache.
        /// </summary>
        /// <param name="uri">A URI to look-up</param>
        /// <returns>Binary content associated with that URI.</returns>
        public byte[] GetBinary(Uri uri) {
            if (!Has(uri)) return null;
            return _cache[uri].BinaryContent;
        }

        /// <summary>
        /// Indexer to Get and Add string to the cache. Indexes on URIs.
        /// </summary>
        public string this[Uri uri] {
            get { return Get(uri); }
            set { Add(uri, value); }
        }

        /// <summary>
        /// Does the cache contain the URI?
        /// </summary>
        public bool Has(Uri uri) {
            return _cache.ContainsKey(uri);
        }

        /// <summary>
        /// Clears the cache completely.
        /// </summary>
        public void Clear() {
            _cache = null;
            _cache = new Dictionary<Uri, DownloadContent>();
        }

        /// <summary>
        /// Downloads the list of URIs, caches the result of the download, and
        /// calls onComplete when finished.
        /// </summary>
        public void Download(List<Uri> uris, Action onComplete) {
            _downloader.Download(uris, onComplete);
        }

        internal class DownloadCacheDownloader {

            private List<Uri> _downloadQueue;
            private DownloadCache _cache;

            internal DownloadCacheDownloader(DownloadCache cache) {
                _cache = cache;
            }

            internal void Download(List<Uri> uris, Action onComplete) {
                if (uris.Count == 0) {
                    onComplete.Invoke();
                    return;
                }
                _downloadQueue = new List<Uri>(uris);
                var iteratingQueue = new List<Uri>(_downloadQueue);
                foreach (var uri in iteratingQueue) {
                    InternalDownload(uri, onComplete);
                }
            }

            private void InternalDownload(Uri uri, Action onComplete) {
                if (HasDomainFailed(uri.Host)) {
                    DownloadWithXmlHttpRequest(uri, onComplete);
                } else {
                    DownloadWithWebClient(uri, onComplete, () => {
                        DomainFailed(uri.Host);
                        DownloadWithXmlHttpRequest(uri, onComplete);
                    });
                }
            }

            private void DownloadComplete(Uri uri, Stream content, Action onComplete) {
                DownloadComplete(uri, StreamToByteArray(content), onComplete);
            }

            private void DownloadComplete(Uri uri, byte[] content, Action onComplete) {
                _cache.Add(uri, content);
                InternalDownloadComplete(uri, onComplete);
            }

            private byte[] StreamToByteArray(Stream stream) {
                byte[] buffer = new byte[stream.Length];
                int read = stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }

            private void DownloadComplete(Uri uri, string content, Action onComplete) {
                _cache.Add(uri, content);
                InternalDownloadComplete(uri, onComplete);
            }

            private void InternalDownloadComplete(Uri uri, Action onComplete) {
                if (_downloadQueue != null) {
                    lock (_downloadQueue) {
                        if (_downloadQueue != null && _downloadQueue.Count > 0 && _downloadQueue.Contains(uri)) {
                            _downloadQueue.Remove(uri);
                            if (_downloadQueue.Count == 0) {
                                _downloadQueue = null;
                                onComplete.Invoke();
                            }
                        }
                    }
                }
            }

            #region WebRequest / XMLHttpRequest fail-over
            private Dictionary<string, bool> _webClientDownloadFailureForDomain = new Dictionary<string, bool>();

            private bool HasDomainFailed(string host) {
                return _webClientDownloadFailureForDomain.ContainsKey(host) && _webClientDownloadFailureForDomain[host];
            }

            private void DomainFailed(string host) {
                _webClientDownloadFailureForDomain[host] = true;
            }
            #endregion

            #region WebRequest
            private void DownloadWithWebClient(Uri uri, Action onComplete, Action onSecurityException) {
                var webClient = new WebClient();
                webClient.OpenReadCompleted += (s, e) => {
                    if (e.Error == null && e.Cancelled == false) {
                        var requestUri = (Uri)((object[])e.UserState)[0];
                        var completeAction = (Action)((object[])e.UserState)[1];
                        DownloadComplete(requestUri, e.Result, completeAction);
                    } else {
                        if (e.Error.InnerException.GetType() == typeof(System.Security.SecurityException)) {
                            onSecurityException();
                        } else {
                            throw e.Error.InnerException;
                        }
                    }
                };
                webClient.OpenReadAsync(uri, new object[] { uri, onComplete });
            }
            #endregion

            #region XMLHttpRequest
            // XMLHttpRequest is used instead of WebClient when this 
            // scenario happens:
            // localhost/index.html --> bar.com/dlr.xap --> localhost/foo.py
            //                                          ^^^
            // WebClient refuses to do the marked request, because you cannot
            // make a request from an internet-zone to a local-zone. XMLHttpRequest
            // works though, since it executes in the local domain.
            //
            // Also, when the XAP is hosted cross-domain, all inbound HTML
            // events and interactions are disabled. This can be re-enabled
            // by both setting the ExternalCallersFromCrossDomain property in the
            // AppManifest.xaml to "ScriptableOnly" and setting the "enableHtmlAccess"
            // param on the Silverlight object tag to "true". This not only allows the
            // "XMLHttpRequest.onreadstatechange" event to call back into managed
            // code, but re-enabled all HTML events, like the REPL. See
            // http://msdn.microsoft.com/en-us/library/cc645023(VS.95).aspx for
            // more information.
            //
            // If for some reason you can't change the AppManifest's settings,
            // you'll have to use polling to detect when the download is done
            // (see "XMLHttpRequest with polling" region below). However this is
            // not enabled anywhere, as the scenario is a bit limited.
            //
            // Also note that OnXmlHttpDownloadComplete catches ALL exceptions
            // to make sure they don't leak out into JavaScript.

            private bool _emittedXMLHttpRequestHander = false;

            private void DownloadWithXmlHttpRequest(Uri uri, Action onComplete) {
                if (!_emittedXMLHttpRequestHander) {
                    if (HtmlPage.BrowserInformation.Name == "Microsoft Internet Explorer") {
                        // IE's JavaScript API has no way of getting to binary data from XMLHttpRequest,
                        // so resort to VBScript. However, the performance of this is horrible.
                        AddScriptTag("vbscript", null, @"
    Function BinaryArrayToAscCSV( aBytes )
        Dim j, sOutput
               sOutput = """"
        For j = 1 to LenB(aBytes)
            sOutput= sOutput & AscB( MidB(aBytes,j,1) )
            sOutput= sOutput & "",""
        Next
        BinaryArrayToAscCSV = sOutput
    End Function
                        ");
                        AddScriptTag(null, "text/javascript", @"
    function request2csv(request) {
        return BinaryArrayToAscCSV(request.responseBody);
    }
                        ");
                    }

                    HtmlPage.Window.Eval(@"
    function OnXmlHttpRequest_ReadyStateChange(file) {
        return function() {
            this.currentSLObject.OnXmlHttpDownloadComplete(this, file);
        }
    }

    var isIE = false;

    function DLR_DownloadResource(uri, binary) {
        request = new XMLHttpRequest();

        request.open('GET', uri, false);
        if (binary && request.overrideMimeType)
            request.overrideMimeType('text/plain; charset=x-user-defined');
        request.send();
        if (request.status != 200) return '';
        var raw;
        if (binary && typeof(request.responseBody) !== 'undefined') {
            isIE = true;
            raw = BinaryArrayToAscCSV(request.responseBody);
            return raw.substring(0, raw.length - 2).split(',');
        } else {
            return request.responseText;
        }
    }

    function DLR_DownloadTextResource(uri) {
        return DLR_DownloadResource(uri, false);
    }

    function DLR_DownloadBinaryResource(uri) {
        var raw = DLR_DownloadResource(uri, true);
        var data = new Array(raw.length);
        if (isIE) {
          for(i = 0; i < raw.length; i++) {
            data[i] = parseInt(raw[i]);
          }
        } else {
          for(i = 0; i < raw.length; i++) {
            data[i] = raw.charCodeAt(i) & 0xff;
          }
        }
        return data;
    }
                    ");
                    _emittedXMLHttpRequestHander = true;
                }

                var file = uri.ToString();

                // HACK treat zip files as binary content
                if (Path.GetExtension(file).Contains("zip")) {

                    ScriptObject bin = (ScriptObject)HtmlPage.Window.Eval(string.Format(@"DLR_DownloadBinaryResource(""{0}"");", file));
                    byte[] binaryContent = new byte[(int)(double)bin.GetProperty("length")];
                    for (int i = 0; i < binaryContent.Length; i++) {
                        binaryContent[i] = (byte)(double)bin.GetProperty(i.ToString());
                    }

                    DownloadComplete(new Uri(file, UriKind.RelativeOrAbsolute), binaryContent, onComplete);

                } else {

                    string content = (string)(HtmlPage.Window.GetProperty("DLR_DownloadTextResource") as ScriptObject).InvokeSelf(file);
                    DownloadComplete(new Uri(file, UriKind.RelativeOrAbsolute), content, onComplete);

                }
            }

            private void AddScriptTag(string lang, string type, string text) {
                var scriptTag = HtmlPage.Document.CreateElement("script");
                if (lang != null) scriptTag.SetAttribute("language", lang);
                if (type != null) scriptTag.SetAttribute("type", type);
                scriptTag.SetProperty("text", text);
                (HtmlPage.Document.GetElementsByTagName("head")[0] as HtmlElement).AppendChild(scriptTag);
            }

#if false
            private Action _onComplete;

            // Managed handler of XmlHttpRequest.onreadstatechange; a JavaScript one is currently
            // used, but switching back to this might be best for performance.
            [ScriptableMember]
            public void OnXmlHttpDownloadComplete(ScriptObject handlerThis, string file) {
                try {
                    object objReadyState = handlerThis.GetProperty("readyState");
                    object objStatus = handlerThis.GetProperty("status");

                    int readyState = 0;
                    int status = 0;

                    if (objStatus != null) status = (int)((double)objStatus / 1);
                    if (objReadyState != null) readyState = (int)((double)objReadyState / 1);

                    if (readyState == 4 && status == 200) {
                        
                        // HACK treat zip files as binary content
                        if (Path.GetExtension(file).Contains("zip")) {
                            
                            string content;
                            byte[] binaryContent;
                            if (HtmlPage.BrowserInformation.UserAgent.IndexOf("MSIE") != -1) {
                                content = (string)HtmlPage.Window.Invoke("request2csv", handlerThis);
                                var strArray = content.Substring("BinaryArrayToAscCSV".Length).Split(',');
                                binaryContent = new byte[strArray.Length];
                                for (int i = 0; i < strArray.Length; i++) {
                                    string strByte = strArray[i];
                                    if (strByte.Length == 0) break;
                                    binaryContent[i] = byte.Parse(strByte);
                                }
                            } else {
                                content = (string)handlerThis.GetProperty("responseText");
                                binaryContent = new byte[content.Length];
                                for (int i = 0; i < content.Length; i++) {
                                    binaryContent[i] = (byte)(((int)content[i]) & 0xff);
                                }
                            }
                            DownloadComplete(new Uri(file, UriKind.RelativeOrAbsolute), binaryContent, _onComplete);

                        } else {

                            string content = (string)handlerThis.GetProperty("responseText");
                            DownloadComplete(new Uri(file, UriKind.RelativeOrAbsolute), content, _onComplete);

                        }
                        
                    } else if (readyState == 4 && status != 200) {
                        throw new Exception(file + " download failed (status: " + status + ")");
                    }
                } catch (Exception e) {
                    // This catch-all is necessary since any unhandled exceptions
                    // will not be processed by Application.UnhandledException, so
                    // call it directly if reporting errors is enabled.
                    if (Settings.ReportUnhandledErrors)
				        DynamicApplication.Current.HandleException(this, e);
                }
            }
#endif
            #endregion

            #region XMLHttpRequest with polling
#if false
            private void DownloadWithXmlHttpRequestAndPolling(Uri uri, Action onComplete) {
                var request = HtmlPage.Window.CreateInstance("XMLHttpRequest");
                request.Invoke("open", "GET", uri.ToString());
                request.SetProperty("onreadystatechange", HtmlPage.Window.Eval("DLR.__onDownloadCompleteToPoll(\"" + uri.ToString() + "\")"));
                request.Invoke("send");
                PollForDownloadComplete(uri, onComplete);
            }

            private void PollForDownloadComplete(Uri uri, Action onComplete) {
                var pollCount = 0;
                var timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
                timer.Tick += (sender, args) => {
                    object objStatus = null;
                    int status = 0;

                    var obj = HtmlPage.Document.GetElementById(uri.ToString());
                    if (obj != null) {
                        objStatus = obj.GetProperty("status");
                        if (objStatus != null) status = (int)((double)objStatus / 1);
                    }

                    Action<Uri, int> onFailure = (duri, dstatus) => {
                        timer.Stop();
                        throw new Exception(duri.ToString() + " download failed (status: " + dstatus + ")");
                    };

                    if (status == 200) {
                        var content = (string)obj.GetProperty("scriptContent");
                        HtmlPage.Document.Body.RemoveChild(obj);
                        timer.Stop();
                        DownloadComplete(uri, content, onComplete);
                    } else if (status == 400) {
                        onFailure(uri, status);
                    } else {
                        if (pollCount < 50) pollCount++;
                        else onFailure(uri, status);
                    }
                };
                timer.Start();
            }
#endif
            #endregion
        }
    }

#if false
    /// <summary>
    /// Read and write files from Isolated Storage
    /// </summary>
    public class IsolatedStorageVirtualFilesystem : BrowserVirtualFilesystem {

        public override string Name() { return "Isolated Storage"; }

        protected override Stream GetFileInternal(object baseUri, Uri relativeUri) {
            throw new NotImplementedException("TODO");
        }
    }
#endif
}
