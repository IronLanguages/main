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
