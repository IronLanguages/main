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
                
        public void Scenario_RubyTokenizer1() {
            LoggingErrorSink log = new LoggingErrorSink();

            List<Tokens> tokens;
            tokens = GetRubyTokens(log, "foo (while (f do end) do end) do end");

            Assert(tokens.Count == 14);
            Assert(tokens[0] == Tokens.Identifier);
            Assert(tokens[1] == Tokens.LparenArg);
            Assert(tokens[2] == Tokens.While);
            Assert(tokens[3] == Tokens.LeftParen);
            Assert(tokens[4] == Tokens.Identifier);
            Assert(tokens[5] == Tokens.Do);
            Assert(tokens[6] == Tokens.End);
            Assert(tokens[7] == (Tokens)')');
            Assert(tokens[8] == Tokens.LoopDo);
            Assert(tokens[9] == Tokens.End);
            Assert(tokens[10] == (Tokens)')');
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
        }

        public void Scenario_RubyCategorizer1() {
            TestCategorizer(Engine, "print 'foo' #bar", -1, new TokenInfo[] {
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(5, 1, 6)), TokenCategory.Identifier, TokenTriggers.None),
                new TokenInfo(new SourceSpan(new SourceLocation(5, 1, 6), new SourceLocation(6, 1, 7)), TokenCategory.WhiteSpace, TokenTriggers.None),
                new TokenInfo(new SourceSpan(new SourceLocation(6, 1, 7), new SourceLocation(7, 1, 8)), TokenCategory.StringLiteral, TokenTriggers.None),
                new TokenInfo(new SourceSpan(new SourceLocation(7, 1, 8), new SourceLocation(10, 1, 11)), TokenCategory.StringLiteral, TokenTriggers.None),
                new TokenInfo(new SourceSpan(new SourceLocation(10, 1, 11), new SourceLocation(11, 1, 12)), TokenCategory.StringLiteral, TokenTriggers.None),
                new TokenInfo(new SourceSpan(new SourceLocation(11, 1, 12), new SourceLocation(12, 1, 13)), TokenCategory.WhiteSpace, TokenTriggers.None),
                new TokenInfo(new SourceSpan(new SourceLocation(12, 1, 13), new SourceLocation(16, 1, 17)), TokenCategory.LineComment, TokenTriggers.None),
            });

            TestCategorizer(Engine, "a\r\nb", -1, new TokenInfo[] { 
                new TokenInfo(new SourceSpan(new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)), TokenCategory.Identifier, TokenTriggers.None),   // a
                new TokenInfo(new SourceSpan(new SourceLocation(1, 1, 2), new SourceLocation(3, 1, 4)), TokenCategory.WhiteSpace, TokenTriggers.None),   // \r\n
                new TokenInfo(new SourceSpan(new SourceLocation(3, 2, 1), new SourceLocation(4, 2, 2)), TokenCategory.Identifier, TokenTriggers.None),   // b
            });

            //                             11111111 11222222222233 333
            //                   012345678901234567 89012345678901 234
            TestCategorizer(Engine, "canvas.Event { |x|\nputs 'string'\n}", -1, new TokenInfo[] {
            //                   1234567890123456789 12345678901234 12
            //                            1111111111          11111   
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
                new TokenInfo(new SourceSpan(new SourceLocation(18, 1, 19), new SourceLocation(19, 1, 20)), TokenCategory.WhiteSpace, TokenTriggers.None),      // \n
                // line 2
                new TokenInfo(new SourceSpan(new SourceLocation(19, 2, 1), new SourceLocation(23, 2, 5)), TokenCategory.Identifier, TokenTriggers.None),        // puts
                new TokenInfo(new SourceSpan(new SourceLocation(23, 2, 5), new SourceLocation(24, 2, 6)), TokenCategory.WhiteSpace, TokenTriggers.None),        //  
                new TokenInfo(new SourceSpan(new SourceLocation(24, 2, 6), new SourceLocation(25, 2, 7)), TokenCategory.StringLiteral, TokenTriggers.None),     // '
                new TokenInfo(new SourceSpan(new SourceLocation(25, 2, 7), new SourceLocation(31, 2, 13)), TokenCategory.StringLiteral, TokenTriggers.None),    // string
                new TokenInfo(new SourceSpan(new SourceLocation(31, 2, 13), new SourceLocation(32, 2, 14)), TokenCategory.StringLiteral, TokenTriggers.None),   // '
                new TokenInfo(new SourceSpan(new SourceLocation(32, 2, 14), new SourceLocation(33, 2, 15)), TokenCategory.WhiteSpace, TokenTriggers.None),      // \n (significant)
                // line 3
                new TokenInfo(new SourceSpan(new SourceLocation(33, 3, 1), new SourceLocation(34, 3, 2)), TokenCategory.Grouping, TokenTriggers.MatchBraces),   // }
            });
        }

        public void Identifiers1() {
            string surrogate = "\ud800\udc00";

            Assert(Tokenizer.IsConstantName("C", false));
            Assert(Tokenizer.IsConstantName("Cx", false));
            Assert(Tokenizer.IsConstantName("C9", false));
            Assert(Tokenizer.IsConstantName("CazAZ0123456789_", false));
            Assert(Tokenizer.IsConstantName("C_", false));
            Assert(!Tokenizer.IsConstantName(null, false));
            Assert(!Tokenizer.IsConstantName("", false));
            Assert(!Tokenizer.IsConstantName("C=", false));
            Assert(!Tokenizer.IsConstantName("C?", false));
            Assert(!Tokenizer.IsConstantName("C!", false));
            Assert(!Tokenizer.IsConstantName("_", false));
            Assert(!Tokenizer.IsConstantName("0", false));
            Assert(!Tokenizer.IsConstantName("c", false));
            Assert(!Tokenizer.IsConstantName("Σ", false));
            Assert(!Tokenizer.IsConstantName("Σ", true));
            Assert(Tokenizer.IsConstantName("CΣ", true));

            Assert(Tokenizer.IsMethodName("C", false));
            Assert(Tokenizer.IsMethodName("Cx", false));
            Assert(Tokenizer.IsMethodName("CazAZ0123456789_", false));
            Assert(Tokenizer.IsMethodName("f=", false));
            Assert(Tokenizer.IsMethodName("f?", false));
            Assert(Tokenizer.IsMethodName("f!", false));
            Assert(Tokenizer.IsMethodName("_", false));
            Assert(Tokenizer.IsMethodName("c", false));
            Assert(!Tokenizer.IsMethodName("=", false));
            Assert(!Tokenizer.IsMethodName("?", false));
            Assert(!Tokenizer.IsMethodName("!", false));
            Assert(!Tokenizer.IsMethodName(null, false));
            Assert(!Tokenizer.IsMethodName("", false));
            Assert(!Tokenizer.IsMethodName("Σ", false));
            Assert(Tokenizer.IsMethodName("Σ", true));
            Assert(Tokenizer.IsMethodName(surrogate, true));

            Assert(Tokenizer.IsGlobalVariableName("$x", false));
            Assert(Tokenizer.IsGlobalVariableName("$XazAZ0123456789_", false));
            Assert(!Tokenizer.IsGlobalVariableName("$f=", false));
            Assert(!Tokenizer.IsGlobalVariableName("$f?", false));
            Assert(!Tokenizer.IsGlobalVariableName("$f!", false));
            Assert(!Tokenizer.IsGlobalVariableName("$f$", false));
            Assert(!Tokenizer.IsGlobalVariableName(null, false));
            Assert(!Tokenizer.IsGlobalVariableName("$", false));
            Assert(!Tokenizer.IsGlobalVariableName("$$", false));
            Assert(!Tokenizer.IsGlobalVariableName("f", false));
            Assert(!Tokenizer.IsGlobalVariableName("ff", false));
            Assert(!Tokenizer.IsGlobalVariableName("fff", false));
            Assert(!Tokenizer.IsGlobalVariableName("$Σ", false));
            Assert(Tokenizer.IsGlobalVariableName("$Σ", true));
            Assert(Tokenizer.IsGlobalVariableName("$" + surrogate, true));

            Assert(Tokenizer.IsInstanceVariableName("@x", false));
            Assert(Tokenizer.IsInstanceVariableName("@XazAZ0123456789_", false));
            Assert(!Tokenizer.IsInstanceVariableName("@f=", false));
            Assert(!Tokenizer.IsInstanceVariableName("@f?", false));
            Assert(!Tokenizer.IsInstanceVariableName("@f!", false));
            Assert(!Tokenizer.IsInstanceVariableName("@f@", false));
            Assert(!Tokenizer.IsInstanceVariableName(null, false));
            Assert(!Tokenizer.IsInstanceVariableName("@", false));
            Assert(!Tokenizer.IsInstanceVariableName("@@", false));
            Assert(!Tokenizer.IsInstanceVariableName("@@@", false));
            Assert(!Tokenizer.IsInstanceVariableName("f", false));
            Assert(!Tokenizer.IsInstanceVariableName("ff", false));
            Assert(!Tokenizer.IsInstanceVariableName("fff", false));
            Assert(!Tokenizer.IsInstanceVariableName("@Σ", false));
            Assert(Tokenizer.IsInstanceVariableName("@Σ", true));
            Assert(Tokenizer.IsInstanceVariableName("@" + surrogate, true));

            Assert(Tokenizer.IsClassVariableName("@@x", false));
            Assert(Tokenizer.IsClassVariableName("@@XazAZ0123456789_", false));
            Assert(!Tokenizer.IsClassVariableName("@@f=", false));
            Assert(!Tokenizer.IsClassVariableName("@@f?", false));
            Assert(!Tokenizer.IsClassVariableName("@@f!", false));
            Assert(!Tokenizer.IsClassVariableName("@@f@", false));
            Assert(!Tokenizer.IsClassVariableName(null, false));
            Assert(!Tokenizer.IsClassVariableName("@", false));
            Assert(!Tokenizer.IsClassVariableName("@@", false));
            Assert(!Tokenizer.IsClassVariableName("@@@", false));
            Assert(!Tokenizer.IsClassVariableName("f", false));
            Assert(!Tokenizer.IsClassVariableName("ff", false));
            Assert(!Tokenizer.IsClassVariableName("fff", false));
            Assert(!Tokenizer.IsClassVariableName("@@Σ", false));
            Assert(Tokenizer.IsClassVariableName("@@Σ", true));
            Assert(Tokenizer.IsClassVariableName("@@" + surrogate, true));
        }

        private void Identifiers2() {
            AssertTokenizer t = NewAssertTokenizer();

            // 'variable' non-terminal needs to set $<String>$ even for keywords, 
            // otherwise the content of previous token is stored in token value and is interpreted as string.
            t.Load("//\ntrue")[Tokens.RegexpBegin][Tokens.RegexpEnd][(Tokens)'\n'][Tokens.True].EOF();

            t.Load("Σ = CΣ", (tok) => tok.AllowNonAsciiIdentifiers = true).
                ReadSymbol(Tokens.Identifier, "Σ")[(Tokens)'='].ReadSymbol(Tokens.ConstantIdentifier, "CΣ").EOF();

            t.Load("Σ = CΣ")[Tokens.InvalidCharacter].Expect(Errors.InvalidCharacterInExpression);

            t.Load("@Σ=@@Σ=$Σ", (tok) => tok.AllowNonAsciiIdentifiers = true)
                [Tokens.InstanceVariable][(Tokens)'=']
                [Tokens.ClassVariable][(Tokens)'=']
                [Tokens.GlobalVariable].EOF();

            t.Load("def Σ;end", (tok) => tok.AllowNonAsciiIdentifiers = true)
                [Tokens.Def][Tokens.Identifier][(Tokens)';'][Tokens.End].EOF();

            // BOM can be used as an identifier in 1.8 -KU mode:
            t.Load(new byte[] { 
                0xEF, 0xBB, 0xBF, (byte)'=', (byte)'1' 
            }, (tok) => { 
                tok.Compatibility = RubyCompatibility.Ruby18; tok.AllowNonAsciiIdentifiers = true; 
            })
            [Tokens.Identifier][(Tokens)'='][1].EOF();

            // we should report a warning if -KCODE is not used and treat BOM as whitespace (MRI 1.8 reports an error):
            t.Load(new byte[] { 
                0xEF, 0xBB, 0xBF, (byte)'=', (byte)'1' 
            }, (tok) => { 
                tok.Compatibility = RubyCompatibility.Ruby18; 
                tok.AllowNonAsciiIdentifiers = false;
                tok.Verbatim = true;
            }) 
            [Tokens.Whitespace][(Tokens)'='][1].Expect(Errors.InvalidUseOfByteOrderMark);
            
            t.Expect();
        }

        public void Scenario_ParseBigInts1() {
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

        private void Scenario_ParseNumbers1() {
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

            t.Load(".20").Read((Tokens)'.').Expect(Errors.NoFloatingLiteral);
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

        private void Scenario_ParseInstanceClassVariables1() {
            AssertTokenizer t = NewAssertTokenizer();

            t.Load("@").Read((Tokens)'@');
            t.Load("@@").Read((Tokens)'@');
            t.Load("@1").Read((Tokens)'@').Expect(Errors.InvalidInstanceVariableName);
            t.Load("@@1").Read((Tokens)'@').Expect(Errors.InvalidClassVariableName);
            t.Load("@_").ReadSymbol(Tokens.InstanceVariable, "@_");
            t.Load("@@_").ReadSymbol(Tokens.ClassVariable, "@@_");
            t.Load("@aA1_").ReadSymbol(Tokens.InstanceVariable, "@aA1_");
            t.Load("@@aA1_").ReadSymbol(Tokens.ClassVariable, "@@aA1_");
            
            t.Expect();
        }

        private void ParseGlobalVariables1() {
            AssertTokenizer t = NewAssertTokenizer();

            t.Load("$")[(Tokens)'$'].EOF();
            t.Load("$_")[Tokens.GlobalVariable, "_"].EOF();
            t.Load("$_1")[Tokens.GlobalVariable, "_1"].EOF();
            t.Load("$_-1")[Tokens.GlobalVariable, "_"][(Tokens)'-'][1].EOF();
            t.Load("$!")[Tokens.GlobalVariable, "!"].EOF();
            t.Load("$@")[Tokens.GlobalVariable, "@"].EOF();
            t.Load("$-")[Tokens.GlobalVariable, "-"].EOF();
            t.Load("$--1")[Tokens.GlobalVariable, "-"][(Tokens)'-'][1].EOF();
            t.Load("$-x")[Tokens.GlobalVariable, "-x"].EOF();
            
            t.Expect();
        }

        private void ParseEolns1() {
            AssertTokenizer t = NewAssertTokenizer();

            // empty source:
            t.Load("").EOF();

            // escaped eoln:
            t.Load("[\\\r\n]")[Tokens.Lbrack][(Tokens)']'].EOF();
            t.Load("[\\\n]")[Tokens.Lbrack][(Tokens)']'].EOF();
            t.Load("[\\\r]")[Tokens.Lbrack][(Tokens)'\\'][(Tokens)']'].EOF();

            // eoln used to quote a string:
            t.Load("x = %\r\nhello\r\n")[Tokens.Identifier][(Tokens)'='][Tokens.StringBegin][Tokens.StringContent, "hello"][Tokens.StringEnd].EOF();
            t.Load("x = %\nhello\r\n")[Tokens.Identifier][(Tokens)'='][Tokens.StringBegin][Tokens.StringContent, "hello"][Tokens.StringEnd].EOF();
            t.Load("x = %\rhello\r\n\r")[Tokens.Identifier][(Tokens)'='][Tokens.StringBegin][Tokens.StringContent, "hello\n"][Tokens.StringEnd].EOF();

            t.Load("%Q\r\nhello\n")[Tokens.StringBegin][Tokens.StringContent, "hello"][Tokens.StringEnd].EOF();
            t.Load("%Q\nhello\n")[Tokens.StringBegin][Tokens.StringContent, "hello"][Tokens.StringEnd].EOF();
            t.Load("%Q\rhello\r\n\r")[Tokens.StringBegin][Tokens.StringContent, "hello\n"][Tokens.StringEnd].EOF();

            t.Load("%w[  foo]")[Tokens.VerbatimWordsBegin][Tokens.StringContent, "foo"][Tokens.WordSeparator][Tokens.StringEnd].EOF();
            t.Load("%w[\n   foo]")[Tokens.VerbatimWordsBegin][Tokens.StringContent, "foo"][Tokens.WordSeparator][Tokens.StringEnd].EOF();
            t.Load("%w(\rx\n \r\n  \nz\n)")[Tokens.VerbatimWordsBegin][Tokens.StringContent, "x"][Tokens.WordSeparator]
                [Tokens.StringContent, "z"][Tokens.WordSeparator][Tokens.StringEnd].EOF();
                    
            t.Load("%1")[(Tokens)'%'].Expect(Errors.UnknownQuotedStringType)[1].EOF();

            // heredoc:
            t.Load("p <<E\n\n1\n2\r3\r\nE\n")[Tokens.Identifier][Tokens.StringBegin]["\n1\n2\r3\n"][Tokens.StringEnd][(Tokens)'\n'].EOF();
            t.Load("p <<E\nE\r\n")[Tokens.Identifier][Tokens.StringBegin][Tokens.StringEnd][(Tokens)'\n'].EOF();
            t.Expect();
        }

        private void ParseEscapes1() {
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

        private void Scenario_ParseRegex1() {
            AssertTokenizer t = NewAssertTokenizer();

            t.Load("//")[Tokens.RegexpBegin][Tokens.RegexpEnd].EOF();
            t.Load("/foo/")[Tokens.RegexpBegin]["foo"][Tokens.RegexpEnd].EOF();

            t.Load("/foo/aib").Skip(2).Read(RubyRegexOptions.IgnoreCase).Expect(Errors.UnknownRegexOption, Errors.UnknownRegexOption);
            t.Load("/foo/9").Skip(2).Read(Tokens.RegexpEnd); // TODO: unexpected token 9
            t.Load("/foo/esuniiimmmxxxooo").Skip(2).
                Read(RubyRegexOptions.IgnoreCase | RubyRegexOptions.Multiline | RubyRegexOptions.Extended | RubyRegexOptions.Once | RubyRegexOptions.FIXED);

            t.Expect();
        }

        private void StringLiterals1() {
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
                            t.Load(str)[(Tokens)'%'][Tokens.Integer][Tokens.Identifier].Expect(Errors.UnknownQuotedStringType).EOF();
                        } else if (Tokenizer.IsUpperLetter(i)) {
                            t.Load(str)[(Tokens)'%'][Tokens.ConstantIdentifier].Expect(Errors.UnknownQuotedStringType).EOF();
                        } else if (Tokenizer.IsLowerLetter(i)) {
                            t.Load(str)[(Tokens)'%'][Tokens.Identifier].Expect(Errors.UnknownQuotedStringType).EOF();
                        } else {
                            t.Load(str)[Tokens.StringBegin]["foo"][Tokens.StringEnd].EOF();
                        }
                        break;
                }
            }

            t.Expect();
        }

        private void Escapes1() {
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

        [Options(Compatibility = RubyCompatibility.Ruby19)]
        private void UnicodeEscapes1() {
            AssertTokenizer t = NewAssertTokenizer();

            int[] values = new[] { 0x20, 0x102020, 0x20, 0x20, 0 };
            int[] width = new[] { 2, 6, 6, 5, 1 };

            for (int i = 0; i < values.Length; i++) {
                t.Load(@"""\u{" + i.ToString("x" + width[i]) + @"}""")[Tokens.StringBegin][Char.ConvertFromUtf32(i)][Tokens.StringEnd].EOF();
            }

            t.Load(@":""\u{123456}""")[Tokens.SymbolBegin][Tokens.StringContent].Expect(Errors.TooLargeUnicodeCodePoint);
            t.Load(@":""\u{0}""")[Tokens.SymbolBegin][Tokens.StringContent].Expect(Errors.NullCharacterInSymbol);
            t.Load(@":""\u0000""")[Tokens.SymbolBegin][Tokens.StringContent].Expect(Errors.NullCharacterInSymbol);
            t.Load(@":""\u111""")[Tokens.SymbolBegin][Tokens.StringContent].Expect(Errors.InvalidEscapeCharacter);
            t.Load(@":""\u""")[Tokens.SymbolBegin][Tokens.StringContent].Expect(Errors.InvalidEscapeCharacter);
            t.Load(@":""\u{123""")[Tokens.SymbolBegin][Tokens.StringContent].Expect(Errors.InvalidEscapeCharacter);
            t.Load(@":""\u{123g}""")[Tokens.SymbolBegin][Tokens.StringContent].Expect(Errors.InvalidEscapeCharacter);

            // regex:
            t.Load(@"/\x20/")[Tokens.RegexpBegin][@"\x20"][Tokens.RegexpEnd].EOF();
            t.Load(@"/\u1234/")[Tokens.RegexpBegin][@"\u1234"][Tokens.RegexpEnd].EOF();
            t.Load(@"/\u{101234}/")[Tokens.RegexpBegin][@"\u{101234}"][Tokens.RegexpEnd].EOF();

            // braces:
            t.Load(@"%{{\u{05d0}}}")[Tokens.StringBegin]["{\u05d0}"][Tokens.StringEnd].EOF();

            // eoln in the middle of \u escape:
            t.Load("\"\\u0020\n\"")[Tokens.StringBegin][" \n"][Tokens.StringEnd].EOF();
            t.Load("\"\\u002\n0\"")[Tokens.StringBegin]["?002\n0"][Tokens.StringEnd].Expect(Errors.InvalidEscapeCharacter).EOF();
            t.Load("\"\\u00\n20\"")[Tokens.StringBegin]["?00\n20"][Tokens.StringEnd].Expect(Errors.InvalidEscapeCharacter).EOF();
            t.Load("\"\\u0\n020\"")[Tokens.StringBegin]["?0\n020"][Tokens.StringEnd].Expect(Errors.InvalidEscapeCharacter).EOF();
            t.Load("\"\\u\n0020\"")[Tokens.StringBegin]["?\n0020"][Tokens.StringEnd].Expect(Errors.InvalidEscapeCharacter).EOF();

            // TODO:
            t.DefaultEncoding = RubyEncoding.Binary;
            t.Load(@"""\u{5d0}""")[Tokens.StringBegin][@"\u{5d0}"][Tokens.StringEnd].Expect(Errors.EncodingsMixed).EOF();

            t.Expect();
        }

        [Options(Compatibility = RubyCompatibility.Ruby18)]
        private void UnicodeEscapes2() {
            AssertTokenizer t = NewAssertTokenizer();

            t.Load(@":""\u{123456789}""")[Tokens.SymbolBegin][@"u{123456789}"][Tokens.StringEnd].EOF();
            t.Load(@":""\u123456789""")[Tokens.SymbolBegin][@"u123456789"][Tokens.StringEnd].EOF();
            t.Load(@"/\u1234/")[Tokens.RegexpBegin][@"\u1234"][Tokens.RegexpEnd].EOF();
            t.Load(@"/\u{101234}/")[Tokens.RegexpBegin][@"\u{101234}"][Tokens.RegexpEnd].EOF();

            t.Expect();
        }

        private void Heredoc1() {
            AssertTokenizer t = NewAssertTokenizer();

            t.Load("<<LABEL\nhello\nLABEL")
                [Tokens.StringBegin]["hello\n"][Tokens.StringEnd][(Tokens)'\n'].EOF();

            t.Load("<<\"LABEL\"\nhello\nLABEL")
                [Tokens.StringBegin]["hello\n"][Tokens.StringEnd][(Tokens)'\n'].EOF();

            t.Load("<<'LABEL'\nhello\nLABEL")
                [Tokens.StringBegin]["hello\n"][Tokens.StringEnd][(Tokens)'\n'].EOF();

            t.Load("<<`LABEL`\nhello\nLABEL")
                [Tokens.ShellStringBegin]["hello\n"][Tokens.StringEnd][(Tokens)'\n'].EOF();

            t.Load("<<LABEL\nLABEL123\nLABEL")
                [Tokens.StringBegin]["LABEL123\n"][Tokens.StringEnd][(Tokens)'\n'].EOF();

            t.Expect();
        }

        public void Symbols1() {
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

        public void KCode1() {
            var sjisEncoding = RubyEncoding.KCodeSJIS;

            var sjisEngine = Ruby.CreateEngine((setup) => {
                setup.Options["KCode"] = RubyEncoding.KCodeSJIS;
            });
            Assert(sjisEngine.Execute("$KCODE").ToString() == "SJIS");
            
            var utf8Engine = Ruby.CreateEngine((setup) => {
                setup.Options["KCode"] = RubyEncoding.KCodeUTF8;
            });
            Assert(utf8Engine.Execute("$KCODE").ToString() == "UTF8");

            // using default encoding (UTF8) for Unicode string source (ignoring KCODE):
            var str = sjisEngine.Execute<MutableString>("Σ = 'Σ'");
            Assert(str.Encoding == RubyEncoding.UTF8 && str.ToString() == "Σ");

            // Use source code encoding no matter what characters are used in the string:
            str = sjisEngine.Execute<MutableString>("'ascii'");
            Assert(str.Encoding == RubyEncoding.UTF8 && str.ToString() == "ascii");

            // Unicode source code (KCODE ignored):
            var bytes = Encoding.UTF8.GetBytes("Σ = 'Σ'");
            str = sjisEngine.CreateScriptSource(new BinaryContentProvider(bytes), null, Encoding.UTF8).Execute<MutableString>();
            Assert(str.Encoding == RubyEncoding.UTF8 && str.ToString() == "Σ");

            // SJIS source code (KCODE ignored):
            bytes = sjisEncoding.Encoding.GetBytes(@"ﾎ = 'ﾎ'");
            str = utf8Engine.CreateScriptSource(new BinaryContentProvider(bytes), null, sjisEncoding.Encoding).Execute<MutableString>();
            Assert(str.Encoding == sjisEncoding && str.ToString() == "ﾎ");

            // eval uses KCODE (binary source, KCODE == SJIS):
            bytes = sjisEncoding.Encoding.GetBytes(@"eval(""ﾎ = 'ﾎ'"")");
            str = sjisEngine.CreateScriptSource(new BinaryContentProvider(bytes), null, BinaryEncoding.Instance).Execute<MutableString>();
            Assert(str.Encoding == sjisEncoding && str.ToString() == "ﾎ");

            // eval uses KCODE (SJIS source, KCODE == SJIS):
            bytes = sjisEncoding.Encoding.GetBytes(@"eval(""ﾎ = 'ﾎ'"")");
            str = sjisEngine.CreateScriptSource(new BinaryContentProvider(bytes), null, sjisEncoding.Encoding).Execute<MutableString>();
            Assert(str.Encoding == sjisEncoding && str.ToString() == "ﾎ");
        }

        private void KCode2() {
            if (_driver.PartialTrust) return;

            var sjisEncoding = RubyEncoding.KCodeSJIS;

            // change KCODE at runtime:
            Context.SetGlobalVariable(null, "KCODE", MS("S"));
            Assert(ReferenceEquals(Context.KCode, sjisEncoding));

            // load file encoded in SJIS:
            var tmpPath = Path.GetTempFileName();
            try {
                Runtime.Globals.SetVariable("TempFileName", MS(tmpPath));
                File.WriteAllBytes(tmpPath, sjisEncoding.Encoding.GetBytes("Cﾎ = 'ﾎ'"));
                Engine.Execute(@"load(TempFileName)");
                var str = Runtime.Globals.GetVariable<MutableString>("Cﾎ");
                Assert(str.Encoding == sjisEncoding && str.ToString() == "ﾎ");
            } finally {
                File.Delete(tmpPath);
            }
        }

        // encodings suported in preamble:
        private static readonly string[] preambleEncodingNames = 
            new[] { "ASCII-8BIT", "ASCII", "BINARY", "US-ASCII", "UTF-8", "EUC-JP", "SJIS", "SHIFT_JIS" };

        [Options(Compatibility = RubyCompatibility.Ruby19)]
        private void Encoding1() {
            foreach (var name in preambleEncodingNames) {
                var encoding = RubyEncoding.GetEncodingByRubyName(name);
                Assert(encoding != null);

                // the encoding must be an identity on ASCII characters:
                Assert(RubyEncoding.IsAsciiIdentity(encoding));
            }

            foreach (var info in Encoding.GetEncodings()) {
                var encoding = info.GetEncoding();
                
                // doesn't blow up (the method checks itself):
                RubyEncoding.IsAsciiIdentity(encoding);

                //Console.WriteLine("case " + info.CodePage + ": // " + encoding.EncodingName);
            }

        }

        [Options(Compatibility = RubyCompatibility.Ruby19)]
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
            AssertOutput(() => source2.Execute(), @"#<Encoding:utf-8>");
        }

        [Options(Compatibility = RubyCompatibility.Ruby18)]
        private void Encoding3() {
            AssertExceptionThrown<MissingMethodException>(() =>
                CompilerTest("__ENCODING__")
            );

            // ignores preamble:
            Context.CreateFileUnit("foo.rb", "# enCoding = UNDEFINED_ENCODING").Execute();
        }

        [Options(Compatibility = RubyCompatibility.Ruby19)]
        private void Encoding4() {
            var enc = Engine.Execute<RubyEncoding>(@"eval('# encoding: SJIS
__ENCODING__
')");
            Assert(enc == RubyEncoding.GetRubyEncoding("SJIS"));
        }

        [Options(Compatibility = RubyCompatibility.Ruby19)]
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
        [Options(Compatibility = RubyCompatibility.Ruby19)]
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

        [Options(Compatibility = RubyCompatibility.Ruby19)]
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

                Assert(b.ToString((uint)@base) == s.TrimStart('0'));

                p.Position = 0;

                universalTime.Start();
                b = p.ParseDefault(s.Length, (uint)@base);
                universalTime.Stop();

                Assert(b.ToString((uint)@base) == s.TrimStart('0'));
            }

            if (repeat != 0) {
                Console.WriteLine("{0}: optimized = {1}ms, universal = {2}ms",
                    @base,
                    optimizedTime.ElapsedMilliseconds,
                    universalTime.ElapsedMilliseconds);
            }
        }

        public void TestCategorizer(ScriptEngine engine, string src, int charCount, params TokenInfo[] expected) {
            if (charCount == -1) charCount = src.Length;

            TokenCategorizer categorizer = engine.GetService<TokenCategorizer>();

            categorizer.Initialize(null, engine.CreateScriptSourceFromString(src), SourceLocation.MinValue);
            IEnumerable<TokenInfo> actual = categorizer.ReadTokens(charCount);

            int i = 0;
            foreach (TokenInfo info in actual) {
                Assert(i < expected.Length);
                if (!info.Equals(expected[i])) {
                    Assert(false);
                }
                i++;
            }
            Assert(i == expected.Length);
        }

        private AssertTokenizer/*!*/ AssertTokenBigInteger(string/*!*/ source, uint @base) {
            return new AssertTokenizer(this).Load(source).ReadBigInteger(source.Replace("_", "").TrimStart('0'), @base);
        }

        private AssertTokenizer/*!*/ NewAssertTokenizer() {
            return new AssertTokenizer(this);
        }

        #endregion
    }
}