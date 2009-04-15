require File.dirname(__FILE__) + '/../../spec_helper'

describe "ILists" do
  before(:each) do
    @list1 = System::Collections::ArrayList.new
    [1,3,2,3].each { |e| @list1.add e }
    @list2 = System::Collections::ArrayList.new
    [5,4,3,4,6].each { |e| @list2.add e }
  end
  
  it "equate to arrays" do
    @list1.should == [1,3,2,3]
    @list2.should == [5,4,3,4,6]
  end
  
  it "allow array operations" do
    (@list1 | @list2).should == [1,3,2,5,4,6]
  end
end