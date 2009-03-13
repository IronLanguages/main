require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::LayoutGenerator do

  before(:each) do
    @generator = Merb::Generators::LayoutGenerator.new('/tmp', {}, 'Stuff')
  end
  
  it_should_behave_like "named generator"
  
  it "should create a layout" do
    @generator.should create('/tmp/app/views/layout/stuff.html.erb')
  end
  
  describe "with rspec" do
    
    it "should create a view spec"
    
  end
  
  it "should render templates successfully" do
    lambda { @generator.render! }.should_not raise_error
  end
    
end