$: << File.dirname(__FILE__) + '/../lib/'
require 'test/spec'

$WARNING = ""
class Object
  def warn(msg)
    $WARNING << msg.to_s
    super msg
  end
end

class Test::Spec::Should
  def _warn
    _wrap_assertion {
      begin
        old, $-w = $-w, nil
        $WARNING = ""
        self.not.raise
        $WARNING.should.blaming("no warning printed").not.be.empty
      ensure
        $-w = old
      end
    }
  end
end

# Hooray for meta-testing.
module MetaTests
  class ShouldFail < Test::Spec::CustomShould
    def initialize
    end

    def assumptions(block)
      block.should.raise(Test::Unit::AssertionFailedError)
    end

    def failure_message
      "Block did not fail."
    end
  end

  class ShouldSucceed < Test::Spec::CustomShould
    def initialize
    end

    def assumptions(block)
      block.should.not.raise(Test::Unit::AssertionFailedError)
    end

    def failure_message
      "Block raised Test::Unit::AssertionFailedError."
    end
  end

  class ShouldBeDeprecated < Test::Spec::CustomShould
    def initialize
    end

    def assumptions(block)
      block.should._warn
      $WARNING.should =~ /deprecated/
    end

    def failure_message
      "warning was not a deprecation"
    end
  end
  

  def fail
    ShouldFail.new
  end

  def succeed
    ShouldSucceed.new
  end

  def deprecated
    ShouldBeDeprecated.new
  end
end

module TestShoulds
  class EmptyShould < Test::Spec::CustomShould
  end

  class EqualString < Test::Spec::CustomShould
    def matches?(other)
      object == other.to_s
    end
  end

  class EqualString2 < Test::Spec::CustomShould
    def matches?(other)
      object == other.to_s
    end

    def failure_message
      "yada yada yada"
    end
  end

  def empty_should(obj)
    EmptyShould.new(obj)
  end
  
  def equal_string(str)
    EqualString.new(str)
  end

  def equal_string2(str)
    EqualString2.new(str)
  end
end

