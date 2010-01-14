require File.dirname(__FILE__) + "/../spec_helper"

describe "nil.GetType()" do
  it 'returns Object (like type inference does)' do
    nil.GetType.ToString.should == 'Microsoft.Scripting.Runtime.DynamicNull'
  end
end

describe "nil.ToString" do
  it "returns 'nil'" do
    nil.ToString.should equal_clr_string('nil')
  end
end

describe "nil.GetHashCode" do
  it "returns 0" do
    nil.GetHashCode.should == 0
  end
end

