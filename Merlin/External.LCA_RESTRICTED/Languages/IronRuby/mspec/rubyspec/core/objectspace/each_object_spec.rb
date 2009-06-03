require File.dirname(__FILE__) + '/../../spec_helper'

describe "ObjectSpace.each_object" do
  it "calls the block once for each living, nonimmediate object in the Ruby process" do
    class ObjectSpaceSpecEachObject; end
    new_obj = ObjectSpaceSpecEachObject.new

    count = ObjectSpace.each_object(ObjectSpaceSpecEachObject) {}
    count.should == 1
    # this is needed to prevent the new_obj from being GC'd too early
    new_obj.should_not == nil
  end

  it "raises NotSupportedException for non-Class classes" do
    lambda { ObjectSpace.each_object(String) {} }.should raise_error(RuntimeError)
  end

  it "works for Module" do
    modules = []
    ObjectSpace.each_object(Module) { |o| modules << o }
    modules.size.should > 90
    modules.each { |m| m.should be_kind_of(Module) }
  end

  it "works for Class" do
    classes = []
    ObjectSpace.each_object(Class) { |o| classes << o }
    classes.size.should > 70
    classes.each { |c| c.should be_kind_of(Class) }
  end

  it "works for singleton Class" do
    moduleClass = class << Module; self; end
    classClass = class << Class; self; end
    ObjectSpace.each_object(moduleClass) {}.should >= 2
  end
end