context "test/spec" do
  include MetaTests
  
  specify "has should.satisfy" do
    lambda { should.satisfy { 1 == 1 } }.should succeed
    lambda { should.satisfy { 1 } }.should succeed

    lambda { should.satisfy { 1 == 2 } }.should fail
    lambda { should.satisfy { false } }.should fail
    lambda { should.satisfy { false } }.should fail

    lambda { 1.should.satisfy { |n| n % 2 == 0 } }.should fail
    lambda { 2.should.satisfy { |n| n % 2 == 0 } }.should succeed
  end

  specify "has should.equal" do
    lambda { "string1".should.equal "string1" }.should succeed
    lambda { "string1".should.equal "string2" }.should fail
    lambda { "1".should.equal 1 }.should fail

    lambda { "string1".should == "string1" }.should succeed
    lambda { "string1".should == "string2" }.should fail
    lambda { "1".should == 1 }.should fail
  end

  specify "has should.raise" do
    lambda { lambda { raise "Error" }.should.raise }.should succeed
    lambda { lambda { raise "Error" }.should.raise RuntimeError }.should succeed
    lambda { lambda { raise "Error" }.should.not.raise }.should fail
    lambda { lambda { raise "Error" }.should.not.raise(RuntimeError) }.should fail

    lambda { lambda { 1 + 1 }.should.raise }.should fail
    lambda { lambda { raise "Error" }.should.raise(Interrupt) }.should fail
  end

  specify "has should.raise with a block" do
    lambda { should.raise { raise "Error" } }.should succeed
    lambda { should.raise(RuntimeError) { raise "Error" } }.should succeed
    lambda { should.not.raise { raise "Error" } }.should fail
    lambda { should.not.raise(RuntimeError) { raise "Error" } }.should fail

    lambda { should.raise { 1 + 1 } }.should fail
    lambda { should.raise(Interrupt) { raise "Error" } }.should fail
  end

  specify "should.raise should return the exception" do
    ex = lambda { raise "foo!" }.should.raise
    ex.should.be.kind_of RuntimeError
    ex.message.should.match(/foo/)
  end
  
  specify "has should.be.an.instance_of" do
    lambda {
      lambda { "string".should.be_an_instance_of String }.should succeed
    }.should.be deprecated
    lambda {
      lambda { "string".should.be_an_instance_of Hash }.should fail
    }.should.be deprecated

    lambda { "string".should.be.instance_of String }.should succeed
    lambda { "string".should.be.instance_of Hash }.should fail

    lambda { "string".should.be.an.instance_of String }.should succeed
    lambda { "string".should.be.an.instance_of Hash }.should fail
  end

  specify "has should.be.nil" do
    lambda { nil.should.be.nil }.should succeed
    lambda { nil.should.be nil }.should succeed
    lambda { nil.should.be_nil }.should.be deprecated

    lambda { nil.should.not.be.nil }.should fail
    lambda { nil.should.not.be nil }.should fail
    lambda { lambda { nil.should.not.be_nil }.should fail }.should.be deprecated

    lambda { "foo".should.be.nil }.should fail
    lambda { "bar".should.be nil }.should fail

    lambda { "foo".should.not.be.nil }.should succeed
    lambda { "bar".should.not.be nil }.should succeed
  end

  specify "has should.include" do
    lambda { [1,2,3].should.include 2 }.should succeed
    lambda { [1,2,3].should.include 4 }.should fail

    lambda { {1=>2, 3=>4}.should.include 1 }.should succeed
    lambda { {1=>2, 3=>4}.should.include 2 }.should fail
  end

  specify "has should.be.a.kind_of" do
    lambda { Array.should.be.kind_of Module }.should succeed
    lambda { "string".should.be.kind_of Object }.should succeed
    lambda { 1.should.be.kind_of Comparable }.should succeed

    lambda { Array.should.be.a.kind_of Module }.should succeed

    lambda { "string".should.be.a.kind_of Class }.should fail
    lambda {
      lambda { "string".should.be_a_kind_of Class }.should fail
    }.should.be deprecated

    lambda { Array.should.be_a_kind_of Module }.should.be deprecated
    lambda { "string".should.be_a_kind_of Object }.should.be deprecated
    lambda { 1.should.be_a_kind_of Comparable }.should.be deprecated
  end

  specify "has should.match" do
    lambda { "string".should.match(/strin./) }.should succeed
    lambda { "string".should.match("strin") }.should succeed
    lambda { "string".should =~ /strin./ }.should succeed
    lambda { "string".should =~ "strin" }.should succeed

    lambda { "string".should.match(/slin./) }.should fail
    lambda { "string".should.match("slin") }.should fail
    lambda { "string".should =~ /slin./ }.should fail
    lambda { "string".should =~ "slin" }.should fail
  end

  specify "has should.be" do
    thing = "thing"
    lambda { thing.should.be thing }.should succeed
    lambda { thing.should.be "thing" }.should fail

    lambda { 1.should.be(2, 3) }.should.raise(ArgumentError)
  end

  specify "has should.not.raise" do
    lambda { lambda { 1 + 1 }.should.not.raise }.should succeed
    lambda { lambda { 1 + 1 }.should.not.raise(Interrupt) }.should succeed

    lambda {
      begin
        lambda {
          raise ZeroDivisionError.new("ArgumentError")
        }.should.not.raise(RuntimeError, StandardError, Comparable)
      rescue ZeroDivisionError
      end
    }.should succeed

    lambda { lambda { raise "Error" }.should.not.raise }.should fail
  end

  specify "has should.not.satisfy" do
    lambda { should.not.satisfy { 1 == 2 } }.should succeed
    lambda { should.not.satisfy { 1 == 1 } }.should fail
  end

  specify "has should.not.be" do
    thing = "thing"
    lambda { thing.should.not.be "thing" }.should succeed
    lambda { thing.should.not.be thing }.should fail

    lambda { thing.should.not.be thing, thing }.should.raise(ArgumentError)
  end

  specify "has should.not.equal" do
    lambda { "string1".should.not.equal "string2" }.should succeed
    lambda { "string1".should.not.equal "string1" }.should fail
  end

  specify "has should.not.match" do
    lambda { "string".should.not.match(/sling/) }.should succeed
    lambda { "string".should.not.match(/string/) }.should fail
    lambda { "string".should.not.match("strin") }.should fail

    lambda { "string".should.not =~ /sling/ }.should succeed
    lambda { "string".should.not =~ /string/ }.should fail
    lambda { "string".should.not =~ "strin" }.should fail
  end

  specify "has should.throw" do
    lambda { lambda { throw :thing }.should.throw(:thing) }.should succeed

    lambda { lambda { throw :thing2 }.should.throw(:thing) }.should fail
    lambda { lambda { 1 + 1 }.should.throw(:thing) }.should fail
  end

  specify "has should.not.throw" do
    lambda { lambda { 1 + 1 }.should.not.throw }.should succeed
    lambda { lambda { throw :thing }.should.not.throw }.should fail
  end

  specify "has should.respond_to" do
    lambda { "foo".should.respond_to :to_s }.should succeed
    lambda { 5.should.respond_to :to_str }.should fail
    lambda { :foo.should.respond_to :nx }.should fail
  end
  
  specify "has should.be_close" do
    lambda { 1.4.should.be.close 1.4, 0 }.should succeed
    lambda { 0.4.should.be.close 0.5, 0.1 }.should succeed
    
    lambda {
      lambda { 1.4.should.be_close 1.4, 0 }.should succeed
    }.should.be deprecated
    lambda {
      lambda { 0.4.should.be_close 0.5, 0.1 }.should succeed
    }.should.be deprecated

    lambda {
      float_thing = Object.new
      def float_thing.to_f
        0.2
      end
      float_thing.should.be.close 0.1, 0.1
    }.should succeed

    lambda { 0.4.should.be.close 0.5, 0.05 }.should fail
    lambda { 0.4.should.be.close Object.new, 0.1 }.should fail
    lambda { 0.4.should.be.close 0.5, -0.1 }.should fail
  end

  specify "multiple negation works" do
    lambda { 1.should.equal 1 }.should succeed
    lambda { 1.should.not.equal 1 }.should fail
    lambda { 1.should.not.not.equal 1 }.should succeed
    lambda { 1.should.not.not.not.equal 1 }.should fail

    lambda { 1.should.equal 2 }.should fail
    lambda { 1.should.not.equal 2 }.should succeed
    lambda { 1.should.not.not.equal 2 }.should fail
    lambda { 1.should.not.not.not.equal 2 }.should succeed
  end

  specify "has should.<predicate>" do
    lambda { [].should.be.empty }.should succeed
    lambda { [1,2,3].should.not.be.empty }.should succeed

    lambda { [].should.not.be.empty }.should fail
    lambda { [1,2,3].should.be.empty }.should fail

    lambda { {1=>2, 3=>4}.should.has_key 1 }.should succeed
    lambda { {1=>2, 3=>4}.should.not.has_key 2 }.should succeed

    lambda { nil.should.bla }.should.raise(NoMethodError)
    lambda { nil.should.not.bla }.should.raise(NoMethodError)
  end

  specify "has should.<predicate>?" do
    lambda { [].should.be.empty? }.should succeed
    lambda { [1,2,3].should.not.be.empty? }.should succeed

    lambda { [].should.not.be.empty? }.should fail
    lambda { [1,2,3].should.be.empty? }.should fail

    lambda { {1=>2, 3=>4}.should.has_key? 1 }.should succeed
    lambda { {1=>2, 3=>4}.should.not.has_key? 2 }.should succeed

    lambda { nil.should.bla? }.should.raise(NoMethodError)
    lambda { nil.should.not.bla? }.should.raise(NoMethodError)
  end

  specify "has should <operator> (>, >=, <, <=, ===)" do
    lambda { 2.should.be > 1 }.should succeed
    lambda { 1.should.be > 2 }.should fail

    lambda { 1.should.be < 2 }.should succeed
    lambda { 2.should.be < 1 }.should fail

    lambda { 2.should.be >= 1 }.should succeed
    lambda { 2.should.be >= 2 }.should succeed
    lambda { 2.should.be >= 2.1 }.should fail

    lambda { 2.should.be <= 1 }.should fail
    lambda { 2.should.be <= 2 }.should succeed
    lambda { 2.should.be <= 2.1 }.should succeed

    lambda { Array.should === [1,2,3] }.should succeed
    lambda { Integer.should === [1,2,3] }.should fail

    lambda { /foo/.should === "foobar" }.should succeed
    lambda { "foobar".should === /foo/ }.should fail
  end

  $contextscope = self
  specify "is robust against careless users" do
    lambda {
      $contextscope.specify
    }.should.raise(ArgumentError)
    lambda {
      $contextscope.specify "foo"
    }.should.raise(ArgumentError)
    lambda {
      $contextscope.xspecify
    }.should.raise(ArgumentError)
    lambda {
      $contextscope.xspecify "foo"
    }.should.not.raise(ArgumentError)    # allow empty xspecifys
    lambda {
      Kernel.send(:context, "foo")
    }.should.raise(ArgumentError)
    lambda {
      context "foo" do
      end
    }.should.raise(Test::Spec::DefinitionError)
  end

  specify "should detect warnings" do
    lambda { lambda { 0 }.should._warn }.should fail
    lambda { lambda { warn "just a test" }.should._warn }.should succeed
  end

  specify "should message/blame faults" do
    begin
      2.should.blaming("Two is not two anymore!").equal 3
    rescue Test::Unit::AssertionFailedError => e
      e.message.should =~ /Two/
    end

    begin
      2.should.messaging("Two is not two anymore!").equal 3
    rescue Test::Unit::AssertionFailedError => e
      e.message.should =~ /Two/
    end

    begin
      2.should.blaming("I thought two was three").not.equal 2
    rescue Test::Unit::AssertionFailedError => e
      e.message.should =~ /three/
    end

    begin
      2.should.blaming("I thought two was three").not.not.not.equal 3
    rescue Test::Unit::AssertionFailedError => e
      e.message.should =~ /three/
    end

    lambda {
      lambda { raise "Error" }.should.messaging("Duh.").not.raise
    }.should.raise(Test::Unit::AssertionFailedError).message.should =~ /Duh/
  end

  include TestShoulds
  specify "should allow for custom shoulds" do
    lambda { (1+1).should equal_string("2") }.should succeed
    lambda { (1+2).should equal_string("2") }.should fail

    lambda { (1+1).should.pass equal_string("2") }.should succeed
    lambda { (1+2).should.pass equal_string("2") }.should fail

    lambda { (1+1).should.be equal_string("2") }.should succeed
    lambda { (1+2).should.be equal_string("2") }.should fail

    lambda { (1+1).should.not equal_string("2") }.should fail
    lambda { (1+2).should.not equal_string("2") }.should succeed
    lambda { (1+2).should.not.not equal_string("2") }.should fail

    lambda { (1+1).should.not.pass equal_string("2") }.should fail
    lambda { (1+2).should.not.pass equal_string("2") }.should succeed

    lambda { (1+1).should.not.be equal_string("2") }.should fail
    lambda { (1+2).should.not.be equal_string("2") }.should succeed

    lambda {
      (1+2).should equal_string("2")
    }.should.raise(Test::Unit::AssertionFailedError).message.should =~ /EqualString/

    lambda { (1+1).should equal_string2("2") }.should succeed
    lambda { (1+2).should equal_string2("2") }.should fail

    lambda {
      (1+2).should equal_string2("2")
    }.should.raise(Test::Unit::AssertionFailedError).message.should =~ /yada/

    lambda {
      (1+2).should.blaming("foo").pass equal_string2("2")
    }.should.raise(Test::Unit::AssertionFailedError).message.should =~ /foo/

    lambda {
      nil.should empty_should(nil)
    }.should.raise(NotImplementedError)

    lambda { nil.should :break, :now }.should.raise(ArgumentError)
    lambda { nil.should.not :pass, :now }.should.raise(ArgumentError)
    lambda { nil.should.not.not :break, :now }.should.raise(ArgumentError)
  end

  xspecify "disabled specification" do
    # just trying
  end

  xspecify "empty specification"

  context "more disabled" do
    xspecify "this is intentional" do
      # ...
    end

    specify "an empty specification" do
      # ...
    end

    xcontext "even more disabled" do
      specify "we can cut out" do
        # ...
      end

      specify "entire contexts, now" do
        # ...
      end
    end
  end
