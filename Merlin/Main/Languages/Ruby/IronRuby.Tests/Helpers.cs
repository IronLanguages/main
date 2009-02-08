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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Builtins;

namespace IronRuby.Tests {

    class ErrorLogRecord {
        public string Message;
        public SourceSpan Span;
        public int Code;
        public Severity Severity;
    }

    class LoggingErrorSink : ErrorCounter {

        public List<ErrorLogRecord> Errors = new List<ErrorLogRecord>();
        private bool _suppressOutput;

        public LoggingErrorSink() {
            _suppressOutput = true;
        }

        public LoggingErrorSink(bool suppressOutput) {
            _suppressOutput = suppressOutput;
        }

        public override void Add(SourceUnit source, string/*!*/ message, SourceSpan span, int errorCode, Severity severity) {
            base.CountError(severity);

            if (!_suppressOutput) {
                Console.Error.WriteLine("{0}({1}:{2}): {3}: RB{4}: {5}", source.Path, span.Start.Line, span.Start.Column,
                    severity, errorCode, message);
            }

            ErrorLogRecord info = new ErrorLogRecord();
            info.Message = message;
            info.Span = span;
            info.Code = errorCode;
            info.Severity = severity;
            Errors.Add(info);
        }
    }

    public class TestHelpers {
        public static int GetClassVersion([NotNull]RubyClass/*!*/ cls) {
            return cls.Version.Value;
        }
    }

    public partial class Tests {
        private readonly Driver/*!*/ _driver;
        private readonly Action[]/*!*/ _methods;

        public Action[] TestMethods { get { return _methods; } }
        public ScriptRuntime Runtime { get { return _driver.TestRuntime.ScriptRuntime; } }
        public ScriptEngine Engine { get { return _driver.TestRuntime.Engine; } }
        public RubyContext Context { get { return _driver.TestRuntime.Context; } }
        
        public string/*!*/ Eval(bool eval, string/*!*/ code) {
            if (eval) {
                return code.Replace("#<","eval %q{").Replace("#>", "}");
            } else {
                return code.Replace("#<", "").Replace("#>", "");
            }
        }

        public void LoadTestLibrary() {
            Context.ObjectClass.SetConstant("TestHelpers", Context.GetClass(typeof(TestHelpers)));
        }

        public void CompilerTest(string/*!*/ code) {
            CompilerTest(code, 0, 0);
        }

        public void CompilerTest(string/*!*/ code, int expectedCompilerWarningCount, int expectedRuntimeWarningCount) {
            LoggingErrorSink log = new LoggingErrorSink(true);
            CompilerTest(code, log);
            Assert(log.ErrorCount == 0 && log.FatalErrorCount == 0, "Compile time error");
            Assert(log.WarningCount == expectedCompilerWarningCount, "Wrong number of compile time errors/warnings");
            Assert(Context.RuntimeErrorSink.WarningCount == expectedRuntimeWarningCount, "Wrong number of runtime warnings");
        }

        public void CompilerTest(string/*!*/ code, ErrorSink/*!*/ sink) {
            Debug.Assert(code != null && sink != null);
            SourceUnit source;

            string name = _driver.TestRuntime.TestName;

            if (_driver.IsDebug) {
                string path = Path.Combine(Snippets.Shared.SnippetsDirectory, name + ".rb");
                Directory.CreateDirectory(Snippets.Shared.SnippetsDirectory);
                File.WriteAllText(path, code);
                source = _driver.TestRuntime.Context.CreateFileUnit(path);
            } else {
                source = _driver.TestRuntime.Context.CreateSnippet(code, name + ".rb", SourceCodeKind.File);
            }

            ScriptCode compiledCode = source.Compile(new RubyCompilerOptions(), sink);
            if (compiledCode != null) {
                compiledCode.Run(new Scope());
            }
        }

        public List<Tokens> GetRubyTokens(ErrorSink log, string source) {
            return GetRubyTokens(log, source, false);
        }

        public List<Tokens> GetRubyTokens(ErrorSink log, string source, bool dumpTokens) {
            return GetRubyTokens(Context, log, source, dumpTokens, false);
        }

        public static List<Tokens> GetRubyTokens(RubyContext context, ErrorSink log, string source, bool dumpTokens, bool dumpReductions) {
            Parser parser = new Parser();
            List<Tokens> tokens = new List<Tokens>();

            if (dumpTokens) {
                parser.Tokenizer.EnableLogging(1, Console.Out);
            }

            parser.TokenSink = delegate(Tokens token, SourceSpan span) {
                tokens.Add(token);
            };

#if DEBUG
            if (dumpReductions) {
                DefaultParserLogger.Attach(parser, Console.Out);
            }
#endif
            parser.Parse(context.CreateSnippet(source, SourceCodeKind.File), new RubyCompilerOptions(), log);

            //tokenizer.Initialize(SourceUnit.CreateSnippet(RB, source));
            //List<Tokens> tokens = new List<Tokens>();
            //Tokens token;
            //while ((token = tokenizer.GetNextToken()) != Tokens.EOF) {
            //    tokens.Add(token);
            //}

            return tokens;
        }        

#if !SILVERLIGHT
        private static int domainId = 0;

