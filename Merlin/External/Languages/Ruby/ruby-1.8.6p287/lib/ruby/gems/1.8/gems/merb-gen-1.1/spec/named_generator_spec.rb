require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::NamedGenerator do

  before(:each) do
    @generator = Merb::Generators::NamedGenerator.new('/tmp', {}, 'Stuff')
  end
  
  it_should_behave_like "named generator"
    
end