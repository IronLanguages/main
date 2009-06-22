require File.dirname(__FILE__) + '/../../spec_helper'

describe "Deriving from generic classes" do
  it "doesn't work on open generics" do
    lambda { Class.new(GenericClass)}.should raise_error(TypeError)
    lambda { Class.new { include IInterfaceGroup}}.should raise_error(TypeError)
  end
end
