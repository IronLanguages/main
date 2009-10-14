require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Methods that don't get mangled" do
  @objs = [NotMangled.new, SubNotMangled.new, Class.new(NotMangled).new, Class.new(SubNotMangled).new, StaticNotMangled, SubStaticNotMangled, Class.new(StaticNotMangled), Class.new(SubStaticNotMangled)]

  Helper.non_mangled_methods.each do |meth_name|
    @objs.each do |obj|
      it "work on #{obj.class} when called with CLR names (#{meth_name})" do
        obj.__send__(meth_name).should == meth_name
      end

      it "don't work on #{obj.class} when called with the mangled name (#{meth_name.to_snake_case})" do
        obj.__send__(meth_name.to_snake_case).should == "base #{meth_name}"
      end
    end
  end
end
