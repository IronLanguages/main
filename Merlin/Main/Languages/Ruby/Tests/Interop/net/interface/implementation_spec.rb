require File.dirname(__FILE__) + '/../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Implementing interfaces" do
  it "works with normal interfaces" do
    stuff = RubyImplementsIDoStuff.new
    ConsumeIDoStuff.ConsumeStuffFoo(stuff).should == 1
    ConsumeIDoStuff.ConsumeStuffBar(stuff).should == "2"
  end
  
  it "works with overloaded methods on an interface" do
    foo = RubyImplementsIDoFoo.new
    ConsumeIDoFoo.ConsumeFoo1(foo).should == 1  
    ConsumeIDoFoo.ConsumeFoo2(foo).should == 1
    ConsumeIDoFoo.ConsumeFoo3(foo).should == 1
  end
  
  it "works with Hash (regression for CP#814)" do
    class HashSubclass < Hash
      include System::Collections::IDictionary
    end

    lambda { HashSubclass.new }.should_not raise_error(TypeError)
  end
end

describe "Implementing interfaces with default methods" do
  it "allows instantiation" do
    lambda { foo = RubyImplementsIDoFooDefaults.new }.should_not raise_error
    lambda { stuff = RubyImplementsIDoStuffDefaults.new }.should_not raise_error
  end

  describe "allows" do
    before(:each) do
      @foo = RubyImplementsIDoFooDefaults.new
      @stuff = RubyImplementsIDoStuffDefaults.new
    end
    
    it "allows method calls" do
      lambda {@foo.foo(1)}.should raise_error NoMethodError
      lambda {@stuff.stuff_foo(1)}.should raise_error NoMethodError
    end

    it "is kind_of the interface type" do
      @foo.should be_kind_of IDoFoo
      @stuff.should be_kind_of IDoStuff
    end

    it "can be passed as an interface object" do
      lambda { ConsumeIDoStuff.ConsumeStuffFoo(@stuff) }.should raise_error NoMethodError
      lambda { ConsumeIDoFoo.ConsumeFoo2(@foo) }.should raise_error NoMethodError
    end
  end

  describe "with MethodMissing" do
    before(:each) do
      @foo = RubyImplementsIDoFooMM.new
      @stuff = RubyImplementsIDoStuffMM.new
    end

    it "calls method missing from Ruby" do
      @foo.foo(1).should == 1
      @foo.tracker.should == "IDoFoo MM foo(1)"
      @stuff.stuff_bar(1).should == "IDoStuff MM stuff_bar(1)"
    end

    it "calls method missing from C#" do
      ConsumeIDoStuff.ConsumeStuffBar(@stuff).should == "IDoStuff MM StuffBar(2)"
      ConsumeIDoFoo.ConsumeFoo1(@foo).should == 1  
      @foo.tracker.should ==  "IDoFoo MM Foo(hello)"
    end
  end
end

describe "Implementing interfaces that define events" do
  before(:each) do
    @klass = Klass.new
  end

  it "allows empty implementation without TypeLoadException" do
    lambda {RubyExposerDefault.new}.should_not raise_error
  end

  it "allows method_missing to be the event managment methods" do 
    exposer = RubyExposerMM.new
    @klass.add_event(exposer)
    exposer.handlers.size.should == 1
    exposer.trigger
    @klass.foo.should == 11
    @klass.remove_event(exposer)
    exposer.handlers.should be_empty
  end

  it "allows add and remove event definitions" do
    exposer = RubyExposer.new
    @klass.add_event(exposer)
    exposer.handlers.size.should == 1
    exposer.trigger
    @klass.foo.should == 11
    @klass.remove_event(exposer)
    exposer.handlers.should be_empty
  end
end

describe "Implementing interfaces that define generic methods" do
  it "can create an object that implements the interface" do
    RubyHasGenerics.should be_classlike
  end

  it "can call interface methods from ruby" do
    rhg = RubyHasGenerics.new
    rhg.generics_here("a").should == "ruby generics here"
    rhg.more_generics_here("a").should == 'ruby more generics here'
  end

  it "cal call interface methods from C#" do
    rhg = RubyHasGenerics.new
    EatIHaveGenerics.test_generics_here(rhg).should == "ruby generics here"
    EatIHaveGenerics.test_more_generics_here(rhg).should == "ruby more generics here"
  end
end
