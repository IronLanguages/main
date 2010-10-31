# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

require "../../Util/simple_test.rb"

describe "String behavior in string expressions" do
  it "calls to_s on the result of an expression" do
    class Bob701
      def to_s
        "bob"
      end
    end
    "#{Bob701.new}".should == "bob"
  end
end

describe "String#<=>(other_string)" do
  skip "compares individual characters based on their ascii value" do
    ascii_order = Array.new(256) { |x| x.chr }
    sort_order = ascii_order.sort
    sort_order.should == ascii_order
  end
  
  it "returns -1 when self is less than other" do
    ("this" <=> "those").should == -1
  end

  it "returns 0 when self is equal to other" do
    ("yep" <=> "yep").should == 0
  end

  it "returns 1 when self is greater than other" do
    ("yoddle" <=> "griddle").should == 1
  end
  
  it "considers string that comes lexicographically first to be less if strings have same size" do
    ("aba" <=> "abc").should == -1
    ("abc" <=> "aba").should == 1
  end

  it "doesn't consider shorter string to be less if longer string starts with shorter one" do
    ("abc" <=> "abcd").should == -1
    ("abcd" <=> "abc").should == 1
  end

  it "compares shorter string with corresponding number of first chars of longer string" do
    ("abx" <=> "abcd").should == 1
    ("abcd" <=> "abx").should == -1
  end
  
  skip "ignores subclass differences" do
    a = "hello"
    b = MyString.new("hello")
    
    (a <=> b).should == 0
    (b <=> a).should == 0
  end
end

describe "String#<=>(obj)" do
  it "returns nil if its argument does not respond to to_str" do
    ("abc" <=> 1).should == nil
    ("abc" <=> :abc).should == nil
    ("abc" <=> Object.new).should == nil
  end
  
  # I believe that this behavior is a bug in Ruby
  skip "returns nil if its argument does not respond to <=>" do
    obj = Object.new
    def obj.to_str() "" end
    
    ("abc" <=> obj).should == nil
  end
  
  # Similarly, I believe that this behavior is a bug in Ruby -- the semantics of
  # to_str should convert obj into a String
  skip "compares its argument and self by calling <=> on obj and turning the result around" do
    obj = Object.new
    def obj.to_str() "" end
    def obj.<=>(arg) 1  end
    
    ("abc" <=> obj).should == -1
    ("xyz" <=> obj).should == -1
  end
end

describe "String#[idx]" do
  it "returns the character code of the character at fixnum" do
    "hello"[0].should == ?h
    "hello"[-1].should == ?o
  end
  
  it "returns nil if idx is outside of self" do
    "hello"[20].should == nil
    "hello"[-20].should == nil
    
    ""[0].should == nil
    ""[-1].should == nil
  end
  
  it "calls to_int on idx" do
    "hello"[0.5].should == ?h
    
    # Too fancy for me right now
    #obj = Object.new
    #obj.should_receive(:to_int, :returning => 1)
    
    class Bob1
      def to_int
        1
      end
    end
    "hello"[Bob1.new].should == ?e
  end
end

describe "String#[idx, length]" do
  it "returns the substring starting at idx and the given length" do
    "hello there"[0,0].should == ""
    "hello there"[0,1].should == "h"
    "hello there"[0,3].should == "hel"
    "hello there"[0,6].should == "hello "
    "hello there"[0,9].should == "hello the"
    "hello there"[0,12].should == "hello there"

    "hello there"[1,0].should == ""
    "hello there"[1,1].should == "e"
    "hello there"[1,3].should == "ell"
    "hello there"[1,6].should == "ello t"
    "hello there"[1,9].should == "ello ther"
    "hello there"[1,12].should == "ello there"

    "hello there"[3,0].should == ""
    "hello there"[3,1].should == "l"
    "hello there"[3,3].should == "lo "
    "hello there"[3,6].should == "lo the"
    "hello there"[3,9].should == "lo there"

    "hello there"[4,0].should == ""
    "hello there"[4,3].should == "o t"
    "hello there"[4,6].should == "o ther"
    "hello there"[4,9].should == "o there"
    
    "foo"[2,1].should == "o"
    "foo"[3,0].should == ""
    "foo"[3,1].should == ""

    ""[0,0].should == ""
    ""[0,1].should == ""

    "x"[0,0].should == ""
    "x"[0,1].should == "x"
    "x"[1,0].should == ""
    "x"[1,1].should == ""

    "x"[-1,0].should == ""
    "x"[-1,1].should == "x"

    "hello there"[-3,2].should == "er"
  end
  
  it "returns nil if the offset falls outside of self" do
    "hello there"[20,3].should == nil
    "hello there"[-20,3].should == nil

    ""[1,0].should == nil
    ""[1,1].should == nil
    
    ""[-1,0].should == nil
    ""[-1,1].should == nil
    
    "x"[2,0].should == nil
    "x"[2,1].should == nil

    "x"[-2,0].should == nil
    "x"[-2,1].should == nil
  end
  
  it "returns nil if the length is negative" do
    "hello there"[4,-3].should == nil
    "hello there"[-4,-3].should == nil
  end
  
  it "converts non-integer numbers to integers" do
    "hello"[0.5, 1].should == "h"
    "hello"[0.5, 2.5].should == "he"
    "hello"[1, 2.5].should == "el"
  end

  it "calls to_int on idx and length" do
    # Too fancy for me right now
    #obj = Object.new
    #obj.should_receive(:to_int, :count => 4, :returning => 2)

    class Bob2
      def to_int
        2
      end
    end

    obj = Bob2.new

    "hello"[obj, 1].should == "l"
    "hello"[obj, obj].should == "ll"
    "hello"[0, obj].should == "he"
  end
  
  skip "returns subclass instances" do
    s = MyString.new("hello")
    s[0,0].class.should == MyString
    s[0,4].class.should == MyString
    s[1,4].class.should == MyString
  end
