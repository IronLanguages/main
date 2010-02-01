require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "YAML.quick_emit" do
  before(:each) do
    ScratchPad.clear
  end

  it "returns a yaml string" do
    y = YAML.quick_emit(nil, {}) do |out| 
      YamlSpecs.write_to_emitter(out, {"greeting" => "Hello"})
    end
    y.should == "--- \ngreeting: Hello\n"
  end

  it "returns a BaseNode node if emitter level is non-zero" do
    ScratchPad.record({})
    YamlSpecs::OuterToYaml.new.to_yaml
    ScratchPad.recorded[:result].should be_kind_of(YAML::BaseNode)
  end
  
  it "accepts Emitter argument" do
    YAML.quick_emit(nil, YAML.emitter) { ScratchPad.record :in_block; YamlSpecs.get_a_node }
    ScratchPad.recorded.should == :in_block
  end
  
  it "requires block to return a BaseNode" do    
    y = YAML.quick_emit(nil, {}) { YamlSpecs.get_a_node }
    y.should == ("--- \n" + $test_yaml_string)
  end

  it "requires opts parameter to be a Hash or Emitter" do
    lambda { YAML.quick_emit(nil, nil) }.should raise_error(Exception)
  end

  it "requires a block" do
    lambda { YAML.quick_emit(nil, {}) }.should raise_error(Exception)
  end
end
