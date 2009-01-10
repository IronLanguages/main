# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

require File.dirname(__FILE__) + '/../../Util/simple_test.rb'
require 'strscan'

class RandomClass
  def to_str
    "bob"
  end
end

describe "StringScanner#new" do
  it "can be constructed" do
    str = 'test string'
    s = StringScanner.new(str)
    s.class.should == StringScanner
    s.eos?.should == false
    s.string.should == str
    s.string.frozen? == false
  end
  
  it "can be constructed from a non-string" do
    s = StringScanner.new(RandomClass.new)
    s.peek(3).should == 'bob'
  end
  
  skip "(test bug? Ruby 1.8.6 also fails this test) ignores the legacy parameter to the constructor" do
    a = "123"
    s = StringScanner.new(a, true)
    s.string.object_id.should == a.object_id
    s = StringScanner.new(a, false)
    s.string.object_id.should == a.object_id
  end
  
  it "implements a legacy class function" do
    StringScanner.must_C_version.should == StringScanner
  end
end

describe "StringScanner#class" do
  it "can be constructed" do
    str = 'test string'
    s = StringScanner.new(str)
    s.class.should == StringScanner
    s.eos?.should == false
    s.string.should == str
  end
  
  it "implements a legacy class function" do
    StringScanner.must_C_version.should == StringScanner
  end
end

describe "StringScanner#concat" do
  it "appends to the internal buffer" do
    s = StringScanner.new('abcde')
    s.string.should == 'abcde'
    s.concat('fgh').string.should == 'abcdefgh'
    s << 'i'
    s.string.should == 'abcdefghi'
    s.bol?.should == true
  end
  
  it "works when at the end of the string" do
    s = StringScanner.new('')
    s.eos?.should == true
    s << '123'
    s.eos?.should == false
    s.pos.should == 0
  end
end

# TODO: treat bytes and chars differently
describe "StringScanner#get_byte" do
  it "gets the next character" do
    s = StringScanner.new('abcde')
    s.get_byte.should == 'a'
    s.get_byte.should == 'b'
    s.get_byte.should == 'c'
  end
  
  it "returns nil with no chars left" do
    s = StringScanner.new('')
    s.get_byte.should == nil
  end
  
  it "updates matched information" do
    s = StringScanner.new('abcde')
    s.get_byte.should == 'a'
    s.get_byte.should == 'b'
    s.matched?.should == true
    s.matched.should == 'b'
    s.pre_match.should == 'a'
    s.post_match.should == 'cde'
    s[0].should == 'b'
    s.pos.should == 2
    s.matched_size.should == 1
  end
end

describe "StringScanner#getch" do
  it "gets the next character" do
    s = StringScanner.new('abcde')
    s.getch.should == 'a'
    s.getch.should == 'b'
    s.getch.should == 'c'
  end
  
  it "returns nil with no chars left" do
    s = StringScanner.new('')
    s.getch.should == nil
  end
  
  it "updates matched information" do
    s = StringScanner.new('abcde')
    s.getch.should == 'a'
    s.getch.should == 'b'
    s.matched?.should == true
    s.matched.should == 'b'
    s.pre_match.should == 'a'
    s.post_match.should == 'cde'
    s[0].should == 'b'
    s.pos.should == 2
    s.matched_size.should == 1
  end
end

describe "StringScanner#check" do
  it "matches at the current position without advancing" do
    s = StringScanner.new('Fri Dec 12 1975 14:39')
    s.check(/Fri/).should == 'Fri'
    s.pos.should == 0
    s.matched.should == 'Fri'
    s[0].should == 'Fri'
  end
  
  it "returns nil when match fails" do
    s = StringScanner.new('Fri Dec 12 1975 14:39')
    s.check(/12/).should == nil
    s.matched.should == nil
  end
end

describe "StringScanner#check_until" do
  it "matches further in the string without advancing" do
    s = StringScanner.new('Fri Dec 12 1975 14:39')
    s.check_until(/12/).should == 'Fri Dec 12'
    s.pos.should == 0
    s.matched.should == '12'
    s[0].should == '12'
  end
  
  it "returns nil when match fails" do
    s = StringScanner.new('Fri Dec 12 1975 14:39')
    s.check_until(/abc/).should == nil
    s.pos.should == 0
    s.matched.should == nil
  end
end

describe "StringScanner#skip_until" do
  it "matches further in the string and advances position" do
    s = StringScanner.new('Fri Dec 12 1975 14:39')
    s.skip_until(/12/).should == 10
    s.pos.should == 10
    s.matched.should == '12'
    s[0].should == '12'
  end
  
  it "returns nil when match fails" do
    s = StringScanner.new('Fri Dec 12 1975 14:39')
    s.check(/abc/).should == nil
    s.pos.should == 0
    s.matched.should == nil
  end
end

describe "StringScanner#exist?" do
  it "matches further in the string without advancing and returns potentially resulting position" do
    s = StringScanner.new('test string')
    s.exist?(/s/).should == 3
    s.pos.should == 0
    s.matched.should == 's'
  end
  
  it "returns nil when match fails" do
    s = StringScanner.new('test string')
    s.exist?(/abc/).should == nil
    s.matched.should == nil
  end
end

describe "StringScanner#peek" do
  it "returns the next n characters without advancing" do
    s = StringScanner.new('test string')
    s.peek(5).should == 'test '
    s.pos.should == 0
  end
  
  it "truncates at the end of the string" do
    s = StringScanner.new('test')
    s.peek(5).should == 'test'
    s.pos.should == 0
  end
  
  it "returns an empty string when appropriate" do
    s = StringScanner.new('a')
    s.getch
    s.peek(5).should == ''
  end
end

describe "StringScanner#match?" do
  it "matches string and returns match length without advancing" do
    s = StringScanner.new('test string')
    s.match?(/\w+/).should == 4
    s.pos.should == 0
    s.matched_size.should == 4
  end
  
  it "returns nil on failure to match" do
    s = StringScanner.new('test string')
    s.match?(/\s+/).should == nil
  end
end

describe "StringScanner#string=" do
  it "resets everything when you set a new string" do
    s = StringScanner.new('test string')
    s.scan(/\w+/).should == 'test'
    s.pos.should == 4
    s.matched?.should == true
    s.string = "123"
    s.pos.should == 0
    s.matched?.should == false
  end
  
  it "clones and then freezes the string when you set a new string" do
    s = StringScanner.new('')
    a = "123"
    s.string = a
    s.string.frozen?.should == true
    s.string.object_id.should_not == a.object_id
  end
end

finished