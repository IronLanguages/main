require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "Exception#backtrace" do
  before(:each) do
    @backtrace = ExceptionSpecs::Backtrace.backtrace
  end

  it "returns nil if no backtrace was set" do
    Exception.new.backtrace.should be_nil
  end

  it "returns an Array" do
    @backtrace.should be_an_instance_of(Array)
  end
  
  it "sets each element to a String" do
    @backtrace.each {|l| l.should be_an_instance_of(String)}
  end

  it "includes the filename of the location where self raised in the first element" do
    @backtrace.first.should =~ /common\.rb/
  end

  it "includes the line number of the location where self raised in the first element" do
    @backtrace.first.should =~ /:22:in /
  end

  it "includes the name of the method from where self raised in the first element" do
    @backtrace.first.should =~ /in `backtrace'/
  end

  it "includes the filename of the location immediately prior to where self raised in the second element" do
    @backtrace[1].should =~ /backtrace_spec\.rb/
  end

  it "includes the line number of the location immediately prior to where self raised in the second element" do
    @backtrace[1].should =~ /:6(:in )?/
  end

  it "contains lines of the same format for each prior position in the stack" do
    @backtrace[2..-1].each do |line|
      # This regexp is deliberately imprecise to account for 1.9 using
      # absolute paths where 1.8 used relative paths, the need to abstract out
      # the paths of the included mspec files, the differences in output
      # between 1.8 and 1.9, and the desire to avoid specifying in any 
      # detail what the in `...' portion looks like.
      line.should =~ /^[^ ]+\:\d+(:in `[^`]+')?$/
    end
  end

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

  it "returns proper line numbers, not 16707566" do
    backtrace = ruby_exe(fixture(__FILE__, "backtrace.rb"), :args => "2>&1").chomp
    backtrace.include?("backtrace.rb:2:in `foo'").should be_true
    backtrace.include?("backtrace.rb:5").should be_true
    backtrace.include?("16707566").should be_false
  end
end
