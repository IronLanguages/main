require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../fixtures/common'

describe "YAML::Omap#[]=" do
  before :each do
    @omap = YAML::Omap.new
    @omap[:key] = "fixture value"
  end
  
  it "works" do
    @omap.size.should == 1
  end

  it "returns false for non-existing key" do
    @omap.has_key?(:non_existent_key).should be_false
  end
  
  it "allows nil key" do
    @omap[nil] = 1
    @omap[nil].should == 1
  end
end