require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "YAML.tagurize" do
  it "converts a type_id to a taguri" do
    YAML.tagurize('abc').should == "tag:yaml.org,2002:abc"
    YAML.tagurize(1).should == 1
  end
  
  it "returns nil for nil" do
    YAML.tagurize(nil).should == nil
  end  
end
