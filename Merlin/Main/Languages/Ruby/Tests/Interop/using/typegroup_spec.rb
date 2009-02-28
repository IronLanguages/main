require File.dirname(__FILE__) + '/../spec_helper'
require 'mscorlib'
require 'rowantest.typesamples'
require 'rowantest.baseclasscs'
include Merlin::Testing

describe "TypeGroups with non-generic member" do
  #regression for RubyForge 24106
  it "allow static methods to be called on the non-generic member" do
    Flag.Set(100)
    lambda { Flag.Check(100)}.should_not raise_error
  end
  
  #regression for RubyForge 24108
  it "don't cache the class information" do
    c = BaseClass::EmptyTypeGroup2.new
    c2 = BaseClass::EmptyTypeGroup1.new
    c2.should_not be_kind_of BaseClass::EmptyTypeGroup2[]
  end
end
