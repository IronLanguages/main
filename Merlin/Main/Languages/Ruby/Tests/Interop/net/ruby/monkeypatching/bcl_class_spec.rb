require File.dirname(__FILE__) + "/../../spec_helper"

describe "Monkey-patching CLR types" do
  before(:all) do
    class System::Collections::ArrayList
      def total
        inject(0) { |accum, i| accum + i }
      end
    end
  end

  after(:all) do
    class System::Collections::ArrayList
      undef :total
    end
  end

  before(:each) do
    @list = System::Collections::ArrayList.new
    @list.add 3
    @list << 2 << 1
  end

  it "is allowed" do
    @list.total.should == 6
  end
end
