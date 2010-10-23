require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'
require File.dirname(__FILE__) + '/fixtures/example_class'

describe "Object#to_yaml_properties" do
  it 'returns a sorted list of instance variable names' do
    obj = Object.new
    obj.instance_variable_set(:@x, 1)
    obj.instance_variable_set(:@y, 2)
    obj.instance_variable_set(:@z, 3)
    
    obj.to_yaml_properties.should == obj.instance_variables.sort
  end
end
