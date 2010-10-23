/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License (Ms-PL). A copy of the license can be found in the license.htm file
 * included in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Microsoft.Win32.SafeHandles;

namespace Cassini {
	
    class Request : SimpleWorkerRequest {
        static char[] badPathChars = new char[] { '%', '>', '<', ':', '\\' };
        static string[] defaultFileNames = new string[] { "default.aspx", "default.htm", "default.html" };

        static string[] restrictedDirs = new string[] { 
                "/bin",
                "/app_browsers", 
                "/app_code", 
                "/app_data", 
                "/app_localresources", 
                "/app_globalresources", 
                "/app_webreferences" };

        const int MaxChunkLength = 64 * 1024;

        Server _server;
        Host _host;
        Connection _connection;

        // security permission to Assert remoting calls to _connection
        IStackWalk _connectionPermission = new PermissionSet(PermissionState.Unrestricted);

        // raw request data
        const int maxHeaderBytes = 32*1024;
        byte[] _headerBytes;
        int _startHeadersOffset;
        int _endHeadersOffset;
        List<ByteString> _headerByteStrings;

        // parsed request data

        bool _isClientScriptPath;

        string _verb;
        string _url;
        string _prot;

        string _path;
        string _filePath;
        string _pathInfo;
        string _pathTranslated;
        string _queryString;
        byte[] _queryStringBytes;

        int _contentLength;
        int _preloadedContentLength;
        byte[] _preloadedContent;

        string _allRawHeaders;
        string[][] _unknownRequestHeaders;
        string[] _knownRequestHeaders;
        bool _specialCaseStaticFileHeaders;

        // cached response
        bool _headersSent;
        int _responseStatus;
        StringBuilder _responseHeadersBuilder;
        List<byte[]> _responseBodyBytes;

        public Request(Server server, Host host, Connection connection) : base(String.Empty, String.Empty, null) {
            _server = server;
            _host = host;
            _connection = connection;
        }

        public void Process() {
            // read the request
            if (!TryParseRequest()) {
                return;
            }

            // 100 response to POST
            if (_verb == "POST" && _contentLength > 0 && _preloadedContentLength < _contentLength) {
                _connection.Write100Continue();
            }

            // special case for client script
            if (_isClientScriptPath) {
                _connection.WriteEntireResponseFromFile(_host.PhysicalClientScriptPath + _path.Substring(_host.NormalizedClientScriptPath.Length), false);
                return;
            }

            // deny access to code, bin, etc.
            if (IsRequestForRestrictedDirectory()) {
                _connection.WriteErrorAndClose(403);
                return;
            }

            // special case for directory listing
            if (ProcessDirectoryListingRequest()) {
                return;
            }

            PrepareResponse();

            // Hand the processing over to HttpRuntime
            HttpRuntime.ProcessRequest(this);
        }

        void Reset() {
            _headerBytes = null;
            _startHeadersOffset = 0;
            _endHeadersOffset = 0;
            _headerByteStrings = null;

            _isClientScriptPath = false;

            _verb = null;
            _url = null;
            _prot = null;

            _path = null;
            _filePath = null;
            _pathInfo = null;
            _pathTranslated = null;
            _queryString = null;
            _queryStringBytes = null;

            _contentLength = 0;
            _preloadedContentLength = 0;
            _preloadedContent = null;

            _allRawHeaders = null;
            _unknownRequestHeaders = null;
            _knownRequestHeaders = null;
            _specialCaseStaticFileHeaders = false;
        }

        bool TryParseRequest() {
            Reset();

            ReadAllHeaders();

            if (_headerBytes == null || _endHeadersOffset < 0 ||
                _headerByteStrings == null || _headerByteStrings.Count == 0) {
                _connection.WriteErrorAndClose(400);
                return false;
            }

            ParseRequestLine();

            // Check for bad path
            if (IsBadPath()) {
                _connection.WriteErrorAndClose(400);
                return false;
            }

            // Check if the path is not well formed or is not for the current app
            if (!_host.IsVirtualPathInApp(_path, out _isClientScriptPath)) {
                _connection.WriteErrorAndClose(404);
                return false;
            }

            ParseHeaders();

            ParsePostedContent();

            return true;
        }

