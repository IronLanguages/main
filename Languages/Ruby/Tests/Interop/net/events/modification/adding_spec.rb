require File.dirname(__FILE__) + '/../../spec_helper'

describe "Basic .NET events allow adding" do
  before :each do
    def foo(s,e); end
    @method = method(:foo)
    @lambda = lambda {|a,b| }
    @proc = proc {|a,b| }

    @klass = BasicEventClass.new
  end

  it "method handlers via add" do
    lambda{ @klass.OnEvent.add @method }.should_not raise_error
  end

  it "lambda's via add" do
    lambda{ @klass.on_event.add @lambda }.should_not raise_error
  end

  it "procs via add" do
    lambda{ @klass.on_event.add @proc }.should_not raise_error
  end

  it "multiple items via add" do
    lambda do
      @klass.on_event.add @method
      @klass.on_event.add @proc
      @klass.on_event.add @lambda
    end.should_not raise_error
  end

  it "one item multiple times via add" do
    lambda do
      @klass.on_event.add @method
      @klass.on_event.add @method
    end.should_not raise_error
  end
end


