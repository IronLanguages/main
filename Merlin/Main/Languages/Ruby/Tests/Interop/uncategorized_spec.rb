require File.dirname(__FILE__) + "/spec_helper"

describe "Regression dev tests" do

end
  
#------------------------------------------------------------------------------

describe "Equality" do
  before :all do
    # Ideally, we would just use mocks. However, we cant use mocks since MockObject does not deal with should_receive(:hash)
    module UncategorizedSpecs
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
      class EqualityCheckerSubtype # < Equatable
      end

      class EqualityCheckerSubtypeWithEql # < Equatable
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
    o = UncategorizedSpecs::RubyClassWithEql.new
    EqualityChecker.equals(o, :ruby_marker).should be_true
  end

  it "maps Object#== to System.Object.Equals for Ruby classes that derive from CLR types" do    
    o = UncategorizedSpecs::RubyDerivedClass.new
    EqualityChecker.equals(o, :ruby_marker).should be_true
  end

  it "allows Object#== to return any type" do
    EqualityChecker.equals(UncategorizedSpecs::RubyClassWithEql.new("hello"), 123).should be_true
    EqualityChecker.equals(UncategorizedSpecs::RubyClassWithEql.new(321), 123).should be_true

    EqualityChecker.equals(UncategorizedSpecs::RubyClassWithEql.new(nil), 123).should be_false
    EqualityChecker.equals(UncategorizedSpecs::RubyClassWithEql.new(false), 123).should be_false
  end

  it "uses reference equality for Array" do
    o = UncategorizedSpecs::RubyClassWithoutEql.new
    a = [o]
    EqualityChecker.equals(a, [o]).should be_false
  end

  it "uses reference equality for Hash" do
    o = UncategorizedSpecs::RubyClassWithEql.new
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
    EqualityChecker.equals(UncategorizedSpecs::EqualityCheckerSubtype.new, "ClrMarker".to_clr_string).should be_true
    (UncategorizedSpecs::EqualityCheckerSubtype.new == "ClrMarker".to_clr_string).should be_true
  end
  
  it "maps System.Object.Equals to Object#== for Ruby sub-classes with #==" do
    EqualityChecker.equals(UncategorizedSpecs::EqualityCheckerSubtypeWithEql.new, "ClrMarker".to_clr_string).should be_false
    EqualityChecker.equals(UncategorizedSpecs::EqualityCheckerSubtypeWithEql.new, :ruby_marker).should be_true
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
    o = UncategorizedSpecs::RubyClassWithEqlAndEquals.new
    (o == :ruby_marker).should be_true
    EqualityChecker.equals(o, :clr_marker).should be_true
  end
end

#------------------------------------------------------------------------------

describe "Hashing" do
  before :all do
    # Ideally, we would just use mocks. However, we cant use mocks since MockObject does not deal with should_receive(:hash)
    module UncategorizedSpecs
      class RubyClassWithHash
        def initialize(h=123) @h = h end
        def hash() @h end
      end

      class RubyClassWithoutHash
        def hash() raise "hash should not be called" end
      end

      class RubyDerivedClass < EmptyClass
        def hash() 123 end
      end

      class ToIntClass
        def to_int() 123 end
      end

      class RubyClassWithHashAndGetHashCode
        def hash() 1 end
        def GetHashCode() 2 end
      end
      
      class HashableSubtype < Hashable
      end

      class HashableSubtypeWithHash < Hashable
        def hash() 234 end
      end
    end
  end

  csc <<-EOL
    public static class Hasher {
      public static int GetHashCode(object o) { return o.GetHashCode(); }
    }
    
    public class Hashable {
      public override int GetHashCode() { return 123; }
    }
  EOL
  
  it "maps Object#hash to System.Object.GetHashCode for Ruby classes" do
    o = UncategorizedSpecs::RubyClassWithHash.new
    Hasher.get_hash_code(o).class.should == Fixnum
  end

  it "maps Object#hash to System.Object.GetHashCode for Ruby classes that derive from CLR types" do    
    o = UncategorizedSpecs::RubyDerivedClass.new
    Hasher.get_hash_code(o).should == 123
  end

  it "allows Object#hash to return a Bignum" do
    o = UncategorizedSpecs::RubyClassWithHash.new(bignum_value(123))
    Hasher.get_hash_code(o).class.should == Fixnum
  end

  it "requires Object#hash to return an Integer" do
    o = UncategorizedSpecs::RubyClassWithHash.new(UncategorizedSpecs::ToIntClass.new)
    lambda { Hasher.get_hash_code(o) }.should raise_error(TypeError)
  end

  it "uses reference hashing for Array" do
    o = UncategorizedSpecs::RubyClassWithHash.new
    a = [o]
    Hasher.get_hash_code(a).should == Hasher.get_hash_code(a << "some object")
  end

  it "uses reference hashing for Hash" do
    o = UncategorizedSpecs::RubyClassWithHash.new
    h = { o => o }
    class << o
      def hash() raise "hash should not be called" end
    end
    Hasher.get_hash_code(h).should == Hasher.get_hash_code(h.clear)
  end

  it "maps System.Object.GetHashCode to Object#hash for CLR objects" do
    o = EmptyClass.new
    o.hash.should == Hasher.get_hash_code(o)
  end

  it "maps System.Object.GetHashCode to Object#hash for Ruby sub-classes" do
    Hasher.get_hash_code(UncategorizedSpecs::HashableSubtype.new).should == 123
    UncategorizedSpecs::HashableSubtype.new.hash.should == 123
  end
  
  it "maps System.Object.GetHashCode to Object#hash for Ruby sub-classes with #hash" do
    Hasher.get_hash_code(UncategorizedSpecs::HashableSubtypeWithHash.new).should == 234
  end
  
  it "does not map System.Object.GetHashCode to Object#hash for monkey-patched CLR objects" do
    o = EmptyClass.new
    class << o
      def hash
        super + 1
      end
    end
    o.hash.should_not == Hasher.get_hash_code(o)
    
    o = Object.new
    class << o
      def hash
        super + 1
      end
    end
    o.hash.should_not == Hasher.get_hash_code(o)
  end
  
  it "allows both Object#hash and System.Object.GetHashCode to be overriden separately" do
    o = UncategorizedSpecs::RubyClassWithHashAndGetHashCode.new
    o.hash.should == 1
    Hasher.get_hash_code(o).should == 2
  end
end

describe "ObjectSpace.each_object" do
  it "raises NotSupportedException for non-Class classes" do
    lambda { ObjectSpace.each_object(String) {} }.should raise_error(RuntimeError)
  end

  it "works for Module" do
    modules = []
    ObjectSpace.each_object(Module) { |o| modules << o }
    modules.size.should > 90
    modules.each { |m| m.should be_kind_of(Module) }
  end

  it "works for Class" do
    classes = []
    ObjectSpace.each_object(Class) { |o| classes << o }
    classes.size.should > 70
    classes.each { |c| c.should be_kind_of(Class) }
  end

  it "works for singleton Class" do
    klass = class << Class; self; end
    ObjectSpace.each_object(klass) {}.should == 0
  end
end