require File.dirname(__FILE__) + "/../../spec_helper"

describe "Hashing" do
  before :all do
    # Ideally, we would just use mocks. However, we cant use mocks since MockObject does not deal with should_receive(:hash)
    module HashingSpecs
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
    o = HashingSpecs::RubyClassWithHash.new
    Hasher.get_hash_code(o).class.should == Fixnum
  end

  it "maps Object#hash to System.Object.GetHashCode for Ruby classes that derive from CLR types" do    
    o = HashingSpecs::RubyDerivedClass.new
    Hasher.get_hash_code(o).should == 123
  end

  it "allows Object#hash to return a Bignum" do
    o = HashingSpecs::RubyClassWithHash.new(bignum_value(123))
    Hasher.get_hash_code(o).class.should == Fixnum
  end

  it "requires Object#hash to return an Integer" do
    o = HashingSpecs::RubyClassWithHash.new(HashingSpecs::ToIntClass.new)
    lambda { Hasher.get_hash_code(o) }.should raise_error(TypeError)
  end

  it "uses reference hashing for Array" do
    o = HashingSpecs::RubyClassWithHash.new
    a = [o]
    Hasher.get_hash_code(a).should == Hasher.get_hash_code(a << "some object")
  end

  it "uses reference hashing for Hash" do
    o = HashingSpecs::RubyClassWithHash.new
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
    Hasher.get_hash_code(HashingSpecs::HashableSubtype.new).should == 123
    HashingSpecs::HashableSubtype.new.hash.should == 123
  end
  
  it "maps System.Object.GetHashCode to Object#hash for Ruby sub-classes with #hash" do
    Hasher.get_hash_code(HashingSpecs::HashableSubtypeWithHash.new).should == 234
  end
  
  it "does not map System.Object.GetHashCode to Object#hash for monkey-patched CLR objects" do
    # Virtual methods cannot be overriden via monkey-patching. Therefore the GetHashCode virtual call is routed to the default implementation on System.Object.
    o = EmptyClass.new
    class << o
      def hash
        super + 1
      end
    end
    o.hash.should_not == Hasher.get_hash_code(o)
  end
    
  it "maps System.Object.GetHashCode to Object#hash for monkey-patched CLR objects" do
    # Object.new returns RubyObject whose GetHashCode is overridden to call hash dynamically.
    o = Object.new
    class << o
      def hash
        super + 1
      end
    end
    o.hash.should == Hasher.get_hash_code(o)
  end
  
  it "allows both Object#hash and System.Object.GetHashCode to be overriden separately" do
    o = HashingSpecs::RubyClassWithHashAndGetHashCode.new
    o.hash.should == 1
    Hasher.get_hash_code(o).should == 2
  end
end