        bool TryReadAllHeaders() {
            // read the first packet (up to 32K)
            byte[] headerBytes = _connection.ReadRequestBytes(maxHeaderBytes);

            if (headerBytes == null || headerBytes.Length == 0)
                return false;

            if (_headerBytes != null) {
                // previous partial read
                int len = headerBytes.Length + _headerBytes.Length;
                if (len > maxHeaderBytes)
                    return false;

                byte[] bytes = new byte[len];
                Buffer.BlockCopy(_headerBytes, 0, bytes, 0, _headerBytes.Length);
                Buffer.BlockCopy(headerBytes, 0, bytes, _headerBytes.Length, headerBytes.Length);
                _headerBytes = bytes;
            }
            else {
                _headerBytes = headerBytes;
            }

            // start parsing
            _startHeadersOffset = -1;
            _endHeadersOffset = -1;
            _headerByteStrings = new List<ByteString>();

            // find the end of headers
            ByteParser parser = new ByteParser(_headerBytes);

            for (;;) {
                ByteString line = parser.ReadLine();

                if (line == null) {
                    break;
                }

                if (_startHeadersOffset < 0) {
                    _startHeadersOffset = parser.CurrentOffset;
                }

                if (line.IsEmpty) {
                    _endHeadersOffset = parser.CurrentOffset;
                    break;
                }

                _headerByteStrings.Add(line);
            }

            return true;
        }

        void ReadAllHeaders() {
            _headerBytes = null;

            do {
                if (!TryReadAllHeaders()) {
                    // something bad happened
                    break;
                }
            }
            while (_endHeadersOffset < 0); // found \r\n\r\n
        }

        void ParseRequestLine() {
            ByteString requestLine = _headerByteStrings[0];
            ByteString[] elems = requestLine.Split(' ');

            if (elems == null || elems.Length < 2 || elems.Length > 3) {
                _connection.WriteErrorAndClose(400);
                return;
            }

            _verb = elems[0].GetString();

            ByteString urlBytes = elems[1];
            _url = urlBytes.GetString();
			
            if (elems.Length == 3) {
                _prot = elems[2].GetString();
            }
            else {
                _prot = "HTTP/1.0";
            }

            // query string

            int iqs = urlBytes.IndexOf('?');
            if (iqs > 0) {
                _queryStringBytes = urlBytes.Substring(iqs+1).GetBytes();
            }
            else {
                _queryStringBytes = new byte[0];
            }

            iqs = _url.IndexOf('?');
            if (iqs > 0) {
                _path = _url.Substring(0, iqs);
                _queryString = _url.Substring(iqs+1);
            }
            else {
                _path = _url;
                _queryStringBytes = new byte[0];
            }

            // url-decode path

            if (_path.IndexOf('%') >= 0) {
                _path = HttpUtility.UrlDecode(_path, Encoding.UTF8);

                iqs = _url.IndexOf('?');
                if (iqs >= 0) {
                    _url = _path + _url.Substring(iqs);
                }
                else {
                    _url = _path;
                }
			}

            // path info

            int lastDot = _path.LastIndexOf('.');
            int lastSlh = _path.LastIndexOf('/');

            if (lastDot >= 0 && lastSlh >= 0 && lastDot < lastSlh) {
                int ipi = _path.IndexOf('/', lastDot);
                _filePath = _path.Substring(0, ipi);
                _pathInfo = _path.Substring(ipi);
            }
            else {
                _filePath = _path;
                _pathInfo = String.Empty;
            }

            _pathTranslated = MapPath(_filePath);
        }

        bool IsBadPath() {
            if (_path.IndexOfAny(badPathChars) >= 0) {
                return true;
            }

            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(_path, "..", CompareOptions.Ordinal) >= 0) {
                return true;
            }

            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(_path, "//", CompareOptions.Ordinal) >= 0) {
                return true;
            }

