require File.dirname(__FILE__) + '/../spec_helper'

describe "Basic .NET classes" do
  it "map to Ruby classes" do
    [EmptyClass, Klass, 
      AbstractClass, EmptyAbstractClass, 
      StaticClass, EmptyStaticClass,
      SealedClass, EmptySealedClass,      
    ].each do |klass|
        klass.should be_kind_of Class
      end
  end

  it "map to Ruby modules if they represent generic definitions" do
    [ GenericClass, EmptyGenericClass,
      Generic2Class, EmptyGeneric2Class
    ].each do |klass|
        klass.should be_kind_of Module
      end
  end
end

