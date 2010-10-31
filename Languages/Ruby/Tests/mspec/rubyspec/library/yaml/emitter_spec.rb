require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "YAML.emitter" do
  it "has a default level of 0" do
    YAML.emitter.level.should == 0
  end
  
  it "increases level as it steps into containes" do
    levels = []
    
    obj = Class.new do
      define_method :to_yaml do |emitter|
        levels << emitter.level
        nil.to_yaml(emitter)
      end
    end.new
    
    [obj].to_yaml
    [{:a => obj}].to_yaml
    [[[obj]]].to_yaml
    
    levels.should == [1,2,3]
  end
end
