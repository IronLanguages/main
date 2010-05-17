require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiable'
require File.dirname(__FILE__) + '/../fixtures/classes'

describe "General .NET classes" do
  it_behaves_like :instantiable_class, Klass  
end

describe "Ruby classes derived from .NET classes with overloaded constructors" do
  it_behaves_like :instantiable_class, OverloadedConstructorClass

  it "properly selects overloaded constructors" do
    OverloadedConstructorClass.new("hello").should be_kind_of(OverloadedConstructorClass)
    OverloadedConstructorClass.new("hello").val.should equal_clr_string("string constructor")
  end
  
  it "properly selects overloaded constructors for super" do
    RubyOverloadedConstructorClass.new("hello").should be_kind_of(RubyOverloadedConstructorClass)
    OverloadedConstructorClass.new("hello").val.should equal_clr_string("string constructor")
  end
end