end

context "setup/teardown" do
  setup do
    @a = 1
    @b = 2
  end

  setup do
    @a = 2
  end

  teardown do
    @a.should.equal 2
    @a = 3
  end

  teardown do
    @a.should.equal 3
  end
  
  specify "run in the right order" do
    @a.should.equal 2
    @b.should.equal 2
  end
end

context "before all" do
  before(:all) { @a = 1 }

  specify "runs parent before all" do
    @a.should == 1
  end
end

context "nested teardown" do
  context "nested" do
    specify "should call local teardown then parent teardown" do
      @a = 3
    end

    teardown do
      @a = 2
    end
  end

  teardown do
    @a.should.equal 2
    @a = 1
  end

  after(:all) do
    @a.should.equal 1
  end
end

context "before all" do
  context "nested" do
    before(:all) do
      @a = 2
    end

    specify "should call parent then local" do
      @a.should.equal 2
      @b.should.equal 2
    end
  end

  before(:all) do
    @a = 1
    @b = 2
  end
end

context "after all" do
  context "after nested" do
    after(:all) do
      @a = 2
    end

    specify "should call local then parent" do
      self.after_all
      @a.should.equal 1
      @b.should.equal 2
    end
  end

  after(:all) do
    @b = @a 
    @a = 1
  end
