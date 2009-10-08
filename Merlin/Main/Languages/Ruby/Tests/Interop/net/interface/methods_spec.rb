require File.dirname(__FILE__) + "/../spec_helper"

describe "Calling interface methods" do
  before(:each) do
    @pc = InterfaceOnlyTest.PrivateClass
  end

  it "works for properties" do
    @pc.Hello = InterfaceOnlyTest.PrivateClass
    @pc.hello.should == @pc
  end

  it "works for methods with interface parameters" do
    lambda { @pc.Foo(@pc) }.should_not raise_error
  end

  it "works for methods with interface return values" do
    @pc.RetInterface.should == @pc
  end

  it "works for events" do
    @fired = false
    def fired(*args)
      @fired = true
      return args[0]
    end
    
    @pc.MyEvent.add method(:fired)
    @pc.FireEvent(@pc.GetEventArgs).should == @pc
    @fired.should == true
    @pc.MyEvent.remove method(:fired)
  end
end
