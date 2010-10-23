require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../fixtures/classes'

describe "Basic class derivation" do
  before(:each) do
    @obj = TestDerived.new
  end
  
  it "works for instantiation" do
    @obj.should be_kind_of(TestDerived)
    @obj.should be_kind_of(Klass)
  end

  it "works for derived methods" do
    @obj.m.should == 1
  end

  it "works for defined methods" do
    @obj.foo.should == 1
  end
end