end


module ContextHelper
  def foo
    42
  end
end

context "contexts" do
  include ContextHelper

  FOO = 42
  $class = self.class

  specify "are defined in class scope" do
    lambda { FOO }.should.not.raise(NameError)
    FOO.should.equal 42
    $class.should.equal Class
  end

  specify "can include modules" do
    lambda { foo }.should.not.raise(NameError)
    foo.should.equal 42
  end
end

class CustomTestUnitSubclass < Test::Unit::TestCase
  def test_truth
    assert true
  end
end

context "contexts with subclasses", CustomTestUnitSubclass do
  specify "use the supplied class as the superclass" do
    self.should.be.a.kind_of CustomTestUnitSubclass
  end
end

xcontext "xcontexts with subclasses", CustomTestUnitSubclass do
  specify "work great!" do
    self.should.be.a.kind_of CustomTestUnitSubclass
  end
end

shared_context "a shared context" do
  specify "can be included several times" do
    true.should.be true
  end

  behaves_like "yet another shared context"
end

shared_context "another shared context" do
  specify "can access data" do
    @answer.should.be 42
  end
end

shared_context "yet another shared context" do
  specify "can include other shared contexts" do
    true.should.be true
  end
end

context "Shared contexts" do
  shared_context "a nested context" do
    specify "can be nested" do
      true.should.be true
    end
  end

  setup do
    @answer = 42
  end

  behaves_like "a shared context"
  it_should_behave_like "a shared context"

  behaves_like "a nested context"

  behaves_like "another shared context"

  ctx = self
  specify "should raise when the context cannot be found" do
    should.raise(NameError) {
      ctx.behaves_like "no such context"
    }
  end
end
