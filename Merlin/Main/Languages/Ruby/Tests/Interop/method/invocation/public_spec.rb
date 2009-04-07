require File.dirname(__FILE__) + '/../../spec_helper'

describe "Invoking a public .NET method" do
  before :each do 
    @obj = ClassWithMethods.new
  end

  it "works directly" do 
    @obj.public_method.should equal_clr_string("public")
  end

  it "works via .send" do
    @obj.send(:public_method).should equal_clr_string("public")
  end

  it "works via .send" do
    @obj.__send__(:public_method).should equal_clr_string("public")
  end

  it "works via .instance_eval" do
    @obj.instance_eval("public_method").should equal_clr_string("public")
  end
end
