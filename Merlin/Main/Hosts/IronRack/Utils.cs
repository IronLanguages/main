/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Web;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using System.IO;
using System.Configuration;

namespace IronRuby.Rack {
    public class Utils {
        private static TextWriter _LogWriter;
        private static FileStream _logStream;
        
        internal const string LogOptionName = "Log";

        internal static void ReportError(HttpContext/*!*/ context, Exception/*!*/ e) {
            var trace = RubyEngine.Engine.GetService<ExceptionOperations>().FormatException(e);

            context.Response.Write("<html>\r\n");
            context.Response.Write(String.Format(@"
<h4>Error: {0}</h4>
<pre>
{1}
</pre>
", HttpUtility.HtmlEncode(e.Message), HttpUtility.HtmlEncode(trace)));

            context.Response.Write("<h4>Search paths</h4>\r\n");
            context.Response.Write("<pre>\r\n");
            foreach (var path in RubyEngine.Engine.GetSearchPaths()) {
                context.Response.Write(HttpUtility.HtmlEncode(path));
                context.Response.Write("\r\n");
            }
            context.Response.Write("</pre>\r\n");
            context.Response.Write("</html>\r\n");
        }

        internal static Hash CreateEnv(HttpContext/*!*/ context) {
            var result = new Hash((RubyContext)HostingHelpers.GetLanguageContext(RubyEngine.Engine));

            var vars = context.Request.ServerVariables;

            foreach (string name in vars.AllKeys) {
                result.Add(MutableString.Create(name), MutableString.Create(String.Join(",", vars.GetValues(name))));
            }

            result[MutableString.Create("CONTENT_LENGTH")] = context.Request.ContentLength;
            return result;
        }

        internal static void InitializeLog() {
            var logPath = ConfigurationManager.AppSettings[LogOptionName];
            if (logPath != null) {
                if (!Path.IsPathRooted(logPath)) {
                    logPath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, logPath);
                }

                _logStream = File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                _LogWriter = new StreamWriter(_logStream);
            }
        }
        
        public static void Log(string/*!*/ message) {
            if (_LogWriter != null) {
                lock (_LogWriter) {
                    _LogWriter.WriteLine(message.Replace("\n", "\r\n\t"));
                    _LogWriter.Flush();
                }
            }
        }
    }
}
