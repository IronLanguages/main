require 'enumerator'

describe :enum_for, :shared => true do

  it "creates a new custom enumerator with the given iterator and arguments" do
    enum = 1.send(@method, :upto, 3)
    enum.kind_of?(Enumerable::Enumerator).should == true
  end

  it "creates a new custom enumerator that responds to #each" do
    enum = 1.send(@method, :upto, 3)
    enum.respond_to?(:each).should == true
  end

  it "creates a new custom enumerator that runs correctly" do
    enum = 1.send(@method, :upto, 3)
    enum.map{|x|x}.should == [1,2,3]
  end

  it "accepts iterator name as string" do
    1.send(@method, "upto", 3).map{|x|x}.should == [1,2,3]
  end

  it "accepts iterator name as string-like object" do
    m = mock("name")
    m.should_receive(:to_str).and_return("upto")
    1.send(@method, m, 3).map{|x|x}.should == [1,2,3]
  end

  it "requires iterator name to be string, string-like or symbol" do
    m = mock("name")
    m.should_not_receive(:to_sym)
    lambda { 1.send(@method, m, 3).map{|x|x} }.should raise_error(TypeError)
  end

end
