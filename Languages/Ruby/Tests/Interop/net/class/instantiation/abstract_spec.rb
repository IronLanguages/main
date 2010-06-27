require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/uninstantiable'
require File.dirname(__FILE__) + '/../shared/instantiable'
require File.dirname(__FILE__) + '/../fixtures/classes'

describe "Empty Abstract classes" do
  it_behaves_like :uninstantiable_class, EmptyAbstractClass
end

describe "Abstract classes" do
  it_behaves_like :uninstantiable_class, AbstractClass
end

describe "Derived from abstract classes" do
  it_behaves_like :instantiable_class, DerivedFromAbstract
end

describe "Ruby derived from abstract classes" do
  it_behaves_like :instantiable_class, RubyDerivedFromAbstract
end

describe "Abstract derived classes" do
  it_behaves_like :uninstantiable_class, AbstractDerived
end

describe "Ruby derived from abstract class with an event" do
  it_behaves_like :instantiable_class, RubyHasAnEvent
end
