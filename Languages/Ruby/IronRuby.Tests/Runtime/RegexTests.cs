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
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using System.Text;
using System.Diagnostics;

namespace IronRuby.Tests {
    public partial class Tests {
        public void Regex1() {
            AssertOutput(delegate() {
                CompilerTest(@"
r = /foo/imx
puts r.to_s
puts r.inspect

puts s = /xx#{r}xx#{r}xx/i.to_s
puts t = /yy#{s}yy/.to_s
");
            }, @"
(?mix:foo)
/foo/mix
(?i-mx:xx(?mix:foo)xx(?mix:foo)xx)
(?-mix:yy(?i-mx:xx(?mix:foo)xx(?mix:foo)xx)yy)
");
        }

        public void Regex2() {
            TestOutput(@"
puts(/#{/a/}/)                     # MRI: (?-mix:a)
puts(/#{nil}#{/a/}#{nil}/)         # MRI: (?-mix:a)
puts(/#{/a/}b/)
puts(/b#{/a/}/)
", @"
(?-mix:(?-mix:a))
(?-mix:(?-mix:a))
(?-mix:(?-mix:a)b)
(?-mix:b(?-mix:a))
");
        }

        [Options(NoRuntime = true)]
        public void RegexTransform1() {
            TestCorrectPatternTranslation(@"", @"");

            // escapes
            TestCorrectPatternTranslation(@"\\", @"\\");
            TestCorrectPatternTranslation(@"\_", @"_");
            TestCorrectPatternTranslation(@"abc\0\01\011", "abc\\0\\01\\011");
            TestCorrectPatternTranslation(@"\n\t\r\f\v\a\e\b\A\B\Z\z", "\\\n\\\t\\\r\f\v\a\u001B\\b\\A\\B\\Z\\z");
            TestCorrectPatternTranslation(@"[\n\t\r\f\v\a\e\b\A\B\Z\z]", "[\\\n\\\t\\\r\f\v\a\u001B\bABZz]");
            TestCorrectPatternTranslation(@"\G", RubyRegexOptions.NONE, @"\G", true);
            
            // meta-characters
            TestCorrectPatternTranslation(@"\xd", "\\\u000d");
            TestCorrectPatternTranslation(@"\xdz", "\\\u000dz");
            TestCorrectPatternTranslation(@"\*", @"\*");
            TestCorrectPatternTranslation(@"\[", @"\[");
            TestCorrectPatternTranslation(@"\#", @"\#");
            TestCorrectPatternTranslation(@"\0", @"\0");
            TestCorrectPatternTranslation(@"\x09\x0a\x0d\x20\u0009\u000a\u000d\u0020\u{9 a d 20}",
                                           "\\\u0009\\\u000a\\\u000d\\\u0020\\\u0009\\\u000a\\\u000d\\\u0020\\\u0009\\\u000a\\\u000d\\\u0020");
            TestCorrectPatternTranslation(@"[a\-z]", @"[a\-z]");

            // Unicode escapes
            TestCorrectPatternTranslation(@"\u002a", @"\*");
            TestCorrectPatternTranslation(@"\u005b", @"\[");
            TestCorrectPatternTranslation(@"\u{005b}", @"\[");
            TestCorrectPatternTranslation(@"\u{5b 2a}", @"\[\*");

            // Posix categories
            TestCorrectPatternTranslation(@"[x[:alnum:]y]", @"[x\p{L}\p{N}\p{M}y]");
            TestCorrectPatternTranslation(@"[a[^:alnum:]b]", @"[a\P{L}b-[\p{N}\p{M}-[ab]]]");
            TestCorrectPatternTranslation(@"[[:alpha:]]", @"[\p{L}\p{M}]");
            TestCorrectPatternTranslation(@"[[^:alpha:]]", @"[\P{L}-[\p{M}]]");
            TestCorrectPatternTranslation(@"[[:ascii:]]", @"[\p{IsBasicLatin}]");
            TestCorrectPatternTranslation(@"[[^:ascii:]]", @"[\P{IsBasicLatin}]");
            TestCorrectPatternTranslation(@"[[:blank:]]", "[\\p{Zs}\t]");
            TestCorrectPatternTranslation(@"[[^:blank:]]", "[\\P{Zs}-[\t]]");
            TestCorrectPatternTranslation(@"[[:cntrl:]]", @"[\p{Cc}]");
            TestCorrectPatternTranslation(@"[[^:cntrl:]]", @"[\P{Cc}]");
            TestCorrectPatternTranslation(@"[[:digit:]]", @"[\p{Nd}]");
            TestCorrectPatternTranslation(@"[[^:digit:]]", @"[\P{Nd}]");
            TestCorrectPatternTranslation(@"[[:lower:]]", @"[\p{Ll}]");
            TestCorrectPatternTranslation(@"[[^:lower:]]", @"[\P{Ll}]");
            TestCorrectPatternTranslation(@"[[:punct:]]", @"[\p{P}]");
            TestCorrectPatternTranslation(@"[[^:punct:]]", @"[\P{P}]");
            TestCorrectPatternTranslation(@"[[:space:]]", "[\\p{Z}\u0085\u0009-\u000d]");
            TestCorrectPatternTranslation(@"[[^:space:]]", "[\\P{Z}-[\u0085\u0009-\u000d]]");
            TestCorrectPatternTranslation(@"[[:upper:]]", @"[\p{Lu}]");
            TestCorrectPatternTranslation(@"[[^:upper:]]", @"[\P{Lu}]");
            TestCorrectPatternTranslation(@"[[:xdigit:]]", "[a-fA-F0-9]");
            TestCorrectPatternTranslation(@"[[^:xdigit:]]", "[\0-\uffff-[a-fA-F0-9]]");

            // Unicode categories
            TestCorrectPatternTranslation(@"\p{L}", @"[\p{L}]");
            TestCorrectPatternTranslation(@"\p{Alnum}", @"[\p{L}\p{N}\p{M}]");
            TestCorrectPatternTranslation(@"\P{Alnum}*", @"[\P{L}-[\p{N}\p{M}]]*");
            TestCorrectPatternTranslation(@"\P{^Alnum}", @"[\p{L}\p{N}\p{M}]");
            TestCorrectPatternTranslation(@"[a\p{Alnum}b]", @"[a\p{L}\p{N}\p{M}b]");
            TestCorrectPatternTranslation(@"[^\p{Alnum}]", "[\0-\uffff-[\\p{L}\\p{N}\\p{M}]]");
            TestCorrectPatternTranslation(@"[\P{Alnum}]", @"[\P{L}-[\p{N}\p{M}]]");
            TestCorrectPatternTranslation(@"\P{^Alnum}", @"[\p{L}\p{N}\p{M}]");
            TestCorrectPatternTranslation(@"\p{^Alnum}", @"[\P{L}-[\p{N}\p{M}]]");
            TestCorrectPatternTranslation(@"\p{Alnum}", @"[\p{L}\p{N}\p{M}]");
            TestCorrectPatternTranslation(@"\P{Alnum}", @"[\P{L}-[\p{N}\p{M}]]");
            TestCorrectPatternTranslation(@"\p{Alpha}", @"[\p{L}\p{M}]");
            TestCorrectPatternTranslation(@"\P{Alpha}", @"[\P{L}-[\p{M}]]");
            TestCorrectPatternTranslation(@"\p{ASCII}", "[\\p{IsBasicLatin}]");
            TestCorrectPatternTranslation(@"\P{ASCII}", "[\\P{IsBasicLatin}]");
            TestCorrectPatternTranslation(@"\p{Blank}", "[\\p{Zs}\t]");
            TestCorrectPatternTranslation(@"\P{Blank}", "[\\P{Zs}-[\t]]");
            TestCorrectPatternTranslation(@"\p{Cntrl}", @"[\p{Cc}]");
            TestCorrectPatternTranslation(@"\P{Cntrl}", @"[\P{Cc}]");
            TestCorrectPatternTranslation(@"\p{Digit}", @"[\p{Nd}]");
            TestCorrectPatternTranslation(@"\P{Digit}", @"[\P{Nd}]");
            TestCorrectPatternTranslation(@"\p{Lower}", @"[\p{Ll}]");
            TestCorrectPatternTranslation(@"\P{Lower}", @"[\P{Ll}]");
            TestCorrectPatternTranslation(@"\p{Punct}", @"[\p{P}]");
            TestCorrectPatternTranslation(@"\P{Punct}", @"[\P{P}]");
            TestCorrectPatternTranslation(@"\p{Space}", "[\\p{Z}\u0085\u0009-\u000d]");
            TestCorrectPatternTranslation(@"\P{Space}", "[\\P{Z}-[\u0085\u0009-\u000d]]");
            TestCorrectPatternTranslation(@"\p{Upper}", @"[\p{Lu}]");
            TestCorrectPatternTranslation(@"\P{Upper}", @"[\P{Lu}]");
            TestCorrectPatternTranslation(@"\p{XDigit}", "[a-fA-F0-9]");
            TestCorrectPatternTranslation(@"\P{XDigit}", "[\0-\uffff-[a-fA-F0-9]]");
       
            // possessive quantifiers
            TestCorrectPatternTranslation(@"xyza*+", @"xyz(?>a*)");
            TestCorrectPatternTranslation(@"x[a-b]*+", @"x(?>[a-b]*)");
            TestCorrectPatternTranslation(@"x[a-b]*+*+", @"x(?>(?>[a-b]*)*)");
            TestCorrectPatternTranslation(@"x[a-b]{1,2}+", @"x(?:[a-b]{1,2})+");
            TestCorrectPatternTranslation(@"x{1,2,*+", @"x{1,2(?>,*)");
            TestCorrectPatternTranslation(@"x{1,2*+", @"x{1,(?>2*)");
            TestCorrectPatternTranslation(@"x{1,*+", @"x{1(?>,*)");
            TestCorrectPatternTranslation(@"x{,*+", @"x{(?>,*)");
            TestCorrectPatternTranslation(@"x{1*+", @"x{(?>1*)");
            TestCorrectPatternTranslation(@"x{*+", @"x(?>{*)");

            // ranges
            TestCorrectPatternTranslation("[a-z]", "[a-z]");
            TestCorrectPatternTranslation(@"[\u{40}-z]", "[\u0040-z]");
            TestCorrectPatternTranslation(@"[\u{40}-\u{60}]", "[\u0040-\u0060]");
            TestCorrectPatternTranslation(@"[\x40-\x60]", "[\u0040-\u0060]");
            TestCorrectPatternTranslation(@"[\001-\7]", "[\u0001-\u0007]");
            TestCorrectPatternTranslation(@"[\u{2a}-\u{2b}]", "[\\*-\\+]");
            TestCorrectPatternTranslation(@"[x-]", @"[x\-]");
            TestCorrectPatternTranslation(@"[---]", @"[\--\-]");
            TestCorrectPatternTranslation(@"[\u{1 2 40}-\u{60 3 4}]", "[\u0001\u0002\u0040-\u0060\u0003\u0004]");
            TestCorrectPatternTranslation(@"[\u{1 2 40}-\u0060]", "[\u0001\u0002\u0040-\u0060]");
            TestCorrectPatternTranslation(@"[\u{1 2 40}-z]", "[\u0001\u0002\u0040-z]");
            TestCorrectPatternTranslation(@"[\x3f-\u{40 1 2}]", "[\\\u003f-\u0040\u0001\u0002]");
            TestCorrectPatternTranslation(@"[\w-]", @"[\w\-]");
            TestCorrectPatternTranslation(@"[\p{Alnum}-]", @"[\p{L}\p{N}\p{M}\-]");

            // character set operations
            TestCorrectPatternTranslation("[a-z&&d-e]", "[a-z-[\0-\uffff-[d-e]]]");
            TestCorrectPatternTranslation("[a-z&&[d-e&&e-f]]", "[a-z-[\0-\uffff-[d-e-[\0-\uffff-[e-f]]]]]");
            TestCorrectPatternTranslation("[a-z&&^[b[^c]]]", "[a-z-[c-[b^]]]");
            TestCorrectPatternTranslation("[a-z&&[^b[^c]]]", "[a-z-[\0-\uffff-[c-[b]]]]");
            TestCorrectPatternTranslation("[[^a-z][e-f][^b-q]]", "[\0-\uffff-[a-z-[\0-\uffff-[b-q-[e-f]]]]]");
            TestCorrectPatternTranslation("[&&d-e]", "[a-[a]]");
            TestCorrectPatternTranslation("[a-z&&[d-e&&e-f]x&&^[b[^c]]]", "[a-z-[\0-\uffff-[d-ex-[\0-\uffff-[e-fx-[c-[b^]]]]]]]");
            TestCorrectPatternTranslation("[^[a-b][c-d][^e-f]&&[a-z&&[^d-e]]]", "[\0-\uffff-[a-z-[d-ee-f-[a-bc-d-[d-e]]]]]");

            // groups
            TestCorrectPatternTranslation("((((a))))", "((((a))))");
            TestCorrectPatternTranslation("(?<name>a)", "(?<name>a)");
            TestCorrectPatternTranslation("(?:a)", "(?:a)");
            TestCorrectPatternTranslation("(?:a)", "(?:a)");
            TestCorrectPatternTranslation("(?mix-mix)", "(?six-six)");
            TestCorrectPatternTranslation("(?mix-mix:)", "(?six-six:)");
            TestCorrectPatternTranslation("(?mix-mix:a)", "(?six-six:a)");
            TestCorrectPatternTranslation("(?-mix:a)", "(?-six:a)");
            TestCorrectPatternTranslation("(?m:a)", "(?s:a)");
            TestCorrectPatternTranslation("(?mi:a)", "(?si:a)");
            TestCorrectPatternTranslation("(?m)", "(?s)");
            TestCorrectPatternTranslation("(?<name2>)(?<name1-name2>a)", "(?<name2>)(?<name1-name2>a)");
            TestCorrectPatternTranslation("(?'name2')(?'name1-name2'a)", "(?'name2')(?'name1-name2'a)");
            TestCorrectPatternTranslation("(?=)", "(?=)");
            TestCorrectPatternTranslation("(?=x)", "(?=x)");
            TestCorrectPatternTranslation("(?<=)", "(?<=)");
            TestCorrectPatternTranslation("(?<=x)", "(?<=x)");
            TestCorrectPatternTranslation("(?<!)", "(?<!)");
            TestCorrectPatternTranslation("(?<!x)", "(?<!x)");
            TestCorrectPatternTranslation("(?>)", "(?>)");
            TestCorrectPatternTranslation("(?>x)", "(?>x)");
            TestCorrectPatternTranslation("(?>(?=(?<!f)(o)(o))(?<bar>))", "(?>(?=(?<!f)(o)(o))(?<bar>))");
            
            // backreferences:
            TestCorrectPatternTranslation(@"(x) (?'name') \k<1> \k<name> \k'1' \k<name>", @"(x) (?'name') \k<1> \k<name> \k'1' \k<name>");

            // error: TestCorrectPatternTranslation("(?<a)b>c)", "(?<a)b>c)");
        }

        //[DebuggerHidden]
        private void TestCorrectPatternTranslation(string/*!*/ pattern, string/*!*/ expected) {
            TestCorrectPatternTranslation(pattern, RubyRegexOptions.NONE, expected, false);
        }

        //[DebuggerHidden]
        private void TestCorrectPatternTranslation(string/*!*/ pattern, RubyRegexOptions options, string/*!*/ expected, bool expectedGAnchor) {
            bool hasGAnchor;
            string actual = RegexpTransformer.Transform(pattern, options, out hasGAnchor);
            AreEqual(expected, actual);
            new Regex(expected);
            Assert(hasGAnchor == expectedGAnchor);
        }

        [Options(NoRuntime = true)]
        public void RegexTransform2() {
            string p = @"^
        ([a-zA-Z][-+.a-zA-Z\d]*):                     (?# 1: scheme)
        (?:
           ((?:[-_.!~*'()a-zA-Z\d;?:@&=+$,]|%[a-fA-F\d]{2})(?:[-_.!~*'()a-zA-Z\d;/?:@&=+$,\[\]]|%[a-fA-F\d]{2})*)              (?# 2: opaque)
        |
           (?:(?:
             //(?:
                 (?:(?:((?:[-_.!~*'()a-zA-Z\d;:&=+$,]|%[a-fA-F\d]{2})*)@)?  (?# 3: userinfo)
                   (?:((?:(?:(?:[a-zA-Z\d](?:[-a-zA-Z\d]*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:[-a-zA-Z\d]*[a-zA-Z\d])?)\.?|\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}|\[(?:(?:[a-fA-F\d]{1,4}:)*(?:[a-fA-F\d]{1,4}|\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})|(?:(?:[a-fA-F\d]{1,4}:)*[a-fA-F\d]{1,4})?::(?:(?:[a-fA-F\d]{1,4}:)*(?:[a-fA-F\d]{1,4}|\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}))?)\]))(?::(\d*))?))?(?# 4: host, 5: port)
               |
                 ((?:[-_.!~*'()a-zA-Z\d$,;+@&=+]|%[a-fA-F\d]{2})+)           (?# 6: registry)
               )
             |
             (?!//))                              (?# XXX: '//' is the mark for hostport)
             (/(?:[-_.!~*'()a-zA-Z\d:@&=+$,]|%[a-fA-F\d]{2})*(?:;(?:[-_.!~*'()a-zA-Z\d:@&=+$,]|%[a-fA-F\d]{2})*)*(?:/(?:[-_.!~*'()a-zA-Z\d:@&=+$,]|%[a-fA-F\d]{2})*(?:;(?:[-_.!~*'()a-zA-Z\d:@&=+$,]|%[a-fA-F\d]{2})*)*)*)?              (?# 7: path)
           )(?:\?((?:[-_.!~*'()a-zA-Z\d;/?:@&=+$,\[\]]|%[a-fA-F\d]{2})*))?           (?# 8: query)
        )
        (?:\#((?:[-_.!~*'()a-zA-Z\d;/?:@&=+$,\[\]]|%[a-fA-F\d]{2})*))?            (?# 9: fragment)
      $";

            string e = @"^
        ([a-zA-Z][\-+.a-zA-Z\d]*):                     
        (?:
           ((?:[\-_.!~*'()a-zA-Z\d;?:@&=+$,]|%[a-fA-F\d]{2})(?:[\-_.!~*'()a-zA-Z\d;/?:@&=+$,\[\]]|%[a-fA-F\d]{2})*)              
        |
           (?:(?:
             //(?:
                 (?:(?:((?:[\-_.!~*'()a-zA-Z\d;:&=+$,]|%[a-fA-F\d]{2})*)@)?  
                   (?:((?:(?:(?:[a-zA-Z\d](?:[\-a-zA-Z\d]*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:[\-a-zA-Z\d]*[a-zA-Z\d])?)\.?|[\d]{1,3}\.[\d]{1,3}\.[\d]{1,3}\.[\d]{1,3}|\[(?:(?:[a-fA-F\d]{1,4}:)*(?:[a-fA-F\d]{1,4}|[\d]{1,3}\.[\d]{1,3}\.[\d]{1,3}\.[\d]{1,3})|(?:(?:[a-fA-F\d]{1,4}:)*[a-fA-F\d]{1,4})?::(?:(?:[a-fA-F\d]{1,4}:)*(?:[a-fA-F\d]{1,4}|[\d]{1,3}\.[\d]{1,3}\.[\d]{1,3}\.[\d]{1,3}))?)\]))(?::([\d]*))?))?
               |
                 ((?:[\-_.!~*'()a-zA-Z\d$,;+@&=+]|%[a-fA-F\d]{2})+)           
               )
             |
             (?!//))                              
             (/(?:[\-_.!~*'()a-zA-Z\d:@&=+$,]|%[a-fA-F\d]{2})*(?:;(?:[\-_.!~*'()a-zA-Z\d:@&=+$,]|%[a-fA-F\d]{2})*)*(?:/(?:[\-_.!~*'()a-zA-Z\d:@&=+$,]|%[a-fA-F\d]{2})*(?:;(?:[\-_.!~*'()a-zA-Z\d:@&=+$,]|%[a-fA-F\d]{2})*)*)*)?              
           )(?:\?((?:[\-_.!~*'()a-zA-Z\d;/?:@&=+$,\[\]]|%[a-fA-F\d]{2})*))?           
        )
        (?:\#((?:[\-_.!~*'()a-zA-Z\d;/?:@&=+$,\[\]]|%[a-fA-F\d]{2})*))?            
      $";
            
            bool hasGAnchor;
            string t = RegexpTransformer.Transform(p, RubyRegexOptions.Extended | RubyRegexOptions.Multiline, out hasGAnchor);
            Assert(e == t);
            Assert(!hasGAnchor);
            new Regex(t);
        }

        public void RegexEscape1() {
            string[] patterns = new string[] {
                @"", 
                @"",

                @"\", 
                @"\\",

                @"(*)", 
                @"\(\*\)",

                "$_^_|_[_]_(_)_\\_._#_-_{_}_*_+_?_\t_\n_\r_\f_\v_\a_\b",
                @"\$_\^_\|_\[_\]_\(_\)_\\_\._\#_\-_\{_\}_\*_\+_\?_\t_\n_\r_\f_" + "\v_\a_\b"
            };

            for (int i = 0; i < patterns.Length; i += 2) {
                string expected = patterns[i + 1];
                string actual = RubyRegex.Escape(patterns[i]);
                Assert(actual == expected);
            }
        }

        public void RegexCondition1() {
            AssertOutput(delegate() {
                CompilerTest(@"
$_ = 'foo'.taint
if /(foo)/ then
  puts $1
  puts $1.tainted?
end
");
            }, @"
foo
true
");
        }

        public void RegexCondition2() {
            AssertOutput(delegate() {
                CompilerTest(@"
z = /foo/
puts(z =~ 'xxxfoo')

class Regexp
  def =~ a
    '=~'    
  end
end

z = /foo/
puts(z =~ 'foo')

puts(/foo/ =~ 'xxxfoo')
");
            }, @"
3
=~
3
");
        }
        
#if OBSOLETE //? 
        [Options(NoRuntime = true)]
        public void RegexEncoding1() {
            MatchData m;
            // the k-coding of the pattern string is irrelevant:
            foreach (var pe in new[] {  RubyEncoding.Binary }) {
                var p = MutableString.CreateBinary(new byte[] { 0x82, 0xa0, (byte)'{', (byte)'2', (byte)'}' }, pe);

                var r = new RubyRegex(p, RubyRegexOptions.NONE);
                var rs = new RubyRegex(p, RubyRegexOptions.SJIS);

                // the k-coding of the string is irrelevant:
                foreach (var se in new[] { RubyEncoding.Binary }) {
                    var s = MutableString.CreateBinary(new byte[] { 0x82, 0xa0,  0xa0 }, se);
                    var t = MutableString.CreateBinary(new byte[] { 0x82, 0xa0,  0xa0,  0x82, 0xa0,  0xa0, 0xff }, se);
                    var u = MutableString.CreateBinary(new byte[] { 0x82, 0xa0,  0x82, 0xa0,  0x82, 0xa0 }, se);

                    // /あ{2}/ does not match "あ\xa0"
                    m = r.Match(RubyEncoding.KCodeSJIS, s);
                    Assert(m == null);

                    // /\x82\xa0{2}/ matches "[ \x82\xa0\xa0 ] \x82\xa0\xa0\xff"
                    m = r.Match(null, s);
                    Assert(m != null && m.Index == 0);

                    // /\x82\xa0{2}/ matches "\x82\xa0\xa0 [ \x82\xa0\xa0 ] \xff" starting from byte #1:
                    m = r.Match(null, t, 1, false);
                    Assert(m != null && m.Index == 3 && m.Length == 3);

                    // /あ{2}/s does not match "あ\xa0", current KCODE is ignored
                    m = rs.Match(null, s);
                    Assert(m == null);

                    // /あ{2}/s does not match "あ\xa0", current KCODE is ignored
                    m = rs.Match(RubyEncoding.KCodeUTF8, s);
                    Assert(m == null);

                    // /あ{2}/s matches "ああ\xff", current KCODE is ignored
                    m = rs.Match(RubyEncoding.KCodeUTF8, u, 2, false);
                    Assert(m != null && m.Index == 2 && m.Length == 4);


                    // /あ{2}/ does not match "あ\xa0あ\xa0"
                    m = r.LastMatch(RubyEncoding.KCodeSJIS, t);
                    Assert(m == null);

                    // /\x82\xa0{2}/ matches "\x82\xa0\xa0 [ \x82\xa0\xa0 ] \xff"
                    m = r.LastMatch(null, t);
                    Assert(m != null && m.Index == 3);

                    // /あ{2}/s does not match "あ\xa0あ\xa0", current KCODE is ignored
                    m = rs.LastMatch(null, t);
                    Assert(m == null);

                    // /あ{2}/s does not match "あ\xa0あ\xa0", current KCODE is ignored
                    m = rs.LastMatch(RubyEncoding.KCodeUTF8, t);
                    Assert(m == null);
                }
            }
        }

        [Options(NoRuntime = true)]
        public void RegexEncoding2() {
            var SJIS = RubyEncoding.KCodeSJIS.StrictEncoding;

            // 1.9 encodings:
            var invalidUtf8 = MutableString.CreateBinary(new byte[] { 0x80 }, RubyEncoding.UTF8);
            AssertExceptionThrown<ArgumentException>(() => new RubyRegex(invalidUtf8, RubyRegexOptions.NONE));

            // LastMatch

            MatchData m;
            var u = MutableString.CreateBinary(SJIS.GetBytes("あああ"), RubyEncoding.KCodeSJIS);
            var p = MutableString.CreateBinary(SJIS.GetBytes("あ{2}"), RubyEncoding.KCodeSJIS);

            var rs = new RubyRegex(p, RubyRegexOptions.SJIS);

            // /あ{2}/ matches "あああ", the resulting index is in bytes:
            m = rs.LastMatch(null, u);
            Assert(m != null && m.Index == 2);

            rs = new RubyRegex(MutableString.CreateBinary(SJIS.GetBytes("あ")), RubyRegexOptions.SJIS);

            // "start at" in the middle of a character:
            m = rs.LastMatch(null, u, 0);
            Assert(m != null && m.Index == 0);

            m = rs.LastMatch(null, u, 1);
            Assert(m != null && m.Index == 0);

            m = rs.LastMatch(null, u, 2);
            Assert(m != null && m.Index == 2);

            m = rs.LastMatch(null, u, 3);
            Assert(m != null && m.Index == 2);

            // Split
            
            u = MutableString.CreateBinary(SJIS.GetBytes("あちあちあ"), RubyEncoding.UTF8);
            rs = new RubyRegex(MutableString.CreateBinary(SJIS.GetBytes("ち")), RubyRegexOptions.SJIS);
            var parts = rs.Split(null, u);
            Assert(parts.Length == 3);
            foreach (var part in parts) {
                Assert(part.Encoding == RubyEncoding.KCodeSJIS);
                Assert(part.ToString() == "あ");
            }

            // groups

            rs = new RubyRegex(MutableString.CreateBinary(SJIS.GetBytes("ち(a(あ+)(b+))+あ")), RubyRegexOptions.SJIS);
            u = MutableString.CreateBinary(SJIS.GetBytes("ちaああbaあbbbあ"));

            m = rs.Match(null, u);
            Assert(m.GroupCount == 4);

            int s, l;
            Assert(m.GetGroupStart(0) == (s = 0));
            Assert(m.GetGroupLength(0) == (l = u.GetByteCount()));
            Assert(m.GetGroupEnd(0) == s + l);

            // the group has 2 captures, the last one is its value:
            Assert(m.GetGroupStart(1) == (s = SJIS.GetByteCount("ちaああb")));
            Assert(m.GetGroupLength(1) == (l = SJIS.GetByteCount("aあbbb")));
            Assert(m.GetGroupEnd(1) == s + l);

            // the group has 2 captures, the last one is its value:
            Assert(m.GetGroupStart(2) == (s = SJIS.GetByteCount("ちaああba")));
            Assert(m.GetGroupLength(2) == (l = SJIS.GetByteCount("あ")));
            Assert(m.GetGroupEnd(2) == s + l);

            // the group has 2 captures, the last one is its value:
            Assert(m.GetGroupStart(3) == (s = SJIS.GetByteCount("ちaああbaあ")));
            Assert(m.GetGroupLength(3) == (l = SJIS.GetByteCount("bbb")));
            Assert(m.GetGroupEnd(3) == s + l);
        }
#endif
    }
}


