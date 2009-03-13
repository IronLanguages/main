require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'
require File.dirname(__FILE__) + '/fixtures/strings'

describe "YAML.each_node" do
  it "invokes block with a BaseNode" do
    node = nil
    YAML.each_node(StringIO.new($test_yaml_string)) { |n| node = n }
    node.should be_kind_of(YAML::BaseNode)
    not_compliant_on :ironruby do
      node.should be_kind_of(YAML::Syck::Map)
    end
  end

  it "invokes block once for a simple string " do
    i = 0
    YAML.each_node(StringIO.new($test_yaml_string)) { |node| i = i + 1 }
    i.should == 1
  end

  it "invokes block n times for a string with n documents" do
    i = 0
    YAML.each_node(StringIO.new($multidocument)) { |node| i = i + 1 }
    i.should == 2
  end
end