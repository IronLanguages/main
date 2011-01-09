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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Dynamic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Runtime;

namespace IronRuby.Tests {
    public class TokenizerTestDriver {
        private readonly RubyContext/*!*/ _context;
        private string _targetDir;
        private string _sources;
        private TextWriter _log;
        private TextWriter _exceptions;
        private TextWriter _regexLog;
        private TextWriter _parserLog;
        private string _currentSourceFile;
        private bool _logProductions;
        private bool _benchmark;

        public TokenizerTestDriver(RubyContext/*!*/ context) {                 
            _context = context;
            _log = Console.Out;
        }

        // /tokenizer <log-dir> <sources>
        public bool ParseArgs(List<string>/*!*/ args) {
            int argIndex;
            if ((argIndex = args.IndexOf("/prod")) != -1) {
                _logProductions = true;
                args.RemoveAt(argIndex);
            } else {
                _logProductions = false;
            }

            if ((argIndex = args.IndexOf("/bm")) != -1) {
                _benchmark = true;
                args.RemoveAt(argIndex);
            } else {
                _benchmark = false;
            }

            _targetDir = (args.Count > 0) ? args[0] : @"C:\RubyTokens";
            _sources = (args.Count > 1) ? args[1] : @"..\..\Languages\Ruby\IronRuby.Tests\Parser\Sources.txt";
            return true;
        }

        private class AssertLog : TraceListener {
            private readonly TokenizerTestDriver/*!*/ _driver;

            public AssertLog(TokenizerTestDriver/*!*/ driver) {
                _driver = driver;
            }

            public override void Fail(string message, string detail) {
                _driver.WriteException("ASSERTION FAILED:\n" + message + "\n" + detail);
            }

            public override void Write(string message) {
                _driver._exceptions.Write(message);
            }

            public override void WriteLine(string message) {
                _driver._exceptions.WriteLine(message);
            }
        }

        public int RunTests() {

            try {
                Directory.Delete(_targetDir, true);
            } catch (DirectoryNotFoundException) {
            } catch (Exception e) {
                Console.WriteLine("Output directory cannot be deleted: {0}", e);
                return -1;
            }

            Directory.CreateDirectory(_targetDir);
            _exceptions = File.CreateText(Path.Combine(_targetDir, "Exceptions.log"));
            _regexLog = File.CreateText(Path.Combine(_targetDir, "Regex.log"));
            _parserLog = File.CreateText(Path.Combine(_targetDir, "Parser.log"));

            // redirect failed assertions to the _exceptions file
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new AssertLog(this));

            string[] lines;

            try {
                lines = File.ReadAllLines(_sources);
            } catch (Exception e) {
                Console.WriteLine("Source file not found: {0}", e);
                return -1;
            }

            _log.WriteLine("Scanning files ...");

            var files = new List<string>();
            var exclude = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            var envVars = Environment.GetEnvironmentVariables();

            for (int i = 0; i < lines.Length; i++) {
                string path = lines[i];

                bool excluding = false;
                if (path.StartsWith("!")) {
                    path = path.Substring(1);
                    excluding = true;
                }
                
                int comment = path.IndexOf('#');
                if (comment >= 0) {
                    path = path.Substring(0, comment);
                }

                StringBuilder sb = new StringBuilder(path);
                foreach (DictionaryEntry envVar in envVars) {
                    sb.Replace("%" + (string)envVar.Key + "%", (string)envVar.Value);
                }

                path = sb.ToString().Trim();

                if (path.Length > 0) {
                    if (File.Exists(path)) {
                        path = Path.GetFullPath(path);
                        if (excluding) {
                            exclude[path] = true;
                        } else {
                            files.Add(path);
                        }
                    } else if (Directory.Exists(path)) {
                        ScanDirs(files, path);
                    } else {
                        Console.Error.WriteLine("Error: {0}({1}): {2} doesn't exist", _sources, i + 1, path);
                    }
                }
            }

            _log.WriteLine();
            _log.WriteLine();

            var filteredFiles = new List<string>();
            foreach (var file in files) {
                if (!exclude.ContainsKey(file)) {
                    filteredFiles.Add(file);
                }
            }

            filteredFiles.Sort();
            if (_benchmark) {
                Benchmark(filteredFiles);
            } else {
                Dump(filteredFiles);
            }
            
