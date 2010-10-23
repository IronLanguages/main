require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Kernel#inspect" do
  it "displays class name" do
    KernelSpecs::IVars.new.inspect.include?("KernelSpecs::IVars").should == true
  end

  it "displays instance variables when called on an instance of a user class or Object" do
    KernelSpecs::IVars.new.inspect.include?("@secret=99").should == true
    
    obj = Object.new
    obj.instance_variable_set(:@foo, 1)
    obj.inspect.include?("@foo=1").should == true
  end

  it "does not display instance variables when called on an instance of a builtin class" do
    [1, 1.2, true, nil, 1..2, "", 1**200, binding, Proc.new {}, Class.new, Module.new, Time.new, 
     Struct.new(:f), $stdin, [], {}].each do |obj|
     
      obj.instance_variable_set(:@bar, 1)
      begin
        obj.inspect.include?("@secret=99").should == false
      rescue
        obj.instance_variable_remove(:@bar)      
      end
    end
  end

  it "needs to be reviewed for spec completeness"
end
