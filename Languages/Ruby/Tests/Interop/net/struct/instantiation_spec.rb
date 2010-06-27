require File.dirname(__FILE__) + "/../spec_helper"
require File.dirname(__FILE__) + '/../class/shared/instantiable'

describe "Instantiating Structs" do
  it_behaves_like :instantiable_class, EmptyStruct
end
