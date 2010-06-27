require File.dirname(__FILE__) + "/../spec_helper"
describe :calling_interface_methods, :shared => true do
  it "works for properties" do
    @target.Hello = @target
    @target.hello.should == @target
  end

  it "works for methods with interface parameters" do
    lambda { @target.Foo(@target) }.should_not raise_error
  end

  it "works for methods with interface return values" do
    @target.RetInterface.should == @target
  end

  it "works for events" do
    @fired = false
    def fired(*args)
      @fired = true
      return args[0]
    end
    
    @target.MyEvent.add method(:fired)
    @target.FireEvent(@target.GetEventArgs).should == @target
    @fired.should == true
    @target.MyEvent.remove method(:fired)
  end
end
describe "Calling interface methods on private classes" do
  before(:each) do
    @target = InterfaceOnlyTest.PrivateClass
  end
  it_behaves_like :calling_interface_methods, nil
end

describe "Calling interface methods on public classes" do
  before(:each) do
    @target = PublicClass.new
  end
  it_behaves_like :calling_interface_methods, nil
end

describe "Calling interface methods on explicit interfaces" do
  before(:each) do
    @pc = PublicIPublicInterface.new
    class << @pc
      def method_missing(meth, *args, &blk)
        clr_member(IPublicInterface, meth).call(*args, &blk)
      end
    end
  end
  it_behaves_like :calling_interface_methods, nil
end

describe "calling explicit interface methods" do
  it "works for basic case" do
    method_matcher do
      add_method(:name => "M", :result => :ne)
      add_method(:base => I1, :name => "M", :result => "I1.M")
      match ClassI1_1.new
    end
  end

  it "works with non-explicit overload" do
    method_matcher do
      add_method(:name => "M", :result => "class M")
      add_method(:base => I1, :name => "M", :result => "I1.M")
      match ClassI1_2.new
    end
  end

  it "works for multiple interfaces" do
    method_matcher do
      add_method(:name => "M", :result => :ne)
      add_method(:base => I1, :name => "M", :result => "I1.M")
      add_method(:base => I2, :name => "M", :result => "I2.M")
      match ClassI2I1.new
    end
  end

  it "works with generic interfaces" do
    method_matcher do
      add_method(:name => "M", :result => "class M")
      add_method(:base => I3[Object], :name => "M", :result => "I3<object>.M")
      match ClassI3Obj.new
    end
  end

  it "works for multiple interfaces with generic interface" do
    method_matcher do
      add_method(:name => "M", :result => "class M")
      add_method(:base => I3[Object], :name => "M", :result => "I3<object>.M")
      add_method(:base => I1, :name => "M", :result => "I1.M")
      add_method(:base => I2, :name => "M", :result => "I2.M")
      match ClassI1I2I3Obj.new
    end
  end

  it "works with a generic class and generic interface" do
    method_matcher do
      add_method(:name => "M", :result => "class M")
      add_method(:base => I3[String], :name => "M", :result => "I3<T>.M")
      add_method(:base => I3[Object], :name => "M", :result => :ne)
      match ClassI3_1[String].new
    end
    method_matcher do
      add_method(:name => "M", :result => :ne)
      add_method(:base => I3[String], :name => "M", :result => "I3<T>.M")
      add_method(:base => I3[Object], :name => "M", :result => :ne)
      match ClassI3_2[String].new
    end
  end

  it "works with generic interface multiple times" do
    method_matcher do
      add_method(:name => "M", :result => :ne)
      add_method(:base => I3[Object], :name => "M", :result => "I3<object>.M")
      add_method(:base => I3[Fixnum], :name => "M", :result => "I3<int>.M")
      add_method(:base => I3[String], :name => "M", :result => :ne)
      match ClassI3ObjI3Int.new
    end
  end

  it "works with different overloads from different interfaces" do
    method_matcher do
      add_method(:name => "M", :result => :ne)
      add_method(:base => I1, :name => "M", :result => "I1.M")
      add_method(:base => I4, :args => 1, :name => "M", :result => "I4.M")
      match ClassI1I4.new
    end
  end
end
