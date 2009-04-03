require File.dirname(__FILE__) + '/../../spec_helper'

describe "Invoking a protected .NET method" do
  before :each do 
    @obj = ClassWithMethods.new
  end

  it "works directly" do 
    @obj.protected_method.to_s.should == "protected"
  end

  it "works via .send" do
    @obj.send(:protected_method).to_s.should == "protected"
  end

  it "works via .send" do
    @obj.__send__(:protected_method).to_s.should == "protected"
  end

  it "works via .instance_eval" do
    @obj.instance_eval("protected_method").to_s.should == "protected"
  end
end

describe "Invoking a protected .NET method on an inherited Ruby class" do
  class RubyClassWithMethods < ClassWithMethods
  end
  before :each do 
    @obj = RubyClassWithMethods.new
  end

  it "works directly" do 
    @obj.protected_method.to_s.should == "protected"
  end

  it "works via .send" do
    @obj.send(:protected_method).to_s.should == "protected"
  end

  it "works via .send" do
    @obj.__send__(:protected_method).to_s.should == "protected"
  end

  it "works via .instance_eval" do
    @obj.instance_eval("protected_method").to_s.should == "protected"
  end
end
