require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::Generator do
  
  describe "#go_up" do
    
    before do
      @generator = Merb::Generators::Generator.new('/tmp', {})
    end
    
    it "should output an empty string for argument 0" do
      @generator.go_up(0).should == ""
    end
    
    it "should output a single '..' for argument 1" do
      @generator.go_up(1).should == "'..'"
    end
    
    it "should concatenate multiple '..' for other arguments" do
      @generator.go_up(3).should == "'..', '..', '..'"
    end
    
  end
  
  describe "#with_modules" do
    
    before do
      @class = Class.new(Merb::Generators::Generator)
      path = File.expand_path('fixtures', File.dirname(__FILE__))
      @class.template(:no_modules) do |template|
        template.source = path / "templates" / "no_modules.test"
        template.destination = path / "results" / "no_modules.test"
      end
      @class.template(:some_modules) do |template|
        template.source = path / "templates" / "some_modules.test"
        template.destination = path / "results" / "some_modules.test"
      end
      @generator = @class.new('/tmp', {})
    end
    
    it "should be correct for no module" do
      @generator.template(:no_modules).should be_identical
    end
    
    it "should be correct for some modulee" do
      @generator.template(:some_modules).should be_identical
    end
    
  end
  
end