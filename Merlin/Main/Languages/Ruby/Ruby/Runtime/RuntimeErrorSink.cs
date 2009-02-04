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
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using System.Runtime.CompilerServices;
using System.Threading;
using IronRuby.Runtime.Calls;
using IronRuby.Compiler;

namespace IronRuby.Runtime {
    /// <summary>
    /// Thread-safe.
    /// </summary>
    public class RuntimeErrorSink : ErrorCounter {
        private readonly RubyContext/*!*/ _context;

        internal RuntimeErrorSink(RubyContext/*!*/ context) {
            Assert.NotNull(context);
            _context = context;
        }

        private CallSite<Func<CallSite, RubyContext, object, object, object>> _WriteSite;

        public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity) {
            if (severity == Severity.Warning && !ReportWarning(_context.Verbose, errorCode)) {
                return;
            }

            CountError(severity);

            string path;
            string codeLine;
            int line = span.Start.Line;
            if (sourceUnit != null) {
                path = sourceUnit.Path;
                codeLine = (line > 0) ? sourceUnit.GetCodeLine(line) : null;
            } else {
                path = null;
                codeLine = null;
            }

            if (severity == Severity.Error || severity == Severity.FatalError) {
                throw new SyntaxError(message, path, line, span.Start.Column, codeLine);
            } else {

                if (_WriteSite == null) {
                    Interlocked.CompareExchange(
                        ref _WriteSite,
                        CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(RubyCallAction.Make("write", 1)),
                        null
                    );
                }

                message = RubyContext.FormatErrorMessage(message, "warning", path, line, span.Start.Column, null);

                _WriteSite.Target(_WriteSite, _context, _context.StandardErrorOutput, MutableString.CreateMutable(message));
            }
        }

        private static bool ReportWarning(object verbose, int errorCode) {
            return verbose is bool && ((bool)verbose || !Errors.IsVerboseWarning(errorCode));
        }
    }
}
