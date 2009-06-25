require File.dirname(__FILE__) + '/../../spec_helper'

describe :acts_as_icomparable, :shared => true do
  it "acts as an IComparable" do
    IComparableConsumer.consume(@obj).should == 1
  end
end

describe "Comparable maps to IComparable" do
  csc <<-EOL
  using System;
  EOL
  csc <<-EOL
  public class IComparableConsumer {
    public static int Consume(IComparable icomp) {
      return icomp.CompareTo(1);
    }
  }
  
  public class IComparableProvider {
  
  }
  EOL
  before(:each) do
    class IComparableProvider
      def <=>(val)
        val <=> 1
      end
      include Comparable
    end
    @obj = IComparableProvider.new
  end
  it_behaves_like :acts_as_icomparable, @obj
end

describe "Comparable mapping to IComparable for metaclasses" do
  before(:each) do
    @obj = Klass.new
    @obj.metaclass_eval do
      def <=>(val)
        val <=> 1
      end
      include Comparable
    end
  end
  it_behaves_like :acts_as_icomparable, @obj
end