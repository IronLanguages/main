require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Method parameter binding with misc parameters" do
  @keys = [:ClassWithMethodsArg, :GenericArg, :OutNonByRefInt32Arg, :IDictionaryOfIntIntArg, :HashtableArg, :ListOfIntArg, :IEnumerableIteratingArg, :IEnumeratorIteratingArg, :IEnumerableOfCharIteratingArg, :IEnumeratorOfCharIteratingArg, :IEnumerableOfIntIteratingArg, :IEnumeratorOfIntIteratingArg, :DelegateArg, :IntIntDelegateArg, :RefByteArrArg, :RefStructImplementsIInterfaceArg, :OutStructImplementsIInterfaceArg, :RefImplementsIInterfaceArg, :RefBooleanArg, :OutBooleanArg, :ParamsIInterfaceArrTestArg, :IListOfIntArg2, :IListOfCharArg, :IListArg]
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
   "StructImplementsIInterfaceInstance" => { :GenericArg => "GenericArg[StructImplementsIInterface]",
     :RefStructImplementsIInterfaceArg => "RefStructImplementsIInterfaceArg",
     :RefBooleanArg => "RefBooleanArg", :ParamsIInterfaceArrTestArg => "ParamsIInterfaceArrTestArg"},
   "RubyImplementsIInterfaceInstance" => { :GenericArg => /GenericArg\[IronRuby\.Classes\.Object\$\d{1,3}\]/,
     :RefBooleanArg => "RefBooleanArg", :ParamsIInterfaceArrTestArg => "ParamsIInterfaceArrTestArg"},
   "ImplementsIInterfaceInstance" => { :GenericArg => "GenericArg[ImplementsIInterface]",
     :RefImplementsIInterfaceArg => "RefImplementsIInterfaceArg",
     :RefBooleanArg => "RefBooleanArg", :ParamsIInterfaceArrTestArg =>"ParamsIInterfaceArrTestArg"},
   "NoArg" => (Hash.new(AE).merge({ 
     :OutStructImplementsIInterfaceArg => "OutStructImplementsIInterfaceArg",  
     :OutBooleanArg => "OutBooleanArg", :ParamsIInterfaceArrTestArg => "ParamsIInterfaceArrTestArg"}))
  }
  before(:each) do
    @target = ClassWithMethods.new
    @target2 = RubyClassWithMethods.new
    @values = Helper.args
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


  it "doesn't special case keyword args" do
    %w{this self class public}.each do |arg|
      [@target, @target2].each do |target|
        target.KeywordsArgs(1, target, arg).should == ["KeywordsArgs", arg.upcase]
        target.tracker.should == arg.upcase
        target.reset
      end
    end
  end

  it "binds ref out and regular params correctly" do
    %w{RefOutInt32Args RefInt32OutArgs Int32RefOutArgs}.each do
      [@target, @target2].each do |target|
        target.RefOutInt32Args(1,2).should == ["RefOutInt32Args", 2, 2]
        target.tracker.should == [2,2,2]
        target.reset
        target.RefInt32OutArgs(1,2).should == ["RefInt32OutArgs", 2, 2]
        target.tracker.should == [2,2,2]
        target.reset
        target.Int32RefOutArgs(1,2).should == ["Int32RefOutArgs", 1, 1]
        target.tracker.should == [1,1,1]
        target.reset
        target.RefInt32Int32OutInt32Arg(1,2).should == ["RefInt32Int32OutInt32Arg", 100, 3]
        target.tracker.should == [100, 2, 3]
        target.reset
      end
    end
  end

  it "binds Byte[] and Ref Byte[] methods" do
    input = System::Array.of(System::Byte).new(2, System::Byte.new(1))
    [@target, @target].each do |target|
      target.ByteArrRefByteArrArg(input , System::Array.of(System::Byte).new(2, System::Byte.new(3))).should == ["ByteArrRefByteArrArg", input]
      target.tracker.should == [input]
      target.reset
    end
  end
  
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

describe "Generic class methods" do
  it "bind correctly" do
    target = GenericClassWithMethods.of(Fixnum).new
    [100, Bignum.Zero, 4.5].each do |param|
      target.GenericArg(param).should == "GenericArg"
      target.tracker.should == param.to_i
      target.reset
    end
  end
end

describe "Binding methods" do
  #This is a temporary regression until I rewrite these specs.
  it "StrongBox works" do
    require 'microsoft.scripting.core'
    SI = System::Runtime::CompilerServices::StrongBox[Fixnum]
    cwm = ClassWithMethods.new
    si = SI.new(0)
    si.value.should == 0
    cwm.RefInt32Arg(si).should == "RefInt32Arg"
    si.value.should == 1
  end

  it "OutNonByRef works" do
    cwm = ClassWithMethods.new
    lambda {cwm.OutNonByRefInt32Arg}.should raise_error
    cwm.OutNonByRefInt32Arg(1).should == 'OutNonByRefInt32Arg'
    cwm.tracker.should == [1]
    cwm.OutNonByRefInt32Arg(2)
    cwm.tracker.should == [1,1]
  end
end
