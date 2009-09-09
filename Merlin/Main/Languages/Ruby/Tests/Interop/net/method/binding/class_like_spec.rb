require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Method parameter binding with Class-like parameters" do
  TE = TypeError
  AE = ArgumentError
  #IInterfaceArg ImplementsIInterfaceArg DerivedFromImplementsIInterfaceArg CStructArg StructImplementsIInterfaceArg AbstractClassArg DerivedFromAbstractArg CustomEnumArg EnumIntArg
  @matrix = {
    "anonymous class" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg" },
    "anonymous classInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg" },
  
    "metaclass" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg" },
    "BindingSpecs::RubyImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "ImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "ImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"},
    
    "DerivedFromImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "DerivedFromImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => "DerivedFromImplementsIInterfaceArg", :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromDerivedFromImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromDerivedFromImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => "DerivedFromImplementsIInterfaceArg", :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromDerivedFromAbstractAndImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromDerivedFromAbstractAndImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => "DerivedFromAbstractArg", :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "DerivedFromAbstract" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "DerivedFromAbstractInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => "DerivedFromAbstractArg", :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromAbstract" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromAbstractInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "AbstractClass" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromDerivedFromAbstract" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromDerivedFromAbstractInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => "DerivedFromAbstractArg", :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "Class" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "ClassInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "Object" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "ObjectInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "CStruct" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "CStructInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => "CStructArg", :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "StructImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "StructImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => "StructImplementsIInterfaceArg", :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "EnumInt" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "EnumIntInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => "CustomEnumArg", :EnumIntArg => "EnumIntArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "CustomEnum" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "CustomEnumInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => "CustomEnumArg", :EnumIntArg => "EnumIntArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "NoArg" => {:IInterfaceArg => AE, :ImplementsIInterfaceArg => AE, :DerivedFromImplementsIInterfaceArg => AE, :CStructArg => AE, :StructImplementsIInterfaceArg => AE, :AbstractClassArg => AE, :DerivedFromAbstractArg => AE, :CustomEnumArg => AE, :EnumIntArg => AE, :ObjectArg => AE, :NoArg => "NoArg", :BooleanArg => AE}, 
  }
  before(:each) do
    @target = ClassWithMethods.new
    @target2 = RubyClassWithMethods.new
    @values = Helper.classlike_args
    nil #extraneous puts statement?
  end
    
  @matrix.each do |input, results|
    Helper.run_matrix(results, input)
  end
end
