require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Method parameter binding with misc parameters" do
  @keys = [:ClassWithMethodsArg, :GenericArg, :OutNonByRefInt32Arg, :IDictionaryOfIntIntArg, :HashtableArg, :ListOfIntArg, :IEnumerableIteratingArg, :IEnumeratorIteratingArg, :IEnumerableOfCharIteratingArg, :IEnumeratorOfCharIteratingArg, :IEnumerableOfIntIteratingArg, :IEnumeratorOfIntIteratingArg, :DelegateArg, :IntIntDelegateArg, :RefByteArrArg, :RefStructImplementsIInterfaceArg, :OutStructImplementsIInterfaceArg, :RefImplementsIInterfaceArg, :OutImplementsIInterfaceArg, :RefBooleanArg, :OutBooleanArg, :ParamsIInterfaceArrTestArg, :IListOfIntArg2, :IListOfCharArg, :IListArg]
  @matrix = {
   "ClassWithMethods" => {  :ClassWithMethodsArg => "ClassWithMethodsArg",
     :GenericArg => "GenericArg[ClassWithMethods]", 
    :RefBooleanArg => "RefBooleanArg"},
   "int" => { :GenericArg => "GenericArg[System.Int32]", :OutNonByRefInt32Arg => "OutNonByRefInt32Arg",
    :RefBooleanArg => "RefBooleanArg"},
   "nil" => {  :ClassWithMethodsArg => "ClassWithMethodsArg",
     :GenericArg => "GenericArg[System.Object]",
     :IDictionaryOfIntIntArg => "IDictionaryOfIntIntArg",
     :HashtableArg => "HashtableArg", :ListOfIntArg => "ListOfIntArg", 
     :RefByteArrArg => "RefByteArrArg",
     :RefImplementsIInterfaceArg => "RefImplementsIInterfaceArg",
     :RefBooleanArg => "RefBooleanArg"},
   "hash" => { :GenericArg => "GenericArg[IronRuby.Builtins.Hash]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefBooleanArg => "RefBooleanArg"},
   "Dictionary[obj,obj]" => { :GenericArg => "GenericArg[System.Collections.Generic.Dictionary`2[System.Object,System.Object]]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefBooleanArg => "RefBooleanArg"},
   "Dictionary[int,str]" => { :GenericArg => "GenericArg[System.Collections.Generic.Dictionary`2[System.Int32,System.String]]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefBooleanArg => "RefBooleanArg"},
   "Dictionary[int,int]" => { :GenericArg => "GenericArg[System.Collections.Generic.Dictionary`2[System.Int32,System.Int32]]",
     :IDictionaryOfIntIntArg => "IDictionaryOfIntIntArg",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefBooleanArg => "RefBooleanArg"},
   "hashtable" => { :GenericArg => "GenericArg[System.Collections.Hashtable]",
     :HashtableArg => "HashtableArg", :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefBooleanArg => "RefBooleanArg"},
   "List[int]" => { :GenericArg => "GenericArg[System.Collections.Generic.List`1[System.Int32]]",
     :ListOfIntArg => "ListOfIntArg", :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :IEnumerableOfIntIteratingArg => "IEnumerableOfIntIteratingArg",
     :RefBooleanArg => "RefBooleanArg", :IListArg => "IListArg", :IListOfIntArg2 => "IListOfIntArg2"},
   "List[obj]" => { :GenericArg => "GenericArg[System.Collections.Generic.List`1[System.Object]]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefBooleanArg => "RefBooleanArg", :IListArg => "IListArg"},
   "Array[int]" => { :GenericArg => "GenericArg[System.Int32[]]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :IEnumerableOfIntIteratingArg => "IEnumerableOfIntIteratingArg",
     :RefBooleanArg => "RefBooleanArg", :IListArg => "IListArg", :IListOfIntArg2 => "IListOfIntArg2"},
   "String" => { :GenericArg => "GenericArg[IronRuby.Builtins.MutableString]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefByteArrArg => "RefByteArrArg",
     :RefBooleanArg => "RefBooleanArg"},
   "Array" => { :GenericArg => "GenericArg[IronRuby.Builtins.RubyArray]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefBooleanArg => "RefBooleanArg", :IListArg => "IListArg"},
   "Array[Char]" => { :GenericArg => "GenericArg[System.Char[]]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :IEnumerableOfCharIteratingArg => "IEnumerableOfCharIteratingArg",
     :RefBooleanArg => "RefBooleanArg", :IListArg => "IListArg", :IListOfCharArg => "IListOfCharArg"},
   "System::String" => { :GenericArg => "GenericArg[System.String]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :IEnumerableOfCharIteratingArg => "IEnumerableOfCharIteratingArg"},
   "Array[System::String]" => { :GenericArg => "GenericArg[System.String[]]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefBooleanArg => "RefBooleanArg", :IListArg => "IListArg", :IListOfCharArg => "IListOfCharArg"},
   "ArrayList" => { :GenericArg => "GenericArg[System.Collections.ArrayList]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefBooleanArg => "RefBooleanArg", :IListArg => "IListArg"},
   "List[Char]" => { :GenericArg => "GenericArg[System.Collections.Generic.List`1[System.Char]]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :IEnumerableOfCharIteratingArg => "IEnumerableOfCharIteratingArg",
     :RefBooleanArg => "RefBooleanArg", :IListArg => "IListArg", :IListOfCharArg => "IListOfCharArg"},
   "IEnumerator" => { :GenericArg => /GenericArg\[IronRuby\.Classes\.Object\$\d{1,3}\]/,
     :IEnumeratorIteratingArg => "IEnumeratorIteratingArg",
     :RefBooleanArg => "RefBooleanArg"},
   "IEnumerator[int]" => { :GenericArg => /GenericArg\[IronRuby\.Classes\.Object\$\d{1,3}\]/,
     :IEnumeratorIteratingArg => "IEnumeratorIteratingArg",
     :IEnumeratorOfIntIteratingArg => "IEnumeratorOfIntIteratingArg",
     :RefBooleanArg => "RefBooleanArg"},
   "IEnumerator[Char]" => { :GenericArg => /GenericArg\[IronRuby\.Classes\.Object\$\d{1,3}\]/,
     :IEnumeratorIteratingArg => "IEnumeratorIteratingArg",
     :IEnumeratorOfCharIteratingArg => "IEnumeratorOfCharIteratingArg",
     :RefBooleanArg => "RefBooleanArg"},
   "IntIntDelegate" => { :GenericArg => "GenericArg[IntIntDelegate]",
     :DelegateArg => "DelegateArg",
     :IntIntDelegateArg => "IntIntDelegateArg",
     :RefBooleanArg => "RefBooleanArg"},
   "lambda" => { :GenericArg => "GenericArg[IronRuby.Builtins.Proc]",
     :DelegateArg => "DelegateArg",
     :IntIntDelegateArg => "IntIntDelegateArg",
     :RefBooleanArg => "RefBooleanArg"},
   "proc" => { :GenericArg => "GenericArg[IronRuby.Builtins.Proc]",
     :DelegateArg => "DelegateArg",
     :IntIntDelegateArg => "IntIntDelegateArg",
     :RefBooleanArg => "RefBooleanArg"},
   "method" => { :GenericArg => "GenericArg[IronRuby.Builtins.RubyMethod]",
     :DelegateArg => "DelegateArg",
     :IntIntDelegateArg => "IntIntDelegateArg",
     :RefBooleanArg => "RefBooleanArg"},
   "unboundmethod" => { :GenericArg => "GenericArg[IronRuby.Builtins.UnboundMethod]", :RefBooleanArg => "RefBooleanArg"},
   "bool" => { :GenericArg => "GenericArg[System.Boolean]",
     :RefBooleanArg => "RefBooleanArg"},
   "Array[byte]" => { :GenericArg => "GenericArg[System.Byte[]]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefByteArrArg => "RefByteArrArg", :RefBooleanArg => "RefBooleanArg", :IListArg => "IListArg"},
   "List[byte]" => { :GenericArg => "GenericArg[System.Collections.Generic.List`1[System.Byte]]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefBooleanArg => "RefBooleanArg", :IListArg => "IListArg"},
   "self" => { :GenericArg => "GenericArg[IronRuby.Builtins.MutableString]",
     :IEnumerableIteratingArg => "IEnumerableIteratingArg",
     :RefByteArrArg => "RefByteArrArg",
     :RefBooleanArg => "RefBooleanArg"},
   "StructImplementsIInterface" => { :GenericArg => "GenericArg[StructImplementsIInterface]",
     :RefStructImplementsIInterfaceArg => "RefStructImplementsIInterfaceArg",
     :RefBooleanArg => "RefBooleanArg", :ParamsIInterfaceArrTestArg => "ParamsIInterfaceArrTestArg"},
   "RubyImplementsIInterface" => { :GenericArg => /GenericArg\[IronRuby\.Classes\.Object\$\d{1,3}\]/,
     :RefBooleanArg => "RefBooleanArg", :ParamsIInterfaceArrTestArg => "ParamsIInterfaceArrTestArg"},
   "ImplementsIInterface" => { :GenericArg => "GenericArg[ImplementsIInterface]",
     :RefImplementsIInterfaceArg => "RefImplementsIInterfaceArg",
     :RefBooleanArg => "RefBooleanArg", :ParamsIInterfaceArrTestArg =>"ParamsIInterfaceArrTestArg"},
  }
  before(:each) do
    @target = ClassWithMethods.new
    @target2 = RubyClassWithMethods.new
    @values = Helper.other_concern_args
  end

  after(:each) do
    ClassWithMethods.StaticReset
    RubyClassWithMethods.StaticReset
  end
    
  @matrix.each do |input, results|
    Helper.run_matrix(results, input, @keys)
  end

  [ClassWithMethods, RubyClassWithMethods].each do |target|
    it "binds 'StaticMethodNoArg' for '' with 'StaticMethodNoArg' (#{target})" do
      target.StaticMethodNoArg.should == "StaticMethodNoArg"
      target.StaticTracker.should == [ nil ]
    end
  
    [@target, @target2, Class.new(ClassWithMethods).new, nil].each do |obj|
      it "binds 'StaticMethodClassWithMethodsArg' for '#{obj.inspect}' with 'StaticMethodNoArg' (#{target})" do
        target.StaticMethodClassWithMethodsArg(obj).should == "StaticMethodClassWithMethodsArg"
      end

      it "passes the correct input (#{obj.inspect}) into the method 'StaticMethodClassWithMethodsArg'" do
        target.StaticMethodClassWithMethodsArg(obj)
        target.StaticTracker.should == [obj]
      end
    end

    [1, "", Object.new, CStruct.new, CustomEnum.A, true, false].each do |obj|
      it "binds 'StaticMethodClassWithMethodsArg' for '#{obj.inspect}' with 'TypeError' (#{target})" do
        lambda {target.send(:StaticMethodClassWithMethodsArg, obj)}.should raise_error TypeError
      end
    end
  end


  #RefOutInt32Args RefInt32OutArgs Int32RefOutArgs 
  #RefInt32Int32OutInt32Arg ByteArrRefByteArrArg KeywordsArg 
  #
    #
  it "binds the proper number of arguments" do
    a = [1,2,3,4,5,6,7,8]
    0.upto(6) do |i|
      lambda{ @target.send(:EightArgs, *(a[0..i])) }.should raise_error AE
    end

    lambda { @target.EightArgs }.should raise_error AE
    @target.EightArgs(*a).should == "EightArgs"
    @target.tracker.should == a
  end
end
