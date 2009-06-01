require File.dirname(__FILE__) + "/../../spec_helper"

describe "Equality" do
  before :all do
    # Ideally, we would just use mocks. However, we cant use mocks since MockObject does not deal with should_receive(:hash)
    module EqualitySpecs
      class RubyClassWithEql
        def initialize(result=nil) @result = result end
        def ==(other) if @result then @result else other == :ruby_marker end end
      end

      class RubyClassWithoutEql
        def ==(other) raise "== should not be called" end
      end

      class RubyDerivedClass < EmptyClass
        def ==(other) other == :ruby_marker end
      end

      class RubyClassWithEqlAndEquals
        def ==(other) other == :ruby_marker end
        def Equals(other) other == :clr_marker end
      end
      
      # Uncommenting the base type declartion causes an assert since Equatable has a static method as well as instance method
      # called "Equals", and IronRuby does not deal with this. Since the example will not work anyway until NewTypeMaker is fixed,
      # we just comment out the base type to prevent a blocking assert from popping up
      class EqualityCheckerSubtype  < Equatable
      end

      class EqualityCheckerSubtypeWithEql  < Equatable
        def ==(other) other == :ruby_marker end
      end
    end
  end

  csc <<-EOL
    public static class EqualityChecker {
      public static new bool Equals(object o1, object o2) { return o1.Equals(o2); }
    }
    
    public class Equatable {
      public override bool Equals(object other) { return (other is string) && ((string)other) == "ClrMarker"; }
      public override int GetHashCode() { throw new NotImplementedException(); }
    }
  EOL
  
  it "maps Object#== to System.Object.Equals for Ruby classes" do
    o = EqualitySpecs::RubyClassWithEql.new
    EqualityChecker.equals(o, :ruby_marker).should be_true
  end

  it "maps Object#== to System.Object.Equals for Ruby classes that derive from CLR types" do    
    o = EqualitySpecs::RubyDerivedClass.new
    EqualityChecker.equals(o, :ruby_marker).should be_true
  end

  it "allows Object#== to return any type" do
    EqualityChecker.equals(EqualitySpecs::RubyClassWithEql.new("hello"), 123).should be_true
    EqualityChecker.equals(EqualitySpecs::RubyClassWithEql.new(321), 123).should be_true

    EqualityChecker.equals(EqualitySpecs::RubyClassWithEql.new(nil), 123).should be_false
    EqualityChecker.equals(EqualitySpecs::RubyClassWithEql.new(false), 123).should be_false
  end

  it "uses reference equality for Array" do
    o = EqualitySpecs::RubyClassWithoutEql.new
    a = [o]
    EqualityChecker.equals(a, [o]).should be_false
  end

  it "uses reference equality for Hash" do
    o = EqualitySpecs::RubyClassWithEql.new
    h1 = { o => o }
    h2 = { o => o }
    class << o
      def ==() raise "== should not be called" end
    end
    EqualityChecker.equals(h1, h2).should be_false
  end

  it "maps System.Object.Equals to Object#== for CLR objects" do
    o = EmptyClass.new
    o2 = EmptyClass.new
    (o == o).should == EqualityChecker.equals(o, o)
    (o == nil).should == EqualityChecker.equals(o, nil)
    (o == o2).should == EqualityChecker.equals(o, o2)
  end

  it "maps System.Object.Equals to Object#== for Ruby sub-classes" do
    EqualityChecker.equals(EqualitySpecs::EqualityCheckerSubtype.new, "ClrMarker".to_clr_string).should be_true
    (EqualitySpecs::EqualityCheckerSubtype.new == "ClrMarker".to_clr_string).should be_true
  end
  
  it "maps System.Object.Equals to Object#== for Ruby sub-classes with #==" do
    EqualityChecker.equals(EqualitySpecs::EqualityCheckerSubtypeWithEql.new, "ClrMarker".to_clr_string).should be_false
    EqualityChecker.equals(EqualitySpecs::EqualityCheckerSubtypeWithEql.new, :ruby_marker).should be_true
  end
  
  it "does not map System.Object.Equals to Object#== for monkey-patched CLR objects" do
    o = EmptyClass.new
    class << o
      def ==(other)
        flunk
      end
    end
    EqualityChecker.equals(o, EmptyClass.new).should be_false
    
    o = Object.new
    class << o
      def ==(other)
        flunk
      end
    end
    EqualityChecker.equals(o, Object.new).should be_false
  end
  
  it "allows both Object#== and System.Object.Equals to be overriden separately" do
    o = EqualitySpecs::RubyClassWithEqlAndEquals.new
    (o == :ruby_marker).should be_true
    EqualityChecker.equals(o, :clr_marker).should be_true
  end
end
