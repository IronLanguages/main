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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Browser;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Silverlight {

    public static class ErrorFormatter {

        #region Error HTML Template
        private const string _errorReportId     = "silverlightDlrErrorReport";
        private const string _errorMessageId    = "silverlightDlrErrorMessage";
        private const string _errorSourceId     = "silverlightDlrErrorSource";
        private const string _errorSourceFileId = "silverlightDlrErrorSourceFile";
        private const string _errorSourceCodeId = "silverlightDlrErrorSourceCode";
        private const string _errorDetailsId    = "silverlightDlrErrorDetails";
        private const string _errorTypeId       = "silverlightDlrErrorType";
        private const string _errorStackTraceId = "silverlightDlrErrorStackTrace";
        private const string _errorLineId       = "silverlightDlrErrorLine";

        // Template for generating error HTML. Parameters are:
        // 0 - message
        // 1 - source file
        // 2 - source code at error location
        // 3 - error type
        // 4 - stack trace
        // 5 - error report id/class
        // 6 - error message id/class
        // 7 - error source id/class
        // 8 - error file id/class
        // 9 - error source code id/class
        // 10 - error details id/class
        // 11 - error type id/class
        // 12 - error stack trace id/class
        private static string _errorHtmlTemplate = @"
<!-- error report html -->
<h2 class=""{6}"" id=""{6}"">{0}</h2>
<div class=""{7}"">
    <div class=""{8}"" id=""{8}"">{1}</div>
    <code class=""{9}"" id=""{9}"">
        {2}
    </code>
</div>  
<div class=""{10}"">
    <div class=""{11}"" id=""{11}"">{3}</div>        
    <code class=""{12}"" id=""{12}""> 
        {4}
    </code>
</div>";

        // template for highlighted error line, inserted into the silverlightDlrErrorSourceCode div in the template above
        private const string _errorLineTemplate = @"<span id=""{1}"" class=""{1}"">{0}</span>";
        #endregion

        static volatile bool _displayedError = false;
        static ScriptRuntime _runtime;

        /// <summary>
        /// Displays an error
        /// </summary>
        /// <param name="targetElementId">HTML id to put error information into</param>
        /// <param name="e">Exception to get error info out of</param>
        internal static void DisplayError(string targetElementId, Exception e) {
            // we only support displaying one error
            if (_displayedError) {
                return;
            }
            _displayedError = true;

            // keep track of the runtime if it exists
            if (DynamicApplication.Current.Engine != null) {
                _runtime = DynamicApplication.Current.Engine.Runtime;
            }

            // show the window in the targetElementId
            Window.Show(targetElementId);

            // show the Repl if we can get to the current engine
            if (DynamicApplication.Current.Engine != null) {
                Repl.Show();
            }

            // format the Exception
            string result;
            try {
                result = FormatErrorAsHtml(e);
            } catch (Exception ex) {
                result = EscapeHtml(ex.ToString());
            }

            // Create a "div" with class/id set as the _errorReportId, and put
            // formatted exception into it.
            var report = HtmlPage.Document.CreateElement("div");
            report.Id = report.CssClass = _errorReportId;
            report.SetProperty("innerHTML", result);

            // Adds a new panel to the "Window", initialize it, and force the panel to be shown.
            Window.Current.AddPanel("Error Report (" + EscapeHtml(new DynamicExceptionInfo(e).ErrorTypeName) + ")", report);
            Window.Current.Initialize();
            Window.Current.ShowPanel(report.Id);
        }

        /// <summary>
        /// Get the dynamic exception info from the CLR exception, and format
        /// it with the _errorHtmlTemplate.
        /// </summary>
        internal static string FormatErrorAsHtml(Exception e) {

            // Get information about this exception object
            DynamicExceptionInfo err = new DynamicExceptionInfo(e);

            return string.Format(
                _errorHtmlTemplate,
                EscapeHtml(err.Message),
                err.SourceFileName != null ? EscapeHtml(err.SourceFileName) : "",
                FormatSourceCode(err),
                EscapeHtml(err.ErrorTypeName),
                FormatStackTrace(err),
                _errorReportId,
                _errorMessageId,
                _errorSourceId,
                _errorSourceFileId,
                _errorSourceCodeId,
                _errorDetailsId,
                _errorTypeId,
                _errorStackTraceId
            );
        }

        /// <summary>
        /// Render the line with the error plus some context lines
        /// </summary>
        private static string FormatSourceCode(DynamicExceptionInfo err) {
            var sourceFile = err.SourceFileName;
            int line = err.SourceLine;

            if (sourceFile == null || line <= 0) {
                return "";
            }

            Stream stream = null;
            try {
                stream = _runtime.Host.PlatformAdaptationLayer.OpenInputFileStream(sourceFile);
                if (stream == null) {
                    return "";
                }
            } catch (IOException) {
                return "";
            }

            int maxLen = (line + 2).ToString().Length;
            var text = new StringBuilder();

            using (StreamReader reader = new StreamReader(stream)) {
                for (int i = 1; i <= line + 2; ++i) {
                    string lineText = reader.ReadLine();
                    if (null == lineText) {
                        break;
                    }
                    if (i < line - 2) {
                        continue;
                    }
                    string lineNum = i.ToString();
                    text.Append("Line ").Append(' ', maxLen - lineNum.Length).Append(lineNum).Append(": ");
                    lineText = EscapeHtml(lineText);
                    if (i == line) {
                        text.AppendFormat(_errorLineTemplate, lineText, _errorLineId);
                    } else {
                        text.Append(lineText);
                    }
                    if (i != line + 2) {
                        text.Append("<br />");
                    }
                }
            }

            return text.ToString();
        }

        /// <summary>
        /// Gets the stack trace using whatever information we have available
        /// </summary>
        private static string FormatStackTrace(DynamicExceptionInfo err) {
            Exception ex = err.Exception;

            if (ex is SyntaxErrorException) {
                return ""; // no meaningful stack trace for syntax errors
            }

            DynamicStackFrame[] dynamicFrames = err.DynamicStackFrames;

            if (dynamicFrames != null && dynamicFrames.Length > 0) {
                // We have a stack trace, either dynamic or directly from the exception
                StringBuilder html = new StringBuilder();
                foreach (DynamicStackFrame frame in dynamicFrames) {
                    if (html.Length != 0) {
                        html.Append("<br />");
                    }
                    html.AppendFormat(
                        "  at {0} in {1}, line {2}",
                        EscapeHtml(frame.GetMethodName()),
                        frame.GetFileName() != null ? EscapeHtml(frame.GetFileName()) : null,
                        frame.GetFileLineNumber()
                    );
                }

                if (Settings.ExceptionDetail) {
                    html.Append("<br/>CLR Stack Trace:<br/>");
                    html.Append(EscapeHtml(ex.StackTrace != null ? ex.StackTrace : ex.ToString()));
                }
                return html.ToString();
            }

            return EscapeHtml(ex.StackTrace != null ? ex.StackTrace : ex.ToString());
        }

        /// <summary>
        /// HtmlEncode, and escape spaces and newlines.
        /// </summary>
        public static string EscapeHtml(string str) {
            return HttpUtility.HtmlEncode(str).Replace(" ", "&nbsp;").Replace("\n", "<br />");
        }

        /// <summary>
        /// Class used to handle syntax errors and throw the proper exception.
        /// </summary>
        public class Sink : ErrorListener {
            public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity) {
                throw new SyntaxErrorException(message, HostingHelpers.GetSourceUnit(source), span, errorCode, severity);
            }
        }

        /// <summary>
        /// Utility class to encapsulate all of the information we retrieve, starting from
        /// the Exception object.
        /// </summary>
        public class DynamicExceptionInfo {
            public Exception Exception {
                get { return _exception; }
            }

            public DynamicStackFrame[] DynamicStackFrames {
                get { return _dynamicStackFrames; }
            }

            public string SourceFileName {
                get { return _sourceFileName; }
            }

            public int SourceLine {
                get { return _sourceLine; }
            }

            public string Message {
                get { return _message; }
            }

            public string ErrorTypeName {
                get { return _errorTypeName; }
            }

            private Exception _exception;
            private DynamicStackFrame[] _dynamicStackFrames;
            private string _sourceFileName, _message, _errorTypeName;
            private int _sourceLine;

            public DynamicExceptionInfo(Exception e) {
                ContractUtils.RequiresNotNull(e, "e");

                _exception = e;
                _dynamicStackFrames = ScriptingRuntimeHelpers.GetDynamicStackFrames(e);

                // We can get the file name and line number from either the 
                // DynamicStackFrame or from a SyntaxErrorException
                SyntaxErrorException se = e as SyntaxErrorException;
                if (null != se) {
                    _sourceFileName = se.GetSymbolDocumentName();
                    _sourceLine = se.Line;
                } else if (_dynamicStackFrames != null && _dynamicStackFrames.Length > 0) {
                    _sourceFileName = _dynamicStackFrames[0].GetFileName();
                    _sourceLine = _dynamicStackFrames[0].GetFileLineNumber();
                }

                // Try to get the ScriptEngine from the source file's extension;
                // if that fails just use the current ScriptEngine
                ScriptEngine engine = null;
                try {
                    if (_sourceFileName != null) {
                        var extension = System.IO.Path.GetExtension(_sourceFileName);
                        _runtime.TryGetEngineByFileExtension(extension, out engine);
                    } else {
                        throw new Exception(); 
                    }
                } catch {
                    if (DynamicApplication.Current.Engine != null) {
                        engine = DynamicApplication.Current.Engine.Engine;
                    }
                }

                // If we have the file name and the engine, use ExceptionOperations
                // to generate the exception message. Otherwise, create it by hand
                if (_sourceFileName != null && engine != null) {
                    ExceptionOperations es = engine.GetService<ExceptionOperations>();
                    es.GetExceptionMessage(_exception, out _message, out _errorTypeName);
                } else {
                    _errorTypeName = _exception.GetType().Name;
                    _message = _errorTypeName + ": " + _exception.Message;
                }
            }
        }
    }
}
