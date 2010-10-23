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
using System.IO;
using System.Diagnostics;
using System.Web;
using System.Web.Hosting;

namespace Microsoft.Scripting.AspNet.Util {
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
