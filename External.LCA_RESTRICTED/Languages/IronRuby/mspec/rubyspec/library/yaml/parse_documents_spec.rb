require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "YAML#parse_documents" do
  it "with an empty string returns false" do
    YAML.parse_documents('').should == nil
  end  
end