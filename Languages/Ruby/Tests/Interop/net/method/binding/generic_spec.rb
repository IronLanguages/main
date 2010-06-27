require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"
describe :simple, :shared => true do
  it "should work on" do
    @targets.each do |target|
      arity = target.method(@method).arity
      [1, @args["anonymous classInstance"], @args["MyStringa"], Class.new(Hash).new, Class.new(Array).new ].each do |arg|
        args = []
        arity.times do
          args << arg
        end
        target.should infer(@method).with(*args)
      end
    end
  end
end

describe :relationships, :shared => true do
  it "should not work with differing types with no inheritance relation" do
    @targets.each do |target|
      target.should infer(@method).except(1, 2.0)
      target.should infer(@method).except(1, 'abc')
    end
  end

  it "should work with differing types with a subclass relationship (Object)" do
    @targets.each do |target| 
      target.should infer(@method).with(@args["obj"], @args["anonymous classInstance"])
      target.should infer(@method).with(@args["anonymous classInstance"], @args["obj"])
    end
  end
    
  it "should work with differing types with a subclass relationship (Array)" do
    @targets.each do |target| 
      my_arr = Class.new(Array).new
      target.should infer(@method).with(my_arr, [])
      target.should infer(@method).with([], my_arr)
    end
  end
end
describe "Generic Type inference" do
  before(:each) do
    @targets = [RubyGenericTypeInference, GenericTypeInference, RubyGenericTypeInferenceInstance.new, GenericTypeInferenceInstance.new]
    @args = Helper.args
  end
  describe "on Tx methods" do
    it_behaves_like :simple, :Tx
  end
  describe "on TxTy methods" do
    it_behaves_like :simple, :TxTy
    it_behaves_like :relationships, :TxTy
  end
  describe "on TxTyTz methods" do
    it_behaves_like :simple, :TxTyTz
  end

  describe "on TRefx methods" do
    it_behaves_like :simple, :TRefx

    it "can be called in a Ruby subclass" do
      [RubyGenericTypeInference, RubyGenericTypeInferenceInstance.new].each do |t|
        t.call_long_method(:TRefx)[0].should == 'IronRuby.Builtins.MutableString'
        t.call_short_method(:TRefx)[0].should == 'IronRuby.Builtins.MutableString'
      end
    end
  end

  
  describe "on TParamsArrx methods" do
    it "should work on simple methods" do
      @targets.each do |target|
        [
          [1], [1,2,3],
          [@args["anonymous classInstance"], @args["anonymous classInstance"]],
          [@args["anonymous classInstance"], @args["anonymous classInstance"], @args["anonymous classInstance"]],
        
        ].each do |args|
          target.should infer(:TParamsArrx).with(*args)
        end
      end
    end
    
    it "should not work with NoArgs" do
      @targets.each do |target|
        target.should infer(:TParamsArrx).except(*[])
      end
    end

    it_behaves_like :relationships, :TParamsArrx
  end
  
  describe "on TxTParamsArry methods" do
    it "should work on simple methods" do
      @targets.each do |target|
        [
          [1], [1,2,3],
          [@args["anonymous classInstance"], @args["anonymous classInstance"]],
          [@args["anonymous classInstance"], @args["anonymous classInstance"], @args["anonymous classInstance"]],
        
        ].each do |args|
          target.should infer(:TxTParamsArry).with(*args)
        end
      end
    end
    
    it "should not work with NoArgs" do
      @targets.each do |target|
        target.should infer(:TxTParamsArry).except(*[])
      end
    end

    it_behaves_like :relationships, :TxTParamsArry
  end

  describe "on Txclass methods" do
    it "should not work with ValueTypes" do
      @targets.each do |target|
        target.should infer(:TxClass).except(1)
        #Ruby's True map pretty much to CLR's true, so for Generics it gets
        #treated as a value type. Documenting that here.
        target.should infer(:TxClass).except(@args["true"])
      end
    end

    it "should work on C# reference types" do
      @targets.each do |target|
        target.should infer(:TxClass).with(@args["System::Stringa"])
        target.should infer(:TxClass).with(@args["obj"])
      end
    end
    
    it "should work on Ruby types" do
      @targets.each do |target|
        target.should infer(:TxClass).with(@args["Stringa"])
        target.should infer(:TxClass).with(@args["anonymous class"])
        target.should infer(:TxClass).with(@args["nil"])
      end
    end
  end
end
