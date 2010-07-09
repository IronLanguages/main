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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Chiron {
    static class HtmlFormatter {

#region HTML templates

        public const string Style =
@"body {font-family:""Verdana"";font-weight:normal;font-size: 8pt;color:black;} 
p {font-family:""Verdana"";font-weight:normal;color:black;margin-top: -5px}
b {font-family:""Verdana"";font-weight:bold;color:black;margin-top: -5px}
h1 { font-family:""Verdana"";font-weight:normal;font-size:18pt;color:red }
h2 { font-family:""Verdana"";font-weight:normal;font-size:14pt;color:maroon }
pre {font-family:""Lucida Console"";font-size: 9pt}
.marker {font-weight: bold; color: black;text-decoration: none;}
.version {color: gray;}
.error {margin-bottom: 10px;}
.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }
";

        const string ErrorFormat =
@"<html>
<head>
<title>{0}</title>
<link rel=""stylesheet"" type=""text/css"" href=""/style.css!"" />
</head>
<body bgcolor=""white"">
<span><h1><img src=""/sl.png!"" />&nbsp;Server Error<hr width=100% size=1 color=silver></h1>
<h2><i>HTTP Error {1} - {2}.</i> </h2></span>
<hr width=100% size=1 color=silver>
<b>Version Information:</b>&nbsp;Chiron/{3}
</body>
</html>
";

        const string DirListingFormat =
@"<html>
<head>
<title>Directory Listing -- {0}</title>
<link rel=""stylesheet"" type=""text/css"" href=""/style.css!"" />
</head>
<body bgcolor=""white"">
<h2><img src=""/sl.png!"" />&nbsp;<i>Directory Listing -- {1}</i> </h2></span>
<hr width=100% size=1 color=silver>
<pre>
{2}
</pre>
<hr width=100% size=1 color=silver>
<b>Version Information:</b>&nbsp;Chiron/{3}
</body>
</html>
";

        const string XapListingFormat =
@"<html>
<head>
<title>XAP Archive Listing -- {0}</title>
<link rel=""stylesheet"" type=""text/css"" href=""/style.css!"" />
</head>
<body bgcolor=""white"">
<h2><img src=""/sl.png!"" />&nbsp;<i>XAP Archive Listing -- {1}</i> </h2></span>
<hr width=100% size=1 color=silver>
<pre>
{2}
</pre>
<hr width=100% size=1 color=silver>
<b>Version Information:</b>&nbsp;Chiron/{3}
</body>
</html>
";

#endregion

        public static string GenerateErrorBody(int statusCode, string statusText, string message) {
            string body = string.Format(ErrorFormat, statusText, statusCode, statusText,
                typeof(Chiron).Assembly.GetName().Version.ToString());
            if (!string.IsNullOrEmpty(message))
                body += "\r\n<!--\r\n" + message + "\r\n-->";
            return body;
        }

        public static string FormatDirectoryListing(string dirPath, string parentPath, IList<FileSystemInfo> elements) {
            StringBuilder sb = new StringBuilder();

            if (parentPath != null) {
                if (!parentPath.EndsWith("/"))
                    parentPath += "/";
                sb.AppendFormat("<a href=\"{0}\">[To Parent Directory]</a>\r\n", parentPath);
            }

            foreach (FileSystemInfo e in elements) {
                if (e is FileInfo) {
                    if (e.Name.EndsWith(".xap", StringComparison.OrdinalIgnoreCase)) {
                        // special handling of XAP files to allow listing requests
                        sb.AppendFormat(
@"{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} <a href=""{2}/"">{3}</a>&nbsp;<a href=""{4}""><img border=""0"" src=""/slx.png!"" title=""Download XAP file"" /></a>
", e.LastWriteTime, ((FileInfo)e).Length, e.Name, e.Name, e.Name);
                    }
                    else if (string.IsNullOrEmpty(HttpSocket.GetMimeType(e.Name))) {
                        sb.AppendFormat(
@"{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} {2}</a>
", e.LastWriteTime, ((FileInfo)e).Length, e.Name);
                    }
                    else {
                        sb.AppendFormat(
@"{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} <a href=""{2}"">{3}</a>
", e.LastWriteTime, ((FileInfo)e).Length, e.Name, e.Name);
                    }
                }
                else if (e is DirectoryInfo) {
                    sb.AppendFormat(
@"{0,38:dddd, MMMM dd, yyyy hh:mm tt}        [dir] <a href=""{1}/"">{2}</a>&nbsp;<a href=""{3}.xap""><img border=""0"" src=""/slx.png!"" title=""Create XAP file from directory contents"" /></a>
",
e.LastWriteTime, e.Name, e.Name, e.Name);
                }
            }

            return string.Format(DirListingFormat, dirPath, dirPath, sb.ToString(),
                typeof(Chiron).Assembly.GetName().Version.ToString());
        }

        public static string FormatXapListing(string xapPath, IList<ZipArchiveFile> elements) {
            StringBuilder sb = new StringBuilder();

            foreach (ZipArchiveFile f in elements) {
                if (string.IsNullOrEmpty(HttpSocket.GetMimeType(f.Name))) {
                    sb.AppendFormat(
@"{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} {2}</a>
", f.LastWriteTime, f.Length, f.Name);
                }
                else {
                    sb.AppendFormat(
@"{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} <a href=""{2}?{3}"">{4}</a>
", f.LastWriteTime, f.Length, xapPath, f.Name, f.Name);
                }
            }

            return string.Format(XapListingFormat, xapPath, xapPath, sb.ToString(),
                typeof(Chiron).Assembly.GetName().Version.ToString());
        }
    }
}
