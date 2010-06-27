csc <<-EOL
using System;
using System.Collections;
EOL
csc <<-EOL
  public partial class Klass {
    public T[] ArrayAcceptingMethod<T>(T[] arg0) {
      return arg0;
    }
    public decimal MyDecimal {get; set;}
    public string A(){
      return "a";
    }

    public string Aa(){
      return "aa";
    }
  }

  public static class EqualityChecker {
    public static new bool Equals(object o1, object o2) { return o1.Equals(o2); }
  }
  
  public class Equatable {
    public override bool Equals(object other) { return (other is string) && ((string)other) == "ClrMarker"; }
    public override int GetHashCode() { throw new NotImplementedException(); }
  }

  public static class Hasher {
    public static int GetHashCode(object o) { return o.GetHashCode(); }
  }
  
  public class Hashable {
    public override int GetHashCode() { return 123; }
  }

  public class IComparableConsumer {
    public static int Consume(IComparable icomp) {
      return icomp.CompareTo(1);
    }
  }

  public class IComparableProvider {

  }

  public interface ITestList : IEnumerable {
  }

  public class Tester {
    public ArrayList Test(ITestList list) {
      ArrayList l = new ArrayList();
      foreach(var item in list) {
        l.Add(item);
      }
      return l;
    }
  }

  public partial class NumericHelper {
    public static int SizeOfByte() {
      return sizeof(Byte);
    }
    public static int SizeOfInt16() {
      return sizeof(Int16);
    }
    public static int SizeOfInt32() {
      return sizeof(Int32);
    }
    public static int SizeOfInt64() {
      return sizeof(Int64);
    }
    public static int SizeOfSByte() {
      return sizeof(SByte);
    }
    public static int SizeOfUInt16() {
      return sizeof(UInt16);
    }
    public static int SizeOfUInt32() {
      return sizeof(UInt32);
    }
    public static int SizeOfUInt64() {
      return sizeof(UInt64);
    }
    public static int SizeOfDecimal() {
      return sizeof(Decimal);
    }
  }
  namespace RegressionSpecs {
    public class B { }
    public class C : B { }
    public interface I1 { int f(); }
    public interface I2 { int g(); }
  }
EOL

no_csc do
  module EqualitySpecs
    class RubyClassWithEql
      def initialize(result=nil) @result = result end
      def eql?(other) if @result then @result else other == :ruby_marker end end
    end

    class RubyClassWithoutEql
      def eql?(other) raise "eql? should not be called" end
    end

    class RubyDerivedClass < EmptyClass
      def eql?(other) other == :ruby_marker end
    end

    class RubyClassWithEqlAndEquals
      def eql?(other) other == :ruby_marker end
      def Equals(other) other == :clr_marker end
    end
    
    class EqualityCheckerSubtype  < Equatable
    end

    class EqualityCheckerSubtypeWithEql  < Equatable
      def eql?(other) other == :ruby_marker end
    end
  end

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
  class IComparableProvider
    def <=>(val)
      val <=> 1
    end
    include Comparable
  end
  class TestListFromArray < Array
    include ITestList

    def get_enumerator
      clr_member(:get_enumerator).call
    end
  end

  class TestList
    include ITestList
    def initialize
      @store = []
    end

    def get_enumerator
      TestListEnumerator.new(@store)
    end

    def <<(val)
      @store << val
      self
    end

    class TestListEnumerator
      include System::Collections::IEnumerator
      attr_reader :list
      def initialize(list)
        @list = list
        @position = -1
      end

      def move_next
        @position += 1
        valid?
      end

      def reset
        @position = -1
      end

      def valid?
        @position != -1 && @position < @list.length
      end

      def current
        if valid?
          @list[@position]
        else
          raise System::InvalidOperationException.new
        end
      end
    end
  end

  class TestListMissing < Array
    include ITestList
  end
  
  class NumericHelper
    def self.max_of(klass)
      klass.MaxValue + 0
    end

    def self.min_of(klass)
      klass.MinValue + 0
    end
  end
end
