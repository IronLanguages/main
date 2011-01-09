/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Configuration;
using System.IO;
using System.Web;
using Microsoft.Scripting.Hosting;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRubyRack {

    public class Utils {

        private static TextWriter _LogWriter;
        private static FileStream _logStream;
        
        internal const string LogOptionName = "Log";

        internal static void ReportError(HttpContext context, Exception e) {
            Utils.ReportErrorToLog(context, e);
#if !DEBUG
            if (Handler.AspNet.Current == null || Handler.AspNet.Current.App.RackEnv == "development") {
#endif
                Utils.ReportErrorToResponse(context, e);
                context.Response.StatusCode = 500;
#if !DEBUG
            }
#endif
        }

        internal static void ReportErrorToLog(HttpContext/*!*/ context, Exception/*!*/ e) {
            ReportErrorAsText(context, e, Log);
        }

        internal static void ReportErrorToResponse(HttpContext/*!*/ context, Exception/*!*/ e) {
            ReportErrorAsHTML(context, e, context.Response.Write);
        }

        internal static void ReportErrorAsText(HttpContext context, Exception e, Action<string> write) {
            if (IronRubyEngine.Engine != null) {
                var exops = IronRubyEngine.Engine.GetService<ExceptionOperations>();
                write(e.Message + "\n" + exops.FormatException(e) + "\n");
            } else {
                write(e.Message + "\n" + e.StackTrace + "\n");
            }
        }

        internal static void ReportErrorAsHTML(HttpContext/*!*/ context, Exception/*!*/ e, Action<string> write) {
            var trace = IronRubyEngine.Engine == null ? 
                e.Message + "\n" + e.StackTrace + "\n" : 
                IronRubyEngine.Engine.GetService<ExceptionOperations>().FormatException(e);

            write("<html>\r\n");
            write(String.Format(@"
<h4>Error: {0}</h4>
<pre>
{1}
</pre>
", HttpUtility.HtmlEncode(e.Message), HttpUtility.HtmlEncode(trace)));

            if (IronRubyEngine.Engine != null) {
                write("<h4>Search paths</h4>\r\n");
                write("<pre>\r\n");
                foreach (var path in IronRubyEngine.Engine.GetSearchPaths()) {
                    write(HttpUtility.HtmlEncode(path));
                    write("\r\n");
                }
                write("</pre>\r\n");
            
                try {
                    var gempaths = IronRubyEngine.Execute<RubyArray>("require 'rubygems'; Gem.path");
                    write("<h4>Gem paths</h4>\r\n");
                    write("<pre>\r\n");
                    foreach (var gempath in gempaths) {
                        write(HttpUtility.HtmlEncode(((MutableString)gempath).ToString()));
                        write("\r\n");
                    }
                    write("</pre>\r\n");
                } catch {
                    // who cares if it fails
                }
            }
            write("</html>\r\n");
        }
        
        public static void Log(string/*!*/ message) {
            if (_LogWriter == null) {
                var logPath = ConfigurationManager.AppSettings[LogOptionName];
                if (logPath != null) {
                    if (!Path.IsPathRooted(logPath)) {
                        logPath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, logPath);
                    }

                    _logStream = File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    _LogWriter = new StreamWriter(_logStream);
                }
            }
            if (_LogWriter != null) {
                lock (_LogWriter) {
                    _LogWriter.WriteLine(message.Replace("\n", "\n    "));
                    _LogWriter.Flush();
                }
            }
        }

        private static readonly JoinConversionStorage _joinStorage = new JoinConversionStorage(IronRubyEngine.Context);

        public static MutableString Join(RubyArray array, string joiner) {
            return IronRuby.Builtins.IListOps.Join(_joinStorage, array, MutableString.Create(joiner, RubyEncoding.UTF8));
        }
    }
}
