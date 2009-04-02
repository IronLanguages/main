require File.join(File.dirname(__FILE__), '..', 'spec_helper')

describe "Shorthand" do
  DataMapper.setup :default, "sqlite3:///"
  DataMapper.setup :alternative, "sqlite3:///"

  module DaModule
    class MyModel
      include DataMapper::Resource

      property :a, String, :key => true
      property :b, String
    end
  end

  it "should define a method with the same name as the class inside the class' module" do
    DaModule.should respond_to(:MyModel)
  end

  it "should assume the default repository when no arguments are passed" do
    DaModule::MyModel.send(:default_repository_name).should == :default
  end

  describe "a generated class" do
    it "should have the default repository set to the right one" do
      DaModule::MyModel(:alternative).send(:default_repository_name).should == :alternative
    end

    it "should respond to class methods" do
      DaModule::MyModel(:alternative).should respond_to(:create)
    end

    it "should have its instances set to the right repository" do
      DaModule::MyModel(:alternative).new.repository.name.should == :alternative
    end

    it "should contain the same properties as the parent" do
      DaModule::MyModel(:alternative).properties.should have(2).items
    end
  end

end
