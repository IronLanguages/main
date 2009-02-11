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

                @"\G",
                @"\G",

                @"[a\-z]",
                @"[a\-z]",
            };

            for (int i = 0; i < incorrectPatterns.Length; i += 2) {
                string expected = incorrectPatterns[i + 1];
                string actual = StringRegex.TransformPattern(incorrectPatterns[i], RubyRegexOptions.NONE);
                AreEqual(expected, actual);
            }

            for (int i = 0; i < correctPatterns.Length; i += 2) {
                string expected = correctPatterns[i + 1];
                string actual = StringRegex.TransformPattern(correctPatterns[i], RubyRegexOptions.NONE);
                AreEqual(expected, actual);
                new Regex(expected);
            }
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
            string t = StringRegex.TransformPattern(e, RubyRegexOptions.Extended | RubyRegexOptions.Multiline);
            Assert(e == t);
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
                string actual = StringRegex.Escape(patterns[i]);
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
    }
}