        private static AppDomain CreateDomain() {
            return AppDomain.CreateDomain("RemoteScripts" + domainId++);
        }
#endif

        [Flags]
        enum OutputFlags {
            None = 0,
            Raw = 1,
            Match = 2
        }

        private void AssertEquals<T>(object actual, T expected)
            where T : IEquatable<T> {
            Assert(actual is T && ((T)actual).Equals(expected));
        }

        private void XAssertOutput(Action f, string expectedOutput) {
            Console.WriteLine("Assertion check skipped.");
            // just run the code
            f();
        }

        private void XAssertOutput(Action f, string expectedOutput, OutputFlags flags) {
            Console.WriteLine("Assertion check skipped.");
            // just run the code
            f();
        }

        private void AssertOutput(Action f, string expectedOutput) {
            AssertOutput(f, expectedOutput, OutputFlags.None);
        }

        private void AssertOutput(Action f, string expectedOutput, OutputFlags flags) {
#if !SILVERLIGHT
            StringBuilder builder = new StringBuilder();

            using (StringWriter output = new StringWriter(builder)) {
                RedirectOutput(output, f);
            }

            string actualOutput = builder.ToString();

            if ((flags & OutputFlags.Raw) == 0) {
                actualOutput = actualOutput.Trim().Replace("\r", "");
                expectedOutput = expectedOutput.Trim().Replace("\r", "");
            }

            if ((flags & OutputFlags.Match) != 0) {
                Regex regex = new Regex(Regex.Escape(expectedOutput).Replace("\\*", ".*").Replace("\\?", "."));
                if (!regex.IsMatch(actualOutput)) {
                    Assert(false, String.Format("Unexpected output: \n\n'{0}'.", actualOutput));
                }
            } else {
                int i = 0;
                while (i < actualOutput.Length && i < expectedOutput.Length && actualOutput[i] == expectedOutput[i]) i++;

                if (actualOutput != expectedOutput) {
                    Assert(false, String.Format("Unexpected output: \n\n'{0}'.\n\nFirst difference ({1}):\nactual = '{2}'\nexpected = '{3}'\n",
                    Escape(builder), i,
                    (i < actualOutput.Length ? Escape(actualOutput[i]) : "<end>"),
                    (i < expectedOutput.Length ? Escape(expectedOutput[i]) : "<end>")
                    ));
                }
            }
#endif
        }

        private static string Escape(char ch) {
            return ch.ToString().Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }

        private static string Escape(string str) {
            return str.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }

        private static string Escape(StringBuilder str) {
            return str.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").ToString();
        }

        private void RedirectOutput(TextWriter/*!*/ output, Action f) {
            // TODO:
            MemoryStream stream = new MemoryStream();
            Runtime.IO.SetOutput(stream, StringUtils.DefaultEncoding);
            Runtime.IO.SetErrorOutput(Console.OpenStandardError(), Console.Error);

            try {
                f();
            } finally {
                output.Write(StringUtils.DefaultEncoding.GetString(stream.ToArray()));
                Runtime.IO.RedirectToConsole();
            }
        }
      
        internal void AssertExceptionThrown<T>(Action f) where T : Exception {
            AssertExceptionThrown<T>(f, null);
        }

        internal void AssertExceptionThrown<T>(Action f, Predicate<T> condition) where T : Exception {
            try {
                RedirectOutput(TextWriter.Null, f);
            } catch (T e) {
                if (condition != null) {
                    Assert(condition(e), "Exception has been thrown but the condition doesn't hold");
                }
                return;
            } catch (Exception e) {
                Assert(false, "Expecting exception '" + typeof(T) + "', got '" + e.GetType() + "'.");
            }

            Assert(false, "Expecting exception '" + typeof(T) + "'.");
        }

        /// <summary>
        /// Asserts two values are equal
        /// </summary>
        internal static void AreEqual(object x, object y) {
            if (x == null && y == null) return;

            Assert(x != null && x.Equals(y), String.Format("values aren't equal: {0} and {1}", x, y));
        }

        /// <summary>
        /// Asserts an condition it true
        /// </summary>
        internal static void Assert(bool condition, string msg) {
            if (!condition) throw new Exception(String.Format("Assertion failed: {0}", msg));
        }

        internal static void Assert(bool condition) {
            if (!condition) throw new Exception("Assertion failed");
        }
    }
}