            return 0;
        }

        private void ScanDirs(List<string>/*!*/ files, string/*!*/ dir) {
            //_log.Write(dir);
            //_log.Write(' ');

            foreach (string file in Directory.GetFiles(dir, "*.rb")) {
                files.Add(Path.GetFullPath(Path.Combine(dir, file)));
                //_log.Write('.');
            }

            //_log.WriteLine();

            foreach (string subdir in Directory.GetDirectories(dir)) {
                ScanDirs(files, Path.Combine(dir, subdir));
            }
        }

        #region Dump

        internal class ErrorLog : ErrorCounter {
            private List<string>/*!*/ _errors = new List<string>();

            public List<string>/*!*/ Errors { get { return _errors; } }

            public override void Add(SourceUnit source, string message, SourceSpan span, int errorCode, Severity severity) {
                CountError(severity);
                _errors.Add(String.Format("{0}: {1}", severity, message));
            }
        }

        private void Dump(List<string>/*!*/ files) {
            foreach (string file in files) {
                DumpFile(file);
            }
        }

        private void DumpFile(string/*!*/ path) {
            _log.WriteLine(path);

            try {
                string fullPath = Path.GetFullPath(path);
                string root = Path.GetPathRoot(fullPath);
                string outputPath = Path.ChangeExtension(Path.Combine(_targetDir, fullPath.Substring(root.Length).TrimEnd('\'', '/')), ".txt");
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                using (TextWriter output = File.CreateText(outputPath)) {

                    output.WriteLine(fullPath);
                    output.WriteLine();
                    output.WriteLine("Tokens:");
                    output.WriteLine();

                    ErrorLog errors = new ErrorLog();
                    Parser parser = new Parser();

                    parser.TokenSink = delegate(Tokens token, SourceSpan span) {
                        DumpTokenDetail(output, parser.Tokenizer, token);
                    };

                    if (_logProductions) {
#if DEBUG
                        parser.EnableLogging(new CoverageParserLogger(parser, _parserLog));
#endif
                    }

                    _currentSourceFile = path;

                    SourceUnitTree ast = null;
                    try {
                        ast = parser.Parse(_context.CreateFileUnit(path), new RubyCompilerOptions(), errors);
                    } catch (Exception e) {
                        WriteException(e.ToString());
                    }

                    output.WriteLine();

                    if (errors.ErrorCount + errors.FatalErrorCount + errors.WarningCount > 0) {
                        output.WriteLine();
                        output.WriteLine("Errors:");

                        foreach (string error in errors.Errors) {
                            output.WriteLine(error);
                        }
                    } else {
                        Debug.Assert(ast != null);
                        DumpRegularExpressions(ast);
                    }

                    output.WriteLine(".");
                }
            } catch (Exception e) {
                _log.WriteLine("!{0}", e.Message);
            } finally {
                _currentSourceFile = null;
                _regexLog.Flush();
                _parserLog.Flush();
            }
        }

        public void DumpTokenDetail(TextWriter/*!*/ output, Tokenizer/*!*/ tokenizer, Tokens token) {
            TokenValue value = tokenizer.TokenValue;

            output.Write("{0}: ", Parser.GetTerminalName((int)token));

            switch (token) {
                default:
                    break;

                case Tokens.Identifier:
                    output.Write(value.String);
                    break;

                case Tokens.Float:
                    output.Write("{0}D", value.Double);
                    break;

                case Tokens.Integer:
                    output.Write(value.Integer1);
                    break;

                case Tokens.BigInteger:
                    output.Write("{0}BI", value.BigInteger.ToString(10));
                    break;

                case Tokens.RegexpEnd:
                    output.Write("RegexOptions({0})", (RubyRegexOptions)value.Integer1);
                    break;

                case Tokens.StringContent:
                    if (value.StringContent is string) {
                        output.Write("String(\"{0}\", {1})", Parser.EscapeString((string)value.StringContent), value.Encoding);
                    } else {
                        output.Write("String({0}), {1}", BitConverter.ToString((byte[])value.StringContent), value.Encoding);
                    }
                    break;

                case Tokens.StringBegin:
                case Tokens.RegexpBegin:
                case Tokens.ShellStringBegin:
                case Tokens.SymbolBegin:
                    output.Write(((Tokenizer.State)tokenizer.CurrentState).CurrentSequence);
                    break;
            }

            output.Write(' ');
            output.Write(tokenizer.LexicalState);
            output.WriteLine();
        }