            return false;
        }

        void ParseHeaders() {
            _knownRequestHeaders = new string[RequestHeaderMaximum];

            // construct unknown headers as array list of name1,value1,...
            var headers = new List<string>();

            for (int i = 1; i < _headerByteStrings.Count; i++) {
                string s = _headerByteStrings[i].GetString();

                int c = s.IndexOf(':');

                if (c >= 0) {
                    string name = s.Substring(0, c).Trim();
                    string value = s.Substring(c + 1).Trim();

                    // remember
                    int knownIndex = GetKnownRequestHeaderIndex(name);
                    if (knownIndex >= 0) {
                        _knownRequestHeaders[knownIndex] = value;
                    }
                    else {
                        headers.Add(name);
                        headers.Add(value);
                    }
                }
            }

            // copy to array unknown headers

            int n = headers.Count / 2;
            _unknownRequestHeaders = new string[n][];
            int j = 0;

            for (int i = 0; i < n; i++) {
                _unknownRequestHeaders[i] = new string[2];
                _unknownRequestHeaders[i][0] = headers[j++];
                _unknownRequestHeaders[i][1] = headers[j++];
            }

            // remember all raw headers as one string

            if (_headerByteStrings.Count > 1) {
                _allRawHeaders = Encoding.UTF8.GetString(_headerBytes, _startHeadersOffset, _endHeadersOffset-_startHeadersOffset);
            }
            else {
                _allRawHeaders = String.Empty;
            }
        }

        void ParsePostedContent() {
            _contentLength = 0;
            _preloadedContentLength = 0;

            string contentLengthValue = _knownRequestHeaders[HttpWorkerRequest.HeaderContentLength];
            if (contentLengthValue != null) {
                try {
                    _contentLength = Int32.Parse(contentLengthValue, CultureInfo.InvariantCulture);
                }
                catch {
                }
            }

            if (_headerBytes.Length > _endHeadersOffset) {
                _preloadedContentLength = _headerBytes.Length - _endHeadersOffset;

                if (_preloadedContentLength > _contentLength) {
                    _preloadedContentLength = _contentLength; // don't read more than the content-length
                }

                if (_preloadedContentLength > 0) {
                    _preloadedContent = new byte[_preloadedContentLength];
                    Buffer.BlockCopy(_headerBytes, _endHeadersOffset, _preloadedContent, 0, _preloadedContentLength);
                }
            }
        }

        void SkipAllPostedContent() {
            if (_contentLength > 0 && _preloadedContentLength < _contentLength) {
                int bytesRemaining = (_contentLength - _preloadedContentLength);

                while (bytesRemaining > 0) {
                    byte[] bytes = _connection.ReadRequestBytes(bytesRemaining);
                    if (bytes == null || bytes.Length == 0) {
                        return;
                    }
                    bytesRemaining -= bytes.Length;
                }
            }
        }

        bool IsRequestForRestrictedDirectory() {
            String p = CultureInfo.InvariantCulture.TextInfo.ToLower(_path);

            if (_host.VirtualPath != "/") {
                p = p.Substring(_host.VirtualPath.Length);
            }

            foreach (String dir in restrictedDirs) {
                if (p.StartsWith(dir, StringComparison.Ordinal)) {
                    if (p.Length == dir.Length || p[dir.Length] == '/') {
                        return true;
                    }
                }
            }

            return false;
        }

        bool ProcessDirectoryListingRequest() {
            if (_verb != "GET") {
                return false;
            }

            String dirPathTranslated = _pathTranslated;

            if (_pathInfo.Length > 0) {
                // directory path can never have pathInfo
                dirPathTranslated = MapPath(_path);
            }

            if (!Directory.Exists(dirPathTranslated)) {
                return false;
            }

            // Make sure directories always end with a trailing slash
            if (!_path.EndsWith("/", StringComparison.Ordinal))
                _path = _path + "/";

            // // have to redirect /foo to /foo/ to allow relative links to work
            // if (!_path.EndsWith("/", StringComparison.Ordinal)) {
            //     string newPath = _path + "/";
            //     string location = "Location: " + UrlEncodeRedirect(newPath) + "\r\n";
            //     string body = "<html><head><title>Object moved</title></head><body>\r\n" +
            //                   "<h2>Object moved to <a href='" + newPath + "'>here</a>.</h2>\r\n" +
            //                   "</body></html>\r\n";
            // 
            //     _connection.WriteEntireResponseFromString(302, location, body, false);
            //     return true;
            // }

            // check for the default file
            foreach (string filename in defaultFileNames) {
                string defaultFilePath = dirPathTranslated + "\\" + filename;

                if (File.Exists(defaultFilePath)) {
                    // pretend the request is for the default file path
                    _path += filename;
                    _filePath = _path;
                    _url = (_queryString != null) ? (_path + "?" + _queryString) : _path;
                    _pathTranslated = defaultFilePath;
                    return false; // go through normal processing
                }
            }

            // // get all files and subdirs
            // FileSystemInfo[] infos = null;
            // try {
            //     infos = (new DirectoryInfo(dirPathTranslated)).GetFileSystemInfos();
            // }
            // catch {
            // }
            // 
            // // determine if parent is appropriate
            // string parentPath = null;
            // 
            // if (_path.Length > 1) {
            //     int i = _path.LastIndexOf('/', _path.Length-2);
            // 
            //     parentPath = (i > 0) ?_path.Substring(0, i) : "/";
            //     if (!_host.IsVirtualPathInApp(parentPath)) {
            //         parentPath = null;
            //     }
            // }
            // 
            // _connection.WriteEntireResponseFromString(200, "Content-type: text/html; charset=utf-8\r\n",
            //                                           Messages.FormatDirectoryListing(_path, parentPath, infos),
            //                                           false);
            // return true;

            return false;
        }

        static char[] IntToHex = new char[16] {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'
        };

        static string UrlEncodeRedirect(string path) {
            // this method mimics the logic in HttpResponse.Redirect (which relies on internal methods)

            // count non-ascii characters
            byte[] bytes = Encoding.UTF8.GetBytes(path);
            int count = bytes.Length;
            int countNonAscii = 0;
            for (int i = 0; i < count; i++) {
                if ((bytes[i] & 0x80) != 0) {
                    countNonAscii++;
                }
            }

            // encode all non-ascii characters using UTF-8 %XX
            if (countNonAscii > 0) {
                // expand not 'safe' characters into %XX, spaces to +s
                byte[] expandedBytes = new byte[count + countNonAscii * 2];
                int pos = 0;
                for (int i = 0; i < count; i++) {
                    byte b = bytes[i];

                    if ((b & 0x80) == 0) {
                        expandedBytes[pos++] = b;
                    }
                    else {
                        expandedBytes[pos++] = (byte)'%';
                        expandedBytes[pos++] = (byte)IntToHex[(b >> 4) & 0xf];
                        expandedBytes[pos++] = (byte)IntToHex[b & 0xf];
                    }
                }

                path = Encoding.ASCII.GetString(expandedBytes);
            }

            // encode spaces into %20
            if (path.IndexOf(' ') >= 0) {
                path = path.Replace(" ", "%20");
            }

            return path;
        }

        void PrepareResponse() {
            _headersSent = false;
            _responseStatus = 200;
            _responseHeadersBuilder = new StringBuilder();
            _responseBodyBytes = new List<byte[]>();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Implementation of HttpWorkerRequest

        public override string GetUriPath() {
            return _path;
        }

        public override string GetQueryString() {
            return _queryString;
        }

        public override byte[] GetQueryStringRawBytes() {
            return _queryStringBytes;
        }

        public override string GetRawUrl() {
            return _url;
        }

        public override string GetHttpVerbName() {
            return _verb;
        }

        public override string GetHttpVersion() {
            return _prot;
        }

        public override string GetRemoteAddress() {
            _connectionPermission.Assert();
            return _connection.RemoteIP;
        }

        public override int GetRemotePort() {
            return 0;
        }

        public override string GetLocalAddress() {
            _connectionPermission.Assert();
            return _connection.LocalIP;
        }

        public override string GetServerName() {
            string localAddress = GetLocalAddress();
            if (localAddress.Equals("127.0.0.1")) {
                return "localhost";
            }
            return localAddress;
        }

        public override int GetLocalPort() {
            return _host.Port;
        }

        public override string GetFilePath() {
            return _filePath;
        }

        public override string GetFilePathTranslated() {
            return _pathTranslated;
        }

        public override string GetPathInfo() {
            return _pathInfo;
        }

        public override string GetAppPath() {
            return _host.VirtualPath;
        }

        public override string GetAppPathTranslated() {
            return _host.PhysicalPath;
        }

        public override byte[] GetPreloadedEntityBody() {
            return _preloadedContent;
        }

        public override bool IsEntireEntityBodyIsPreloaded() {
            return (_contentLength == _preloadedContentLength);
        }

        public override int ReadEntityBody(byte[] buffer, int size)  {
            int bytesRead = 0;

            _connectionPermission.Assert();
            byte[] bytes = _connection.ReadRequestBytes(size);

            if (bytes != null && bytes.Length > 0) {
                bytesRead = bytes.Length;
                Buffer.BlockCopy(bytes, 0, buffer, 0, bytesRead);
            }

            return bytesRead;
        }

        public override string GetKnownRequestHeader(int index)  {
            return _knownRequestHeaders[index];
        }

        public override string GetUnknownRequestHeader(string name) {
            int n = _unknownRequestHeaders.Length;

            for (int i = 0; i < n; i++) {
                if (string.Compare(name, _unknownRequestHeaders[i][0], StringComparison.OrdinalIgnoreCase) == 0) {
                    return _unknownRequestHeaders[i][1];
                }
            }

            return null;
        }

        public override string[][] GetUnknownRequestHeaders() {
            return _unknownRequestHeaders;
        }

        public override string GetServerVariable(string name) {
            string s = String.Empty;

            switch (name) {
                case "ALL_RAW":
                    s = _allRawHeaders;
                    break;

                case "SERVER_PROTOCOL":
                    s = _prot;
                    break;

                case "SERVER_SOFTWARE":
                    s = "Cassini/" + Messages.VersionString;
                    break;
            }

            return s;
        }

        public override string MapPath(string path) {
            string mappedPath = String.Empty;
            bool isClientScriptPath = false;

            if (path == null || path.Length == 0 || path.Equals("/")) {
            // asking for the site root
                if (_host.VirtualPath == "/") {
                    // app at the site root
                    mappedPath = _host.PhysicalPath;
                }
                else {
                    // unknown site root - don't point to app root to avoid double config inclusion
                    mappedPath = Environment.SystemDirectory;
                }
            }
            else if (_host.IsVirtualPathAppPath(path)) {
                // application path
                mappedPath = _host.PhysicalPath;
            }
            else if (_host.IsVirtualPathInApp(path, out isClientScriptPath)) {
                if (isClientScriptPath) {
                    mappedPath = _host.PhysicalClientScriptPath + path.Substring(_host.NormalizedClientScriptPath.Length);
                }
                else {
                    // inside app but not the app path itself
                    mappedPath = _host.PhysicalPath + path.Substring(_host.NormalizedVirtualPath.Length);
                }
            }
            else {
                // outside of app -- make relative to app path
                if (path.StartsWith("/", StringComparison.Ordinal)) {
                    mappedPath = _host.PhysicalPath + path.Substring(1);
                }
                else {
                    mappedPath = _host.PhysicalPath + path;
                }
            }

            mappedPath = mappedPath.Replace('/', '\\');

            if (mappedPath.EndsWith("\\", StringComparison.Ordinal) && !mappedPath.EndsWith(":\\", StringComparison.Ordinal)) {
                mappedPath = mappedPath.Substring(0, mappedPath.Length-1);
            }

            return mappedPath;
        }

        public override void SendStatus(int statusCode, string statusDescription) {
            _responseStatus = statusCode;
        }

        public override void SendKnownResponseHeader(int index, string value) {
            if (_headersSent) {
                return;
            }

            switch (index) {
                case HttpWorkerRequest.HeaderServer:
                case HttpWorkerRequest.HeaderDate:
                case HttpWorkerRequest.HeaderConnection:
                    // ignore these
                    return;
                case HttpWorkerRequest.HeaderAcceptRanges:
                    if (value == "bytes") {
                        // use this header to detect when we're processing a static file
                        _specialCaseStaticFileHeaders = true;
                        return;
                    }
                    break;
                case HttpWorkerRequest.HeaderExpires:
                case HttpWorkerRequest.HeaderLastModified:
                    if (_specialCaseStaticFileHeaders) {
                        // NOTE: Ignore these for static files. These are generated
                        //       by the StaticFileHandler, but they shouldn't be.
                        return;
                    }
                    break;
            }

            _responseHeadersBuilder.Append(GetKnownResponseHeaderName(index));
            _responseHeadersBuilder.Append(": ");
            _responseHeadersBuilder.Append(value);
            _responseHeadersBuilder.Append("\r\n");
        }

        public override void SendUnknownResponseHeader(string name, string value) {
            if (_headersSent)
                return;

            _responseHeadersBuilder.Append(name);
            _responseHeadersBuilder.Append(": ");
            _responseHeadersBuilder.Append(value);
            _responseHeadersBuilder.Append("\r\n");
        }

        public override void SendCalculatedContentLength(int contentLength) {
            if (!_headersSent) {
                _responseHeadersBuilder.Append("Content-Length: ");
                _responseHeadersBuilder.Append(contentLength.ToString(CultureInfo.InvariantCulture));
                _responseHeadersBuilder.Append("\r\n");
            }
        }

        public override bool HeadersSent() {
            return _headersSent;
        }

        public override bool IsClientConnected() {
            _connectionPermission.Assert();
            return _connection.Connected;
        }

        public override void CloseConnection() {
            _connectionPermission.Assert();
            _connection.Close();
        }

        public override void SendResponseFromMemory(byte[] data, int length) {
            if (length > 0) {
                byte[] bytes = new byte[length];

                Buffer.BlockCopy(data, 0, bytes, 0, length);
                _responseBodyBytes.Add(bytes);
            }
        }

        public override void SendResponseFromFile(string filename, long offset, long length) {
            if (length == 0) {
                return;
            }

            FileStream f = null;
            try {
                f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                SendResponseFromFileStream(f, offset, length);
            }
            finally {
                if (f != null) {
                    f.Close();
                }
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length) {
            if (length == 0) {
                return;
            }

            FileStream f = null;
            try {
                SafeFileHandle sfh = new SafeFileHandle(handle, false);
                f = new FileStream(sfh, FileAccess.Read);
                SendResponseFromFileStream(f, offset, length);
            }
            finally {
                if (f != null) {
                    f.Close();
                    f = null;
                }
            }
        }

        void SendResponseFromFileStream(FileStream f, long offset, long length)  {
            long fileSize = f.Length;

            if (length == -1) {
                length = fileSize - offset;
            }

            if (length == 0 || offset < 0 || length > fileSize - offset) {
                return;
            }

            if (offset > 0) {
                f.Seek(offset, SeekOrigin.Begin);
            }

            if (length <= MaxChunkLength) {
                byte[] fileBytes = new byte[(int)length];
                int bytesRead = f.Read(fileBytes, 0, (int)length);
                SendResponseFromMemory(fileBytes, bytesRead);
            }
            else {
                byte[] chunk = new byte[MaxChunkLength];
                int bytesRemaining = (int)length;

                while (bytesRemaining > 0) {
                    int bytesToRead = (bytesRemaining < MaxChunkLength) ? bytesRemaining : MaxChunkLength;
                    int bytesRead = f.Read(chunk, 0, bytesToRead);

                    SendResponseFromMemory(chunk, bytesRead);
                    bytesRemaining -= bytesRead;

                    // flush to release keep memory
                    if ((bytesRemaining > 0) && (bytesRead > 0)) {
                        FlushResponse(false);
                    }
                }
            }
        }

        public override void FlushResponse(bool finalFlush) {
            _connectionPermission.Assert();

            if (!_headersSent) {
                _connection.WriteHeaders(_responseStatus, _responseHeadersBuilder.ToString());
                _headersSent = true;
            }

            for (int i = 0; i < _responseBodyBytes.Count; i++) {
                byte[] bytes = _responseBodyBytes[i];
                _connection.WriteBody(bytes, 0, bytes.Length);
            }

            _responseBodyBytes = new List<byte[]>();

            if (finalFlush) {
                _connection.Close();
            }
        }

        public override void EndOfRequest() {
            Connection conn = _connection;

            if (conn != null) {
                _connection = null;
                _server.OnRequestEnd(conn);
            }
        }
    }
}
