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
using System.Text;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Tests {
    public partial class Tests {

        // manual test, measures initialization and JIT times (hence it must be the first run)
        public void Scenario_Startup() {
            Stopwatch w = new Stopwatch();
            w.Start();
            Engine.CreateScriptSourceFromString("def foo; puts 2 + 2; end").Execute(Engine.CreateScope());
            w.Stop();
            Console.WriteLine("> total time: {0}ms", w.ElapsedMilliseconds);
        }

        // manual test
        public void Scenario_ParserLogging() {
            if (!_driver.PartialTrust) {
                ParserLoggingTest();
            }
        }

        private void ParserLoggingTest() {
#if DEBUG
            string source = "def foo(a); end";
            var sourceUnit = Context.CreateSnippet(source, SourceCodeKind.Statements);
            var options = new RubyCompilerOptions();

            string temp = Path.Combine(Path.GetTempPath(), "RubyParser");
            Console.WriteLine("> see {0}", temp);
            Directory.CreateDirectory(temp);

            Parser parser = new Parser();
            using (TextWriter writer = File.CreateText(Path.Combine(temp, "default.log"))) {
                DefaultParserLogger.Attach(parser, writer);
                parser.Parse(sourceUnit, options, ErrorSink.Null);
            }

            using (TextWriter writer = File.CreateText(Path.Combine(temp, "tables.csv"))) {
                parser.DumpTables(writer);
            }

            using (TextWriter writer = File.CreateText(Path.Combine(temp, "productions.txt"))) {
                for (int i = 0; i < parser.Rules.Length; i++) {
                    writer.WriteLine("{0}\t{1}", i, parser.RuleToString(i));
                }
            }

            parser = new Parser();
            using (TextWriter writer = File.CreateText(Path.Combine(temp, "productions.txt"))) {
                for (int i = 0; i < parser.Rules.Length; i++) {
                    writer.WriteLine("{0}\t{1}", i, parser.RuleToString(i));
                }
            }

            using (TextWriter writer = File.CreateText(Path.Combine(temp, "second_order.log"))) {
                parser.EnableLogging(new CoverageParserLogger(parser, writer));
                parser.Parse(sourceUnit, options, ErrorSink.Null);
            }
#endif
        }
     
        public void Parser1() {
            LoggingErrorSink log = new LoggingErrorSink();

            List<Tokens> tokens;

            tokens = GetRubyTokens(log, "foo (while (f do end) do end) do end");

            Assert(tokens.Count == 14);
            Assert(tokens[0] == Tokens.Identifier);
            Assert(tokens[1] == Tokens.LeftArgParenthesis);
            Assert(tokens[2] == Tokens.While);
            Assert(tokens[3] == Tokens.LeftExprParenthesis);
            Assert(tokens[4] == Tokens.Identifier);
            Assert(tokens[5] == Tokens.Do);
            Assert(tokens[6] == Tokens.End);
            Assert(tokens[7] == Tokens.RightParenthesis);
            Assert(tokens[8] == Tokens.LoopDo);
            Assert(tokens[9] == Tokens.End);
            Assert(tokens[10] == Tokens.RightParenthesis);
            Assert(tokens[11] == Tokens.BlockDo);
            Assert(tokens[12] == Tokens.End);
            Assert(tokens[13] == Tokens.EndOfFile);

            log.Errors.Clear();

            tokens = GetRubyTokens(log, "print 'foo'");

            Assert(log.Errors.Count == 0);

            Assert(tokens.Count == 5 &&
                tokens[0] == Tokens.Identifier &&
                tokens[1] == Tokens.StringBegin &&
                tokens[2] == Tokens.StringContent &&
                tokens[3] == Tokens.StringEnd &&
                tokens[4] == Tokens.EndOfFile);

            Assert(log.Errors.Count == 0);

            tokens = GetRubyTokens(log, "print '");

            Assert(log.Errors.Count == 1 &&
                log.Errors[0].Severity == Severity.Error
            );

            Assert(tokens.Count == 4 &&
                tokens[0] == Tokens.Identifier &&
                tokens[1] == Tokens.StringBegin &&
                tokens[2] == Tokens.StringEnd &&
                tokens[3] == Tokens.EndOfFile);

            log.Errors.Clear();

            tokens = GetRubyTokens(log, "f \"a#{g.h /x/}\" do\nend");

            Assert(tokens.ToArray().ValueEquals(new[] {
                Tokens.Identifier,
                Tokens.StringBegin,
                Tokens.StringContent,
                Tokens.StringEmbeddedCodeBegin,
                Tokens.Identifier,
                Tokens.Dot,
                Tokens.Identifier,
                Tokens.RegexpBegin,
                Tokens.StringContent,
                Tokens.RegexpEnd,
                Tokens.StringEmbeddedCodeEnd,
                Tokens.StringEnd,
                Tokens.BlockDo,
                Tokens.End,
                Tokens.EndOfFile
            }));

            Assert(log.Errors.Count == 1 &&
                log.Errors[0].Severity == Severity.Warning
            );

            log.Errors.Clear();

            // 1.9:
            tokens = GetRubyTokens(log, "def !@\nend");

            Assert(tokens.ToArray().ValueEquals(new[] {
                Tokens.Def,
                Tokens.Bang,
                Tokens.NewLine,
                Tokens.End,
                Tokens.EndOfFile
            }));

            Assert(log.Errors.Count == 0);

            // 1.9:
            tokens = GetRubyTokens(log, "puts <<L, foo\n  .bar\nL\n  .baz");
            Assert(tokens.ToArray().ValueEquals(new[] {
		        Tokens.Identifier,
		        Tokens.StringBegin,
		        Tokens.StringContent,
		        Tokens.StringEnd,
		        Tokens.Comma,
		        Tokens.Identifier,
		        Tokens.Dot,
		        Tokens.Identifier,
		        Tokens.EndOfFile,
            }));

            Assert(log.Errors.Count == 0);

            // EXPR_VALUE is included in IsBeginLexicalState (discriminates left bracket kind):
            tokens = GetRubyTokens(log, "x = 1 ? [] : []");
            Assert(tokens.ToArray().ValueEquals(new[] {
		        Tokens.Identifier,
		        Tokens.Assignment,
		        Tokens.Integer,
		        Tokens.QuestionMark,
		        Tokens.LeftBracket,
		        Tokens.RightBracket,
		        Tokens.Colon,
		        Tokens.LeftBracket,
		        Tokens.RightBracket,
		        Tokens.EndOfFile,
            }));

            Assert(log.Errors.Count == 0);

            // Default block parameter is different from default method parameter.
            // Block parameter doesn't allow using binary expressions etc. due to ambiguity with binary OR operator.
            tokens = GetRubyTokens(log, "lambda {|x=1|}");
            Assert(tokens.ToArray().ValueEquals(new[] {
                Tokens.Identifier,
                Tokens.LeftBlockBrace,
                Tokens.Pipe,
                Tokens.Identifier,
                Tokens.Assignment,
                Tokens.Integer,
                Tokens.Pipe,
                Tokens.RightBrace,
                Tokens.EndOfFile,
            }));

            Assert(log.Errors.Count == 0);

            tokens = GetRubyTokens(log, "lambda { ->(){} }");
            Assert(tokens.ToArray().ValueEquals(new[] {
                Tokens.Identifier,
		        Tokens.LeftBlockBrace,
		        Tokens.Lambda,
		        Tokens.LeftParenthesis,
		        Tokens.RightParenthesis,	
		        Tokens.LeftLambdaBrace,
		        Tokens.RightBrace,
		        Tokens.RightBrace,
		        Tokens.EndOfFile,
            }));

            Assert(log.Errors.Count == 0);

            tokens = GetRubyTokens(log, "\"#{->{ }}\"\n-> do\nend");

            Assert(tokens.ToArray().ValueEquals(new[] {
                Tokens.StringBegin,
		        Tokens.StringEmbeddedCodeBegin,
		        Tokens.Lambda,
		        Tokens.LeftLambdaBrace,	
		        Tokens.RightBrace,
		        Tokens.StringEmbeddedCodeEnd,
                Tokens.StringEnd,
                Tokens.NewLine,
                Tokens.Lambda,
                Tokens.LambdaDo,
                Tokens.NewLine,
                Tokens.End,
		        Tokens.EndOfFile,
            }));

            Assert(log.Errors.Count == 0);
        }
        
        private void ParserErrors1() {
            LoggingErrorSink log = new LoggingErrorSink();

            var tokens = GetRubyTokens(log, "def [");

            Assert(tokens.ToArray().ValueEquals(new[] {
                Tokens.Def,
                Tokens.LeftIndexingBracket,
            }));

            Assert(log.Errors.Count == 1 &&
                log.Errors[0].Severity == Severity.FatalError
            );

            log.Errors.Clear();

            tokens = GetRubyTokens(log, "${");

            Assert(tokens.ToArray().ValueEquals(new[] {
                Tokens.Dollar,
            }));

            Assert(log.Errors.Count == 1 &&
                log.Errors[0].Severity == Severity.FatalError
            );

            log.Errors.Clear();

            // Error: block argument should not be given (yield as a command):
            tokens = GetRubyTokens(log, "a[yield 1, &x]");
            Assert(tokens.ToArray().ValueEquals(new[] {
		        Tokens.Identifier,
		        Tokens.LeftIndexingBracket,
		        Tokens.Yield,
		        Tokens.Integer,
		        Tokens.Comma,
		        Tokens.BlockReference,
		        Tokens.Identifier,
		        Tokens.RightBracket,
		        Tokens.EndOfFile,
            }));

            Assert(log.Errors.Count == 1 &&
                log.Errors[0].Severity == Severity.Error
            );

            log.Errors.Clear();

            // restrictions on jump statements:
            foreach (var src in new[] {
              "a[return 1, &x]",
              "puts if return 1",
              "1 && return 1",
              "(a ? return 1 : 1) while true",
              "a while return",
              "1 + return",
              "return / 1",
              "a = return",
              "return.foo = bar",
              "a = 1 rescue return 2",
            }) {
                tokens = GetRubyTokens(log, src);
                Assert(log.Errors.Count > 0 && log.Errors.Exists((e) => e.Severity == Severity.FatalError));
                log.Errors.Clear();
            }

            // Error: Duplicate parameter name:
            tokens = GetRubyTokens(log, "lambda { |x,x| }");
            Assert(log.Errors.Count == 1 && log.Errors[0].Severity == Severity.Error);
            log.Errors.Clear();

            // Underscore may be duplicated:
            tokens = GetRubyTokens(log, "lambda { |_,_| }");
            Assert(log.Errors.Count == 0);
            log.Errors.Clear();
        }

        [Options(NoRuntime = true)]
        public void TokenCategorizer1() {
            var allTokens = Enum.GetValues(typeof(Tokens));
            foreach (Tokens token in allTokens) {
                if (token != Tokens.None && token != Tokens.Lowest && token != Tokens.LastToken) {
                    var info = Tokenizer.GetTokenInfo(token);
                    Assert(info.Category != TokenCategory.None);
                }
            }
            foreach (Tokens token in allTokens) {
                if (token != Tokens.None && token != Tokens.Lowest && token != Tokens.LastToken) {
                    Assert(!String.IsNullOrEmpty(Tokenizer.GetTokenDescription(token)));
                }
            }
        }

        public void TokenCategorizer2() {
            // initial position:
            TestCategorizer(Engine, null, "1\n2", new SourceLocation(10, 2, 5),
                // 1
                new TokenInfo(new SourceSpan(new SourceLocation(10, 2, 5), new SourceLocation(11, 2, 6)), TokenCategory.NumericLiteral, TokenTriggers.None),
                // \n
                new TokenInfo(new SourceSpan(new SourceLocation(11, 2, 6), new SourceLocation(12, 3, 1)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // 2
                new TokenInfo(new SourceSpan(new SourceLocation(12, 3, 1), new SourceLocation(13, 3, 2)), TokenCategory.NumericLiteral, TokenTriggers.None)
            );

            // regexes:
            TestCategorizer(Engine, null, "/x/",
                // /
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)), TokenCategory.StringLiteral, TokenTriggers.None),
                // hello
                new TokenInfo(new SourceSpan(new SourceLocation(1, 1, 2), new SourceLocation(2, 1, 3)), TokenCategory.StringLiteral, TokenTriggers.None),
                // /
                new TokenInfo(new SourceSpan(new SourceLocation(2, 1, 3), new SourceLocation(3, 1, 4)), TokenCategory.StringLiteral, TokenTriggers.None)
            );

            // whitespace:
            TestCategorizer(Engine, null, "print 'foo' #bar", 
                // print
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(5, 1, 6)), TokenCategory.Identifier, TokenTriggers.None),
                // ' '
                new TokenInfo(new SourceSpan(new SourceLocation(5, 1, 6), new SourceLocation(6, 1, 7)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // '
                new TokenInfo(new SourceSpan(new SourceLocation(6, 1, 7), new SourceLocation(7, 1, 8)), TokenCategory.StringLiteral, TokenTriggers.None),
                // foo
                new TokenInfo(new SourceSpan(new SourceLocation(7, 1, 8), new SourceLocation(10, 1, 11)), TokenCategory.StringLiteral, TokenTriggers.None),
                // '
                new TokenInfo(new SourceSpan(new SourceLocation(10, 1, 11), new SourceLocation(11, 1, 12)), TokenCategory.StringLiteral, TokenTriggers.None),
                // ' '
                new TokenInfo(new SourceSpan(new SourceLocation(11, 1, 12), new SourceLocation(12, 1, 13)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // #bar
                new TokenInfo(new SourceSpan(new SourceLocation(12, 1, 13), new SourceLocation(16, 1, 17)), TokenCategory.LineComment, TokenTriggers.None)
            );

            // eolns:
            TestCategorizer(Engine, null, "a\r\nb",
                // a
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)), TokenCategory.Identifier, TokenTriggers.None),   
                // \r\n
                new TokenInfo(new SourceSpan(new SourceLocation(1, 1, 2), new SourceLocation(3, 2, 1)), TokenCategory.WhiteSpace, TokenTriggers.None),   
                // b
                new TokenInfo(new SourceSpan(new SourceLocation(3, 2, 1), new SourceLocation(4, 2, 2)), TokenCategory.Identifier, TokenTriggers.None)  
            );

            //                                       11111111 11222222222233 333
            //                             012345678901234567 89012345678901 234
            TestCategorizer(Engine, null, "canvas.Event { |x|\nputs 'string'\n}", 
            //                             1234567890123456789 12345678901234 12
            //                                      1111111111          11111   
                // line 1                    
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(6, 1, 7)), TokenCategory.Identifier, TokenTriggers.None),          // canvas
                new TokenInfo(new SourceSpan(new SourceLocation(6, 1, 7), new SourceLocation(7, 1, 8)), TokenCategory.Delimiter, TokenTriggers.MemberSelect),   // .
                new TokenInfo(new SourceSpan(new SourceLocation(7, 1, 8), new SourceLocation(12, 1, 13)), TokenCategory.Identifier, TokenTriggers.None),        // Event
                new TokenInfo(new SourceSpan(new SourceLocation(12, 1, 13), new SourceLocation(13, 1, 14)), TokenCategory.WhiteSpace, TokenTriggers.None),      //  
                new TokenInfo(new SourceSpan(new SourceLocation(13, 1, 14), new SourceLocation(14, 1, 15)), TokenCategory.Grouping, TokenTriggers.MatchBraces), // {
                new TokenInfo(new SourceSpan(new SourceLocation(14, 1, 15), new SourceLocation(15, 1, 16)), TokenCategory.WhiteSpace, TokenTriggers.None),      //  
                new TokenInfo(new SourceSpan(new SourceLocation(15, 1, 16), new SourceLocation(16, 1, 17)), TokenCategory.Grouping, TokenTriggers.MatchBraces), // |
                new TokenInfo(new SourceSpan(new SourceLocation(16, 1, 17), new SourceLocation(17, 1, 18)), TokenCategory.Identifier, TokenTriggers.None),      // x
                new TokenInfo(new SourceSpan(new SourceLocation(17, 1, 18), new SourceLocation(18, 1, 19)), TokenCategory.Grouping, TokenTriggers.MatchBraces), // |
                new TokenInfo(new SourceSpan(new SourceLocation(18, 1, 19), new SourceLocation(19, 2, 1)), TokenCategory.WhiteSpace, TokenTriggers.None),       // \n
                // line 2
                new TokenInfo(new SourceSpan(new SourceLocation(19, 2, 1), new SourceLocation(23, 2, 5)), TokenCategory.Identifier, TokenTriggers.None),        // puts
                new TokenInfo(new SourceSpan(new SourceLocation(23, 2, 5), new SourceLocation(24, 2, 6)), TokenCategory.WhiteSpace, TokenTriggers.None),        //  
                new TokenInfo(new SourceSpan(new SourceLocation(24, 2, 6), new SourceLocation(25, 2, 7)), TokenCategory.StringLiteral, TokenTriggers.None),     // '
                new TokenInfo(new SourceSpan(new SourceLocation(25, 2, 7), new SourceLocation(31, 2, 13)), TokenCategory.StringLiteral, TokenTriggers.None),    // string
                new TokenInfo(new SourceSpan(new SourceLocation(31, 2, 13), new SourceLocation(32, 2, 14)), TokenCategory.StringLiteral, TokenTriggers.None),   // '
                new TokenInfo(new SourceSpan(new SourceLocation(32, 2, 14), new SourceLocation(33, 3, 1)), TokenCategory.WhiteSpace, TokenTriggers.None),       // \n (significant)
                // line 3
                new TokenInfo(new SourceSpan(new SourceLocation(33, 3, 1), new SourceLocation(34, 3, 2)), TokenCategory.Grouping, TokenTriggers.MatchBraces)    // }
            );

            // state transfer: strings //

            object state = null;
            state = TestCategorizer(Engine, state, "\"a\n", 
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)), TokenCategory.StringLiteral, TokenTriggers.None), // "
                new TokenInfo(new SourceSpan(new SourceLocation(1, 1, 2), new SourceLocation(3, 2, 1)), TokenCategory.StringLiteral, TokenTriggers.None)  // a\n
            );

            state = TestCategorizer(Engine, state, "b\n",
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(2, 2, 1)), TokenCategory.StringLiteral, TokenTriggers.None)  // b\n
            );

            state = TestCategorizer(Engine, state, "\"",
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)), TokenCategory.StringLiteral, TokenTriggers.None)  // ""
            );

            // state transfer: multi-line comments //

            state = null;
            state = TestCategorizer(Engine, state, "=begin\n",
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(7, 2, 1)), TokenCategory.Comment, TokenTriggers.None)
            );

            for (int i = 0; i < 3; i++) {
                state = TestCategorizer(Engine, state, "foo\n",
                    new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(4, 2, 1)), TokenCategory.Comment, TokenTriggers.None)
                );
            }

            state = TestCategorizer(Engine, state, "a\nb\n",
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(4, 3, 1)), TokenCategory.Comment, TokenTriggers.None)
            );

            state = TestCategorizer(Engine, state, "=end",
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5)), TokenCategory.Comment, TokenTriggers.None)
            );

            Assert(((Tokenizer.State)state).CurrentSequence == TokenSequenceState.None);

            // state transfer: nested strings //

            state = null;
            state = TestCategorizer(Engine, state, "\"a",
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)), TokenCategory.StringLiteral, TokenTriggers.None),
                new TokenInfo(new SourceSpan(new SourceLocation(1, 1, 2), new SourceLocation(2, 1, 3)), TokenCategory.StringLiteral, TokenTriggers.None)
            );

            state = TestCategorizer(Engine, state, "#{",
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(2, 1, 3)), TokenCategory.Grouping, TokenTriggers.MatchBraces)
            );

            state = TestCategorizer(Engine, state, "1",
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)), TokenCategory.NumericLiteral, TokenTriggers.None)
            );

            state = TestCategorizer(Engine, state, "}",
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)), TokenCategory.Grouping, TokenTriggers.MatchBraces)
            );

            state = TestCategorizer(Engine, state, "\"",
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)), TokenCategory.StringLiteral, TokenTriggers.None)
            );

            Assert(((Tokenizer.State)state).CurrentSequence == TokenSequenceState.None);
        }

        public void TokenizeIdentifiers1() {
            string surrogate = "\ud800\udc00";

            Assert(Tokenizer.IsConstantName("C"));
            Assert(Tokenizer.IsConstantName("Cx"));
            Assert(Tokenizer.IsConstantName("C9"));
            Assert(Tokenizer.IsConstantName("CazAZ0123456789_"));
            Assert(Tokenizer.IsConstantName("C_"));
            Assert(!Tokenizer.IsConstantName(null));
            Assert(!Tokenizer.IsConstantName(""));
            Assert(!Tokenizer.IsConstantName("C="));
            Assert(!Tokenizer.IsConstantName("C?"));
            Assert(!Tokenizer.IsConstantName("C!"));
            Assert(!Tokenizer.IsConstantName("_"));
            Assert(!Tokenizer.IsConstantName("0"));
            Assert(!Tokenizer.IsConstantName("c"));
            Assert(!Tokenizer.IsConstantName("Σ"));
            Assert(Tokenizer.IsConstantName("CΣ"));

            Assert(Tokenizer.IsMethodName("C"));
            Assert(Tokenizer.IsMethodName("Cx"));
            Assert(Tokenizer.IsMethodName("CazAZ0123456789_"));
            Assert(Tokenizer.IsMethodName("f="));
            Assert(Tokenizer.IsMethodName("f?"));
            Assert(Tokenizer.IsMethodName("f!"));
            Assert(Tokenizer.IsMethodName("_"));
            Assert(Tokenizer.IsMethodName("c"));
            Assert(!Tokenizer.IsMethodName("="));
            Assert(!Tokenizer.IsMethodName("?"));
            Assert(!Tokenizer.IsMethodName("!"));
            Assert(!Tokenizer.IsMethodName(null));
            Assert(!Tokenizer.IsMethodName(""));
            Assert(Tokenizer.IsMethodName("Σ"));
            Assert(Tokenizer.IsMethodName(surrogate));

            Assert(Tokenizer.IsGlobalVariableName("$x"));
            Assert(Tokenizer.IsGlobalVariableName("$XazAZ0123456789_"));
            Assert(!Tokenizer.IsGlobalVariableName("$f="));
            Assert(!Tokenizer.IsGlobalVariableName("$f?"));
            Assert(!Tokenizer.IsGlobalVariableName("$f!"));
            Assert(!Tokenizer.IsGlobalVariableName("$f$"));
            Assert(!Tokenizer.IsGlobalVariableName(null));
            Assert(!Tokenizer.IsGlobalVariableName("$"));
            Assert(!Tokenizer.IsGlobalVariableName("$$"));
            Assert(!Tokenizer.IsGlobalVariableName("f"));
            Assert(!Tokenizer.IsGlobalVariableName("ff"));
            Assert(!Tokenizer.IsGlobalVariableName("fff"));
            Assert(Tokenizer.IsGlobalVariableName("$Σ"));
            Assert(Tokenizer.IsGlobalVariableName("$" + surrogate));

            Assert(Tokenizer.IsInstanceVariableName("@x"));
            Assert(Tokenizer.IsInstanceVariableName("@XazAZ0123456789_"));
            Assert(!Tokenizer.IsInstanceVariableName("@f="));
            Assert(!Tokenizer.IsInstanceVariableName("@f?"));
            Assert(!Tokenizer.IsInstanceVariableName("@f!"));
            Assert(!Tokenizer.IsInstanceVariableName("@f@"));
            Assert(!Tokenizer.IsInstanceVariableName(null));
            Assert(!Tokenizer.IsInstanceVariableName("@"));
            Assert(!Tokenizer.IsInstanceVariableName("@@"));
            Assert(!Tokenizer.IsInstanceVariableName("@@@"));
            Assert(!Tokenizer.IsInstanceVariableName("f"));
            Assert(!Tokenizer.IsInstanceVariableName("ff"));
            Assert(!Tokenizer.IsInstanceVariableName("fff"));
            Assert(Tokenizer.IsInstanceVariableName("@Σ"));
            Assert(Tokenizer.IsInstanceVariableName("@" + surrogate));

            Assert(Tokenizer.IsClassVariableName("@@x"));
            Assert(Tokenizer.IsClassVariableName("@@XazAZ0123456789_"));
            Assert(!Tokenizer.IsClassVariableName("@@f="));
            Assert(!Tokenizer.IsClassVariableName("@@f?"));
            Assert(!Tokenizer.IsClassVariableName("@@f!"));
            Assert(!Tokenizer.IsClassVariableName("@@f@"));
            Assert(!Tokenizer.IsClassVariableName(null));
            Assert(!Tokenizer.IsClassVariableName("@"));
            Assert(!Tokenizer.IsClassVariableName("@@"));
            Assert(!Tokenizer.IsClassVariableName("@@@"));
            Assert(!Tokenizer.IsClassVariableName("f"));
            Assert(!Tokenizer.IsClassVariableName("ff"));
            Assert(!Tokenizer.IsClassVariableName("fff"));
            Assert(Tokenizer.IsClassVariableName("@@Σ"));
            Assert(Tokenizer.IsClassVariableName("@@" + surrogate));
        }

        private void Identifiers2() {
            AssertTokenizer t = NewAssertTokenizer();

            // 'variable' non-terminal needs to set $<String>$ even for keywords, 
            // otherwise the content of previous token is stored in token value and is interpreted as string.
            t.Load("//\ntrue")[Tokens.RegexpBegin][Tokens.RegexpEnd][Tokens.NewLine][Tokens.True].EOF();

            t.Load("Σ = CΣ", (tok) => tok.AllowNonAsciiIdentifiers = true).
                ReadSymbol(Tokens.Identifier, "Σ")[Tokens.Assignment].ReadSymbol(Tokens.ConstantIdentifier, "CΣ").EOF();

            t.Load("Σ = CΣ")[Tokens.InvalidCharacter].Expect(Errors.InvalidCharacterInExpression);

            t.Load("@Σ=@@Σ=$Σ", (tok) => tok.AllowNonAsciiIdentifiers = true)
                [Tokens.InstanceVariable][Tokens.Assignment]
                [Tokens.ClassVariable][Tokens.Assignment]
                [Tokens.GlobalVariable].EOF();

            t.Load("def Σ;end", (tok) => tok.AllowNonAsciiIdentifiers = true)
                [Tokens.Def][Tokens.Identifier][Tokens.Semicolon][Tokens.End].EOF();

#if OBSOLETE // ???
            // we should report a warning if -KU is used and treat BOM as whitespace (MRI 1.8 treats the BOM as identifier):
            t.Load(new byte[] { 
                0xEF, 0xBB, 0xBF, (byte)'x'
            }, (tok) => { 
                tok.Compatibility = RubyCompatibility.Ruby186; 
                tok.Encoding = RubyEncoding.KCodeUTF8;
                tok.Verbatim = true;
            })
            [Tokens.Whitespace][Tokens.Identifier].Expect(Errors.ByteOrderMarkIgnored);

            // we should report a warning if -KCODE is not used and treat BOM as whitespace (MRI 1.8 reports an error):
            t.Load(new byte[] { 
                0xEF, 0xBB, 0xBF, (byte)'=', (byte)'1' 
            }, (tok) => { 
                tok.Compatibility = RubyCompatibility.Ruby186; 
                tok.AllowNonAsciiIdentifiers = false;
                tok.Verbatim = true;
            }) 
            [Tokens.Whitespace][Tokens.Assignment][1].Expect(Errors.ByteOrderMarkIgnored);
#endif
            t.Expect();
        }

        public void ParseBigInts1() {
            TestBigInt("00010010000_00100000_01001000_00010000_00000010_01001000_11111111_00010000_00100000_00000011", 2);
            TestBigInt("000100_afe1_1231_4980_6FEA_1470_5100_afe1_1231_4980_6FEA_1471", 16);
            TestBigInt("65_6465477756_5454111111_1112365414_65_4365414564_5463215467_3215456713", 8);
            TestBigInt("645456600_000798789_789798798_798789798_798765461_237676891_968734198_467346797", 10);
            TestBigInt("hhs15489231dfu89765460z", 36);
            TestBigInt("lafkhalkdhflkghalgAKD12", 28);

            for (int i = 3; i <= 36; i++) {
                TestBigInt("00010010000_00100000_01001000_00010000_00000010_01001000_11111111_00010000_00100000_00000011", i, 0);
            }
        }

        public void ParseIntegers1() {
            IntegerValue x;
            Assert((x = Tokenizer.ParseInteger("", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("", 16)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("    ", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("-", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("+", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("0", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("00", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("0x", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("0x", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("-0x", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("+0x", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("  1234   ", 0)).Equals(1234));
            Assert((x = Tokenizer.ParseInteger("  1_2_3_4   ", 0)).Equals(1234));
            Assert((x = Tokenizer.ParseInteger("  _1234   ", 0)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("  12a34   ", 0)).Equals(12));
            Assert((x = Tokenizer.ParseInteger("  1_2__34   ", 0)).Equals(12));
            Assert((x = Tokenizer.ParseInteger("  -1_2", 0)).Equals(-12));
            Assert((x = Tokenizer.ParseInteger("0x1234", 0)).Equals(0x1234));
            Assert((x = Tokenizer.ParseInteger("0x1234", 10)).Equals(0));
            Assert((x = Tokenizer.ParseInteger("0b102", 0)).Equals(2));
            Assert((x = Tokenizer.ParseInteger("1000_000000_0000000000", 0)).Bignum.ToString() == "10000000000000000000");
            Assert((x = Tokenizer.ParseInteger("1000000_000000_0000000", 16)).Bignum.ToString() == "75557863725914323419136");
            Assert((x = Tokenizer.ParseInteger("0x1000000_000000_0000000", 0)).Bignum.ToString() == "75557863725914323419136");
            Assert((x = Tokenizer.ParseInteger("0b1000000_000000_0000000", 0)).Equals(524288));
            Assert((x = Tokenizer.ParseInteger("-0b1000000_000000_0000000", 0)).Equals(-524288));
        }

        private void TokenizeNumbers1() {
            AssertTokenizer t = NewAssertTokenizer();

            t.Load("0").Read(0);
            t.Load("0000").Read(0);
            t.Load("0_0_00000_0_0_00000000000000000000_00_00000000000000000000000000000").Read(0);

            t.Load("0777").Read(Convert.ToInt32("777", 8));
            t.Load("000000000000000000000000000000000000076541").Read(Convert.ToInt32("76541", 8));
            AssertTokenBigInteger("0100000000000000_000000000000000000000076541", 8);

            t.Load("0x0").Read(0);
            t.Load("0xa_F").Read(Convert.ToInt32("af", 16));
            t.Load("0x000000_0000000000000000000aF").Read(Convert.ToInt32("af", 16));
            t.Load("0x10000000000_00000000000000aF").ReadBigInteger("1000000000000000000000000aF", 16);

            t.Load("0b0").Read(0);
            t.Load("0b000000000000_000000000000000000000000000101").Read(Convert.ToInt32("101", 2));
            t.Load("0b10000000_0000000000000000000000000000000101").ReadBigInteger("100000000000000000000000000000000000000101", 2);

            t.Load("0d0").Read(0);
            t.Load("0d00000000_0000000000000000000000000000000101").Read(101);
            t.Load("0d10000000_0000000000000000000000_000000000101").ReadBigInteger("100000000000000000000000000000000000000101", 10);

            t.Load("0o0").Read(0);
            t.Load("0o000_000000000000000000000000000000000076541").Read(Convert.ToInt32("76541", 8));
            t.Load("0o100000000_000000000000000000000000000076541").ReadBigInteger("100000000000000000000000000000000000076541", 8);
            t.Load("0.0").Read(0.0D);

            t.Load("0e-000").Read(0.0D);
            t.Load("0e2").Read(0.0D);
            t.Load("0e+2").Read(0.0D);
            t.Load("1e2").Read(100.0D);
            t.Load("1e+2").Read(100.0D);
            t.Load("1e-2").Read(0.01D);

            t.Load("3_1_3_2_1_3_2_1_3_5_4_6_5_3_1_3_2.0").Read(31321321354653132.0D);
            t.Load("1_3_2.3_1_3_2_1_3").Read(132.313213D);

            t.Load("1.1e-0").Read(1.1D);
            t.Load("1.1e+0").Read(1.1D);
            t.Load("1.1e0").Read(1.1D);
            t.Load("1.1e-1").Read(0.11D);

            t.Load("1.1e-1020").Read(0.0D);
            t.Load("1.1e-1021").Read(0.0D).Expect(Errors.FloatOutOfRange);

            t.Load("1.1e1024").Read(Double.PositiveInfinity);
            t.Load("1.1e1025").Read(Double.PositiveInfinity).Expect(Errors.FloatOutOfRange);

            t.Load("1.1e-30000").Read(0.0D).Expect(Errors.FloatOutOfRange);
            t.Load("1.1e3_1_3_2_1_3_2_1_3_5_4_6_5_3_1_3_2").Read(Double.PositiveInfinity).Expect(Errors.FloatOutOfRange);
            t.Load("4.94065645841247e-324").Read(Double.Epsilon);
            t.Load("1_2.4_5e2_2").Read(12.45e22);
            t.Load("1._1").Read(1);
            t.Load("1.").Read(1);

            t.Load("122312.   1212").Read(122312);
            t.Load("01234e12").Read(Convert.ToInt32("1234", 8));
            t.Load("12__1212").Read(12).Expect(Errors.TrailingUnderscoreInNumber);
            t.Load("123_.123").Read(123).Expect(Errors.TrailingUnderscoreInNumber);
            t.Load("08").Read(8).Expect(Errors.IllegalOctalDigit);
            t.Load("0_8").Read(0).Expect(Errors.TrailingUnderscoreInNumber);
            t.Load("0_x").Read(0).Expect(Errors.TrailingUnderscoreInNumber);
            t.Load("0_").Read(0).Expect(Errors.TrailingUnderscoreInNumber);
            t.Load("0x_").Read(0).Expect(Errors.NumericLiteralWithoutDigits);
            t.Load("0x").Read(0).Expect(Errors.NumericLiteralWithoutDigits);
            t.Load("0x_1").Read(0).Expect(Errors.NumericLiteralWithoutDigits);

            t.Load(".20").Read(Tokens.Dot).Expect(Errors.NoFloatingLiteral);
            t.Load("1.e").Read(1);
            t.Load("1.2e").Read(1.2D).Expect(Errors.TrailingEInNumber);
            t.Load("1e").Read(1).Expect(Errors.TrailingEInNumber);
            t.Load("1e-").Read(1).Expect(Errors.TrailingMinusInNumber);
            t.Load("1e+").Read(1).Expect(Errors.TrailingPlusInNumber);

            t.Load("00.0").Read(0).Expect(Errors.NoFloatingLiteral);
            t.Load("00.foo").Read(0);
            t.Load("00.e-1").Read(0);
            t.Load("00.foo").Read(0);
            t.Load("0x.0").Read(0).Expect(Errors.NumericLiteralWithoutDigits, Errors.NoFloatingLiteral);
            t.Load("0x.foo").Read(0).Expect(Errors.NumericLiteralWithoutDigits);
        
            t.Expect();
        }

        private void TokenizeInstanceClassVariables1() {
            AssertTokenizer t = NewAssertTokenizer();

            t.Load("@").Read(Tokens.At);
            t.Load("@@").Read(Tokens.At);
            t.Load("@1").Read(Tokens.At).Expect(Errors.InvalidInstanceVariableName);
            t.Load("@@1").Read(Tokens.At).Expect(Errors.InvalidClassVariableName);
            t.Load("@_").ReadSymbol(Tokens.InstanceVariable, "@_");
            t.Load("@@_").ReadSymbol(Tokens.ClassVariable, "@@_");
            t.Load("@aA1_").ReadSymbol(Tokens.InstanceVariable, "@aA1_");
            t.Load("@@aA1_").ReadSymbol(Tokens.ClassVariable, "@@aA1_");
            
            t.Expect();
        }

        private void TokenizeGlobalVariables1() {
            AssertTokenizer t = NewAssertTokenizer();

            t.Load("$")[Tokens.Dollar].EOF();
            t.Load("$_")[Tokens.GlobalVariable, "_"].EOF();
            t.Load("$_1")[Tokens.GlobalVariable, "_1"].EOF();
            t.Load("$_-1")[Tokens.GlobalVariable, "_"][Tokens.Minus][1].EOF();
            t.Load("$!")[Tokens.GlobalVariable, "!"].EOF();
            t.Load("$@")[Tokens.GlobalVariable, "@"].EOF();
            t.Load("$-")[Tokens.GlobalVariable, "-"].EOF();
            t.Load("$--1")[Tokens.GlobalVariable, "-"][Tokens.Minus][1].EOF();
            t.Load("$-x")[Tokens.GlobalVariable, "-x"].EOF();
            
            t.Expect();
        }

        private void TokenizeEolns1() {
            AssertTokenizer t = NewAssertTokenizer();

            // empty source:
            t.Load("").EOF();

            // escaped eoln:
            t.Load("[\\\r\n]")[Tokens.LeftBracket][Tokens.RightBracket].EOF();
            t.Load("[\\\n]")[Tokens.LeftBracket][Tokens.RightBracket].EOF();
            t.Load("[\\\r]")[Tokens.LeftBracket][Tokens.Backslash][Tokens.RightBracket].EOF();

            // 1.9 dot:
            t.Load("foo\n\t .bar")[Tokens.Identifier][Tokens.Dot][Tokens.Identifier].EOF();
            t.Load("foo\n\t ..bar")[Tokens.Identifier][Tokens.NewLine][Tokens.DoubleDot][Tokens.Identifier].EOF();
            t.Load("foo\n\t \nbar")[Tokens.Identifier][Tokens.NewLine][Tokens.Identifier].EOF();

            // eoln used to quote a string:
            t.Load("x = %\r\nhello\r\n")[Tokens.Identifier][Tokens.Assignment][Tokens.StringBegin][Tokens.StringContent, "hello"][Tokens.StringEnd].EOF();
            t.Load("x = %\nhello\r\n")[Tokens.Identifier][Tokens.Assignment][Tokens.StringBegin][Tokens.StringContent, "hello"][Tokens.StringEnd].EOF();
            t.Load("x = %\rhello\r\n\r")[Tokens.Identifier][Tokens.Assignment][Tokens.StringBegin][Tokens.StringContent, "hello\n"][Tokens.StringEnd].EOF();

            t.Load("%Q\r\nhello\n")[Tokens.StringBegin][Tokens.StringContent, "hello"][Tokens.StringEnd].EOF();
            t.Load("%Q\nhello\n")[Tokens.StringBegin][Tokens.StringContent, "hello"][Tokens.StringEnd].EOF();
            t.Load("%Q\rhello\r\n\r")[Tokens.StringBegin][Tokens.StringContent, "hello\n"][Tokens.StringEnd].EOF();

            t.Load("%w[  ]")[Tokens.VerbatimWordsBegin][Tokens.StringEnd].EOF();
            t.Load("%w[  foo]")[Tokens.VerbatimWordsBegin][Tokens.StringContent, "foo"][Tokens.StringEnd].EOF();
            t.Load("%w[\n   foo]")[Tokens.VerbatimWordsBegin][Tokens.StringContent, "foo"][Tokens.StringEnd].EOF();
            t.Load("%w(\rx\n \r\n  \nz\n)")[Tokens.VerbatimWordsBegin][Tokens.StringContent, "x"][Tokens.WordSeparator][Tokens.StringContent, "z"][Tokens.StringEnd].EOF();
                    
            t.Load("%1")[Tokens.Percent].Expect(Errors.UnknownQuotedStringType)[1].EOF();

            // heredoc:
            t.Load("p <<E\n\n1\n2\r3\r\nE\n")[Tokens.Identifier][Tokens.StringBegin]["\n1\n2\r3\n"][Tokens.StringEnd][Tokens.NewLine].EOF();
            t.Load("p <<E\nE\r\n")[Tokens.Identifier][Tokens.StringBegin][Tokens.StringEnd][Tokens.NewLine].EOF();
            t.Expect();
        }

        private void TokenizeEscapes1() {
            AssertTokenizer t = NewAssertTokenizer();
            t.DefaultEncoding = RubyEncoding.Binary;

            const string CR = "\r";
            const string LF = "\n";
            const string Q = "\"";
            const string BS = "\\";

            // string:

            t.Load(Q + BS + CR + LF + Q)[Tokens.StringBegin][Tokens.StringContent, ""][Tokens.StringEnd].EOF();
            t.Load(Q + BS + LF + Q)[Tokens.StringBegin][Tokens.StringContent, ""][Tokens.StringEnd].EOF();
            t.Load(Q + BS + CR + Q)[Tokens.StringBegin][Tokens.StringContent, "\r"][Tokens.StringEnd].EOF();

            t.Load(Q + BS + "M-" + CR + LF + Q)[Tokens.StringBegin][Tokens.StringContent, "\u008A"][Tokens.StringEnd].EOF();
            t.Load(Q + BS + "M-" + LF + Q)[Tokens.StringBegin][Tokens.StringContent, "\u008A"][Tokens.StringEnd].EOF();
            t.Load(Q + BS + "M-" + CR + Q)[Tokens.StringBegin][Tokens.StringContent, "\u008D"][Tokens.StringEnd].EOF();

            t.Load(Q + BS + "C-" + CR + LF + Q)[Tokens.StringBegin][Tokens.StringContent, "\n"][Tokens.StringEnd].EOF();
            t.Load(Q + BS + "C-" + LF + Q)[Tokens.StringBegin][Tokens.StringContent, "\n"][Tokens.StringEnd].EOF();
            t.Load(Q + BS + "C-" + CR + Q)[Tokens.StringBegin][Tokens.StringContent, "\r"][Tokens.StringEnd].EOF();

            t.Load(Q + BS + "c" + CR + LF + Q)[Tokens.StringBegin][Tokens.StringContent, "\n"][Tokens.StringEnd].EOF();
            t.Load(Q + BS + "c" + LF + Q)[Tokens.StringBegin][Tokens.StringContent, "\n"][Tokens.StringEnd].EOF();
            t.Load(Q + BS + "c" + CR + Q)[Tokens.StringBegin][Tokens.StringContent, "\r"][Tokens.StringEnd].EOF();

            // regex:

            t.Load("/" + BS + CR + LF + "/")[Tokens.RegexpBegin][Tokens.StringContent, ""][Tokens.RegexpEnd].EOF();
            t.Load("/" + BS + LF + "/")[Tokens.RegexpBegin][Tokens.StringContent, ""][Tokens.RegexpEnd].EOF();
            t.Load("/" + BS + CR + "/")[Tokens.RegexpBegin][Tokens.StringContent, BS + CR][Tokens.RegexpEnd].EOF();

            t.Load("/" + BS + "M-" + CR + LF + "/")[Tokens.RegexpBegin][Tokens.StringContent, BS + "M-\n"][Tokens.RegexpEnd].EOF();
            t.Load("/" + BS + "M-" + LF + "/")[Tokens.RegexpBegin][Tokens.StringContent, BS + "M-\n"][Tokens.RegexpEnd].EOF();
            t.Load("/" + BS + "M-" + CR + "/")[Tokens.RegexpBegin][Tokens.StringContent, BS + "M-\r"][Tokens.RegexpEnd].EOF();

            t.Load("/" + BS + "C-" + CR + LF + "/")[Tokens.RegexpBegin][Tokens.StringContent, BS + "C-\n"][Tokens.RegexpEnd].EOF();
            t.Load("/" + BS + "C-" + LF + "/")[Tokens.RegexpBegin][Tokens.StringContent, BS + "C-\n"][Tokens.RegexpEnd].EOF();
            t.Load("/" + BS + "C-" + CR + "/")[Tokens.RegexpBegin][Tokens.StringContent, BS + "C-\r"][Tokens.RegexpEnd].EOF();

            t.Load("/" + BS + "c" + CR + LF + "/")[Tokens.RegexpBegin][Tokens.StringContent, BS + "c\n"][Tokens.RegexpEnd].EOF();
            t.Load("/" + BS + "c" + LF + "/")[Tokens.RegexpBegin][Tokens.StringContent, BS + "c\n"][Tokens.RegexpEnd].EOF();
            t.Load("/" + BS + "c" + CR + "/")[Tokens.RegexpBegin][Tokens.StringContent, BS + "c\r"][Tokens.RegexpEnd].EOF();

            t.Expect();
        }

        private void TokenizeRegex1() {
            AssertTokenizer t = NewAssertTokenizer();

            t.Load("//")[Tokens.RegexpBegin][Tokens.RegexpEnd].EOF();
            t.Load("/foo/")[Tokens.RegexpBegin]["foo"][Tokens.RegexpEnd].EOF();

            t.Load("/foo/aib").Skip(2).Read(RubyRegexOptions.IgnoreCase).Expect(Errors.UnknownRegexOption, Errors.UnknownRegexOption);
            t.Load("/foo/9").Skip(2).Read(Tokens.RegexpEnd); // TODO: unexpected token 9
            t.Load("/foo/esuniiimmmxxxooo").Skip(2).
                Read(RubyRegexOptions.IgnoreCase | RubyRegexOptions.Multiline | RubyRegexOptions.Extended | RubyRegexOptions.Once | RubyRegexOptions.FIXED);

            t.Expect();
        }

        private void TokenizeStrings1() {
            AssertTokenizer t = NewAssertTokenizer();

            for (int i = 0; i < 128; i++) {
                switch (i) {
                    case '(':
                    case '{':
                    case '[':
                    case '<':
                    case 'Q':
                    case 'q':
                    case 'W':
                    case 'w':
                    case 'x':
                    case 'r':
                    case 's':
                        break;

                    default:
                        var str = "%" + (char)i + "foo" + (char)i;

                        if (Tokenizer.IsDecimalDigit(i)) {
                            t.Load(str)[Tokens.Percent][Tokens.Integer][Tokens.Identifier].Expect(Errors.UnknownQuotedStringType).EOF();
                        } else if (Tokenizer.IsUpperLetter(i)) {
                            t.Load(str)[Tokens.Percent][Tokens.ConstantIdentifier].Expect(Errors.UnknownQuotedStringType).EOF();
                        } else if (Tokenizer.IsLowerLetter(i)) {
                            t.Load(str)[Tokens.Percent][Tokens.Identifier].Expect(Errors.UnknownQuotedStringType).EOF();
                        } else {
                            t.Load(str)[Tokens.StringBegin]["foo"][Tokens.StringEnd].EOF();
                        }
                        break;
                }
            }

            t.Expect();
        }

        private void TokenizeStrings2() {
            AssertTokenizer t = new AssertTokenizer(this) {
                Verbatim = true
            };
            
            // string nested in a string:
            t.Load(@"""abc#{""x#@hello#{{}}y""}def""")
                [Tokens.StringBegin]
                [Tokens.StringContent]
                [Tokens.StringEmbeddedCodeBegin]
                    [Tokens.StringBegin]
                    [Tokens.StringContent]
                    [Tokens.StringEmbeddedVariableBegin]
                        [Tokens.InstanceVariable]
                    [Tokens.StringEmbeddedCodeBegin]
                        [Tokens.LeftBrace]
                        [Tokens.RightBrace]
                    [Tokens.StringEmbeddedCodeEnd]
                    [Tokens.StringContent]
                    [Tokens.StringEnd]
                [Tokens.StringEmbeddedCodeEnd]
                [Tokens.StringContent]
                [Tokens.StringEnd].
            EOF();

            // nested braces:
            t.Load(@"""a#{{{""#{{""#{1}""=>""c""}}""=>""#{2}""}=>""#{3}""}}a""")
                [Tokens.StringBegin]
                [Tokens.StringContent]
                [Tokens.StringEmbeddedCodeBegin]
                    [Tokens.LeftBrace]
                        [Tokens.LeftBrace]
                            [Tokens.StringBegin]
                            [Tokens.StringEmbeddedCodeBegin]
                                [Tokens.LeftBrace]
                                    [Tokens.StringBegin]
                                    [Tokens.StringEmbeddedCodeBegin]
                                        [1]
                                    [Tokens.StringEmbeddedCodeEnd]
                                    [Tokens.StringEnd]
                                [Tokens.DoubleArrow]
                                    [Tokens.StringBegin]
                                    [Tokens.StringContent]
                                    [Tokens.StringEnd]
                                [Tokens.RightBrace]
                            [Tokens.StringEmbeddedCodeEnd]
                            [Tokens.StringEnd]
                        [Tokens.DoubleArrow]
                            [Tokens.StringBegin]
                            [Tokens.StringEmbeddedCodeBegin]
                                [2]
                            [Tokens.StringEmbeddedCodeEnd]
                            [Tokens.StringEnd]
                        [Tokens.RightBrace]
                    [Tokens.DoubleArrow]
                        [Tokens.StringBegin]
                        [Tokens.StringEmbeddedCodeBegin]
                            [3]
                        [Tokens.StringEmbeddedCodeEnd]
                        [Tokens.StringEnd]
                    [Tokens.RightBrace]
                [Tokens.StringEmbeddedCodeEnd]
                [Tokens.StringContent]
                [Tokens.StringEnd].
            EOF();
            
            // =begin .. =end nested in a string:
            t.Load("\"hello#{\n=begin\nxxx\n=end\nidf\n}world\"")
                [Tokens.StringBegin]
                [Tokens.StringContent]
                    [Tokens.StringEmbeddedCodeBegin]
                    [Tokens.EndOfLine]
                    [Tokens.MultiLineComment]
                    [Tokens.Identifier, "idf"]
                    [Tokens.NewLine]
                    [Tokens.StringEmbeddedCodeEnd]
                [Tokens.StringContent]
                [Tokens.StringEnd].
            EOF();

            // braces nesting in word array:
            t.Load("%w{#$a #{b + 1} c}")
                [Tokens.VerbatimWordsBegin]
                [Tokens.StringContent, "#$a"][Tokens.WordSeparator]
                [Tokens.StringContent, "#{b"][Tokens.WordSeparator]
                [Tokens.StringContent, "+"][Tokens.WordSeparator]
                [Tokens.StringContent, "1}"][Tokens.WordSeparator]
                [Tokens.StringContent, "c"]
                [Tokens.StringEnd].
            EOF();
        }

        private void TokenizeEscapes2() {
            AssertTokenizer t = NewAssertTokenizer();
     
            // hexa:
            t.Load("\"\\x\n20\"")[Tokens.StringBegin]["?\n20"][Tokens.StringEnd].Expect(Errors.InvalidEscapeCharacter);
            t.Load("\"\\x2\n0\"")[Tokens.StringBegin]["\u0002\n0"][Tokens.StringEnd].EOF();
            t.Load("\"\\x20\n\"")[Tokens.StringBegin][" \n"][Tokens.StringEnd].EOF();
            
            // octal:
            t.Load("\"\\0\n40\"")[Tokens.StringBegin]["\0\n40"][Tokens.StringEnd].EOF();
            t.Load("\"\\04\n0\"")[Tokens.StringBegin]["\u0004\n0"][Tokens.StringEnd].EOF();
            t.Load("\"\\040\n\"")[Tokens.StringBegin][" \n"][Tokens.StringEnd].EOF();
            t.Load("\"\\123\"")[Tokens.StringBegin]["S"][Tokens.StringEnd].EOF();

            // multi-byte characters:
            t.Load(@"""abΣ\xce\xa3cd\xce\xa3e""")[Tokens.StringBegin]["abΣΣcdΣe", Encoding.UTF8][Tokens.StringEnd].EOF();

            // an incomplete character:
            t.Load(@"""ab\xce""")[Tokens.StringBegin][new byte[] { (byte)'a', (byte)'b', 0xce }][Tokens.StringEnd].EOF();

            t.Expect();
        }

        private void UnicodeEscapes1() {
            AssertTokenizer t = NewAssertTokenizer();

            int[] values = new[] { 0x20, 0x102020, 0x20, 0x20, 0 };
            int[] width = new[] { 2, 6, 6, 5, 1 };

            t.Load(@":""\u111""")[Tokens.SymbolBegin][Tokens.StringContent].Expect(Errors.InvalidEscapeCharacter);
            t.Load(@":""\u""")[Tokens.SymbolBegin][Tokens.StringContent].Expect(Errors.InvalidEscapeCharacter);

            // regex:
            t.Load(@"/\x20/")[Tokens.RegexpBegin][@"\x20"][Tokens.RegexpEnd].EOF();
            t.Load(@"/\u1234/")[Tokens.RegexpBegin][@"\u1234"][Tokens.RegexpEnd].EOF();
            t.Load(@"/\u{101234}/")[Tokens.RegexpBegin][@"\u{101234}"][Tokens.RegexpEnd].EOF();

            // braces:
            t.Load(@":""\u{123456}""")[Tokens.SymbolBegin][Tokens.StringContent].Expect(Errors.TooLargeUnicodeCodePoint);
            t.Load(@"%[{\u{05d0}}]")[Tokens.StringBegin]["{\u05d0}"][Tokens.StringEnd].EOF();
            t.Load(@"%[\u{1 2 3 4}]")[Tokens.StringBegin]["\u0001\u0002\u0003\u0004"][Tokens.StringEnd].EOF();
            t.Load(@"%[\u{}]")[Tokens.StringBegin][""][Tokens.StringEnd].Expect(Errors.InvalidUnicodeEscape);
            t.Load(@"%[\u{1 }]")[Tokens.StringBegin]["\u0001"][Tokens.StringEnd].Expect(Errors.InvalidUnicodeEscape);
            t.Load(@"%[\u{1  }]")[Tokens.StringBegin]["\u0001"][Tokens.StringEnd].Expect(Errors.InvalidUnicodeEscape);
            t.Load(@"%[\u{FFFFFF FFFFFFFFFFFFFFFF 3 4}]")[Tokens.StringBegin]["??\u0003\u0004"][Tokens.StringEnd].Expect(Errors.TooLargeUnicodeCodePoint, Errors.TooLargeUnicodeCodePoint);
            t.Load(@"%[\u{]")[Tokens.StringBegin][""][Tokens.StringEnd].Expect(Errors.UntermintedUnicodeEscape);
            t.Load(@"%[\u{1]")[Tokens.StringBegin]["\u0001"][Tokens.StringEnd].Expect(Errors.UntermintedUnicodeEscape);
            t.Load(@"%[\u{")[Tokens.StringBegin][""][Tokens.StringEnd].Expect(Errors.UntermintedUnicodeEscape, Errors.UnterminatedString);

            for (int i = 0; i < values.Length; i++) {
                t.Load(@"""\u{" + i.ToString("x" + width[i]) + @"}""")[Tokens.StringBegin][Char.ConvertFromUtf32(i)][Tokens.StringEnd].EOF();
            }

            // eoln in the middle of \u escape:
            t.Load("\"\\u0020\n\"")[Tokens.StringBegin][" \n"][Tokens.StringEnd].EOF();
            t.Load("\"\\u002\n0\"")[Tokens.StringBegin]["?002\n0"][Tokens.StringEnd].Expect(Errors.InvalidEscapeCharacter).EOF();
            t.Load("\"\\u00\n20\"")[Tokens.StringBegin]["?00\n20"][Tokens.StringEnd].Expect(Errors.InvalidEscapeCharacter).EOF();
            t.Load("\"\\u0\n020\"")[Tokens.StringBegin]["?0\n020"][Tokens.StringEnd].Expect(Errors.InvalidEscapeCharacter).EOF();
            t.Load("\"\\u\n0020\"")[Tokens.StringBegin]["?\n0020"][Tokens.StringEnd].Expect(Errors.InvalidEscapeCharacter).EOF();

            // TODO:
#if TODO
            t.DefaultEncoding = RubyEncoding.Binary;
            t.Load(@"""\u{5d0}""")[Tokens.StringBegin][@"\u{5d0}"][Tokens.StringEnd].Expect(Errors.EncodingsMixed).EOF();
#endif
            t.Expect();
        }

        private void CharacterToken1() {
            AssertTokenizer t = NewAssertTokenizer();
            t.Load("?a")[Tokens.Character, "a"].EOF();
            t.Load("?Σ")[Tokens.Character, "Σ"].EOF();
            
            // surrogate:
            string u12345 = Char.ConvertFromUtf32(0x12345);
            t.Load("?" + u12345)[Tokens.Character, u12345].EOF();
            
            // escapes:
            t.Load(@"?\u{1}")[Tokens.Character, "\u0001"].EOF();
            t.Load(@"?\u{}")[Tokens.Character, "\0"].Expect(Errors.InvalidUnicodeEscape);
            t.Load(@"?\u{")[Tokens.Character, "\0"].Expect(Errors.UntermintedUnicodeEscape);
            t.Load(@"?\u{1")[Tokens.Character, "\u0001"].Expect(Errors.UntermintedUnicodeEscape);
            t.Load(@"?\u{1 2}")[Tokens.Character, "\u0001"].Expect(Errors.UntermintedUnicodeEscape);
            t.Load(@"?\u{1123455}")[Tokens.Character, "?"].Expect(Errors.TooLargeUnicodeCodePoint);
        }

        private void LexicalState1() {
            AssertTokenizer t = NewAssertTokenizer();

            // command mode:
            t.Load("a")[Tokens.Identifier, "a"].State(LexicalState.EXPR_CMDARG).EOF();
            t.Load("1;a")[1][Tokens.Semicolon][Tokens.Identifier, "a"].State(LexicalState.EXPR_CMDARG).EOF();

            // 1.8 specific
            //t.Load("a(b c)");
            //t[Tokens.Identifier, "a"].State(LexicalState.EXPR_CMDARG);
            //t[Tokens.LeftParenthesis].State(LexicalState.EXPR_BEG);       // switches to command mode for the next non-whitespace token
            //t[Tokens.Identifier, "b"].State(LexicalState.EXPR_CMDARG);
            //t[Tokens.Identifier, "c"].State(LexicalState.EXPR_ARG);       // command mode switched off
            //t[Tokens.RightParenthesis].State(LexicalState.EXPR_END).EOF();

            t.Load("a\nb");
            t[Tokens.Identifier, "a"].State(LexicalState.EXPR_CMDARG);
            t[Tokens.NewLine].State(LexicalState.EXPR_BEG);                 // switches to command mode for the next non-whitespace token
            t[Tokens.Identifier, "b"].State(LexicalState.EXPR_CMDARG).EOF();

            t.Load("foo do a end");
            t[Tokens.Identifier, "foo"].State(LexicalState.EXPR_CMDARG);
            t[Tokens.Do].State(LexicalState.EXPR_BEG);                 // switches to command mode for the next non-whitespace token
            t[Tokens.Identifier, "a"].State(LexicalState.EXPR_CMDARG);
            t[Tokens.End].State(LexicalState.EXPR_END);
            t.EOF();
        }

        private void Heredoc1() {
            AssertTokenizer t = new AssertTokenizer(this) { Verbatim = false };

            t.Load("<<LABEL\nhello\nLABEL")
                [Tokens.StringBegin]["hello\n"][Tokens.StringEnd][Tokens.NewLine].EOF();

            t.Load("<<\"LABEL\"\nhello\nLABEL")
                [Tokens.StringBegin]["hello\n"][Tokens.StringEnd][Tokens.NewLine].EOF();

            t.Load("<<'LABEL'\nhello\nLABEL")
                [Tokens.StringBegin]["hello\n"][Tokens.StringEnd][Tokens.NewLine].EOF();

            t.Load("<<`LABEL`\nhello\nLABEL")
                [Tokens.ShellStringBegin]["hello\n"][Tokens.StringEnd][Tokens.NewLine].EOF();

            t.Load("<<LABEL\nLABEL123\nLABEL")
                [Tokens.StringBegin]["LABEL123\n"][Tokens.StringEnd][Tokens.NewLine].EOF();

            t.Load("puts <<L1, 1, <<L2, 2\naaa\nL1\nbbb\nL2\n3")
                [Tokens.Identifier, "puts"]
                [Tokens.StringBegin]["aaa\n"][Tokens.StringEnd]
                [Tokens.Comma][1][Tokens.Comma]
                [Tokens.StringBegin]["bbb\n"][Tokens.StringEnd]
                [Tokens.Comma][2]
                [Tokens.NewLine]
                [3].EOF();

            t.Load("puts <<A,1\\\n...\nA\n,2")
                [Tokens.Identifier, "puts"]
                [Tokens.StringBegin]
                ["...\n"]
                [Tokens.StringEnd].State(LexicalState.EXPR_END)
                [Tokens.Comma].State(LexicalState.EXPR_BEG)
                [1].State(LexicalState.EXPR_END)    // \\n is a whitespace
                [Tokens.Comma].State(LexicalState.EXPR_BEG)
                [2].State(LexicalState.EXPR_END).
            EOF();

            t.Load("puts <<A,(f\\\n...\nA\n())")
                [Tokens.Identifier, "puts"]
                [Tokens.StringBegin]
                ["...\n"]
                [Tokens.StringEnd].State(LexicalState.EXPR_END)
                [Tokens.Comma].State(LexicalState.EXPR_BEG)
                [Tokens.LeftExprParenthesis].State(LexicalState.EXPR_BEG)  
                [Tokens.Identifier, "f"].State(LexicalState.EXPR_ARG)      // \\n is a whitespace, WhitespaceSeen == true
                [Tokens.LeftArgParenthesis].State(LexicalState.EXPR_BEG)
                [Tokens.RightParenthesis]
                [Tokens.RightParenthesis].
            EOF();
            t.Expect();

            AssertTokenizer vt = new AssertTokenizer(this) { Verbatim = true };

            vt.Load("puts <<A,1\\\n...\nA\n,2")
                [Tokens.Identifier, "puts"]
                [Tokens.Whitespace]
                [Tokens.VerbatimHeredocBegin]
                [Tokens.Comma].State(LexicalState.EXPR_BEG)
                [1].State(LexicalState.EXPR_END)    
                [Tokens.Whitespace]                             // \\n 
                [Tokens.StringContent, "...\n"]
                [Tokens.VerbatimHeredocEnd].State(LexicalState.EXPR_END) // A label
                [Tokens.Comma].State(LexicalState.EXPR_BEG)
                [2].State(LexicalState.EXPR_END).
            EOF();

            vt.Load("puts <<A,(f\\\n...\nA\n())")
                [Tokens.Identifier, "puts"]
                [Tokens.Whitespace]
                [Tokens.VerbatimHeredocBegin]
                [Tokens.Comma].State(LexicalState.EXPR_BEG)
                [Tokens.LeftExprParenthesis].State(LexicalState.EXPR_BEG)
                [Tokens.Identifier, "f"].State(LexicalState.EXPR_ARG)   
                [Tokens.Whitespace]
                ["...\n"]
                [Tokens.VerbatimHeredocEnd].State(LexicalState.EXPR_ARG)       
                [Tokens.LeftArgParenthesis].State(LexicalState.EXPR_BEG)
                [Tokens.RightParenthesis]
                [Tokens.RightParenthesis].
            EOF();

            vt.Load(@"puts <<A,<<B
1
2#{f <<C,<<D}3#{g <<E}4
c
C
d#{f <<F}d
f
F
D
e
E
5
A
b
b
B")
                [Tokens.Identifier, "puts"]
                [Tokens.Whitespace]
                [Tokens.VerbatimHeredocBegin] // <<A
                [Tokens.Comma]
                [Tokens.VerbatimHeredocBegin] // <<B
                [Tokens.EndOfLine]
                ["1\n2"]
                [Tokens.StringEmbeddedCodeBegin]
                [Tokens.Identifier, "f"]
                [Tokens.Whitespace]
                [Tokens.VerbatimHeredocBegin] // <<C
                [Tokens.Comma]
                [Tokens.VerbatimHeredocBegin] // <<D
                [Tokens.StringEmbeddedCodeEnd]
                ["3"]
                [Tokens.StringEmbeddedCodeBegin]
                [Tokens.Identifier, "g"]
                [Tokens.Whitespace]
                [Tokens.VerbatimHeredocBegin] // <<E
                [Tokens.StringEmbeddedCodeEnd]
                ["4\n"]
                ["c\n"]
                [Tokens.VerbatimHeredocEnd]   // C
                ["d"]
                [Tokens.StringEmbeddedCodeBegin]
                [Tokens.Identifier, "f"]
                [Tokens.Whitespace]
                [Tokens.VerbatimHeredocBegin] // <<F
                [Tokens.StringEmbeddedCodeEnd]
                ["d\n"]
                ["f\n"]
                [Tokens.VerbatimHeredocEnd]   // F
                [Tokens.VerbatimHeredocEnd]   // D
                ["e\n"]
                [Tokens.VerbatimHeredocEnd]   // E
                ["5\n"]
                [Tokens.VerbatimHeredocEnd]   // A
                ["b\nb\n"]
                [Tokens.VerbatimHeredocEnd]   // B
            .EOF();

            t.Expect();

            // index:                                111111111122 2222 222 2333 333 3 3
            //                             0123456789012345678901 2345 678 9012 345 6 7
            TestCategorizer(Engine, null, "puts <<L1, 1, <<L2, 2\naaa\nL1\nbbb\nL2\r\n3", 
            // column:                     1234567890123456789012 1234 123 1234 123 4 1 
            // line:                       1111111111111111111111 2222 333 4444 555 5 6
            // 
                // puts
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5)), TokenCategory.Identifier, TokenTriggers.None),
                // ' '
                new TokenInfo(new SourceSpan(new SourceLocation(4, 1, 5), new SourceLocation(5, 1, 6)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // <<L1
                new TokenInfo(new SourceSpan(new SourceLocation(5, 1, 6), new SourceLocation(9, 1, 10)), TokenCategory.StringLiteral, TokenTriggers.None),
                // ,
                new TokenInfo(new SourceSpan(new SourceLocation(9, 1, 10), new SourceLocation(10, 1, 11)), TokenCategory.Delimiter, TokenTriggers.ParameterNext),
                // ' '
                new TokenInfo(new SourceSpan(new SourceLocation(10, 1, 11), new SourceLocation(11, 1, 12)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // 1
                new TokenInfo(new SourceSpan(new SourceLocation(11, 1, 12), new SourceLocation(12, 1, 13)), TokenCategory.NumericLiteral, TokenTriggers.None),
                // ,
                new TokenInfo(new SourceSpan(new SourceLocation(12, 1, 13), new SourceLocation(13, 1, 14)), TokenCategory.Delimiter, TokenTriggers.ParameterNext),
                // ' '
                new TokenInfo(new SourceSpan(new SourceLocation(13, 1, 14), new SourceLocation(14, 1, 15)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // <<L2
                new TokenInfo(new SourceSpan(new SourceLocation(14, 1, 15), new SourceLocation(18, 1, 19)), TokenCategory.StringLiteral, TokenTriggers.None),
                // ,
                new TokenInfo(new SourceSpan(new SourceLocation(18, 1, 19), new SourceLocation(19, 1, 20)), TokenCategory.Delimiter, TokenTriggers.ParameterNext),
                // ' '
                new TokenInfo(new SourceSpan(new SourceLocation(19, 1, 20), new SourceLocation(20, 1, 21)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // 2
                new TokenInfo(new SourceSpan(new SourceLocation(20, 1, 21), new SourceLocation(21, 1, 22)), TokenCategory.NumericLiteral, TokenTriggers.None),
                // \n
                new TokenInfo(new SourceSpan(new SourceLocation(21, 1, 22), new SourceLocation(22, 2, 1)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // aaa\n
                new TokenInfo(new SourceSpan(new SourceLocation(22, 2, 1), new SourceLocation(26, 3, 1)), TokenCategory.StringLiteral, TokenTriggers.None),
                // L1\n
                new TokenInfo(new SourceSpan(new SourceLocation(26, 3, 1), new SourceLocation(29, 4, 1)), TokenCategory.StringLiteral, TokenTriggers.None),
                // bbb\n
                new TokenInfo(new SourceSpan(new SourceLocation(29, 4, 1), new SourceLocation(33, 5, 1)), TokenCategory.StringLiteral, TokenTriggers.None),
                // L2\r\n
                new TokenInfo(new SourceSpan(new SourceLocation(33, 5, 1), new SourceLocation(37, 6, 1)), TokenCategory.StringLiteral, TokenTriggers.None),
                // 3
                new TokenInfo(new SourceSpan(new SourceLocation(37, 6, 1), new SourceLocation(38, 6, 2)), TokenCategory.NumericLiteral, TokenTriggers.None)
            );

            // index:                                 1111
            //                             0123456789 0123
            TestCategorizer(Engine, null, "puts <<L1\naaa", 
            // column:                     1234567890 1234
            // line:                       1111111111 2222
            // 
                // puts
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5)), TokenCategory.Identifier, TokenTriggers.None),
                // ' '
                new TokenInfo(new SourceSpan(new SourceLocation(4, 1, 5), new SourceLocation(5, 1, 6)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // <<L1
                new TokenInfo(new SourceSpan(new SourceLocation(5, 1, 6), new SourceLocation(9, 1, 10)), TokenCategory.StringLiteral, TokenTriggers.None),
                // \n
                new TokenInfo(new SourceSpan(new SourceLocation(9, 1, 10), new SourceLocation(10, 2, 1)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // aaa\n
                new TokenInfo(new SourceSpan(new SourceLocation(10, 2, 1), new SourceLocation(13, 2, 4)), TokenCategory.StringLiteral, TokenTriggers.None),
                // <missing heredoc end>
                new TokenInfo(new SourceSpan(new SourceLocation(13, 2, 4), new SourceLocation(13, 2, 4)), TokenCategory.StringLiteral, TokenTriggers.None)
            );

            // index:                                1 1111 11111
            //                             01234567890 1234 56789
            TestCategorizer(Engine, null, "puts <<-L1\naaa\n  L1",
            // column:                     12345678901 1234 12345
            // line:                       11111111111 2222 33333
            // 
                // puts
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5)), TokenCategory.Identifier, TokenTriggers.None),
                // ' '
                new TokenInfo(new SourceSpan(new SourceLocation(4, 1, 5), new SourceLocation(5, 1, 6)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // <<-L1
                new TokenInfo(new SourceSpan(new SourceLocation(5, 1, 6), new SourceLocation(10, 1, 11)), TokenCategory.StringLiteral, TokenTriggers.None),
                // \n
                new TokenInfo(new SourceSpan(new SourceLocation(10, 1, 11), new SourceLocation(11, 2, 1)), TokenCategory.WhiteSpace, TokenTriggers.None),
                // aaa\n
                new TokenInfo(new SourceSpan(new SourceLocation(11, 2, 1), new SourceLocation(15, 3, 1)), TokenCategory.StringLiteral, TokenTriggers.None),
                // L1
                new TokenInfo(new SourceSpan(new SourceLocation(17, 3, 3), new SourceLocation(19, 3, 5)), TokenCategory.StringLiteral, TokenTriggers.None)
            );
        }

        public void ParseSymbols1() {
            AssertTokens(":''", Tokens.SymbolBegin, Tokens.StringEnd, Tokens.EndOfFile);
            AssertTokens(":do", Tokens.SymbolBegin, Tokens.Do, Tokens.EndOfFile);
            AssertTokens("foo :do", Tokens.Identifier, Tokens.SymbolBegin, Tokens.BlockDo, Tokens.EndOfFile);
        }

        private void AssertTokens(string/*!*/ source, params Tokens[] expected) {
            var tokens = new List<Tokens>();
            var parser = new Parser() {
                TokenSink = (token, span) => tokens.Add(token)
            };

            parser.Parse(Context.CreateSnippet(source, SourceCodeKind.AutoDetect), new RubyCompilerOptions(), ErrorSink.Null);
            Assert(tokens.ToArray().ValueEquals(expected));
        }

        // encodings suported in preamble:
        private static readonly string[] preambleEncodingNames = 
            new[] { "ASCII-8BIT", "ASCII", "BINARY", "US-ASCII", "UTF-8", "EUC-JP", "SJIS", "SHIFT_JIS", "LOCALE", "FILESYSTEM" };

        private void Encoding1() {
            foreach (var name in preambleEncodingNames) {
                var encoding = Context.GetEncodingByRubyName(name);
                Assert(encoding != null);

                // the encoding must be an identity on ASCII characters:
                Assert(RubyEncoding.AsciiIdentity(encoding));
            }

            foreach (var info in Encoding.GetEncodings()) {
                var encoding = info.GetEncoding();
                
                // doesn't blow up (the method checks itself):
                RubyEncoding.AsciiIdentity(encoding);

                //Console.WriteLine("case " + info.CodePage + ": // " + encoding.EncodingName);
            }

        }

        private void Encoding2() {
            var source1 = Context.CreateSourceUnit(new BinaryContentProvider(BinaryEncoding.Instance.GetBytes(
@"#! foo bar
# enCoding = ascii-8BIT

p __ENCODING__
")), null, Encoding.UTF8, SourceCodeKind.File);

            using (var reader = source1.GetReader()) {
                Assert(reader.Encoding == BinaryEncoding.Instance);
            }

            AssertOutput(() => source1.Execute(), @"#<Encoding:ASCII-8BIT>");

            // default hosted encoding is UTF8:
            var source2 = Context.CreateSnippet("p __ENCODING__", SourceCodeKind.Expression);
            AssertOutput(() => source2.Execute(), @"#<Encoding:UTF-8>");
        }
        
        private void Encoding4() {
            var enc = Engine.Execute<RubyEncoding>(@"eval('# encoding: SJIS
__ENCODING__
')");
            Assert(enc == RubyEncoding.SJIS);
        }

        private void Encoding_Host1() {
            Encoding_HostHelper(Encoding.UTF8, "\u0394", true);
            Encoding_HostHelper(Encoding.UTF32, "\u0394", false);
        }

        private void Encoding_HostHelper(Encoding/*!*/ encoding, string/*!*/ specialChars, bool shouldSucceed) {
            var preamble = "# coding = " + encoding.WebName + "\r\n";
            var src = "$X = '" + specialChars + "'";

            var content = new List<byte>();
            content.AddRange(BinaryEncoding.Instance.GetBytes(preamble));
            content.AddRange(encoding.GetBytes(src));

            var source = Engine.CreateScriptSource(new BinaryContentProvider(content.ToArray()), null);
            
            // test encoding:
            try {
                var detectedEncoding = source.DetectEncoding();
                Assert(encoding.WebName == detectedEncoding.WebName);
            } catch (IOException) {
                Assert(!shouldSucceed);
            }

            // test content:
            try {
                var code = source.GetCode();
                Assert(StringComparer.Ordinal.Equals(code, preamble + src));
            } catch (IOException) {
                Assert(!shouldSucceed);
            }
        }

        // Ruby preamble overrides the encoding's preamble (BOM)
        public void Encoding_Host2() {
            var src = "# encoding: ASCII-8BIT\r\n$X = '\u0394'";
            var binsrc = Encoding.UTF8.GetBytes(src);

            var content = new List<byte>();
            content.AddRange(Encoding.UTF8.GetPreamble());
            content.AddRange(binsrc);
            var source = Engine.CreateScriptSource(new BinaryContentProvider(content.ToArray()), null);
            var encoding = source.DetectEncoding();
            Assert(encoding == BinaryEncoding.Instance);
            var actualCode = source.GetCode();

            // \u0394 is encoded in 2 bytes, which are represented by 2 characters in binary encoding:
            Assert(actualCode.Length == src.Length + 1);
            Assert(actualCode.Length == binsrc.Length);
        }

        public void NamesEncoding1() {
            // TODO:
            XTestOutput(@"#encoding: SJIS
class Cﾎ
  def initialize
    @ﾎ = 1
    @@ﾎ = 2
  end
  
  def ﾎ
  end
  
  Xﾎ = 5
end

$Xﾎ = 3
ﾎ = 4

puts Cﾎ.name
puts Cﾎ.name.encoding
puts Cﾎ.new.inspect

Cﾎ.instance_methods(false).each { |x| puts x,x.encoding }
Cﾎ.new.instance_variables.each { |x| puts x,x.encoding }
Cﾎ.class_variables.each { |x| puts x,x.encoding }
Cﾎ.constants.each { |x| puts x,x.encoding }
local_variables.each { |x| puts x,x.encoding }
global_variables.each { |x| puts x,x.encoding if x[0] == 'X' }
", @"
Cﾎ
#<Encoding: SJIS>
#<Cﾎ TODO>
ﾎ
#<Encoding: SJIS>
@ﾎ
#<Encoding: SJIS>
@@ﾎ
#<Encoding: SJIS>
Xﾎ
#<Encoding: SJIS>
ﾎ
#<Encoding: SJIS>
$ﾎ
#<Encoding: SJIS>
");
        }

        // TODO:
        public void BlockParameterSyntax1() {
            // "yield(&p)" -> block argument should not be given
            // "yield &p" -> block argument should not be given
            // "yield 1=>2, &p" -> block argument should not be given
        }

        private void AstLocations1() {
#if FEATURE_CALL_SITE_TRACER
            // DumpExpression uses private reflection:
            if (_driver.PartialTrust) return;

            var sourceUnit = Context.CreateSnippet(@"
def add a,b
  a + b
end

add 1, 1
add 'foo', 'bar'
", SourceCodeKind.Expression);

            var options = new RubyCompilerOptions();
            var parser = new Parser();
            var tokens = new List<KeyValuePair<SourceSpan, Tokens>>();

            parser.TokenSink = (token, span) => { tokens.Add(new KeyValuePair<SourceSpan, Tokens>(span, token)); };
            var ast = parser.Parse(sourceUnit, options, Context.RuntimeErrorSink);

            const int Id = 0x12345678;

            var lambda = CallSiteTracer.Transform<Func<RubyScope, object, object>>(ast, sourceUnit, options, Id);
            var code = new RubyScriptCode(lambda, sourceUnit, TopScopeFactoryKind.Hosted);

            var locations = new List<int>();
            CallSiteTracer.Register((context, args, result, id, location) => {
                locations.Add(location);
                Debug.Assert(id == Id);
                Debug.Assert(location > 0);

                //Console.WriteLine("-- {0} ---------", location);
                //Console.WriteLine(this);
                //Console.WriteLine(AstUtils.DumpExpression(result.Restrictions.ToExpression()));
                //Console.WriteLine();
                //Console.WriteLine(AstUtils.DumpExpression(result.Expression));
                //Console.WriteLine("----------------");
            });

            code.Run();

            // TODO: doesn't include method body since its is lazily compiled:
            Debug.Assert(locations.Count == 2 && locations[0] == 31 && locations[1] == 41);
            // Debug.Assert(locations.Count == 4 && locations[0] == 31 && locations[1] == 19 && locations[2] == 41 && locations[3] == 19);
#endif
        }

        #region Helpers

        private void TestBigInt(string/*!*/ number, int @base) {
            TestBigInt(number, @base, 10);
        }

        private void TestBigInt(string/*!*/ number, int @base, int repeat) {
            Stopwatch optimizedTime = new Stopwatch();
            Stopwatch universalTime = new Stopwatch();

            StringBuilder n = new StringBuilder(number);
            for (int j = 0; j < repeat; j++) {
                n.Append(number);
            }
            number = n.ToString();

            for (int i = 0; i < number.Length - 1; i++) {
                string sub = number.Substring(i);
                string s = sub.Replace("_", "").ToUpper();
                BigInteger b;
                Tokenizer.BignumParser p = new Tokenizer.BignumParser();
                p.Position = 0;
                p.Buffer = s.ToCharArray();

                optimizedTime.Start();
                b = p.Parse(s.Length, @base);
                optimizedTime.Stop();

                Assert(b.ToString(@base) == s.TrimStart('0'));

                p.Position = 0;

                universalTime.Start();
                b = p.ParseDefault(s.Length, (uint)@base);
                universalTime.Stop();

                Assert(b.ToString(@base) == s.TrimStart('0'));
            }

            if (repeat != 0) {
                Console.WriteLine("{0}: optimized = {1}ms, universal = {2}ms",
                    @base,
                    optimizedTime.ElapsedMilliseconds,
                    universalTime.ElapsedMilliseconds);
            }
        }

        private AssertTokenizer/*!*/ AssertTokenBigInteger(string/*!*/ source, int @base) {
            return new AssertTokenizer(this).Load(source).ReadBigInteger(source.Replace("_", "").TrimStart('0'), @base);
        }

        private AssertTokenizer/*!*/ NewAssertTokenizer() {
            return new AssertTokenizer(this);
        }

        #endregion
    }
}