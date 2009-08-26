require File.dirname(__FILE__) + '/../../spec_helper'

describe "Basic .NET events allow removing" do
  before :each do
    def foo(s,e); end
    @method = method(:foo)
    @lambda = lambda {|a,b| }
    @proc = proc {|a,b| }

    @klass = BasicEventClass.new
    @klass.on_event.add @method
    @klass.on_event.add @method
    @klass.on_event.add @lambda
    @klass.on_event.add @proc
  end

  it "method handlers via remove" do
    lambda{ @klass.OnEvent.remove @method }.should_not raise_error
  end

  it "lambda's via remove" do
    lambda{ @klass.on_event.remove @lambda }.should_not raise_error
  end

  it "procs via remove" do
    lambda{ @klass.on_event.remove @proc }.should_not raise_error
  end

  it "multiple items via remove" do
    lambda do
      @klass.on_event.remove @method
      @klass.on_event.remove @proc
      @klass.on_event.remove @lambda
    end.should_not raise_error
  end

  it "one item multiple times via remove" do
    lambda do
      @klass.on_event.remove @method
      @klass.on_event.remove @method
    end.should_not raise_error
  end

  it "one item more times than it is in the list via remove" do
    lambda do
      @klass.on_event.remove @method
      @klass.on_event.remove @method
      @klass.on_event.remove @method
    end.should_not raise_error
  end
end


