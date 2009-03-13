require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::ResourceControllerGenerator do

  before(:each) do
    @generator = Merb::Generators::ResourceControllerGenerator.new('/tmp', {}, 'Stuff')
  end
  
  it_should_behave_like "namespaced generator"
  
  describe "#model_class_name" do
    it "should camel case and singularize the controller name" do
      @generator.name = "project_pictures"
      @generator.model_class_name == "ProjectPicture"
    end
  end
  
  describe "#plural_model" do
    it "should snake case the controller name" do
      @generator.name = "ProjectPictures"
      @generator.plural_model == "project_pictures"
    end
  end
  
  describe "#singular_model" do
    it "should snake case and singularize the controller name" do
      @generator.name = "ProjectPictures"
      @generator.singular_model == "project_picture"
    end
  end
  
  describe "#resource_path" do
    it "should snake case and slash separate the full controller name" do
      @generator.name = "Monkey::BlahWorld::ProjectPictures"
      @generator.singular_model == "monkey/blah_world/project_picture"
    end
  end
  
  it "should create a controller" do
    @generator.should create('/tmp/app/controllers/stuff.rb')
  end
  
  it "should create views" do
    @generator.should create('/tmp/app/views/stuff/index.html.erb')
    @generator.should create('/tmp/app/views/stuff/new.html.erb')
    @generator.should create('/tmp/app/views/stuff/edit.html.erb')
    @generator.should create('/tmp/app/views/stuff/show.html.erb')
  end


  
  describe "with a namespace" do
    
    before(:each) do
      @generator = Merb::Generators::ResourceControllerGenerator.new('/tmp', {}, 'John::Monkey::Stuff')
    end
    
    it "should create a controller" do
      @generator.should create('/tmp/app/controllers/john/monkey/stuff.rb')
    end

    it "should create views" do
      @generator.should create('/tmp/app/views/john/monkey/stuff/index.html.erb')
      @generator.should create('/tmp/app/views/john/monkey/stuff/new.html.erb')
      @generator.should create('/tmp/app/views/john/monkey/stuff/edit.html.erb')
      @generator.should create('/tmp/app/views/john/monkey/stuff/show.html.erb')
    end

    it "should create a helper" do
      @generator.should create('/tmp/app/helpers/john/monkey/stuff_helper.rb')
    end
    
  end
  
end