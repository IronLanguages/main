require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::MerbPluginGenerator do
  
  describe "templates" do
    
    before do
      @generator = Merb::Generators::MerbPluginGenerator.new('/tmp', {}, 'testing')
    end
    
    it "should spec stuff"
    
  end
  
end