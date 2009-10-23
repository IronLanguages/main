describe :enumerable_entries, :shared => true do
  before :each do
    @entries = [2, 4, 6, 8, 10]
    @numerous = EnumerableSpecs::Numerous.new(*@entries)
  end
  
  it "returns an array containing the items in enum." do
    @numerous.send(@method).should == @entries
  end

  it "returns an array containing the elements" do
    numerous = EnumerableSpecs::Numerous.new(1, nil, 'a', 2, false, true)
    numerous.send(@method).should == [1, nil, "a", 2, false, true]
  end
  
  ruby_version_is '1.8.7' do
    it "passes arguments to each" do
      count = EnumerableSpecs::EachCounter.new(1, 2, 3)
      count.to_a(:hello, "world").should == [1, 2, 3]
      count.arguments_passed.should == [:hello, "world"]
    end
  end
  
  it "calls each" do
    class EnumTest2
      include Enumerable
      attr_reader :each_called
      def initialize
        @each_called = false
      end
      def each
        @each_called = true
        yield 123
      end
    end
    
    e = EnumTest2.new
    e.to_a.should == [123]
    e.each_called.should == true
  end
end
