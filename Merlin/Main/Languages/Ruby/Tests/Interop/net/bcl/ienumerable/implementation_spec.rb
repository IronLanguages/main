require File.dirname(__FILE__) + '/../../spec_helper'

describe "Implementing IEnumerable from IronRuby" do
  csc <<-EOL
  using System.Collections;
  EOL
  csc <<-EOL
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
  EOL

  before(:all) do
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
      end

      class TestListEnumerator
        include System::Collections::IEnumerator
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
  end

  it "uses the enumerator method defined" do
    list = TestList.new
    list << "a"
    Tester.new.test(list).should include "a"
  end

  it "uses the enumerator method defined in Array (via clr_member)" do
    list_a = TestListFromArray.new
    list_a << "b"
    Tester.new.test(list_a).should include "b"
  end

  it "doesn't use Array's get_enumerator" do
    list = TestListMissing.new
    list << "c"
    lambda {Tester.new.test(list)}.should raise_error NoMethodError
  end

end
