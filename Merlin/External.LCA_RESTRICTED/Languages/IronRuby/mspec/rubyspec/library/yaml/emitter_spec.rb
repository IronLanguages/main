require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "YAML.emitter" do
  it "has a default level of 0" do
    YAML.emitter.level.should == 0
  end
end
