require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Module#instance_method" do
  before :all do
    @parent_um = ModuleSpecs::InstanceMeth.instance_method(:foo)
    @superclass_um = ModuleSpecs::InstanceMethChild.instance_method(:foo)
    @child_um = ModuleSpecs::InstanceMethChild.instance_method(:child_foo)
    @mod_um = ModuleSpecs::InstanceMethChild.instance_method(:module_method)

    # Singletons behave a bit differently
    @singleton_um = ModuleSpecs::InstanceMethodSingletonClass.instance_method(:singleton_method)
    @singleton_superclass_um = ModuleSpecs::InstanceMethodSingletonClass.instance_method(:foo)

    @module_singleton_um = ModuleSpecs::SubModuleSingletonClass.instance_method(:module_singleton_method)
    @module_singleton_superclass_um = ModuleSpecs::SubModuleSingletonClass.instance_method(:name)
    
    @singleton_class_singleton_um = ModuleSpecs::SingletonInstanceMethChild.instance_method(:singleton_subclass_method)
    @singleton_class_singleton_superclass_um = ModuleSpecs::SingletonInstanceMethChild.instance_method(:singleton_class_method)
    
  end

  it "returns an UnboundMethod" do
    @parent_um.class.should == UnboundMethod
    @child_um.class.should == UnboundMethod
    @mod_um.class.should == UnboundMethod
    @singleton_um.class.should == UnboundMethod
    @singleton_superclass_um.class.should == UnboundMethod
  end
  
  it "returns an UnboundMethod corresponding to the given name" do
    @parent_um.bind(ModuleSpecs::InstanceMeth.new).call.should == :foo
    @parent_um.bind(ModuleSpecs::InstanceMethChild.new).call.should == :foo
  end

  it "returns an UnboundMethod corresponding to the given name from a superclass" do
    lambda { @superclass_um.bind(ModuleSpecs::InstanceMeth.new) }.should raise_error(TypeError)
    @superclass_um.bind(ModuleSpecs::InstanceMethChild.new).call.should == :foo
  end

  it "returns an UnboundMethod corresponding to the given name from a child class" do
    lambda { @child_um.bind(ModuleSpecs::InstanceMeth.new) }.should raise_error(TypeError)
    @child_um.bind(ModuleSpecs::InstanceMethChild.new).call.should == :child_foo
  end

  it "returns an UnboundMethod corresponding to the given name from an included Module" do
    @mod_um.bind(ModuleSpecs::InstanceMethChild.new).call.should == :module_method
  end
  
  it "returns an UnboundMethod corresponding to the given name from a singleton class" do
    @singleton_um.bind(ModuleSpecs::InstanceMethodSingleton).call.should == :singleton_method
    @module_singleton_um.bind(ModuleSpecs::SubModuleSingleton).call.should == :module_singleton_method
    @singleton_class_singleton_um.bind(ModuleSpecs::InstanceMethChild).call.should == :singleton_subclass_method
  end

  it "returns an UnboundMethod corresponding to the given name from a singleton's superclass, but constrains it to the superclass, not the singleton class" do
    lambda { @singleton_superclass_um.bind(ModuleSpecs::InstanceMeth.new) }.should raise_error(TypeError)
    @singleton_superclass_um.bind(ModuleSpecs::InstanceMethChild.new).call.should == :foo
    @singleton_superclass_um.bind(ModuleSpecs::InstanceMethodSingleton).call.should == :foo
    
    lambda { @module_singleton_superclass_um.bind(Module.new) }.should raise_error(TypeError)
    @module_singleton_superclass_um.bind(ModuleSpecs::SubModuleInstance).call.should == "ModuleSpecs::SubModuleInstance"
    @module_singleton_superclass_um.bind(ModuleSpecs::SubModuleSingleton).call.should == "ModuleSpecs::SubModuleSingleton"    
  end
      
  it "returns an UnboundMethod corresponding to the given name from a singleton's superclass, but constrains it to the superclass, not the singleton class (for Class singletons)" do
    @singleton_class_singleton_superclass_um.bind(ModuleSpecs::InstanceMeth).call.should == :singleton_class_method
    # Is this a Ruby bug? This behavior seems inconsistent with kind_of? since "ModuleSpecs::SingletonInstanceMethChild.kind_of? ModuleSpecs::SingletonInstanceMeth" returns true
    lambda { @singleton_class_singleton_superclass_um.bind(ModuleSpecs::InstanceMethChild) }.should raise_error(TypeError)
  end

  it "gives UnboundMethod method name, Module defined in and Module extracted from" do
    @parent_um.inspect.should =~ /\bfoo\b/
    @parent_um.inspect.should =~ /\bModuleSpecs::InstanceMeth\b/
    
    @superclass_um.inspect.should  =~ /\bfoo\b/
    @superclass_um.inspect.should  =~ /\bModuleSpecs::InstanceMeth\b/
    @superclass_um.inspect.should  =~ /\bModuleSpecs::InstanceMethChild\b/
    
    @child_um.inspect.should  =~ /\bchild_foo\b/
    @child_um.inspect.should  =~ /\bModuleSpecs::InstanceMethChild\b/
    
    @mod_um.inspect.should    =~ /\bmodule_method\b/
    @mod_um.inspect.should    =~ /\bModuleSpecs::InstanceMethMod\b/
    @mod_um.inspect.should    =~ /\bModuleSpecs::InstanceMethChild\b/

    @singleton_um.inspect.should    =~ /\bsingleton_method\b/
    @singleton_um.inspect.should    =~ /\bModuleSpecs::InstanceMethChild\b/

    @singleton_superclass_um.inspect.should    =~ /\bfoo\b/
    @singleton_superclass_um.inspect.should    =~ /\bModuleSpecs::InstanceMethChild\b/

    @module_singleton_um.inspect.should    =~ /\bmodule_singleton_method\b/
    @module_singleton_um.inspect.should    =~ /\bModuleSpecs::SubModuleSingleton\b/

    @module_singleton_superclass_um.inspect.should    =~ /\bname\b/
    @module_singleton_superclass_um.inspect.should    =~ /\bModule\b/
    
    @singleton_class_singleton_um.inspect.should =~ /\bsingleton_subclass_method\b/
    @singleton_class_singleton_um.inspect.should =~ /\bModuleSpecs::InstanceMethChild\b/

    @singleton_class_singleton_superclass_um.inspect.should =~ /\bsingleton_class_method\b/
    @singleton_class_singleton_superclass_um.inspect.should =~ /\bModuleSpecs::InstanceMeth\b/
  end

  it "raises a TypeError if the given name is not a string/symbol" do
    lambda { Object.instance_method(nil)       }.should raise_error(TypeError)
    lambda { Object.instance_method(mock('x')) }.should raise_error(TypeError)
  end

  it "raises a NameError if the given method doesn't exist" do
    lambda { Object.instance_method(:missing) }.should raise_error(NameError)
    lambda { Object.instance_method(:missing) }.should_not raise_error(NoMethodError)
  end
end
