require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Method parameter binding with Class-like parameters" do
  #IInterfaceArg ImplementsIInterfaceArg DerivedFromImplementsIInterfaceArg CStructArg StructImplementsIInterfaceArg AbstractClassArg DerivedFromAbstractArg CustomEnumArg EnumIntArg
  @keys = [:IInterfaceArg, :ImplementsIInterfaceArg, :DerivedFromImplementsIInterfaceArg, :CStructArg, :StructImplementsIInterfaceArg, :AbstractClassArg, :DerivedFromAbstractArg, :CustomEnumArg, :EnumIntArg, :ObjectArg, :NoArg, :BooleanArg]
  @matrix = {
    "anonymous class" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg" },
    "anonymous classInstance" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg" },
  
    "metaclass" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg" },
    "BindingSpecs::RubyImplementsIInterface" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "ImplementsIInterface" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "ImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromImplementsIInterface" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"},
    
    "DerivedFromImplementsIInterface" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "DerivedFromImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => "DerivedFromImplementsIInterfaceArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromDerivedFromImplementsIInterface" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromDerivedFromImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => "DerivedFromImplementsIInterfaceArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromDerivedFromAbstractAndImplementsIInterface" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromDerivedFromAbstractAndImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => "DerivedFromAbstractArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "DerivedFromAbstract" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "DerivedFromAbstractInstance" => {:AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => "DerivedFromAbstractArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromAbstract" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromAbstractInstance" => {:AbstractClassArg => "AbstractClassArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "AbstractClass" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromDerivedFromAbstract" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromDerivedFromAbstractInstance" => {:AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => "DerivedFromAbstractArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "Class" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "ClassInstance" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "Object" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "ObjectInstance" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "CStruct" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "CStructInstance" => {:CStructArg => "CStructArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "StructImplementsIInterface" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "StructImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :StructImplementsIInterfaceArg => "StructImplementsIInterfaceArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "EnumInt" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "EnumIntInstance" => {:CustomEnumArg => "CustomEnumArg", :EnumIntArg => "EnumIntArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "CustomEnum" => {:ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "CustomEnumInstance" => {:CustomEnumArg => "CustomEnumArg", :EnumIntArg => "EnumIntArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "NoArg" => {:IInterfaceArg => AE, :ImplementsIInterfaceArg => AE, :DerivedFromImplementsIInterfaceArg => AE, :CStructArg => AE, :StructImplementsIInterfaceArg => AE, :AbstractClassArg => AE, :DerivedFromAbstractArg => AE, :CustomEnumArg => AE, :EnumIntArg => AE, :ObjectArg => AE, :NoArg => "NoArg", :BooleanArg => AE, :OutInt32Arg => "OutInt32Arg"}, 
    
    "nil" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => "DerivedFromImplementsIInterfaceArg", :AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => "DerivedFromAbstractArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
  }
  before(:each) do
    @target = ClassWithMethods.new
    @target2 = RubyClassWithMethods.new
    @values = Helper.args
    nil #extraneous puts statement?
  end
    
  @matrix.each do |input, results|
    Helper.run_matrix(results, input, @keys)
  end
end
