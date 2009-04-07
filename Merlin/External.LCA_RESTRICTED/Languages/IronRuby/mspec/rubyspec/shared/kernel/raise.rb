require File.dirname(__FILE__) + '/../../fixtures/kernel/classes'

describe :kernel_raise, :shared => true do
  before :each do
    ScratchPad.clear
  end

  it "aborts execution" do
    lambda do
      @object.raise Exception, "abort"
      ScratchPad.record :no_abort
    end.should raise_error(Exception, "abort")

    ScratchPad.recorded.should be_nil
  end

  it "raises RuntimeError if no exception class is given" do
    lambda { @object.raise }.should raise_error(RuntimeError)
  end

  it "re-raises the rescued exception" do
    lambda do
      begin
        raise Exception, "outer"
        ScratchPad.record :no_abort
      rescue
        begin
          raise StandardError, "inner"
        rescue
        end

        @object.raise
        ScratchPad.record :no_reraise
      end
    end.should raise_error(Exception, "outer")

    ScratchPad.recorded.should be_nil
  end

  it "allows Exception, message, and backtrace parameters" do
    b = ["func0", "func1"]
    begin
      @object.raise(ArgumentError, "test message", b)
    rescue ArgumentError => e
    end
    e.backtrace.should == b
    e.message == "test message"
  end
  
  it "calls Exception#exception when raising an existing Exception object" do
    ScratchPad.record []
    existing = KernelSpecs::TestException.new true
    begin
      raise existing
    rescue => e
      ScratchPad << e
    end
    ScratchPad.recorded[0].should equal(existing)
    ScratchPad.recorded[1..2].should == [:exception_method, "default value"]
    ScratchPad.recorded[3].should_not equal(existing)
  end

  it "calls Exception#set_backtrace when raising an existing Exception object" do
    ScratchPad.record []
    existing = KernelSpecs::TestException.new false, true
    begin
      raise existing
    rescue => e
      ScratchPad << e
    end
    ScratchPad.recorded[0].should_not equal(existing)
    ScratchPad.recorded[1..3].should == [:set_backtrace_method, Array, String]
    ScratchPad.recorded[0].should equal(ScratchPad.recorded[4])
  end
  
  it "accepts any object responding to #exception" do
    m = mock("non-Exception")
    m.should_receive(:exception).and_return(KernelSpecs::TestException.new)
    lambda { raise m }.should raise_error(KernelSpecs::TestException)
  end

  it "requires #exception to return an Exception" do
    m = mock("non-Exception")
    m.should_receive(:exception).and_return(m)
    lambda { raise m }.should raise_error(TypeError)
  end

  it "requires an object responding to #exception" do
    m = mock("non-Exception")
    lambda { raise m }.should raise_error(TypeError)
  end
end
