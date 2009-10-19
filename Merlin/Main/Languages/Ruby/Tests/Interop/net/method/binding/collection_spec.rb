require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Method parameter binding for collections" do
  
  @keys = [:NoArg, :Int32ArrArg, :ObjectArrArg, :IInterfaceArrArg, :ParamsInt32ArrArg, :ParamsIInterfaceArrArg, :ParamsCStructArrArg, :Int32ArgParamsInt32ArrArg, :IInterfaceArgParamsIInterfaceArrArg, :IListOfIntArg, :IListOfObjArg, :ArrayArg, :IEnumerableOfIntArg, :IEnumerableArg,  :IEnumeratorArg, :ArrayListArg, :IDictionaryOfObjectObjectArg, :IDictionaryOfIntStringArg]
  @matrix = {
    #Int32ArrArg, ObjectArrArg, IInterfaceArrArg, ParamsInt32ArrArg,
    #ParamsIInterfaceArrArg, ParamsCStructArrArg,
    #Int32ArgParamsInt32ArrArg, IInterfaceArgParamsIInterfaceArrArg,
    #IListOfIntArg, IListOfObjArg, ArrayArg, 
    #IEnumerableOfIntArg, IEnumeratorOfIntArg, IEnumerableArg,
    #IEnumeratorArg, ArrayListArg, IDictionaryOfObjectObjectArg,
    #IDictionaryOfIntStringArg
    "monkeypatched" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg"},
    "ArrayInstance" => {:NoArg => AE, :IListOfObjArg => "IListOfObjArg", :IEnumerableArg => "IEnumerableArg",},
    "ArrayInstanceEmpty" => {:NoArg => AE, :IListOfObjArg => "IListOfObjArg", :IEnumerableArg => "IEnumerableArg"},
    "HashInstance" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg", :IDictionaryOfObjectObjectArg => "IDictionaryOfObjectObjectArg"},
    "HashInstanceEmpty" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg", :IDictionaryOfObjectObjectArg => "IDictionaryOfObjectObjectArg"},
    "ImplementsEnumerableInstance" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg"},
    "StringInstance" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg"},
    "StringInstanceEmpty" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg"},
    "System::Array[Fixnum]InstanceEmpty" => {:NoArg => AE, :Int32ArrArg => "Int32ArrArg", :ParamsInt32ArrArg => "ParamsInt32ArrArg", :IListOfIntArg => "IListOfIntArg", :ArrayArg => "ArrayArg", :IEnumerableOfIntArg => "IEnumerableOfIntArg", :IEnumerableArg => "IEnumerableArg"},
    "System::Array[Fixnum]Instance" => {:NoArg => AE, :Int32ArrArg => "Int32ArrArg", :ParamsInt32ArrArg => "ParamsInt32ArrArg", :IListOfIntArg => "IListOfIntArg", :ArrayArg => "ArrayArg", :IEnumerableOfIntArg => "IEnumerableOfIntArg", :IEnumerableArg => "IEnumerableArg"},
    "System::Array[Object]InstanceEmpty" => {:NoArg => AE, :ObjectArrArg => "ObjectArrArg", :IListOfObjArg => "IListOfObjArg", :ArrayArg => "ArrayArg", :IEnumerableArg => "IEnumerableArg"},
    "System::Array[Object]Instance" => {:NoArg => AE, :ObjectArrArg => "ObjectArrArg", :IListOfObjArg => "IListOfObjArg", :ArrayArg => "ArrayArg", :IEnumerableArg => "IEnumerableArg"},
    "System::Array[IInterface]Instance" => {:NoArg => AE, :ObjectArrArg => "ObjectArrArg", :IInterfaceArrArg => "IInterfaceArrArg", :ParamsIInterfaceArrArg => "ParamsIInterfaceArrArg", :IListOfObjArg => "IListOfObjArg", :ArrayArg => "ArrayArg", :IEnumerableArg => "IEnumerableArg"},
    "System::Array[IInterface]InstanceEmpty" => {:NoArg => AE, :ObjectArrArg => "ObjectArrArg", :IInterfaceArrArg => "IInterfaceArrArg", :ParamsIInterfaceArrArg => "ParamsIInterfaceArrArg", :IListOfObjArg => "IListOfObjArg", :ArrayArg => "ArrayArg", :IEnumerableArg => "IEnumerableArg"},
    "System::Array[CStruct]Instance" => {:NoArg => AE, :ObjectArrArg => "ObjectArrArg", :ParamsCStructArrArg => "ParamsCStructArrArg", :IListOfObjArg => "IListOfObjArg", :ArrayArg => "ArrayArg", :IEnumerableArg => "IEnumerableArg"},
    "System::Array[CStruct]InstanceEmpty" => {:NoArg => AE, :ObjectArrArg => "ObjectArrArg", :ParamsCStructArrArg => "ParamsCStructArrArg", :IListOfObjArg => "IListOfObjArg", :ArrayArg => "ArrayArg", :IEnumerableArg => "IEnumerableArg"},
    "ArrayListInstance" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg", :ArrayListArg => "ArrayListArg"},
    "ArrayListInstanceEmpty" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg", :ArrayListArg => "ArrayListArg"},
    "Dictionary[Object,Object]InstanceEmpty" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg", :IDictionaryOfObjectObjectArg => "IDictionaryOfObjectObjectArg"},
    "Dictionary[Object,Object]Instance" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg", :IDictionaryOfObjectObjectArg => "IDictionaryOfObjectObjectArg"},
    "Dictionary[Fixnum,String]InstanceEmpty" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg", :IDictionaryOfIntStringArg => "IDictionaryOfIntStringArg"},
    "TestListInstanceEmpty" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg"},
    "TestListInstance" => {:NoArg => AE, :IEnumerableArg => "IEnumerableArg"},
    "TestListEnumeratorInstance" => {:NoArg => AE, :IEnumeratorArg => "IEnumeratorArg"},
    "TestListEnumeratorInstanceEmpty" => {:NoArg => AE, :IEnumeratorArg => "IEnumeratorArg"},
    "CStructInstance" => {:NoArg => AE, :ParamsCStructArrArg => "ParamsCStructArrArg"},
    "Int32Instance" => {:NoArg => AE, :ParamsInt32ArrArg => "ParamsInt32ArrArg", :Int32ArgParamsInt32ArrArg => "Int32ArgParamsInt32ArrArg"},
    "IInterfaceInstance" => {:NoArg => AE, :ParamsIInterfaceArrArg => "ParamsIInterfaceArrArg", :IInterfaceArgParamsIInterfaceArrArg => "IInterfaceArgParamsIInterfaceArrArg"},
    "System::Collections::Generic::List[Fixnum]Instance" => {:NoArg => AE, :IListOfIntArg => "IListOfIntArg", :IEnumerableOfIntArg => "IEnumerableOfIntArg", :IEnumerableArg => "IEnumerableArg"},
    "System::Collections::Generic::List[Fixnum]InstanceEmpty" => {:NoArg => AE, :IListOfIntArg => "IListOfIntArg", :IEnumerableOfIntArg => "IEnumerableOfIntArg", :IEnumerableArg => "IEnumerableArg"},
    "System::Collections::Generic::List[Object]InstanceEmpty" => {:NoArg => AE, :IListOfObjArg => "IListOfObjArg", :IEnumerableArg => "IEnumerableArg"},
    "System::Collections::Generic::List[Object]Instance" => {:NoArg => AE, :IListOfObjArg => "IListOfObjArg", :IEnumerableArg => "IEnumerableArg"},
    "NoArg" => {:NoArg => "NoArg", :Int32ArrArg => AE, :ObjectArrArg => AE, :IInterfaceArrArg => AE, :ParamsInt32ArrArg => "ParamsInt32ArrArg", :ParamsIInterfaceArrArg => "ParamsIInterfaceArrArg", :ParamsCStructArrArg => "ParamsCStructArrArg", :Int32ArgParamsInt32ArrArg => AE, :IInterfaceArgParamsIInterfaceArrArg => AE, :IListOfIntArg => AE, :IListOfObjArg => AE, :ArrayArg => AE, :IEnumerableOfIntArg => AE, :IEnumerableArg => AE, :IEnumeratorArg => AE, :ArrayListArg => AE, :IDictionaryOfObjectObjectArg => AE, :IDictionaryOfIntStringArg => AE, :OutInt32Arg => "OutInt32Arg"},
    "nil" => {:NoArg => AE, :Int32ArrArg => "Int32ArrArg", :ObjectArrArg => "ObjectArrArg", :IInterfaceArrArg => "IInterfaceArrArg", :ParamsInt32ArrArg => "ParamsInt32ArrArg", :ParamsIInterfaceArrArg => "ParamsIInterfaceArrArg", :ParamsCStructArrArg => "ParamsCStructArrArg", :IInterfaceArgParamsIInterfaceArrArg => "IInterfaceArgParamsIInterfaceArrArg", :IListOfIntArg => "IListOfIntArg", :IListOfObjArg => "IListOfObjArg", :ArrayArg => "ArrayArg", :IEnumerableOfIntArg => "IEnumerableOfIntArg", :IEnumerableArg => "IEnumerableArg", :IEnumeratorArg => "IEnumeratorArg", :ArrayListArg => "ArrayListArg", :IDictionaryOfObjectObjectArg => "IDictionaryOfObjectObjectArg", :IDictionaryOfIntStringArg => "IDictionaryOfIntStringArg"},
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
