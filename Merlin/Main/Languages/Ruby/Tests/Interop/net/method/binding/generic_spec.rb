require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"
class InferMatcher
  
  def initialize(meth)
    @meth = meth
    @exceptions = []
    @passing = []
  end

  def with(*args)
    @passing <<  args 
    self
  end
  
  def except(*args)
    @exceptions << args
    self
  end

  def matches?(target)
    @target_type = target.GetType
    pass = []
    @passing.each do |arg|
      pass << (target.send(@meth, *arg) == type_to_string(*arg))
    end
    @exceptions.each do |arg|
      pass << begin
                target.send(@meth, *arg)
                false
              rescue ArgumentError
                true
              end
    end
    pass.all? {|e| e}
  end
  
  def type_to_string(*type)
    type = type.last
    if type == nil
      'System.Object'
    else
      type.GetType.ToString
    end
  end
  
  def failure_message
    ["Expected to be able to infer the generic type", "from calling #{@meth} on #{@target_type}"]
  end

  def negative_failure_message
    ["Expected not to be able to infer the generic type", "from calling #{@meth} on #{@target_type}"]
  end
end

class Object
  def infer(meth)
    InferMatcher.new(meth)
  end
end
describe "Generic Type inference" do
  before(:each) do
    @targets = [RubyGenericTypeInference, GenericTypeInference, RubyGenericTypeInferenceInstance.new, GenericTypeInferenceInstance.new]
    @args = Helper.args
  end

  it "should work on simple Tx methods" do
    @targets.each do |target|
      [1, @args["anonymous classInstance"], @args["MyStringa"], Class.new(Hash).new, Class.new(Array).new ].each do |args|
        target.should infer(:Tx).with(args)
      end
    end
  end

  it "should work on simple TxTy methods" do
    @targets.each do |target|
      [1, @args["anonymous classInstance"], @args["MyStringa"], Class.new(Hash).new, Class.new(Array).new ].each do |args|
        target.should infer(:TxTy).with(args, args)
      end
    end
  end

  it "should not work for simple TxTy methods with differing types with no inheritance relation" do
    @targets.each do |target|
      target.should infer(:TxTy).except(1, 2.0)
      target.should infer(:TxTy).except(1, 'abc')
    end
  end

  it "should work for simple TxTy methods with differing types with a subclass relationship (Object)" do
    @targets.each do |target| 
      target.should infer(:TxTy).with(@args["obj"], @args["anonymous classInstance"])
      target.should infer(:TxTy).with(@args["anonymous classInstance"], @args["obj"])
    end
  end
    
  it "should work for simple TxTy methods with differing types with a subclass relationship (Array)" do
    @targets.each do |target| 
      my_arr = Class.new(Array).new
      target.should infer(:TxTy).with(my_arr, [])
      target.should infer(:TxTy).with([], my_arr)
    end
  end
    
  it "should work on simple TxTyTz methods" do
    @targets.each do |target|
      [1, @args["anonymous classInstance"], @args["MyStringa"], Class.new(Hash).new, Class.new(Array).new ].each do |args|
        target.should infer(:TxTyTz).with(args, args, args)
      end
    end
  end
  
  it "should work on simple TParamsArrx methods" do
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
  
  it "should not work for simple TParamsArrx methods with NoArgs" do
    @targets.each do |target|
      target.should infer(:TParamsArrx).except(*[])
    end
  end
  it "should not work for simple TParamsArrx methods with differing types with no inheritance relation" do
    @targets.each do |target|
      target.should infer(:TParamsArrx).except(1, 2.0)
      target.should infer(:TParamsArrx).except(1, 'abc')
    end
  end

  it "should work for simple TParamsArrx methods with differing types with a subclass relationship (Object)" do
    @targets.each do |target| 
      target.should infer(:TParamsArrx).with(@args["obj"], @args["anonymous classInstance"])
      target.should infer(:TParamsArrx).with(@args["anonymous classInstance"], @args["obj"])
    end
  end
    
  it "should work for simple TParamsArrx methods with differing types with a subclass relationship (Array)" do
    @targets.each do |target| 
      target.should infer(:TParamsArrx).with(Class.new(Array).new, [])
      target.should infer(:TParamsArrx).with([], Class.new(Array).new)
    end
  end
  
  it "should work on simple TxTParamsArry methods" do
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
  
  it "should not work for simple TxTParamsArry methods with NoArgs" do
    @targets.each do |target|
      target.should infer(:TxTParamsArry).except(*[])
    end
  end
  it "should not work for simple TxTParamsArry methods with differing types with no inheritance relation" do
    @targets.each do |target|
      target.should infer(:TxTParamsArry).except(1, 2.0)
      target.should infer(:TxTParamsArry).except(1, 'abc')
    end
  end

  it "should work for simple TxTParamsArry methods with differing types with a subclass relationship (Object)" do
    @targets.each do |target| 
      target.should infer(:TxTParamsArry).with(@args["obj"], @args["anonymous classInstance"])
      target.should infer(:TxTParamsArry).with(@args["anonymous classInstance"], @args["obj"])
    end
  end
    
  it "should work for simple TxTParamsArry methods with differing types with a subclass relationship (Array)" do
    @targets.each do |target| 
      target.should infer(:TxTParamsArry).with(Class.new(Array).new, [])
      target.should infer(:TxTParamsArry).with([], Class.new(Array).new)
    end
  end
end
