require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "YAML.emitter" do
  it "is of type Emitter" do
    (YAML.emitter.methods - Object.new.methods).sort.should == YamlSpecs::EmitterMethods
    not_compliant_on :ironruby do
      YAML.emitter.class.should == YAML::Syck::Emitter
    end
  end
  
  it "has a default level of 0" do
    YAML.emitter.level.should == 0
  end
end