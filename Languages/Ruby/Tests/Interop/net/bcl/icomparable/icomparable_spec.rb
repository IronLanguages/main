require File.dirname(__FILE__) + '/../../spec_helper'

describe "IComparable maps to Comparable" do
  before(:each) do
    @comparable1 = System::Version.new(1,0,0,0)
    @comparable2 = System::Version.new(2,0,0,0)
    @comparable3 = System::Version.new(3,0,0,0)
  end
  
  it "with the spaceship operator" do
    (@comparable1 <=> @comparable1).should == 0
    (@comparable1 <=> @comparable2).should == -1
  end
  
  it "with Comparable in the ancestor list" do
    System::Version.ancestors.should include(Comparable)
  end
  
  it "with the builtin Comparable methods" do
    (@comparable1 < @comparable2).should == true
    @comparable2.between?(@comparable1, @comparable3).should == true
  end
end