        private void WriteException(string/*!*/ str) {
            _exceptions.WriteLine();
            _exceptions.WriteLine(_currentSourceFile);
            _exceptions.WriteLine();
            _exceptions.WriteLine(str);
            _exceptions.WriteLine(new String('-', 50));
            _exceptions.Flush();
        }

        private void DumpRegularExpressions(SourceUnitTree/*!*/ ast) {
            new RegexDumper(_regexLog).Walk(ast);
        }

        private class RegexDumper : Walker {
            private readonly TextWriter/*!*/ _regexLog;

            public RegexDumper(TextWriter/*!*/ regexLog) {
                _regexLog = regexLog;
            }

            public override bool Enter(RegularExpression/*!*/ node) {
                if (node.Pattern.Count == 1) {
                    var literal = node.Pattern[0] as StringLiteral;
                    if (literal != null) {
                        _regexLog.WriteLine("/{0}/{{{1}}}", literal.Value, node.Options);
                    }
                }
                return true;
            }
        }

        #endregion

        #region Benchmarks

        private void Benchmark(List<string>/*!*/ files) {
            
            var sources = new List<SourceUnit>();
            Stopwatch readTime = new Stopwatch();
            long totalSize = 0;
            readTime.Start();
            foreach (string path in files) {
                try {
                    byte[] data = File.ReadAllBytes(path);
                    sources.Add(_context.CreateSourceUnit(new BinaryContentProvider(data), path, Encoding.Default, SourceCodeKind.File));
                    totalSize += data.Length;
                } catch (Exception) {
                    Console.WriteLine("Error: {0}", path);
                }
            }
            readTime.Stop();

            Console.WriteLine("Read: {0} kB in {1}", totalSize / 1024, readTime.Elapsed);
            
#if F
            Stopwatch tokenizeTime = new Stopwatch();
            tokenizeTime.Start();
            foreach (var source in sources) {
                try {
                    var tokenizer = new Tokenizer();
                    tokenizer.Initialize(source);

                    Tokens token;
                    do {
                        token = tokenizer.GetNextToken();
                    } while (token != Tokens.EndOfFile);
                } catch (Exception) {
                    Console.WriteLine("Tokenization error: {0}", source.Path);
                    break;
                }
            }
            tokenizeTime.Stop();
#endif
            //var stackSizes = new Dictionary<int, int>();

            var options = new RubyCompilerOptions();
            Stopwatch parseTime = new Stopwatch();
            Stopwatch transformTime = new Stopwatch();
            foreach (var source in sources) {
                try {
                    parseTime.Start();
                    var parser = new Parser();
                    parser.Parse(source, options, ErrorSink.Null);
                    //int mt;
                    //stackSizes[parser.StackMaxTop] = stackSizes.TryGetValue(parser.StackMaxTop, out mt) ? mt + 1 : 1;
                    parseTime.Stop();
#if F
                    if (rubyTree != null) {
                        transformTime.Start();
                        var lambda = _context.TransformTree<DlrMainCallTarget>(rubyTree, source, options);
                        transformTime.Stop();
                    } else {
                        Console.WriteLine("SyntaxError: {0}", source.Path);
                    }
#endif
                } catch (Exception e) {
                    Console.WriteLine("{0}: {1}: {2}", e.GetType().Name, source.Path, e.Message);
                    break;
                }
            }

          //  Console.WriteLine("Tokenize:        {0}", tokenizeTime.Elapsed);
            Console.WriteLine("Parse:           {0}", parseTime.Elapsed);
            //Console.WriteLine("Idf/Kwd/Loc: {0}/{1}/{2}", Tokenizer.IdfLength, Tokenizer.KwdLength, Tokenizer.LocLength);
           // Console.WriteLine("Transform:       {0}", transformTime.Elapsed);

            //PerfTrack.DumpHistogram(Parser.Reductions);
            //PerfTrack.DumpHistogram(stackSizes);
        }

        #endregion
    }
}
