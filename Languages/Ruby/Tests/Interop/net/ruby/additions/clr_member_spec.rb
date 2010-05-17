require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Added method clr_member" do
  it "returns a method object" do
    Object.new.clr_member(:Finalize).should be_kind_of Method
  end

  it "returns CLR members from mixed types (Fixnum, Object, etc)" do
    lambda { Object.new.clr_member(:Finalize) }.should_not raise_error
    lambda { 1.clr_member(:Finalize) }.should_not raise_error
  end

  it "returns CLR members from CLR types" do
    k = Klass.new
    k.clr_member(:a).should be_kind_of Method
    #getter only
    k.clr_member(:foo).should be_kind_of Method
    #getter/setter
    k.clr_member(:my_decimal).should be_kind_of Method
  end

  it "returns the original CLR implementation for overridden methods" do
    k = Klass.new
    class << k
      def a
        "b"
      end
      
      def A
        "B"
      end
    end
    k.a.should == "b"
    k.A.should == "B"
    k.clr_member(:a).call.should == "a"
    k.clr_member(:A).call.should == "a"
  end

  #Waiting for clarification from Tomas
  #it "returns CLR members from interfaces" do
    #c = Class.new do
      #include IInterface
      #def m
      #end
    #end
    #c.new.clr_member(:m).should be_kind_of Method
  #end

  it "raises an error when method doesn't exist" do
    lambda { Object.new.clr_member(:not_a_ruby_or_clr_method) }.should raise_error(NameError)
  end

  it "raises an error if the method is a ruby method" do
    lambda { Object.new.clr_member(:type) }.should raise_error(NameError)
    lambda { Object.new.method(:type) }.should_not raise_error
  end

  it "returns CLR members using CLR Name or Ruby name" do
    o = Object.new
    o.clr_member(:GetType).should be_kind_of Method
    o.clr_member(:get_type).should be_kind_of Method
  end

  it "attempts to convert with to_str" do
    obj = mock('obj')
    obj.should_not_receive(:to_s)
    obj.should_receive(:to_str).and_return("get_type")
    Object.new.clr_member(obj)
  end

  it "converts ints" do
    lambda { Object.new.clr_member(:get_type.to_i) }.should_not raise_error
  end

  describe "with type argument" do
    it "works as without type argument if the type argument == the current type" do
      lambda { Object.new.clr_member(Object, :Finalize) }.should_not raise_error
      lambda { 1.clr_member(Fixnum, :Finalize) }.should_not raise_error

      k = Klass.new
      k.clr_member(Klass, :a).should be_kind_of Method
      #getter only
      k.clr_member(Klass, :foo).should be_kind_of Method
      #getter/setter
      k.clr_member(Klass, :my_decimal).should be_kind_of Method

      k = Klass.new
      class << k
        def a
        "b"
        end

        def A
        "B"
        end
      end
      k.a.should == "b"
      k.A.should == "B"
      k.clr_member(Klass, :a).call.should == "a"
      k.clr_member(Klass, :A).call.should == "a"
    end

    it "can be used to get explicit interface members" do
      e = ExplicitIInterface.new
      e.m
      e.tracker.should == 1
      e.reset
      e.tracker.should == 0
      e.clr_member(ExplicitIInterface, :m).call
      e.tracker.should == 1
      e.reset
      e.clr_member(IInterface, :m).call
      e.tracker.should == 2
    end
  end
end
