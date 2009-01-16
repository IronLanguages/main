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
using System.Dynamic;
using System.Text;
using System.Windows.Browser;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Hosting.Providers;


namespace Microsoft.Scripting.Silverlight {

    internal static class ErrorFormatter {

        // Template for generating error HTML. Parameters are:
        // 0 - message
        // 1 - source file
        // 2 - source code at error location
        // 3 - error type
        // 4 - stack trace
        static string _ErrorHtmlTemplate = @"
<div id=""silverlightDlrErrorReport"" class=""silverlightDlrErrorReport"">

<div id=""silverlightDlrErrorWrapper"" class=""silverlightDlrErrorWrapper"">

  <h2 id=""silverlightDlrErrorMessage"" class=""silverlightDlrErrorMessage"">{0}</h2>

  <div class=""silverlightDlrErrorSource"">
    
    <div id=""silverlightDlrErrorSourceFile"" class=""silverlightDlrErrorSourceFile"">{1}</div>        
    <code id=""silverlightDlrErrorSourceCode"" class=""silverlightDlrErrorSourceCode"">{2}</code>
  
  </div>  
  
  <div class=""silverlightDlrErrorDetails"">
    
    <div id=""silverlightDlrErrorType"" class=""silverlightDlrErrorType"">{3}</div>        
    <code id=""silverlightDlrErrorStackTrace"" class=""silverlightDlrErrorStackTrace"">{4}</code>

  </div>

</div>

<div class=""silverlightDlrErrorMenu"" id=""silverlightDlrErrorHeader"" style=""display: none"">
  <a href=""javascript:void(0);"" onclick=""document.getElementById('silverlightDlrErrorFooter').style.display = 'block'; document.getElementById('silverlightDlrErrorWrapper').style.display = 'block'; document.getElementById('silverlightDlrErrorHeader').style.display = 'none'; document.body.style.paddingBottom = '15px';"">&uArr; {0}</a>
</div>

<div class=""silverlightDlrErrorMenu"" id=""silverlightDlrErrorFooter"">
  <a href=""javascript:void(0);"" onclick=""document.getElementById('silverlightDlrErrorFooter').style.display = 'none'; document.getElementById('silverlightDlrErrorWrapper').style.display = 'none'; document.getElementById('silverlightDlrErrorHeader').style.display = 'block'; document.body.style.paddingBottom = '15px';"">&dArr; Hide</a>
</div>

</div>
";
        // template for highlighted error line, inserted into the silverlightDlrErrorSourceCode div in the template above
        const string _ErrorLineTemplate = @"<span id=""silverlightDlrErrorLine"" class=""silverlightDlrErrorLine"">{0}</span>";


        static volatile bool _displayedError = false;

        internal static void DisplayError(string targetElementId, Exception e) {
            // we only support displaying one error
            if (_displayedError) {
                return;
            }
            _displayedError = true;
            try {

                HtmlElement target = HtmlPage.Document.GetElementById(targetElementId);
                if (target == null) {
                    // Create a div with this ID
                    target = HtmlPage.Document.CreateElement("div");
                    if (!string.IsNullOrEmpty(targetElementId)) {
                        target.Id = targetElementId;
                    }

                    (HtmlPage.Document.GetElementsByTagName("body")[0] as HtmlElement).AppendChild(target);
                }

                target.SetProperty("innerHTML", FormatErrorAsHtml(e));

            } catch (Exception ex) {
                HtmlPage.Document.GetElementById(targetElementId).SetProperty("innerHTML", EscapeHtml(ex.ToString()));
            }
        }

        private static string FormatErrorAsHtml(Exception e) {

            // Get information about this exception object
            DynamicExceptionInfo err = new DynamicExceptionInfo(e);

            // Template for generating error HTML. Parameters are:
            // 0 - message
            // 1 - source file
            // 2 - source code at error location
            // 3 - error type
            // 4 - stack trace
            return string.Format(
                _ErrorHtmlTemplate,
                EscapeHtml(err.Message),
                err.SourceFileName != null ? EscapeHtml(err.SourceFileName) : "",
                FormatSourceCode(err),
                EscapeHtml(err.ErrorTypeName),
                FormatStackTrace(err));
        }

        // Print the line with the error plus some context lines
        private static string FormatSourceCode(DynamicExceptionInfo err) {
            var sourceFile = err.SourceFileName;
            int line = err.SourceLine;

            if (sourceFile == null || line <= 0) {
                return "";
            }

            var stream = Package.GetFile(sourceFile);
            if (stream == null) {
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
                        text.AppendFormat(_ErrorLineTemplate, lineText);
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

        // Gets the stack trace using whatever information we have available
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

                if (DynamicApplication.Current.ExceptionDetail) {
                    html.Append("<br/>CLR Stack Trace:<br/>");
                    html.Append(EscapeHtml(ex.StackTrace != null ? ex.StackTrace : ex.ToString()));
                }
                return html.ToString();
            }

            return EscapeHtml(ex.StackTrace != null ? ex.StackTrace : ex.ToString());
        }

        private static string EscapeHtml(string str) {
            return HttpUtility.HtmlEncode(str).Replace(" ", "&nbsp;").Replace("\n", "<br />");
        }

        internal class Sink : ErrorListener {
            public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity) {
                throw new SyntaxErrorException(message, HostingHelpers.GetSourceUnit(source), span, errorCode, severity);
            }
        }

        /// <summary>
        /// Utility class to encapsulate all of the information we retrieve, starting from
        /// the Exception object.
        /// </summary>
        private class DynamicExceptionInfo {
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

                // We can get the source code context either from the DynamicStackFrame or from
                // a SyntaxErrorException
                SyntaxErrorException se = e as SyntaxErrorException;
                if (null != se) {
                    _sourceFileName = se.GetSymbolDocumentName();
                    _sourceLine = se.Line;
                } else if (_dynamicStackFrames != null && _dynamicStackFrames.Length > 0) {
                    _sourceFileName = _dynamicStackFrames[0].GetFileName();
                    _sourceLine = _dynamicStackFrames[0].GetFileLineNumber();
                }

                ScriptEngine engine;
                if (_sourceFileName != null &&
                    DynamicApplication.Current.Runtime.TryGetEngineByFileExtension(System.IO.Path.GetExtension(_sourceFileName), out engine)) {
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
