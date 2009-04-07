require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "Exception#backtrace" do
  it "is set for exceptions in an ensure block" do
    # IronRuby used to get this wrong.
    ExceptionSpecs.record_exception_from_ensure_block_with_rescue_clauses
    /record_exception_from_ensure_block_with_rescue_clauses/.match(ScratchPad.recorded.join).should_not be_nil
  end
  
  it "is nil by default" do
    RuntimeError.new.backtrace.should be_nil
  end

  it "is set for raising an existing exception" do
    existing = RuntimeError.new "hello"
    begin
      # Raising with a different message was causing IronRuby to grab the current backtrace
      raise existing, "hello again"
    rescue => e
    end

    e.backtrace.join("\n").should =~ /backtrace_spec/
  end
end
