require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiable'

describe "Sealed classes" do
  it_behaves_like :instantiable_class, SealedClass
end

describe "Empty sealed classes" do
  it_behaves_like :instantiable_class, EmptySealedClass
end
