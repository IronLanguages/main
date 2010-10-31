require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../fixtures/class'
require File.dirname(__FILE__) + '/fixtures/common'
require File.dirname(__FILE__) + '/shared/new'

describe "Exception.exception" do
  it_behaves_like(:exception_new, :exception)
end

describe "Exception.exception" do
  it "returns new exception even if argument is Exception (unlike Exception#exception)" do
    e = Exception.new
    Exception.exception(e).message.should equal(e)
  end

  it "returns new exception with string message" do
    Exception.exception("new message").message.should == "new message"
  end

  it "creates new object if an argument responding to to_s is given" do
    m = ClassSpecs::A.new
    m.should_receive(:to_s).any_number_of_times.and_return("new message")
    e2 = Exception.exception(m)
    e2.message.should equal(m)
  end

  it "allows nil argument" do
    e2 = Exception.exception(nil)
    e2.message.should == "Exception"
  end
end

describe "Exception#exception" do
  before(:each) do
    @e = RuntimeError.new "test message"
    @e.set_backtrace ["func0", "func1"]
  end
  
  it "returns self if no arguments are given" do
    @e.exception.should equal(@e)
  end

  ruby_bug("http://redmine.ruby-lang.org/issues/show/1248", "1.8.6") do
    it "returns self if argument is self" do
      @e.exception(@e).should_not equal(@e)
    end
  end

  it "preserves backtrace" do
    b = @e.backtrace
    @e.exception.backtrace.should equal(b)
  end
  
  it "creates new object ands sets #message to argument" do
    ["new message", ClassSpecs::A.new, ClassSpecs::Undef_to_s.new].each do |m|
      e2 = @e.exception(m)
      e2.message.should equal(m)
    end
  end

  it "does not call initialize" do
    e = ExceptionSpecs::InitializedException.new
    ScratchPad.clear
    e2 = e.exception("new message")

    e2.should_not equal(e)
    ScratchPad.recorded.should == nil    
  end
  
  it "creates a new object with an empty backtrace" do
    e2 = @e.exception("new message")
    e2.backtrace.should be_nil
  end
  
  it "allows nil argument" do
    e2 = @e.exception(nil)
    e2.should_not equal(@e)
    e2.message.should == @e.class.to_s
  end
end

describe "Exception" do
  it "is a Class" do
    Exception.should be_kind_of(Class)
  end

  it "is a superclass of NoMemoryError" do
    Exception.should be_ancestor_of(NoMemoryError)
  end

  it "is a superclass of ScriptError" do
    Exception.should be_ancestor_of(ScriptError)
  end
  
  it "is a superclass of SignalException" do
    Exception.should be_ancestor_of(SignalException)
  end
  
  it "is a superclass of Interrupt" do
    SignalException.should be_ancestor_of(Interrupt)
  end

  it "is a superclass of StandardError" do
    Exception.should be_ancestor_of(StandardError)
  end
  
  it "is a superclass of SystemExit" do
    Exception.should be_ancestor_of(SystemExit)
  end

  it "is a superclass of SystemStackError" do
    Exception.should be_ancestor_of(SystemStackError)
  end

  ruby_version_is "1.9" do
    it "is a superclass of SecurityError" do
      Exception.should be_ancestor_of(SecurityError)
    end

    it "is a superclass of EncodingError" do
      Exception.should be_ancestor_of(EncodingError)
    end
  end
end

describe "Exception#exception" do
  it "returns self when passed no argument" do
    e = RuntimeError.new
    e.should == e.exception
  end

  it "returns self when passed self as an argument" do
    e = RuntimeError.new
    e.should == e.exception(e)
  end

  it "returns an exception of the same class as self with the message given as argument" do
    e = RuntimeError.new
    e2 = e.exception(:message)
    e2.should be_an_instance_of(RuntimeError)
    e2.message.should == :message
  end
end