end

describe "String#[range]" do
  it "returns the substring given by the offsets of the range" do
    "hello there"[1..1].should == "e"
    "hello there"[1..3].should == "ell"
    "hello there"[1...3].should == "el"
    "hello there"[-4..-2].should == "her"
    "hello there"[-4...-2].should == "he"
    "hello there"[5..-1].should == " there"
    "hello there"[5...-1].should == " ther"
    
    ""[0..0].should == ""

    "x"[0..0].should == "x"
    "x"[0..1].should == "x"
    "x"[0...1].should == "x"
    "x"[0..-1].should == "x"
    
    "x"[1..1].should == ""
    "x"[1..-1].should == ""
  end
  
  it "returns nil if the beginning of the range falls outside of self" do
    "hello there"[12..-1].should == nil
    "hello there"[20..25].should == nil
    "hello there"[20..1].should == nil
    "hello there"[-20..1].should == nil
    "hello there"[-20..-1].should == nil

    ""[-1..-1].should == nil
    ""[-1...-1].should == nil
    ""[-1..0].should == nil
    ""[-1...0].should == nil
  end
  
  it "returns an empty string if the number of characters returned from the range (end - begin + ?1) is < 0" do
    "hello there"[1...1].should == ""
    "hello there"[4..2].should == ""
    "hello"[4..-4].should == ""
    "hello there"[-5..-6].should == ""
    "hello there"[-2..-4].should == ""
    "hello there"[-5..-6].should == ""
    "hello there"[-5..2].should == ""

    ""[0...0].should == ""
    ""[0..-1].should == ""
    ""[0...-1].should == ""
    
    "x"[0...0].should == ""
    "x"[0...-1].should == ""
    "x"[1...1].should == ""
    "x"[1...-1].should == ""
  end
  
  skip "returns subclass instances" do
    s = MyString.new("hello")
    s[0...0].class.should == MyString
    s[0..4].class.should == MyString
    s[1..4].class.should == MyString
  end
  
  skip "calls to_int on range arguments" do
    from = Object.new
    to = Object.new

    # So we can construct a range out of them...
    def from.<=>(o) 0 end
    def to.<=>(o) 0 end

    def from.to_int() 1 end
    def to.to_int() -2 end
      
    "hello there"[from..to].should == "ello ther"
    "hello there"[from...to].should == "ello the"
  end
end

# There's some serious bug here -- these tests pass just fine if run in
# isolation but when run at this point in this test case will raise an error:
# Cannot convert ZZZ to Integer -- as if we are calling to_int on the type
# rather than treating it as a regex - some kind of bug with dynamic sites?

