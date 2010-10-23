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

        private CallSite<Func<CallSite, object, object, object>> _WriteSite;

        internal void WriteMessage(MutableString/*!*/ message) {
            if (_WriteSite == null) {
                Interlocked.CompareExchange(
                    ref _WriteSite,
                    CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(_context, "write", 1)),
                    null
                );
            }

            _WriteSite.Target(_WriteSite, _context.StandardErrorOutput, message);
        }

        public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity) {
            if (severity == Severity.Warning && !ReportWarning(_context.Verbose, errorCode)) {
                return;
            }

            CountError(severity);

            string path;
            string codeLine;
            RubyEncoding encoding;
            int line = span.Start.Line;
            if (sourceUnit != null) {
                path = sourceUnit.Path;
                using (SourceCodeReader reader = sourceUnit.GetReader()) {
                    if (line > 0) {
                        try {
                            reader.SeekLine(line);
                            codeLine = reader.ReadLine();
                        } catch (Exception) {
                            codeLine = null;
                        }
                    } else {
                        codeLine = null;
                    }
                    encoding = reader.Encoding != null ? RubyEncoding.GetRubyEncoding(reader.Encoding) : RubyEncoding.UTF8;
                }
            } else {
                path = null;
                codeLine = null;
                encoding = RubyEncoding.UTF8;
            }

            if (severity == Severity.Error || severity == Severity.FatalError) {
                throw new SyntaxError(message, path, line, span.Start.Column, codeLine);
            } else {
                WriteMessage(
                    MutableString.Create(RubyContext.FormatErrorMessage(message, "warning", path, line, span.Start.Column, null), encoding)
                );
            }
        }

        private static bool ReportWarning(object verbose, int errorCode) {
            return verbose is bool && ((bool)verbose || !Errors.IsVerboseWarning(errorCode));
        }
    }
}
