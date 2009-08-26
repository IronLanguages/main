require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::ResourceGenerator do
  
  before do
    @generator = Merb::Generators::ResourceGenerator.new('/tmp', {}, 'Project', { :name => :string })
  end
  
  it "should invoke the resource controller generator with the pluralized name" do
    @generator.should invoke(Merb::Generators::ResourceControllerGenerator).with('Projects', { :name => :string })

  end
  
  it "should invoke the model generator" do
    @generator.should invoke(Merb::Generators::ModelGenerator).with('Project', { :name => :string })
  end
  
end