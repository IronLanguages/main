require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "YAML.quick_emit" do
  it "returns a yaml string" do
    y = YAML.quick_emit(nil, {}) do |out| 
      YamlSpecs.write_to_emitter(out, {"greeting" => "Hello"})
    end
    y.should == "--- \ngreeting: Hello\n"
  end

  it "returns a BaseNode node if emitter level is non-zero" do
    ScratchPad.record Hash.new
    YamlSpecs::OuterToYaml.new.to_yaml
    ScratchPad.recorded[:result].should be_kind_of(YAML::BaseNode)
  end
  
  it "passes Out to block" do
    out = nil
    YAML.quick_emit(nil, {}) { |o| out = o; YamlSpecs.write_to_emitter(o, {"greeting" => "Hello"}) }

    (out.methods - Object.new.methods).sort.should == YamlSpecs::OutMethods
    not_compliant_on :ironruby do
      out.should be_kind_of(YAML::Syck::Out)
    end
  end

  it "accepts Emitter argument" do
    ScratchPad.clear
    YAML.quick_emit(nil, YAML.emitter) { ScratchPad.record :in_block; YamlSpecs.get_a_node }
    ScratchPad.recorded.should == :in_block
  end
  
  it "requires block to return a BaseNode" do    
    y = YAML.quick_emit(nil, {}) { YamlSpecs.get_a_node }
    y.should == ("--- \n" + $test_yaml_string)
  end

  it "requires opts parameter to be a Hash or Emitter" do
    lambda { YAML.quick_emit(nil, nil) }.should raise_error(TypeError, "wrong argument type nil (expected Hash)")
  end

  it "requires a block" do
    lambda { YAML.quick_emit(nil, {}) }.should raise_error(NoMethodError, "undefined method `call' for nil:NilClass")
  end
end