require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/uninstantiable'

describe "Empty Abstract classes" do
  it_behaves_like :uninstantiable_class, EmptyAbstractClass
end

describe "Abstract classes" do
  it_behaves_like :uninstantiable_class, AbstractClass
end
