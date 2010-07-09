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

using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Chiron {
    class HttpSocket {
        Socket _socket;

        int _statusCode, _bytesSent;
        string _message;

        // logging
        public int StatusCode { get { return _statusCode; } set { _statusCode = value; } }
        public int BytesSent { get { return _bytesSent; } set { _bytesSent = value; } }
        public string Message { get { return _message; } set { _message = value; } }

        public HttpSocket(Socket socket) {
            _socket = socket;
        }

        public void Close() {
            try {
                if (_socket != null) {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                }
            }
            catch {} finally {
                _socket = null;
            }
        }

#region Reading Request

        public bool TryReadRequest(out HttpRequestData r) {
            r = new HttpRequestData();

            // wait for some request bytes
            if (WaitForRequestBytes() == 0) return false;

            // reader header bytes until \r\n\r\n
            const int MaxHeaderBytes = 32000;
            const byte CR = ((byte)'\r');
            const byte LF = ((byte)'\n');

            byte[] hb = null;
            int hbLen = -1;

            byte[] body = null;
            int bodyLen = 0;

            do {
                byte[] newBytes = ReadRequestBytes(MaxHeaderBytes);
                if (newBytes == null) return false;

                if (hb == null) {
                    hb = newBytes;
                }
                else {
                    int len = hb.Length + newBytes.Length;
                    if (len > MaxHeaderBytes) return false;

                    byte[] bytes = new byte[len];
                    Buffer.BlockCopy(hb, 0, bytes, 0, hb.Length);
                    Buffer.BlockCopy(newBytes, 0, bytes, hb.Length, newBytes.Length);
                    hb = bytes;
                }

                // look for \r\n\r\n in hb
                int l = hb.Length;
                for (int i = 0; i < l - 1; i++) {
                    if (i < l - 3 && hb[i] == CR && hb[i + 1] == LF && hb[i + 2] == CR && hb[i + 3] == LF) {
                        hbLen = i + 3;
                        break;
                    }
                    else if (hb[i] == LF && hb[i + 1] == LF) {
                        hbLen = i + 1;
                        break;
                    }
                }
            }
            while (hbLen < 0);

            // store the initial body chunk
            if (hbLen < hb.Length) {
                bodyLen = hb.Length - hbLen;
                body = new byte[bodyLen];
                Buffer.BlockCopy(hb, hbLen, body, 0, bodyLen);
            }

            // convert headers to strings
            string[] headers = Encoding.UTF8.GetString(hb, 0, hbLen).Replace("\r\n", "\n").Split('\n');

            // parse request line
            string[] firstLine = headers[0].Split(' ');
            if (firstLine.Length < 2) return false;
            r.Method = firstLine[0].Trim();
            r.Uri = firstLine[1].Trim();

            // parse headers
            int contentLength = -1;

            for (int i = 1; i < headers.Length; i++) {
                string h = headers[i];
                int j = h.IndexOf(':');
                if (j > 0) {
                    string k = h.Substring(0, j).Trim();
                    string v = h.Substring(j + 1).Trim();
                    r.Headers.Add(new KeyValuePair<string, string>(k, v));

                    if (string.Compare(k, "content-length", StringComparison.OrdinalIgnoreCase) == 0) {
                        if (!int.TryParse(v, out contentLength)) contentLength = -1;
                    }
                }
            }

            // store the body from the first chunk
            if (bodyLen > 0) r.Body.Add(body);

            // read remaining body, if any
            if (contentLength > 0 && bodyLen < contentLength) {
                // 100 response to POST
                if (r.Method == "POST") WriteResponse(100, null, null, true);

                while (bodyLen < contentLength) {
                    byte[] bytes = ReadRequestBytes(contentLength - bodyLen);
                    if (bytes == null || bytes.Length == 0) {
                        return false;
                    }
                    bodyLen += bytes.Length;
                    r.Body.Add(bytes);
                }
            }

            return true;
        }

        int WaitForRequestBytes() {
            int availBytes = 0;

            try {
                if (_socket.Available == 0) {
                    // poll until there is data
                    _socket.Poll(100000 /* 100ms */, SelectMode.SelectRead);
                    if (_socket.Available == 0 && _socket.Connected) {
                        _socket.Poll(30000000 /* 30sec */, SelectMode.SelectRead);
                    }
                }

                availBytes = _socket.Available;
            }
            catch {
            }

            return availBytes;
        }

        byte[] ReadRequestBytes(int maxBytes) {
            try {
                if (WaitForRequestBytes() == 0)
                    return null;

                int numBytes = _socket.Available;
                if (numBytes > maxBytes)
                    numBytes = maxBytes;

                int numReceived = 0;
                byte[] buffer = new byte[numBytes];

                if (numBytes > 0) {
                    numReceived = _socket.Receive(buffer, 0, numBytes, SocketFlags.None);
                }

                if (numReceived < numBytes) {
                    byte[] tempBuffer = new byte[numReceived];

                    if (numReceived > 0) {
                        Buffer.BlockCopy(buffer, 0, tempBuffer, 0, numReceived);
                    }

                    buffer = tempBuffer;
                }

                return buffer;
            }
            catch {
                return null;
            }
        }


#endregion

#region Writing Response

        public void WriteResponse(int statusCode, String headers, byte[] body, bool keepAlive) {
            try {
                Socket s = _socket;
                if (s != null) {
                    int bodyLength = (body != null) ? body.Length : 0;
                    byte[] headerBytes = GenerateResponseHeaders(statusCode, headers, bodyLength, keepAlive);
                    s.Send(headerBytes);
                    if (bodyLength > 0) _socket.Send(body);
                    BytesSent += headerBytes.Length + bodyLength;
                }
            }
            catch (SocketException) {
            }
            finally {
                StatusCode = statusCode;
                if (!keepAlive) Close();
            }
        }

        public void WriteTextResponse(int statusCode, string textType, string body, bool keepAlive) {
            byte[] bodyBytes = string.IsNullOrEmpty(body) ? null : Encoding.UTF8.GetBytes(body);
            WriteResponse(statusCode, 
                string.Format("Content-type: text/{0}; charset=utf-8\r\n", textType),
                bodyBytes, keepAlive);
        }

        public void WriteBinaryResponse(int statusCode, string mimeType, byte[] body, bool keepAlive) {
            WriteResponse(statusCode,
                string.Format("Content-Type: {0}\r\n", mimeType),
                body, keepAlive);
        }

        public void WriteErrorResponse(int statusCode, string message) {
            string html = HtmlFormatter.GenerateErrorBody(statusCode, GetStatusCodeText(statusCode), message);
            WriteTextResponse(statusCode, "html", html, false);
            Message = message;
        }

        public void WriteErrorResponse(int statusCode) {
            WriteErrorResponse(statusCode, null);
        }

        public static string GetMimeType(string filename) {
            string mime;
            return Chiron.MimeMap.TryGetValue(Path.GetExtension(filename).ToLowerInvariant(), out mime) ? mime : null;
        }

#endregion

#region Helpers

        static Dictionary<int, string> StatusCodes;

        static HttpSocket() {
            StatusCodes = new Dictionary<int, string>();
            StatusCodes[100] = "Continue";
            StatusCodes[200] = "OK";
            StatusCodes[302] = "Found";
            StatusCodes[400] = "Bad Request";
            StatusCodes[403] = "Forbidden";
            StatusCodes[404] = "Not Found";
            StatusCodes[405] = "Method Not Allowed";
            StatusCodes[500] = "Server Error";
        }

        static byte[] GenerateResponseHeaders(int statusCode, string extraHeaders, int contentLength, bool keepAlive) {
            StringBuilder sb = new StringBuilder();
            Version ver = typeof(Chiron).Assembly.GetName().Version;
            sb.AppendFormat("HTTP/1.1 {0} {1}\r\n", statusCode, GetStatusCodeText(statusCode));
            sb.AppendFormat("Date: {0}\r\n", DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo));
            sb.AppendFormat("Server: Chiron/{0}.{1}\r\n", ver.Major, ver.MajorRevision);

            if (contentLength >= 0) sb.AppendFormat("Content-Length: {0}\r\n", contentLength);
            if (extraHeaders != null) sb.Append(extraHeaders);
            if (!keepAlive) sb.Append("Connection: Close\r\n");
            sb.Append("\r\n");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        static string GetStatusCodeText(int code) {
            string statusText;
            if (!StatusCodes.TryGetValue(code, out statusText))
                statusText = "Unexpected";
            return statusText;
        }

#endregion
    }
}

