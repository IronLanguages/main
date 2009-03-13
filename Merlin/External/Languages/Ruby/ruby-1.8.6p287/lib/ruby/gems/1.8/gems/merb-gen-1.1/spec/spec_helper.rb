$:.push File.join(File.dirname(__FILE__), '..', 'lib')

require 'rubygems'
require 'spec'
require 'merb-core'
require 'merb-gen'
require 'templater/spec/helpers'

Merb.disable(:initfile)

Spec::Runner.configure do |config|
  config.include Templater::Spec::Helpers
end

describe "named generator", :shared => true do

  describe '#file_name' do

    it "should convert the name to snake case" do
      @generator.name = 'SomeMoreStuff'
      @generator.file_name.should == 'some_more_stuff'
    end

    it "should preserve dashes" do
      @generator.name = "project-pictures"
      @generator.file_name.should == "project-pictures"
    end

  end
  
  describe '#base_name' do

    it "should convert the name to snake case" do
      @generator.name = 'SomeMoreStuff'
      @generator.base_name.should == 'some_more_stuff'
    end

    it "should preserve dashes" do
      @generator.name = "project-pictures"
      @generator.base_name.should == "project-pictures"
    end

  end

  describe "#symbol_name" do

    it "should snakify the name" do
      @generator.name = "ProjectPictures"
      @generator.symbol_name.should == "project_pictures"
    end
    
    it "should replace dashes with underscores" do
      @generator.name = "project-pictures"
      @generator.symbol_name.should == "project_pictures"
    end

  end

  describe '#class_name' do
  
    it "should convert the name to camel case" do
      @generator.name = 'some_more_stuff'
      @generator.class_name.should == 'SomeMoreStuff'
    end
    
    it "should convert a name with dashes to camel case" do
      @generator.name = 'some-more-stuff'
      @generator.class_name.should == 'SomeMoreStuff'
    end
  
  end
  
  describe '#module_name' do
  
    it "should convert the name to camel case" do
      @generator.name = 'some_more_stuff'
      @generator.module_name.should == 'SomeMoreStuff'
    end
    
    it "should convert a name with dashes to camel case" do
      @generator.name = 'some-more-stuff'
      @generator.module_name.should == 'SomeMoreStuff'
    end
  
  end
  
  describe '#test_class_name' do
    
    it "should convert the name to camel case and append 'test'" do
      @generator.name = 'some_more_stuff'
      @generator.test_class_name.should == 'SomeMoreStuffTest'
    end
    
  end

end

