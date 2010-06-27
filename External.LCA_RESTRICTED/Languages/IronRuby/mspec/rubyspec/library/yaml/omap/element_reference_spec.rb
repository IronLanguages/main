require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../fixtures/common'

describe "YAML::Omap#[]" do
  before :each do
    @omap = YAML::Omap.new
    @omap[:key] = "fixture value"
  end
  
  it "allows indexing by key" do
    @omap[:key].should == "fixture value"
  end

  it "returns nil for non-existent key" do
    @omap[:non_existent_key].should be_nil
  end
end