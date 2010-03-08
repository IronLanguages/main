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
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using System.Text;

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
            AssertOutput(delegate() {
                CompilerTest(@"
puts(/#{/a/}/)                     # MRI: (?-mix:a)
puts(/#{nil}#{/a/}#{nil}/)         # MRI: (?-mix:a)
puts(/#{/a/}b/)
puts(/b#{/a/}/)
");
            }, @"
(?-mix:(?-mix:a))
(?-mix:(?-mix:a))
(?-mix:(?-mix:a)b)
(?-mix:b(?-mix:a))
");
        }

        public void RegexTransform1() {
            string[] incorrectPatterns = new string[] {
                @"\", 
                @"\",

                @"\\\", 
                @"\\\",

                @"\x",
                @"\x",

                @"\1", // TODO
                @"\1",
            };

            string[] correctPatterns = new string[] {
                @"", 
                @"",

                @"\\", 
                @"\\",

                @"\_",
                @"_",

                @"abc\0\01\011\a\sabc\Wabc\w", 
                @"abc\0\01\011\a\sabc\Wabc\w",

                @"\xd",
                @"\x0d",

                @"\xdz",
                @"\x0dz",

                @"\*",
                @"\*",

                @"\[",
                @"\[",

                @"\#",
                @"\#",

                @"\0",
                @"\0",

                @"[a\-z]",
                @"[a\-z]",
            };

            string[] correctPatternsWithG = new string[] {
                @"\G",
                @"\G",
            };

            for (int i = 0; i < incorrectPatterns.Length; i += 2) {
                bool hasGAnchor;
                string expected = incorrectPatterns[i + 1];
                string actual = RegexpTransformer.Transform(incorrectPatterns[i], RubyRegexOptions.NONE, out hasGAnchor);
                AreEqual(expected, actual);
            }

            for (int i = 0; i < correctPatterns.Length; i += 2) {
                TestCorrectPatternTranslation(correctPatterns[i], correctPatterns[i + 1], false);
            }

            for (int i = 0; i < correctPatternsWithG.Length; i += 2) {
                TestCorrectPatternTranslation(correctPatternsWithG[i], correctPatternsWithG[i + 1], true);
            }
        }

        private void TestCorrectPatternTranslation(string/*!*/ pattern, string/*!*/ expected, bool expectedGAnchor) {
            bool hasGAnchor;
            string actual = RegexpTransformer.Transform(pattern, RubyRegexOptions.NONE, out hasGAnchor);
            AreEqual(expected, actual);
            new Regex(expected);
            Assert(hasGAnchor == expectedGAnchor);
        }

        public void RegexTransform2() {
            string e = @"^
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
            bool hasGAnchor;
            string t = RegexpTransformer.Transform(e, RubyRegexOptions.Extended | RubyRegexOptions.Multiline, out hasGAnchor);
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
        
        [Options(NoRuntime = true)]
        public void RegexEncoding1() {
            MatchData m;
            // the k-coding of the pattern string is irrelevant:
            foreach (var pe in new[] { RubyEncoding.KCodeSJIS, RubyEncoding.KCodeUTF8, RubyEncoding.Binary }) {
                var p = MutableString.CreateBinary(new byte[] { 0x82, 0xa0, (byte)'{', (byte)'2', (byte)'}' }, pe);

                var r = new RubyRegex(p, RubyRegexOptions.NONE);
                var rs = new RubyRegex(p, RubyRegexOptions.SJIS);

                // the k-coding of the string is irrelevant:
                foreach (var se in new[] { RubyEncoding.KCodeSJIS, RubyEncoding.KCodeUTF8, RubyEncoding.Binary }) {
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
    }
}