shared_examples_for "namespaced generator" do

  describe "#class_name" do
    it "should camelize the name" do
      @generator.name = "project_pictures"
      @generator.class_name.should == "ProjectPictures"
    end
    
    it "should split off the last double colon separated chunk" do
      @generator.name = "Test::Monkey::ProjectPictures"
      @generator.class_name.should == "ProjectPictures"
    end
    
    it "should split off the last slash separated chunk" do
      @generator.name = "test/monkey/project_pictures"
      @generator.class_name.should == "ProjectPictures"
    end
    
    it "should convert a name with dashes to camel case" do
      @generator.name = 'some-more-stuff'
      @generator.class_name.should == 'SomeMoreStuff'
    end
  end
  
  describe "#module_name" do
    it "should camelize the name" do
      @generator.name = "project_pictures"
      @generator.module_name.should == "ProjectPictures"
    end
    
    it "should split off the last double colon separated chunk" do
      @generator.name = "Test::Monkey::ProjectPictures"
      @generator.module_name.should == "ProjectPictures"
    end
    
    it "should split off the last slash separated chunk" do
      @generator.name = "test/monkey/project_pictures"
      @generator.module_name.should == "ProjectPictures"
    end
    
    it "should convert a name with dashes to camel case" do
      @generator.name = 'some-more-stuff'
      @generator.module_name.should == 'SomeMoreStuff'
    end
  end
  
  describe "#modules" do
    it "should be empty if no modules are passed to the name" do
      @generator.name = "project_pictures"
      @generator.modules.should == []
    end
    
    it "should split off all but the last double colon separated chunks" do
      @generator.name = "Test::Monkey::ProjectPictures"
      @generator.modules.should == ["Test", "Monkey"]
    end
    
    it "should split off all but the last slash separated chunk" do
      @generator.name = "test/monkey/project_pictures"
      @generator.modules.should == ["Test", "Monkey"]
    end
  end
  
  describe "#file_name" do
    it "should snakify the name" do
      @generator.name = "ProjectPictures"
      @generator.file_name.should == "project_pictures"
    end
    
    it "should preserve dashes" do
      @generator.name = "project-pictures"
      @generator.file_name.should == "project-pictures"
    end
    
    it "should split off the last double colon separated chunk and snakify it" do
      @generator.name = "Test::Monkey::ProjectPictures"
      @generator.file_name.should == "project_pictures"
    end
    
    it "should split off the last slash separated chunk and snakify it" do
      @generator.name = "test/monkey/project_pictures"
      @generator.file_name.should == "project_pictures"
    end
  end
  
  describe "#base_name" do
    it "should snakify the name" do
      @generator.name = "ProjectPictures"
      @generator.base_name.should == "project_pictures"
    end
    
    it "should preserve dashes" do
      @generator.name = "project-pictures"
      @generator.base_name.should == "project-pictures"
    end
    
    it "should split off the last double colon separated chunk and snakify it" do
      @generator.name = "Test::Monkey::ProjectPictures"
      @generator.base_name.should == "project_pictures"
    end
    
    it "should split off the last slash separated chunk and snakify it" do
      @generator.name = "test/monkey/project_pictures"
      @generator.base_name.should == "project_pictures"
    end
  end
  
  describe "#symbol_name" do
    it "should snakify the name and replace dashes with underscores" do
      @generator.name = "project-pictures"
      @generator.symbol_name.should == "project_pictures"
    end
    
    it "should split off the last slash separated chunk, snakify it and replace dashes with underscores" do
      @generator.name = "test/monkey/project-pictures"
      @generator.symbol_name.should == "project_pictures"
    end
  end
  
  describe "#test_class_name" do
    it "should camelize the name and append 'Test'" do
      @generator.name = "project_pictures"
      @generator.test_class_name.should == "ProjectPicturesTest"
    end
    
    it "should split off the last double colon separated chunk" do
      @generator.name = "Test::Monkey::ProjectPictures"
      @generator.test_class_name.should == "ProjectPicturesTest"
    end
    
    it "should split off the last slash separated chunk" do
      @generator.name = "test/monkey/project_pictures"
      @generator.test_class_name.should == "ProjectPicturesTest"
    end
  end
  
  describe "#full_class_name" do
    it "should camelize the name" do
      @generator.name = "project_pictures"
      @generator.full_class_name.should == "ProjectPictures"
    end
    
    it "should camelize a name with dashes" do
      @generator.name = "project-pictures"
      @generator.full_class_name.should == "ProjectPictures"
    end
    
    it "should leave double colon separated chunks" do
      @generator.name = "Test::Monkey::ProjectPictures"
      @generator.full_class_name.should == "Test::Monkey::ProjectPictures"
    end
    
    it "should convert slashes to double colons and camel case" do
      @generator.name = "test/monkey/project_pictures"
      @generator.full_class_name.should == "Test::Monkey::ProjectPictures"
    end
  end
  
  describe "#base_path" do
    it "should be blank for no namespaces" do
      @generator.name = "project_pictures"
      @generator.base_path.should == ""
    end
    
    it "should snakify and join namespace for double colon separated chunk" do
      @generator.name = "Test::Monkey::ProjectPictures"
      @generator.base_path.should == "test/monkey"
    end
    
    it "should leave slashes but only use the namespace part" do
      @generator.name = "test/monkey/project_pictures"
      @generator.base_path.should == "test/monkey"
    end
  end

end
