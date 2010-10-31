require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../fixtures/common'

describe "YAML::Omap#[]" do
  before :each do
    @omap = YAML::Omap.new
    @omap[:key] = "fixture value"
  end
  
  it "returns array with the key-value pairs" do
    result = []
    @omap.each { |i| result << i }
    result.should == [[:key, "fixture value"]]
  end
end