require File.dirname(__FILE__) + '/../spec_helper'
require 'mscorlib'
require 'rowantest.baseclasscs'
require 'rowantest.delegatedefinitions'
require 'rowantest.typesamples'

include Merlin::Testing
include Merlin::Testing::BaseClass
include Merlin::Testing::Delegate

describe ".NET methods" do
  before(:each) do
    @obj = Class41.new
  end

  #other versions of method command
  it "are included in an objects method list" do
    @obj.methods.grep(/m41/).should_not == []
  end
  #csc %{ C# code
  #//...
  #}
  it "can be called" do
    @obj.m41.should == 41
    @obj.send(:m41).should == 41
    @obj.instance_eval("m41").should == 41
  end

  it "are able to be grabbed as an object" do
    m41 = @obj.method(:m41)
    m41.call.should == 41
  end

  it "are able to be aliased by IronRuby" do
    class << @obj
      alias _m41 m41
    end
    @obj._m41.should == 41
    class << @obj
      alias m41 _m41
    end

    @obj.m41.should == 41
  end

  it "are able to be overwritten by IronRuby" do
    class << @obj
      alias _m41 m41
      def m41
        :not_41
      end
    end

    @obj.m41.should == :not_41

    class << @obj
      alias m41 _m41
    end

    @obj.m41.should == 41
  end
end

describe "Abstract .NET methods" do
  before(:each) do
    @meth = Class200a.instance_method(:m200)
  end
  it "should be able to be grabbed as an object" do
    @meth.should be_kind_of UnboundMethod
  end

  it "should not be callable" do
    lambda {@meth.bind(Class200b).call}.should raise_error(TypeError)
  end

  #regression test for Rubyforge 24104
  it "should be able to be grabbed as an object after call to #method" do
    Class200a.method(:m200) rescue nil
    Class200a.instance_method(:m200).should be_kind_of UnboundMethod
  end
end

describe ".NET methods as Ruby objects" do
  before(:each) do
    @meth = Class41.new.method(:m41)
  end

  it "are Ruby Methods" do
    @meth.should be_kind_of Method
  end

  it "contain a Group of CLR Methods" do
    @meth.clr_members[0].should be_kind_of System::Reflection::MemberInfo
  end

  it "can be called" do
    @meth.call.should == 41
    @meth[].should == 41
  end

  it "can be unbound" do
    m = @meth.unbind
    m.should be_kind_of UnboundMethod
    m = m.bind(Class41.new)
    m.call.should == 41
  end

  it "call the correct method" do
    m = Class210a.instance_method(:m210)
    m.bind(Class210b.new).call.should == 210
  end
  it "call the correct method for overrides" do 
    #override methods cannot be rebound
    m = Class210a.instance_method(:m210)
    m.bind(Class210c.new).call.should == 212
  end
end

describe "Overloaded .NET methods" do
  before(:each) do
    @methods = ClassWithTargetMethods.new.method(:m_overload1)
  end

  it "act as a single Ruby method" do
    @methods.should be_kind_of Method
    @methods.call
    Flag[].value.should == 100
  end

  it "contain .NET method objects" do 
    @methods.clr_members.each do |meth|
      meth.should be_kind_of System::Reflection::MemberInfo
    end
  end

  it "perform overload resolution" do
    @methods.call(100)
    Flag[].value.should == 110
    @methods.call(100, 100)
    Flag[].value.should == 120
  end

  it "allow you to pick the overload" do
    @methods.overloads(Fixnum, Fixnum).call(100,100)
    Flag[].value.should == 120
  end
  
  #regression test for RubyForge 24112
  it "correctly report error message" do
    lambda {@methods.overloads(Fixnum).call}.should raise_error(ArgumentError, /0 for 1/)
  end
end
