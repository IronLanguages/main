require File.dirname(__FILE__) + '/../spec_helper'

describe "Converting procs to delegates" do
  before :each do
    @klass = DelegateConversionClass.new("lambda {|a| a.to_i}")
  end

  #TODO: does this belong somewhere else?
  it "can directly invoke a lambda" do
    @klass.direct_invoke.should == 1
  end

  it "can convert to a lambda" do
    @klass.convert_to_delegate.should == 1
  end
end
