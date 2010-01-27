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
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Chiron {
    class HttpServer {
        int _port;
        string _dir;
        bool _shutdown;
        Socket _socketIPv6;
        Socket _socketIPv4;

        public HttpServer(int port, string dir) {
            _port = port;
            _dir = dir;
            if (_dir[_dir.Length - 1] != Path.DirectorySeparatorChar)
                _dir += Path.DirectorySeparatorChar;
        }

        private Socket Connect(AddressFamily family, IPAddress address) {
            Socket socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(address, _port));
            socket.Listen(0x1000);
            return socket;
        }

        public void Start() {
            // Listen to IPv6 and IPv4 at the same time.
            // (Otherwise Firefox is very slow because it will try IPv6 first for
            // every single download & time out)
            bool ipv6Works = false;
            try {
                _socketIPv6 = Connect(AddressFamily.InterNetworkV6, IPAddress.IPv6Loopback);
                Accept(_socketIPv6);
                ipv6Works = true;
            }
            catch { }

            try {
                _socketIPv4 = Connect(AddressFamily.InterNetwork, IPAddress.Loopback);
                Accept(_socketIPv4);
            }
            catch {
                if (!ipv6Works)
                    throw;
            }
        }

        public void Stop() {
            _shutdown = true;

            try {
                if (_socketIPv4 != null) _socketIPv4.Close();
                if (_socketIPv6 != null) _socketIPv6.Close();
            }
            catch {} finally {
                _socketIPv4 = null;
                _socketIPv6 = null;
            }
        }

        public bool IsRunning {
            get {
                return (_socketIPv4 != null || _socketIPv6 != null);
            }
        }

        void Accept(Socket mySocket) {
            ThreadPool.QueueUserWorkItem(delegate {
                while (!_shutdown) {
                    try {
                        Socket socket = mySocket.Accept();
                        ThreadPool.QueueUserWorkItem(delegate {
                            if (!_shutdown)
                                ProcessRequest(new HttpSocket(socket));
                        }, socket);
                    } catch {
                        Thread.Sleep(100);
                    }
                }
            });
        }

        void ProcessRequest(HttpSocket s) {
            HttpRequestData r = null;
            string path = null;

            // reply to unreadable requests
            if (!s.TryReadRequest(out r)) {
                s.WriteErrorResponse(400, "Unparsable bad request");
            }
            // deny non-GET requests
            else if (r.Method != "GET") {
                s.WriteErrorResponse(405, "Method other than GET");
            }
            // process special commands
            else if (TryProcessSpecialCommand(s, r.Uri)) {
                // done
            }
            // deny requests that cannot be mapped to disk
            else if (!TryMapUri(r.Uri, out path)) {
                s.WriteErrorResponse(404, "URI cannot be mapped to disk");
            }
            // process file requests
            else if (TryProcessFileRequest(s, r.Uri, path)) {
                // done
            }
            // process directory requests
            else if (TryProcessDirectoryRequest(s, r.Uri, path)) {
                // done
            }
            // process XAP requests
            else if (TryProcessXapRequest(s, path)) {
                // done
            }
            // process XAP listing requests
            else if (TryProcessXapListingRequest(s, r.Uri, path)) {
                // done
            }
            // process requests for assemblies contained in Chiron's localAssemblyPath
            else if (TryProcessAssemblyRequest(s, r.Uri)) {
                // done
            }
            // process requests for .slvx files contained in Chiron's localAssemblyPath
            else if (TryProcessExternalRequest(s, r.Uri)) {
              //done
            } 
            else {
                // not found
                s.WriteErrorResponse(404, "Resource not found");
            }

            Chiron.Log(s.StatusCode, (r != null && r.Uri != null ? r.Uri : "[unknown]"), s.BytesSent, s.Message);
        }

        bool TryMapUri(string uri, out string path) {
            path = null;

            // strip query string
            int i = uri.IndexOf('?');
            if (i > 0) uri = uri.Substring(0, i);

            // check for special cases
            if (string.IsNullOrEmpty(uri) || uri[0] != '/' || uri.IndexOf("..") >= 0 || uri.IndexOf('\\') >= 0)
                return false;
            if (uri == "/") { path = _dir; return true; }

            // decode %XX
            if (uri.IndexOf('%') >= 0) {
                uri = DecodeUri(uri);
                if (uri.IndexOf('%') >= 0) return false;
            }

            // combine path and validate
            try {
                string p1 = Path.Combine(_dir, uri.Substring(1)).Replace('/', Path.DirectorySeparatorChar);
                string p2 = Path.GetFullPath(p1);
                // normalization check
                if (string.Compare(p1, p2, StringComparison.OrdinalIgnoreCase) != 0) return false;
                path = p2;
                return true;
            }
            catch {
                return false;
            }
        }

        bool TryProcessSpecialCommand(HttpSocket s, string uri) {
            uri = uri.ToLowerInvariant();
            switch (uri) {
                case "/bye!":
                    s.WriteTextResponse(200,  "plain", ":(", false);
                    ThreadPool.QueueUserWorkItem(delegate { Thread.Sleep(100); Stop(); });
                    return true;
                case "/ping!":
                    s.WriteTextResponse(200, "plain", ":)", false);
                    return true;
                case "/sl.png!":
                    s.WriteBinaryResponse(200, "image/png", GetResourceBytes("sl.png"), false);
                    return true;
                case "/slx.png!":
                    s.WriteBinaryResponse(200, "image/png", GetResourceBytes("slx.png"), false);
                    return true;
                case "/style.css!":
                    s.WriteTextResponse(200, "css", HtmlFormatter.Style, false);
                    return true;
                default:
                    return false;
            }
        }

        bool TryProcessFileRequest(HttpSocket s, string uri, string path) {
            // path shouldn't end with '\' (that's for XAP listing)
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString())) return false;

            // file must exist
            if (!File.Exists(path)) return false;

            // check extension
            string mimeType = HttpSocket.GetMimeType(path);
            if (string.IsNullOrEmpty(mimeType)) {
                s.WriteErrorResponse(403);
                return true;
            }

            // read the file
            byte[] body = null;
            try {
                body = File.ReadAllBytes(path);
            }
            catch (Exception ex) {
                s.WriteErrorResponse(500, ex.Message + "\r\n" + ex.StackTrace);
                return true;
            }

            // write the response
            s.WriteResponse(200, string.Format("Content-type: {0}\r\n", mimeType), body, false);
            return true;
        }

        bool TryProcessDirectoryRequest(HttpSocket s, string uri, string path) {
            if (!Directory.Exists(path)) return false;

            string q = string.Empty;
            int i = uri.IndexOf('?');
            if (i > 0) { q = uri.Substring(i); uri = uri.Substring(0, i); }

            // redirect to trailing '/' to fix-up relative links
            if (!uri.EndsWith("/")) {
                string newUri = uri + "/" + q;

                s.WriteResponse(302,
                    string.Format("Content-Type: text/html; charset=utf-8\r\nLocation: {0}\r\n", newUri),
                    Encoding.UTF8.GetBytes(
string.Format(@"<html><head><title>Object moved</title></head><body>
<h2>Object moved to <a href=""{0}"">here</a>.</h2></body></html>", newUri)),
                    false);

                return true;
            }

            // get all files and subdirs
            FileSystemInfo[] infos;
            try {
                infos = new DirectoryInfo(path).GetFileSystemInfos();
            }
            catch {
                infos = new FileSystemInfo[0];
            }

            // decode %XX
            uri = DecodeUri(uri);

            // determine if parent is appropriate
            string parent = null;
            if (uri.Length > 1) {
                i = uri.LastIndexOf('/', uri.Length-2);
                parent = (i > 0) ? uri.Substring(0, i) : "/";
            }

            // write the response
            s.WriteTextResponse(200, "html", HtmlFormatter.FormatDirectoryListing(uri, parent, infos), false);
            return true;
        }

        bool TryProcessXapRequest(HttpSocket s, string path) {
            // must end with XAP
            if (string.Compare(Path.GetExtension(path), ".xap", StringComparison.OrdinalIgnoreCase) != 0)
                return false;

            // XAP already there?
            if (File.Exists(path))
                return false;

            // Directory must be present
            string dir = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path);
            if (!Directory.Exists(dir))
                return false;

            byte[] xapBytes = null;

            try {
                xapBytes = XapBuilder.XapToMemory(dir);
            }
            catch (Exception e) {
                s.WriteErrorResponse(500, "error generating XAP: " + e.Message);
                return true;
            }

            s.WriteBinaryResponse(200, "application/x-zip-compressed", xapBytes, false);
            return true;
        }

        bool TryProcessXapListingRequest(HttpSocket s, string uri, string path) {
            // path should end with '\' (for XAP listing)
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString())) return false;
            path = path.Substring(0, path.Length - 1);

            // must end with XAP
            if (string.Compare(Path.GetExtension(path), ".xap", StringComparison.OrdinalIgnoreCase) != 0)
                return false;

            // file must exist
            if (!File.Exists(path)) return false;

            // see if need to serve file from XAP
            string filename = null;
            int iq = uri.IndexOf('?');
            if (iq >= 0) filename = uri.Substring(iq + 1);

            ZipArchive xap = null;

            try {
                // open XAP file
                xap = new ZipArchive(path, FileAccess.Read);

                if (string.IsNullOrEmpty(filename)) {
                    // list contents
                    List<ZipArchiveFile> xapContents = new List<ZipArchiveFile>();
                    foreach (KeyValuePair<string, ZipArchiveFile> p in xap.entries) xapContents.Add(p.Value);
                    s.WriteTextResponse(200, "html", HtmlFormatter.FormatXapListing(uri, xapContents), false);
                    return true;
                }

                // server file from XAP
                ZipArchiveFile f = null;
                if (!xap.entries.TryGetValue(filename, out f)) {
                    s.WriteErrorResponse(404, "Resource not found in XAP");
                    return true;
                }

                // check mime type
                string mimeType = HttpSocket.GetMimeType(filename);
                if (string.IsNullOrEmpty(mimeType)) {
                    s.WriteErrorResponse(403);
                    return true;
                }

                // get the content
                byte[] body = new byte[(int)f.Length];
                if (body.Length > 0) {
                    using (Stream fs = f.OpenRead()) {
                        fs.Read(body, 0, body.Length);
                    }
                }

                // write the resposne
                s.WriteResponse(200, string.Format("Content-type: {0}\r\n", mimeType), body, false);
                return true;
            }
            catch {
                s.WriteErrorResponse(500, "error reading XAP");
                return true;
            }
            finally {
                if (xap != null) xap.Close();
            }
        }

        bool TryProcessAssemblyRequest(HttpSocket s, string uri) {
            if (Chiron.UrlPrefix == "")
                return false;

            int slash = uri.LastIndexOf('/');
            if (slash == -1)
                return false;

            // must start with URL prefix
            if (string.Compare(uri.Substring(0, slash + 1), Chiron.UrlPrefix, StringComparison.OrdinalIgnoreCase) != 0)
                return false;

            uri = uri.Substring(slash + 1);

            // must end with DLL or PDB
            if (string.Compare(Path.GetExtension(uri), ".dll", StringComparison.OrdinalIgnoreCase) != 0 &&
                string.Compare(Path.GetExtension(uri), ".pdb", StringComparison.OrdinalIgnoreCase) != 0)
                return false;

            // get mime type
            string mimeType = HttpSocket.GetMimeType(uri);
            if (string.IsNullOrEmpty(mimeType)) {
                s.WriteErrorResponse(403);
                return true;
            }

            // see if the file exists in the assembly reference path
            string path = Chiron.TryGetAssemblyPath(uri);
            if (path == null)
                return false;

            // read the file
            byte[] body = null;
            try {
                body = File.ReadAllBytes(path);
            } catch (Exception ex) {
                s.WriteErrorResponse(500, ex.Message + "\r\n" + ex.StackTrace);
                return true;
            }

            // write the response
            s.WriteResponse(200, string.Format("Content-type: {0}\r\n", mimeType), body, false);
            return true;
        }

        bool TryProcessExternalRequest(HttpSocket s, string uri) {
            if (Chiron.ExternalUrlPrefix == "")
                return false;

            int slash = uri.LastIndexOf('/');
            if (slash == -1)
                return false;

            // must start with external URL prefix
            if (string.Compare(uri.Substring(0, slash + 1), Chiron.ExternalUrlPrefix, StringComparison.OrdinalIgnoreCase) != 0)
                return false;

            uri = uri.Substring(slash + 1);

            // must end with SLVX
            if (string.Compare(Path.GetExtension(uri), ".slvx", StringComparison.OrdinalIgnoreCase) != 0)
                return false;

            // get mime type
            string mimeType = HttpSocket.GetMimeType(uri);
            if (string.IsNullOrEmpty(mimeType)) {
                s.WriteErrorResponse(403);
                return true;
            }

            // see if the file exists in the assembly reference path
            string path = Chiron.TryGetAssemblyPath(uri);
            if (path == null)
                return false;

            // read the file
            byte[] body = null;
            try {
                body = File.ReadAllBytes(path);
            } catch (Exception ex) {
                s.WriteErrorResponse(500, ex.Message + "\r\n" + ex.StackTrace);
                return true;
            }

            // write the response
            s.WriteResponse(200, string.Format("Content-type: {0}\r\n", mimeType), body, false);
            return true;
        }

        internal static byte[] GetResourceBytes(string name) {
            Stream s = typeof(Chiron).Assembly.GetManifestResourceStream("Chiron." + name);
            byte[] b = new byte[(int)s.Length];
            s.Read(b, 0, b.Length);
            return b;
        }

        static string DecodeUri(string uri) {
            try {
                return new Uri("http://localhost" + uri).LocalPath;
            }
            catch {
                return uri;
            }
        }
    }
}
