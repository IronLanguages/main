require File.dirname(__FILE__) + '/../../spec_helper'

describe "ObjectSpace.each_object" do
  not_supported_on :ironruby do
    it "calls the block once for each living, non-immediate object in the Ruby process" do
      class ObjectSpaceSpecEachObject; end
      new_obj = ObjectSpaceSpecEachObject.new

      yields = 0
      count = ObjectSpace.each_object(ObjectSpaceSpecEachObject) do |obj|
        obj.should == new_obj
        yields += 1
      end
      count.should == 1
      yields.should == 1

      # this is needed to prevent the new_obj from being GC'd too early
      new_obj.should_not == nil
    end
  end

  deviates_on :ironruby do
    it "raises NotSupportedException for non-Class classes" do
      lambda { ObjectSpace.each_object(String) {} }.should raise_error(RuntimeError)
    end
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
  ruby_version_is '1.8.7' do
    it "returns an enumerator if not given a block" do
      class ObjectSpaceSpecEachOtherObject; end
      new_obj = ObjectSpaceSpecEachOtherObject.new

      counter = ObjectSpace.each_object(ObjectSpaceSpecEachOtherObject)
      counter.should be_kind_of(enumerator_class)
      counter.each{}.should == 1
      # this is needed to prevent the new_obj from being GC'd too early
      new_obj.should_not == nil
    end
  end
end
