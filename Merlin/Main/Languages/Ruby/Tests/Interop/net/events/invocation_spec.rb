require File.dirname(__FILE__) + '/../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Invoking events" do
  before :each do
    @helper = EventHandlerHelper.new
    @method = @helper.method(:foo)
    @lambda = lambda { |s, count| @helper[:lambda] += count }
    @proc = proc { |s, count| @helper[:proc] += count }
    @klass = ClassWithEvents.new
    @no_event_klass = ClassWithEvents.new
  end

  it "works with methods via add" do
    @klass.full_event.add @method
    @klass.invoke_full_event(1)
    @helper[:method].should == 1
  end

  it "works with lambdas via add" do
    @klass.full_event.add @lambda
    @klass.invoke_full_event(1)
    @helper[:lambda].should == 1
  end

  it "works with procs via add" do
    @klass.full_event.add @proc
    @klass.invoke_full_event(1)
    @helper[:proc].should == 1
  end

  it "works with to_proc syntax" do
    @klass.full_event &@lambda
    @klass.invoke_full_event(1)
    @helper[:lambda].should == 1
  end

  it "works with block syntax" do
    @klass.full_event {|s,e| @helper[:block] += e} 
    @klass.invoke_full_event(1)
    @helper[:block].should == 1
  end

  it "works with multiple objects via add" do
    @klass.full_event.add @method
    @klass.full_event.add @proc
    @klass.full_event.add @lambda
    @klass.invoke_full_event(1)
    @helper[:proc].should == 1
    @helper[:method].should == 1
    @helper[:lambda].should == 1
  end
  
  it "works with multiple of one callback via add" do
    @klass.full_event.add @method
    @klass.full_event.add @method
    @klass.invoke_full_event(1)
    @helper[:method].should == 2
  end

  it "registers adds and removes" do
    @klass.full_event.add @method
    @klass.full_event.add @method
    @klass.invoke_full_event(1)
    @klass.full_event.remove @method
    @klass.invoke_full_event(1)
    @helper[:method].should == 3
  end
end
