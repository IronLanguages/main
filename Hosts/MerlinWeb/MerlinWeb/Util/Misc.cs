using System;
using System.IO;
using System.Diagnostics;
using System.Web;
using System.Web.Hosting;

namespace Microsoft.Web.Scripting.Util {
    static class Misc {
        internal static void ThrowException(string message, Exception innerException,
            string virtualPath, int line) {
            throw new HttpParseException(message, innerException, virtualPath, null, line);
        }
        
        internal static string GetStringFromVirtualPath(string virtualPath) {
            using (Stream stream = VirtualPathProvider.OpenFile(virtualPath)) {
                using (TextReader reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }

        internal static string GetVirtualPathFromPhysicalPath(string physicalPath) {
            // If the physical path is not under the app, we can't get a virtual path for it
            if (!physicalPath.StartsWith(HttpRuntime.AppDomainAppPath, StringComparison.OrdinalIgnoreCase))
                return null;

            string virtualPath = "~/" + physicalPath.Substring(HttpRuntime.AppDomainAppPath.Length);
            return VirtualPathUtility.ToAbsolute(virtualPath);
        }

        internal static int LineCount(string text) {
            return LineCount(text, 0, text.Length);
        }

        internal static int LineCount(string text, int offset, int newoffset) {

            Debug.Assert(offset <= newoffset);

            int linecount = 0;

            while (offset < newoffset) {
                if (text[offset] == '\r' || (text[offset] == '\n' && (offset == 0 || text[offset - 1] != '\r')))
                    linecount++;
                offset++;
            }

            return linecount;
        }
    }
}
