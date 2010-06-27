require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Module#to_s" do
  it "returns the to_s of self" do
    ModuleSpecs.to_s.should == "ModuleSpecs"
    ModuleSpecs::Child.to_s.should == "ModuleSpecs::Child"
    ModuleSpecs::Parent.to_s.should == "ModuleSpecs::Parent"
    ModuleSpecs::Basic.to_s.should == "ModuleSpecs::Basic"
    ModuleSpecs::Super.to_s.should == "ModuleSpecs::Super"
    
    begin
      (ModuleSpecs::X = Module.new).to_s.should == "ModuleSpecs::X"
    ensure
      ModuleSpecs.send :remove_const, :X
    end
  end
end
