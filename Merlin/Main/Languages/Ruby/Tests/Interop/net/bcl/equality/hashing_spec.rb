require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + '/../fixtures/classes'
describe "Hashing" do
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

  it "returns a reference based hash code of an object returned from Object#hash if it is not a Fixnum or Bignum" do
    hashResult = HashingSpecs::ToIntClass.new
    o = HashingSpecs::RubyClassWithHash.new(hashResult)
    Hasher.get_hash_code(o).should == System::Runtime::CompilerServices::RuntimeHelpers.get_hash_code(hashResult)
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