skip "String#[regexp]" do
  it "returns the matching portion of self" do
    "hello there"[/[aeiou](.)\1/].should == "ell"
    ""[//].should == ""
  end
  
  it "returns nil if there is no match" do
    "hello there"[/xyz/].should == nil
  end
  
  skip "returns subclass instances" do
    s = MyString.new("hello")
    s[//].class.should == MyString
    s[/../].class.should == MyString
  end
end

skip "String#[regexp, idx]" do
  it "returns the capture for idx" do
    "hello there"[/[aeiou](.)/,0].should == "el"
    "hello there"[/[aeiou](.)/,1].should == "o "
    "hello there"[/[aeiou](.)/,2].should == "er"

    # TODO: .NET regex semantics do not work with any but the first of the
    # expressions below! I need to rewrite this test case to focus on 
    # this method and not regular expressions themselves.
    #"hello there"[/[aeiou](.)\1/, 0].should == "ell"
    #"hello there"[/[aeiou](.)\1/, 1].should == "l"
    #"hello there"[/[aeiou](.)\1/, -1].should == "l"

    #"har"[/(.)(.)(.)/, 0].should == "har"
    #"har"[/(.)(.)(.)/, 1].should == "h"
    #"har"[/(.)(.)(.)/, 2].should == "a"
    #"har"[/(.)(.)(.)/, 3].should == "r"
    #"har"[/(.)(.)(.)/, -1].should == "r"
    #"har"[/(.)(.)(.)/, -2].should == "a"
    #"har"[/(.)(.)(.)/, -3].should == "h"
  end
  
  skip "returns nil if there is no match" do
    "hello there"[/(what?)/, 1].should == nil
  end
  
  skip "returns nil if there is no capture for idx" do
    "hello there"[/[aeiou](.)\1/, 2].should == nil
    # You can't refer to 0 using negative indices
    "hello there"[/[aeiou](.)\1/, -2].should == nil
  end
  
  skip "calls to_int on idx" do
    obj = Object.new
    obj.should_receive(:to_int, :returning => 2)
      
    "har"[/(.)(.)(.)/, 1.5].should == "h"
    "har"[/(.)(.)(.)/, obj].should == "a"
  end
  
  skip "returns subclass instances" do
    s = MyString.new("hello")
    s[/(.)(.)/, 0].class.should == MyString
    s[/(.)(.)/, 1].class.should == MyString
  end
end

skip "String#[other_string]" do
  it "returns the string if it occurs in self" do
    s = "lo"
    "hello there"[s].should == s
  end
  
  it "returns nil if there is no match" do
    "hello there"["bye"].should == nil
  end
  
  skip "doesn't call to_str on its argument" do
    o = Object.new
    o.should_not_receive(:to_str)
      
    should_raise(TypeError) { "hello"[o] }
  end
  
  skip "returns a subclass instance when given a subclass instance" do
    s = MyString.new("el")
    r = "hello"[s]
    r.should == "el"
    r.class.should == MyString
  end
end

describe "String#[idx] = char" do
  it "sets the code of the character at idx to char modulo 256" do
    a = "hello"
    a[0] = ?b
    a.should == "bello"
    a[-1] = ?a
    a.should == "bella"
    a[-1] = 0
    a.should == "bell\x00"
    a[-5] = 0
    a.should == "\x00ell\x00"
    
    a = "x"
    a[0] = ?y
    a.should == "y"
    a[-1] = ?z
    a.should == "z"
    
    a[0] = 255
    a[0].should == 255
    a[0] = 256
    a[0].should == 0
    a[0] = 256 * 3 + 42
    a[0].should == 42
    a[0] = -214
    a[0].should == 42
  end
 
  it "raises an IndexError without changing self if idx is outside of self" do
    a = "hello"
    
    should_raise(IndexError) { a[20] = ?a }
    a.should == "hello"
    
    should_raise(IndexError) { a[-20] = ?a }
    a.should == "hello"
    
    should_raise(IndexError) { ""[0] = ?a }
    should_raise(IndexError) { ""[-1] = ?a }
  end
end

describe "String#[idx] = other_str" do
  it "replaces the char at idx with other_str" do
    a = "hello"
    a[0] = "bam"
    a.should == "bamello"
    a[-2] = ""
    a.should == "bamelo"
  end

  it "raises an IndexError  without changing self if idx is outside of self" do
    str = "hello"

    should_raise(IndexError) { str[20] = "bam" }    
    str.should == "hello"
    
    should_raise(IndexError) { str[-20] = "bam" }
    str.should == "hello"

    should_raise(IndexError) { ""[0] = "bam" }
    should_raise(IndexError) { ""[-1] = "bam" }
  end

  skip "raises a TypeError when self is frozen" do
    a = "hello"
    a.freeze
    
    should_raise(TypeError) { a[0] = "bam" }
  end
  
  # Broken in MRI 1.8.4
  it "calls to_int on idx" do
    if defined? IRONRUBY_VERSION
      str = "hello"
      str[0.5] = "hi "
      str.should == "hi ello"
    
      class Bob4
        def to_int
          -1
        end
      end
      str[Bob4.new] = "!"
      str.should == "hi ell!"
    end  
  end
  
  # BUGBUG: This raises an error only when run after all the other tests here, but not
  # in isolation. This smells like another variant of the dynamic site caching
  # bug that we see elsewhere in this set of tests
  skip "tries to convert other_str to a String using to_str" do
    class Bob55
      def to_str
        "-test-"
      end
    end
    other_str = Bob55.new
    
    a = "abc"
    a[1] = other_str
    a.should == "a-test-c"
  end
  
  skip "raises a TypeError if other_str can't be converted to a String" do
    should_raise(TypeError) { "test"[1] = :test }
    should_raise(TypeError) { "test"[1] = Object.new }
    should_raise(TypeError) { "test"[1] = nil }
  end
end

# MSFT
describe "String#[idx, chars_to_overwrite] = other_str" do
  it "starts at idx and overwrites chars_to_overwrite characters before inserting the rest of other_str" do
    a = "hello"
    a[0, 2] = "xx"
    a.should == "xxllo"
    a = "hello"
    a[0, 2] = "jello"
    a.should == "jellollo"
  end

  it "counts negative idx values from end of the string" do
    a = "hello"
    a[-1, 0] = "bob"
    a.should == "hellbobo"
    a = "hello"
    a[-5, 0] = "bob"
    a.should == "bobhello"
  end

  it "overwrites and deletes characters if chars_to_overwrite is less than the length of other_str" do
    a = "hello"
    a[0, 4] = "x"
    a.should == "xo"
    a = "hello"
    a[0, 5] = "x"
    a.should == "x"
  end

  it "deletes characters if other_str is an empty string" do
    a = "hello"
    a[0, 2] = ""
    a.should == "llo"
  end

  it "deletes characters up to the maximum length of the existing string" do
    a = "hello"
    a[0, 6] = "x"
    a.should == "x"
    a = "hello"
    a[0, 100] = ""
    a.should == ""
  end

  it "appends other_str to the end of the string if idx == the length of the string" do
    a = "hello"
    a[5, 0] = "bob"
    a.should == "hellobob"
  end

  it "ignores the length parameter if idx == the length of the string and just appends other_str to the end of the string" do
    a = "hello"
    a[5, 1] = "bob"
    a.should == "hellobob"
  end

  it "throws an IndexError if |idx| is greater than the length of the string" do
    should_raise(IndexError) { "hello"[6, 0] = "bob" }
    should_raise(IndexError) { "hello"[-6, 0] = "bob" }
  end

  it "throws an IndexError if chars_to_overwrite < 0" do
    should_raise(IndexError) { "hello"[0, -1] = "bob" }
    should_raise(IndexError) { "hello"[1, -1] = "bob" }
  end

  it "throws a TypeError if other_str is a type other than String" do
    #should_raise(TypeError) { "hello"[0, 2] = nil } # this doesn't work for
    #some reason????
    should_raise(TypeError) { "hello"[0, 2] = :bob }
    should_raise(TypeError) { "hello"[0, 2] = 33 }
  end
end

# MSFT
describe "String#[range] = other_str" do
  it "overwrites characters defined by range with other_str when range size and other_str size are equal" do
    a = "hello"
    a[0..2] = "abc"
    a.should == "abclo"
    a = "hello"
    a[0..4] = "world"
    a.should == "world"
  end
  
  it "overwrites the first character defined by a single character range if other_str is a single character" do
    a = "hello"
    a[0..0] = "j"
    a.should == "jello"
    a = "hello"
    a[4..4] = "p"
    a.should == "hellp"
  end
end

describe "String#*count" do
  it "returns a new string containing count copies of self" do
    ("cool" * 0).should == ""
    ("cool" * 1).should == "cool"
    ("cool" * 3).should == "coolcoolcool"
  end

  skip "tries to convert the given argument to an integer using to_int" do
    ("cool" * 3.1).should == "coolcoolcool"
    ("a" * 3.999).should == "aaa"
  end
end

describe "String#capitalize" do
  it "returns a copy of self with the first character converted to uppercase and the remainder to lowercase" do
    "hello".capitalize.should == "Hello"
    "HELLO".capitalize.should == "Hello"
    "123ABC".capitalize.should == "123abc"
  end

  it "is locale insensitive (only upcases a-z and only downcases A-Z)" do
    "Ã„Ã–Ãœ".capitalize.should == "Ã„Ã–Ãœ"
    "Ã¤rger".capitalize.should == "Ã¤rger"
    "BÃ„R".capitalize.should == "BÃ„r"
  end
end

describe "String#capitalize!" do
  it "capitalizes self in place" do
    a = "hello"
    a.capitalize!.should == "Hello"
    a.should == "Hello"
  end
  
  it "returns nil when no changes are made" do
    a = "Hello"
    a.capitalize!.should == nil
    a.should == "Hello"
    
    "".capitalize!.should == nil 
  end

  skip "raises a TypeError when self is frozen" do
    ["", "Hello", "hello"].each do |a|
      a.freeze
      should_raise(TypeError) { a.capitalize! }
    end
  end
end

describe "String#center" do
  it "returns self when the centering length is less than self's length" do
    "12345".center(4).should == "12345"
  end

  it "returns self centered and padded with spaces when no padding string is provided" do
    "12345".center(16).should == "     12345      "
  end

  it "returns self centered and padded with spaces when no padding string is provided" do
    "12345".center(16, "abc").should == "abcab12345abcabc"
  end
  
  it "raises an ArgumentError when the padding string is empty" do
    should_raise(ArgumentError) { "12345".center(16, "") }
  end
end

describe "String#chomp" do
  it "returns string without record separator, if present" do
    "12345".chomp("45").should == "123"
    "12345".chomp("34").should == "12345"
    "".chomp("34").should == ""
  end

  it "for default record separator, returns string without terminating CR, LF and CRLF" do
    "12345\n".chomp.should == "12345"
    "12345\r\n".chomp.should == "12345"
    "12345\r".chomp.should == "12345"
    "12345".chomp.should == "12345"
    "".chomp.should == ""
  end
end

describe "String#chomp!" do
  it "removes record separator in-place from the end of the string if present" do
    a = "12345"
    a.chomp!("345").should == "12"
    a.should == "12"
  end

  it "for default record separator, removes CR, LF and CRLF from the end of the string" do
    a = "12345\r\n"
    a.chomp!.should == "12345"
    a.should == "12345"
  end

  it "returns nil if no modifications were made" do
    a = "12345"
    a.chomp!.should == nil
    a.should == "12345"
  end
end

describe "String#chop" do
  it "returns string without last character; last two for CRLF" do
    "12345".chop.should == "1234"
    "12345\n".chop.should == "12345"
    "12345\r".chop.should == "12345"
    "12345\r\n".chop.should == "12345"
    "".chop.should == ""
  end
end

describe "String#chop!" do
  it "removes last character in-place from the end of the string if" do
    a = "12345\r\n"
    a.chop!.should == "12345"
    a.should == "12345"
  end

  it "returns nil if no modifications were made" do
    a = ""
    a.chop!.should == nil
    a.should == ""
  end
end

describe "String#downcase" do
  it "returns a copy of self with all uppercase letters downcased" do
    "hELLO".downcase.should == "hello"
    "hello".downcase.should == "hello"
  end
  
  it "is locale insensitive (only replacing A-Z)" do
    "Ã„Ã–Ãœ".downcase.should == "Ã„Ã–Ãœ"
  end
end

describe "String#downcase!" do
  it "modifies self in place" do
    a = "HeLlO"
    a.downcase!.should == "hello"
    a.should == "hello"
  end
  
  it "returns nil if no modifications were made" do
    a = "hello"
    a.downcase!.should == nil
    a.should == "hello"
  end

  skip "raises a TypeError when self is frozen" do
    should_raise(TypeError) do
      a = "HeLlO"
      a.freeze
      a.downcase!
    end
  end
end

describe "String#dump" do
  skip "produces a version of self with all nonprinting charaters replaced by \\nnn notation" do
    ("\000".."A").to_a.to_s.dump.should == "\"\\000\\001\\002\\003\\004\\005\\006\\a\\b\\t\\n\\v\\f\\r\\016\\017\\020\\021\\022\\023\\024\\025\\026\\027\\030\\031\\032\\e\\034\\035\\036\\037 !\\\"\\\#$%&'()*+,-./0123456789\""
  end
  
  skip "ignores the $KCODE setting" do
    old_kcode = $KCODE

    begin
      $KCODE = "NONE"
      "Ã¤Ã¶Ã¼".dump.should == "\"\\303\\244\\303\\266\\303\\274\""

      $KCODE = "UTF-8"
      "Ã¤Ã¶Ã¼".dump.should == "\"\\303\\244\\303\\266\\303\\274\""
    ensure
      $KCODE = old_kcode
    end
  end
end

describe "String#empty?" do
  it "returns true if the string has a length of zero" do
    "hello".empty?.should == false
    " ".empty?.should == false
    "".empty?.should == true
  end
end

describe "String#include?(other)" do
  it "returns true if self contains other" do
    "hello".include?("lo").should == true
    "hello".include?("ol").should == false
  end
  
  it "tries to convert other to string using to_str" do
    class Bob3
      def to_str
        "lo"
      end
    end
    
    "hello".include?(Bob3.new).should == true
  end
  
  it "raises a TypeError if other can't be converted to string" do
    should_raise(TypeError) do
      "hello".include?(:lo)
    end
    
    should_raise(TypeError) do
      "hello".include?(Object.new)
    end
  end
end

describe "String#include?(fixnum)" do
  it "returns true if self contains the given char" do
    "hello".include?(?h).should == true
    "hello".include?(?z).should == false
  end
end

# TODO: inadequate tests ... must write proper set for the next two describes
describe "String#index(fixnum [, offset])" do
  it "returns the index of the first occurence of the given character" do
    "hello".index(?e).should == 1
  end
  
  it "starts the search at the given offset" do
    "hello".index(?o, -2).should == 4
  end
  
  it "returns nil if no occurence is found" do
    "hello".index(?z).should == nil
    "hello".index(?e, -2).should == nil
  end
end

describe "String#index(substring [, offset])" do
  it "returns the index of the first occurence of the given substring" do
    "hello".index('e').should == 1
    "hello".index('lo').should == 3
  end
  
  it "starts the search at the given offset" do
    "hello".index('o', -3).should == 4
  end
  
  it "returns nil if no occurence is found" do
    "hello".index('z').should == nil
    "hello".index('e', -2).should == nil
  end
  
  it "raises a TypeError if no string was given" do
    should_raise(TypeError) do
      "hello".index(:sym)
    end
    
    #should_raise(TypeError) do
    #  "hello".index(Object.new)
    #end
    #"a   b c  ".split(/\s+/).should == ["a", "b", "c"]
  end
end

describe "String#insert(index, other)" do
  it "inserts other before the character at the given index" do
    "abcd".insert(0, 'X').should == "Xabcd"
    "abcd".insert(3, 'X').should == "abcXd"
    "abcd".insert(4, 'X').should == "abcdX"
  end
  
  it "modifies self in place" do
    a = "abcd"
    a.insert(4, 'X').should == "abcdX"
    a.should == "abcdX"
  end
  
  it "inserts after the given character on an negative count" do
    "abcd".insert(-3, 'X').should == "abXcd"
    "abcd".insert(-1, 'X').should == "abcdX"
  end
  
  it "raises an IndexError if the index is out of string" do
    should_raise(IndexError) { "abcd".insert(5, 'X') }
    should_raise(IndexError) { "abcd".insert(-6, 'X') }
  end
  
  it "converts other to a string using to_str" do
    class Bob6
      def to_str
        "XYZ"
      end
    end
    
    "abcd".insert(-3, Bob6.new).should == "abXYZcd"
  end
  
  it "raises a TypeError if other can't be converted to string" do
    should_raise(TypeError) do
      "abcd".insert(-6, :sym)
    end
    
    should_raise(TypeError) do
      "abcd".insert(-6, 12)
    end
    
    should_raise(TypeError) do
      "abcd".insert(-6, Object.new)
    end
  end
  
  skip "raises a TypeError if self is frozen" do
    should_raise(TypeError) do
      a = "abcd"
      a.freeze
      a.insert(4, 'X')
    end
  end
end

describe "String#scan" do
  it "scans for fixed strings when passed a string" do
    a = "hello world"
    a.scan("l").should == ["l", "l", "l"]
    a.scan("ld").should == ["ld"]
    a.scan("he").should == ["he"]
    a.scan("abc").should == []
    
    s = ""
    result = a.scan("l") { |c| s += c }
    result.should == a
    s.should == "lll"
  end

  it "returns n+1 empty strings for a string of n chars when passed a blank string to match" do
    a = "abc"
    a.scan("").should == ["", "", "", ""]
    
    s = ""
    result = a.scan("") { |c| s += c }
    result.should == a
    s.should == ""
  end

  it "returns matches for a regex when there's no grouping" do
    a = "hello world"
    a.scan(/\w+/).should == ["hello", "world"]
    a.scan(/.../).should == ["hel", "lo ", "wor"]
    
    s = ""
    result = a.scan(/\w+/) { |c| s = s + "|" + c }
    result.should == a
    s.should == "|hello|world"
  end

  it "returns group information for a regex with groups" do
    a = "hello world"
    a.scan(/(...)/).should == [["hel"], ["lo "], ["wor"]]
    a.scan(/(..)(..)/).should == [["he", "ll"], ["o ", "wo"]]
    
    s = ""
    result = a.scan(/(..)(..)/) { |b, c| s = s + c + "." + b }
    result.should == a
    s.should == "ll.hewo.o "
  end
end

describe "String#succ" do
  it "increments alphanumeric characters" do
    "a".succ.should == "b"
    "0".succ.should == "1"
    "A".succ.should == "B"
  end

  it "performs carry operations on alphanumeric characters" do
    "az".succ.should == "ba"
    "09".succ.should == "10"
    "AZ".succ.should == "BA"
    "Az".succ.should == "Ba"
    "a9".succ.should == "b0"
    "aZ".succ.should == "bA"
  end

  it "inserts a new character if it runs out of alphanumeric characters to apply a carry to" do
    "z".succ.should == "aa"
    "9".succ.should == "10"
    "Z".succ.should == "AA"
  end

  it "skips over non alphanumeric characters when looking for next character to apply a carry to" do
    "a!!!z".succ.should == "b!!!a"
    "a!!!9".succ.should == "b!!!0"
  end

  it "increments the byte-based value of the char by one for non alphanumeric characters" do
    "?".succ.should == "@"
    "~".succ.should == "\177"   # 127
    "\000\377".succ.should == "\001\000"
    "\000\377\377".succ.should == "\001\000\000"
  end

  it "inserts a new character if it runs out of non alphanumeric characters to apply a carry to" do
    "\377".succ.should == "\001\000"
    "\377\377".succ.should == "\001\000\000"
  end
end

describe "String#succ!" do
  it "behaves identically to String#succ except for the fact that it does things in-place" do
    a = "a"
    a.succ!.should == "b"
    a.should == "b"
    a = "ZZZ9999"
    a.succ!.should == "AAAA0000"
    a.should == "AAAA0000"
  end
end

describe "String#swapcase" do
  it "returns a copy of self with all lowercase letters upcased and all uppercase letters downcased" do
    "hELLO".swapcase.should == "Hello"
    "hello".swapcase.should == "HELLO"
    "HELLO".swapcase.should == "hello"
  end
  
  it "is locale insensitive (only upcases a-z and only downcases A-Z)" do
    "ã„âåÃäœ".swapcase.should == "ã„âåÃäœ"
  end
end

describe "String#swapcase!" do
  it "modifies self in place" do
    a = "HeLlO"
    a.swapcase!.should == "hElLo"
    a.should == "hElLo"
  end
  
  it "returns nil if no modifications were made" do
    a = "12345"
    a.swapcase!.should == nil
    a.should == "12345"
  end

  skip "raises a TypeError when self is frozen" do
    should_raise(TypeError) do
      a = "HeLlO"
      a.freeze
      a.swapcase!
    end
  end
end

describe "String#upcase" do
  it "returns a copy of self with all lowercase letters upcased" do
    "hELLO".upcase.should == "HELLO"
    "hello".upcase.should == "HELLO"
    "HELLO".upcase.should == "HELLO"
  end
  
  it "is locale insensitive (only replacing a-z)" do
    "ã„âå-äœ".upcase.should == "ã„âå-äœ"
  end
end

describe "String#upcase!" do
  it "modifies self in place" do
    a = "HeLlO"
    a.upcase!.should == "HELLO"
    a.should == "HELLO"
  end
  
  it "returns nil if no modifications were made" do
    a = "HELLO"
    a.upcase!.should == nil
    a.should == "HELLO"
  end

  skip "raises a TypeError when self is frozen" do
    should_raise(TypeError) do
      a = "HeLlO"
      a.freeze
      a.upcase!
    end
  end
end

describe "String#upto" do
  it "enumerates the range between the strings" do
    s = ""
    n = 0
    result = "b8".upto("c2") { |c|
      s += c
      n += 1
    }
    result.should == "b8"
    s.should == "b8b9c0c1c2"
    n.should == 5
  end
end

# TODO: haven't implemented regex parse tree transformation yet
#describe "String#index(regexp [, offset])" do
#  skip "returns the index of the first match with the given regexp" do
#    "hello".index(/[aeiou]/).should == 1
#  end
  
#  skip "starts the search at the given offset" do
#    "hello".index(/[aeiou]/, -3).should == 4
#  end
  
#  skip "returns nil if no occurence is found" do
#    "hello".index(/z/).should == nil
#    "hello".index(/e/, -2).should == nil
#  end
#end

describe "string creation" do
  it "creates a string using language syntax" do
    a = "test"
    a.length.should == 4
    a.should == "test"
  end

  it "creates a string using String.new" do
    b = String.new("test")
    b.length.should == 4
    b.should == "test"
  end
  
  it "String.new doesn't accept nil" do
    should_raise(TypeError) { String.new(nil) }   
  end

  it "String.new doesn't accept nil" do
    obj = Object.new
    class << obj
      def to_s
        "to_s"
      end
      
      def to_str
        "to_str"
      end
    end
    String.new(obj).should == "to_str"
  end

  it "(*) creates a repeated string" do
    a = "bob" * 3
    a.length.should == 9
    a.should == "bobbobbob"

    b = "bob" * 0
    b.length.should == 0
    b.should == ""

    should_raise(ArgumentError) { c = "bob" * -3 }

    d = "bob" * 3.333
    d.length.should == 9
    d.should == "bobbobbob"
  end

  it "(+) creates a concatenated string" do
    a = "hello"
    b = "world"
    (a + b).should == "helloworld"
  end

  it "(<<) appends strings" do
    a = "hello"
    oid = a.object_id
    a << "world"
    a.object_id.should == oid
    a.should == "helloworld"
  end
end

describe "String#each, String#each_line" do
  # Test case data
  testcases = [
    "",
    "\n",
    "\n\n",
    "\n\n\n",
    "abc",
    "abc\n",
    "\nabc",
    "\nabc\n",
    "abc\n\n",
    "\n\nabc",
    "\n\nabc\n\n",
    "abc\ndef",
    "abc\ndef\n",
    "\nabc\ndef",
    "\nabc\ndef\n",
    "abc\ndef\n\n",
    "\n\nabc\ndef",
    "\n\nabc\ndef\n\n",
    "abc\n\ndef",
    "abc\n\ndef\n",
    "\nabc\n\ndef",
    "\nabc\n\ndef\n",
    "abc\n\ndef\n\n",
    "\n\nabc\n\ndef",
    "\n\nabc\n\ndef\n\n",
  ]
      
  it "test normal mode" do
    expected = [
      [],
      ["\n"],
      ["\n", "\n"],
      ["\n", "\n", "\n"],
      ["abc"],
      ["abc\n"],
      ["\n", "abc"],
      ["\n", "abc\n"],
      ["abc\n", "\n"],
      ["\n", "\n", "abc"],
      ["\n", "\n", "abc\n", "\n"],
      ["abc\n", "def"],
      ["abc\n", "def\n"],
      ["\n", "abc\n", "def"],
      ["\n", "abc\n", "def\n"],
      ["abc\n", "def\n", "\n"],
      ["\n", "\n", "abc\n", "def"],
      ["\n", "\n", "abc\n", "def\n", "\n"],
      ["abc\n", "\n", "def"],
      ["abc\n", "\n", "def\n"],
      ["\n", "abc\n", "\n", "def"],
      ["\n", "abc\n", "\n", "def\n"],
      ["abc\n", "\n", "def\n", "\n"],
      ["\n", "\n", "abc\n", "\n", "def"],
      ["\n", "\n", "abc\n", "\n", "def\n", "\n"],      
    ]
    
    testcases.length.times do |i|
      a = []
      testcases[i].each { |e| a << e }
      a.should == expected[i]
      
      a = []
      testcases[i].each_line { |e| a << e }
      a.should == expected[i]
    end
  end
  
  it "test paragraph mode" do
    expected = [
      [],
      ["\n"],
      ["\n\n"],
      ["\n\n\n"],
      ["abc"],
      ["abc\n"],
      ["\nabc"],
      ["\nabc\n"],
      ["abc\n\n"],
      ["\n\n", "abc"],
      ["\n\n", "abc\n\n"],
      ["abc\ndef"],
      ["abc\ndef\n"],
      ["\nabc\ndef"],
      ["\nabc\ndef\n"],
      ["abc\ndef\n\n"],
      ["\n\n", "abc\ndef"],
      ["\n\n", "abc\ndef\n\n"],
      ["abc\n\n", "def"],
      ["abc\n\n", "def\n"],
      ["\nabc\n\n", "def"],
      ["\nabc\n\n", "def\n"],
      ["abc\n\n", "def\n\n"],
      ["\n\n", "abc\n\n", "def"],
      ["\n\n", "abc\n\n", "def\n\n"],
    ]
    
    testcases.length.times do |i|
      a = []
      testcases[i].each('') { |e| a << e }
      a.should == expected[i]

      a = []
      testcases[i].each_line('') { |e| a << e }
      a.should == expected[i]
    end
  end
  
end

describe "String#each_byte" do
  # TODO: we cannot enable this test until method dispatch correctly detects
  # missing block parameters
  skip "raises a LocalJumpError if no block given" do
    should_raise(LocalJumpError) { "bob".each_byte }
  end

  it "passes each byte in self to the given block" do
    a = []
    "hello\x00".each_byte { |c| a << c }
    a.should == [104, 101, 108, 108, 111, 0]
  end

  # globals used since closures are broken
  it "respects the fact that the string can change length during iteration" do
    s = "hello"
    t = ""
    s.each_byte do |c|
      t << c
      if t.length < 3
        s << c
      end
    end
    s.should == "hellohe"
    t.should == "hellohe"
  end
end

describe "String#<<(string)" do
  it "concatenates the given argument to self and returns self" do
    str = 'hello '
    (str << 'world').equal?(str).should == true
    str.should == "hello world"
  end
  
  it "converts the given argument to a String using to_str" do
    class Bob500
      def to_str
        "world!"
      end
    end
    obj = Bob500.new
    
    a = 'hello ' << obj
    a.should == 'hello world!'
  end
  
  it "raises a TypeError if the given argument can't be converted to a String" do
    should_raise(TypeError) do
      a = 'hello ' << :world
    end

    should_raise(TypeError) do
      a = 'hello ' << Object.new
    end
  end

  skip "raises a TypeError when self is frozen" do
    a = "hello"
    a.freeze

    should_raise(TypeError) { a << "" }
    should_raise(TypeError) { a << "test" }
  end
  
  skip "works when given a subclass instance" do
    a = "hello"
    a << MyString.new(" world")
    a.should == "hello world"
  end
end

describe "String#<<(fixnum)" do
  it "converts the given Fixnum to a char before concatenating" do
    b = 'hello ' << 'world' << 33
    b.should == "hello world!"
    b << 0
    b.should == "hello world!\x00"
  end
  
  it "raises a TypeError when the given Fixnum is not between 0 and 255" do
    should_raise(TypeError) do
      "hello world" << 333
    end
  end

  it "doesn't call to_int on its argument" do
    should_raise(TypeError) { "" << Object.new }
  end

  skip "raises a TypeError when self is frozen" do
    a = "hello"
    a.freeze

    should_raise(TypeError) { a << 0 }
    should_raise(TypeError) { a << 33 }
  end
end

finished
