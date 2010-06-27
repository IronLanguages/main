require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../fixtures/common'

describe "YAML::Omap#has_key?" do
  before :each do
    @omap = YAML::Omap.new
    @omap[:key] = "fixture value"
  end
  
  it "returns true for existing key" do
    @omap.has_key?(:key).should be_true
  end

  it "returns false for non-existing key" do
    @omap.has_key?(:non_existent_key).should be_false
  end
end