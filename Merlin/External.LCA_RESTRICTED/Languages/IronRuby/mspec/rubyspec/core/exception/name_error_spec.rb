require File.dirname(__FILE__) + '/../../spec_helper'

describe "NameError" do
  it "is a superclass of NoMethodError" do
    NameError.should be_ancestor_of(NoMethodError)
  end
end

describe "NameError.new" do
  it "NameError.new should take optional name argument" do
    NameError.new("msg","name").name.should == "name"
  end  

  it "calls #inspect on self" do
    m = mock("throwing object")
    s = "#<mock inspect result>"
    m.should_receive(:inspect).and_return(s)
    lambda { m.instance_eval { non_existent_method } }.should raise_error(NameError, /#{s}/)
  end
end
