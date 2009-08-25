require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::NamespacedGenerator do

  before(:each) do
    @generator = Merb::Generators::NamespacedGenerator.new('/tmp', {}, 'Stuff')
  end
  
  it_should_behave_like "namespaced generator"

end