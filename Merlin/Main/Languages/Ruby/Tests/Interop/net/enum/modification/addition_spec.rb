require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + "/../../shared/modification"

describe "Adding methods on an Enum" do
  before(:each) do
    @klass = EnumInt
    @obj = EnumInt.A
  end
  it_behaves_like :adding_a_method, EnumInt
  it_behaves_like :adding_class_methods, EnumInt
end