require File.dirname(__FILE__) + '/../../spec_helper'

describe "IEnumerable maps to Enumerable" do
  before(:each) do
    @enumerable = System::Collections::ArrayList.new
    @enumerable.add 1
    @enumerable.add 2
    @enumerable.add 3
  end
  
  it "with Enumerable in the ancesotor list" do
    System::Collections::ArrayList.ancestors.should include(Enumerable)
  end
  
  it "with the each method" do
    b = []
    @enumerable.each { |i| b << i }
    b.should == [1,2,3]
  end
  
  it "with the builtin Enumerable methods" do
    @enumerable.map { |i| i + 3 }.should == [4,5,6]
    @enumerable.select { |i| i % 2 == 0 }.should == [2]
  end
end