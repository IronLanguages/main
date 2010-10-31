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

describe "range creation" do

  it "creates an inclusive range object from integer literals" do
    a = 0..1
    a.class.name.should == "Range"
    a.begin.should == 0
    a.end.should == 1
    a.exclude_end?.should == false
  end

  it "creates an inclusive range object from integer expressions" do
    x = 0
    y = 1
    a = x..y + 1
    a.class.name.should == "Range"
    a.begin.should == 0
    a.end.should == 2
    a.exclude_end?.should == false
  end

  it "creates an exclusive range object from integer literals" do
    a = 0...2
    a.class.name.should == "Range"
    a.begin.should == 0
    a.end.should == 2
    a.exclude_end?.should == true
  end

  it "creates an exclusive range object from integer expressions" do
    x = 0
    y = 1
    a = x...y + 1
    a.class.name.should == "Range"
    a.begin.should == 0
    a.end.should == 2
    a.exclude_end?.should == true
  end
  
  it "explicit creation" do
    should_raise(ArgumentError) { Range.new }
    should_raise(ArgumentError) { Range.new 1,2,true,true }
    Range.new(5, 10).should == (5..10)
    Range.new('a', 'z', false).should == ('a'..'z')
    Range.new('a', 'z', true).should == ('a'...'z')
    Range.new('a', 'z', Object.new).should == ('a'...'z')
  end
  
  it "creation of a derived type" do
    class MyRange < Range; end
    should_raise(ArgumentError) { MyRange.new }
    should_raise(ArgumentError) { MyRange.new 1,2,true,true }
    MyRange.new(5, 10).inspect.should == (5..10).inspect
    MyRange.new('a', 'z', false).inspect.should == ('a'..'z').inspect
    MyRange.new('a', 'z', true).inspect.should == ('a'...'z').inspect
    MyRange.new('a', 'z', Object.new).inspect.should == ('a'...'z').inspect
  end
end

describe "inspecting range objects" do
  it "tests for range equality in inclusive and exclusive ranges" do
    (0..1).should == (0..1)
    (0..1).should == (0..1)
    (0...1).should == (0...1)
    (0..1).should_not == (0...1)
    (0..1).should_not == (0..2)
    (-1..1).should_not == (0..1)
  end
end

finished
