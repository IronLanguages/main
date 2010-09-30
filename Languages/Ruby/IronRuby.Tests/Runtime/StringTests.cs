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
using IronRuby.Builtins;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using IronRuby.Runtime;
using Microsoft.Scripting;
using System.Runtime.CompilerServices;

namespace IronRuby.Tests {
    public partial class Tests {
        public void StringsPlus() {
            AssertExceptionThrown<InvalidOperationException>(delegate() {
                CompilerTest(@"
puts 'foo' + nil
");
            });
        }

        public void Strings0() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts ""foo""
puts ""foo"" 'bar' ""baz""
");
            }, @"
foo
foobarbaz
");
        }

        public void Strings1() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts ""foo#{""bar""}baz""
");
            }, @"
foobarbaz
");
        }

        public void Strings2() {
            TestOutput(@"
puts ""foo#{1;2;3}baz""
", @"
foo3baz
");
        }

        public void Strings3() {
            TestOutput(@"
class String; def to_s; 'S'; end; end
class Fixnum; def to_s; 'N'; end; end

p """"
puts ""#{1}""
puts ""#{1}-""
puts ""-#{1}""
puts ""-#{1}-""
puts ""#{1}#{1}""
puts ""#{1}+#{1}""
puts ""-#{1}+#{1}""
puts ""-#{1}+#{1}-""

puts ""-#{x = 'bob'}-""
", @"
""""
N
N-
-N
-N-
NN
N+N
-N+N
-N+N-
-bob-
");
        }

        public void Strings4() {
            AssertOutput(delegate() {
                CompilerTest(@"
p ""#{nil}""
p ""#{nil}-""
p ""-#{nil}""
p ""-#{nil}-""
p ""#{nil}#{nil}""
p ""-#{nil}+#{nil}-""
");
            }, @"
""""
""-""
""-""
""--""
""""
""-+-""
");
        }

        public void Strings5() {
            AssertOutput(delegate() {
                CompilerTest(@"
class String; def to_s; 'S'; end; end
class Fixnum; def to_s; 'N'; end; end

puts :""#{1}""
puts :""#{1}-""
puts :""-#{1}""
puts :""-#{1}-""
puts :""#{1}#{1}""
puts :""#{1}+#{1}""
puts :""-#{1}+#{1}""
puts :""-#{1}+#{1}-""

puts ""-#{x = 'bob'}-""
");
            }, @"
N
N-
-N
-N-
NN
N+N
-N+N
-N+N-
-bob-
");
        }

        public void Strings6() {
            TestOutput(@"
p :""#{}""
p :""#{}#{}""
p :""#{}#{''}#{}""

p :""#{nil}a""
p :""a#{nil}""
p :""a#{nil}b""
p :""a#{nil}b#{nil}c""
", @"
:""""
:""""
:""""
:a
:a
:ab
:abc
");
        }

        public void Strings7() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts 'foobarbaz'[3,3]
");
            }, "bar");
        }

        public void Strings8() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts 'foo  bar'.split
");
            }, @"
foo
bar
");
        }

        public void Strings9() {
            // TODO:
#if TODO
            var source = Engine.CreateScriptSource(new BinaryContentProvider(BinaryEncoding.Instance.GetBytes(@"""\u03a3""")), null, BinaryEncoding.Instance);
            AssertExceptionThrown<SyntaxErrorException>(() => source.Execute<MutableString>());
#endif
            // TODO: mixing incompatible encodings at compile time (literals "foo" "bar") or runtime "foo" + "bar"
        }

        /// <summary>
        /// Embedded string does call "to_s" w/o calling "respond_to?" first.
        /// </summary>
        public void ToSConversion1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class X
  def respond_to? name
    puts name
  end
  
  def to_s
    'TO_S'
  end

  puts ""#{new}""
end
");
            }, @"
TO_S
");
        }

        public void ToSConversion2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class NilClass
  def to_s; 'NULL1'; end
  puts ""#{nil}""

  def to_s; 'NULL2'; end
  puts ""#{nil}""
end

class SubString < String
  def to_s; 'XXX'; end
  puts ""#{new 'SUB'}""
end
");
            }, @"
NULL1
NULL2
SUB
");
        }

        public void Symbols1() {
            byte[] bytes = Encoding.UTF8.GetBytes("α");

            RubySymbol a, b;
#if OBSOLTE
            a = Context.CreateSymbolInternal(MutableString.CreateBinary(bytes, RubyEncoding.Binary));
            b = Context.CreateSymbolInternal(MutableString.CreateBinary(bytes, RubyEncoding.KCodeSJIS));
            c = Context.CreateSymbolInternal(MutableString.CreateBinary(bytes, RubyEncoding.KCodeUTF8));
            d = Context.CreateSymbolInternal(MutableString.Create("α", RubyEncoding.KCodeUTF8));

            Assert(a.Equals(b));
            Assert(a.Equals(c));
            Assert(a.Equals(d));
#endif
            a = Context.CreateSymbol(MutableString.CreateBinary(Encoding.ASCII.GetBytes("foo"), RubyEncoding.Binary), false);
            b = Context.CreateSymbol(MutableString.CreateMutable("foo", RubyEncoding.UTF8), false);
            Assert(a.Equals(b));
        }
        
#if OBSOLETE
        [Options(Compatibility = RubyCompatibility.Ruby186)]
        private void Inspect1() {
            const char sq = '\'';

            var sjisEncoding = RubyEncoding.KCodeSJIS.StrictEncoding;
            // あ
            var sjisWide = new byte[] { 0x82, 0xa0 };
            // \u{12345} in UTF-8:
            var utf8 = new byte[] { 0xF0, 0x92, 0x8D, 0x85 };
            // surrogates: U+d808 U+df45 
            var utf16 = Encoding.UTF8.GetString(utf8);

            string s;

            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(utf8, RubyEncoding.Binary), Context, false, sq).ToString();
            Assert(s == @"'\360\222\215\205'");

            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(utf8, RubyEncoding.Binary), Context, true, sq).ToString();
            Assert(s == @"'\360\222\215\205'");

            Context.KCode = RubyEncoding.KCodeUTF8;
            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(utf8, RubyEncoding.KCodeUTF8), Context, false, sq).ToString();
            Assert(s == "'" + utf16 + "'");

            // incomplete character:
            Context.KCode = RubyEncoding.KCodeUTF8;
            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(new byte[] { 0xF0, 0x92 }, RubyEncoding.KCodeUTF8), Context, false, sq).ToString();
            Assert(s == @"'\360\222'");

            Context.KCode = RubyEncoding.KCodeUTF8;
            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(utf8, RubyEncoding.KCodeSJIS), Context, false, sq).ToString();
            Assert(s == "'" + utf16 + "'");

            Context.KCode = RubyEncoding.KCodeUTF8;
            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(utf8, RubyEncoding.KCodeUTF8), Context, true, sq).ToString();
            Assert(s == @"'\360\222\215\205'");

            Context.KCode = RubyEncoding.KCodeSJIS;
            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(sjisWide, RubyEncoding.KCodeSJIS), Context, false, sq).ToString();
            Assert(s == @"'あ'");

            Context.KCode = RubyEncoding.KCodeSJIS;
            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(sjisWide, RubyEncoding.Binary), Context, false, sq).ToString();
            Assert(s == @"'あ'");
            
            Context.KCode = RubyEncoding.KCodeSJIS;
            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(sjisWide, RubyEncoding.KCodeUTF8), Context, true, sq).ToString();
            Assert(s == @"'\202\240'");
        }

#endif
        [Options(NoRuntime = true)]
        private void Inspect2() {
            const char sq = '\'';

            var sjisEncoding = RubyEncoding.SJIS;
            // あ
            var sjisWide = new byte[] { 0x82, 0xa0 };
            // \u{12345} in UTF-8:
            var utf8 = new byte[] { 0xF0, 0x92, 0x8D, 0x85 };
            // \u{12345} in UTF-16: U+d808 U+df45 
            var utf16 = Encoding.UTF8.GetString(utf8);

            string s;

            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(utf8, RubyEncoding.Binary), false, sq).ToString();
            Assert(s == @"'\xF0\x92\x8D\x85'");

            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(utf8, RubyEncoding.Binary), true, sq).ToString();
            Assert(s == @"'\xF0\x92\x8D\x85'");

            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(utf8, RubyEncoding.UTF8), false, sq).ToString();
            Assert(s == @"'\u{12345}'");

            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(utf8, RubyEncoding.UTF8), true, sq).ToString();
            Assert(s == @"'\u{12345}'");

            // incomplete character:
            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.Create("\ud808\udf45\ud808", RubyEncoding.UTF8), false, sq).ToString();
            Assert(s == @"'\u{12345}\u{d808}'");
            
            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(sjisWide, sjisEncoding), false, sq).ToString();
            Assert(s == @"'\x82\xA0'");

            s = MutableStringOps.GetQuotedStringRepresentation(MutableString.CreateBinary(sjisWide, sjisEncoding), true, sq).ToString();
            Assert(s == @"'\x82\xA0'");
        }
    }
}