require File.dirname(__FILE__) + '/../../../../spec_helper'

describe "IronRuby::Clr::Name#match" do
  before(:each) do
    @hello = IronRuby::Clr::Name.new "hello"
  end

  it "matches the pattern against self" do
    @hello.match(/(.)\1/)[0].should == 'll'
  end

  it "tries to convert pattern to a string via to_str" do
    obj = mock('.')
    def obj.to_str() "." end
    @hello.match(obj)[0].should == "h"
    
    obj = mock('.')
    def obj.respond_to?(type) true end
    def obj.method_missing(*args) "." end
    @hello.match(obj)[0].should == "h"    
  end
  
  it "raises a TypeError if pattern is not a regexp or a string" do
    lambda { @hello.match(10)   }.should raise_error(TypeError)
    lambda { @hello.match(:ell) }.should raise_error(TypeError)
  end

  it "converts string patterns to regexps without escaping" do
    @hello.match('(.)\1')[0].should == 'll'
  end
  
  it "returns nil if there's no match" do
    @hello.match('xx').should == nil
  end

  it "matches \\G at the start of the string" do
    @hello.match(/\Gh/)[0].should == 'h'
    @hello.match(/\Go/).should == nil
  end

  it "sets $~ to MatchData of match or nil when there is none" do
    @hello.match(/./)
    $~[0].should == 'h'
    Regexp.last_match[0].should == 'h'

    @hello.match(/X/)
    $~.should == nil
    Regexp.last_match.should == nil
  end
end

describe "IronRuby::Clr::Name#=~" do
  before(:each) do
    @rudder = IronRuby::Clr::Name.new "rudder"
    @boat = IronRuby::Clr::Name.new "boat"
    @bean = IronRuby::Clr::Name.new "bean"
    @true = IronRuby::Clr::Name.new "true"
    @some_string = IronRuby::Clr::Name.new "some string"
  end

  it "behaves the same way as index() when given a regexp" do
    (@rudder =~ /udder/).should == "rudder".index(/udder/)
    (@boat =~ /[^fl]oat/).should == "boat".index(/[^fl]oat/)
    (@bean =~ /bag/).should == "bean".index(/bag/)
    (@true =~ /false/).should == "true".index(/false/)
  end

  it "raises a TypeError if a obj is a string" do
    lambda { @some_string =~ "another string" }.should raise_error(TypeError)
    lambda { @some_string =~ @rudder }.should raise_error(TypeError)
  end
  
  it "invokes obj.=~ with self if obj is neither a string nor regexp" do
    str = IronRuby::Clr::Name.new "w00t"
    obj = mock('x')

    obj.should_receive(:=~).with(str).any_number_of_times.and_return(true)
    str.should =~ obj

    obj = mock('y')
    obj.should_receive(:=~).with(str).any_number_of_times.and_return(false)
    str.should_not =~ obj
  end
  
  it "sets $~ to MatchData when there is a match and nil when there's none" do
    @rudder =~ /./
    $~[0].should == 'r'
    
    @rudder =~ /not/
    $~.should == nil
  end
end

