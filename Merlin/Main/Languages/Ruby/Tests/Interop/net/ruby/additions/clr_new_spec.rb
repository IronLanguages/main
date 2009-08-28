require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Added method clr_new" do
  it "calls the CLR ctor for a CLR type" do
    ctor = CLRNew::Ctor.clr_new
    ctor.should be_kind_of CLRNew::Ctor
    ctor.tracker.should == 1
  end

  it "calls the CLR ctor for a subclassed CLR type" do
    ctor = CLRNew::Ctor.clr_new
    ctor.should be_kind_of CLRNew::Ctor
    ctor.tracker.should == 1
  end

  it "calls the CLR ctor for aliased CLR types" do
    Array.clr_new.should == []
    Hash.clr_new.should == {}
    (Thread.clr_new(System::Threading::ThreadStart.new {})).should be_kind_of Thread
    IO.clr_new.should be_kind_of IO
    String.clr_new.should == ""
    Object.clr_new.should be_kind_of Object
    Exception.clr_new.should be_kind_of Exception
    #TODO: All builtins?
  end

  it "doesn't call any Ruby initializer" do
    ctor = CLRNew::Ctor.clr_new
    ctor.tracker.should_not == 2
  end

  it "raises a TypeError if called on a pure Ruby type" do
    class Bar;end
    lambda { Bar.clr_new }.should raise_error TypeError
    lambda { Class.new.clr_new }.should raise_error TypeError
    lambda { Numeric.clr_new }.should raise_error TypeError
  end
end
