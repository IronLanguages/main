require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiable'
require File.dirname(__FILE__) + '/../shared/uninstantiable'

describe "Generic .NET classes" do
  it_behaves_like :uninstantiable_generic_class, GenericClass
end

describe "Generic .NET classes with type param" do
  it_behaves_like :instantiable_class, GenericClass[Object]

  describe "of interface" do
    it_behaves_like :instantiable_class, GenericClass[IInterface]
  end
end

describe "Empty generic .NET classes" do
  it_behaves_like :uninstantiable_generic_class, EmptyGenericClass
end

describe "Empty generic .NET classes with type param" do
  it_behaves_like :instantiable_class, EmptyGenericClass[Object]

  describe "of interface" do
    it_behaves_like :instantiable_class, EmptyGenericClass[IInterface]
  end
end

describe "Generic .NET classes with 2 params" do
  it_behaves_like :uninstantiable_generic_class, Generic2Class
end

describe "Generic .NET classes with 2 params with type param" do
  it_behaves_like :instantiable_class, Generic2Class[Object, Fixnum]
end

describe "Empty generic .NET classes with 2 params" do
  it_behaves_like :uninstantiable_generic_class, EmptyGeneric2Class
end

describe "Empty generic .NET classes with 2 params with type param" do
  it_behaves_like :instantiable_class, EmptyGeneric2Class[Object,Fixnum]
end
