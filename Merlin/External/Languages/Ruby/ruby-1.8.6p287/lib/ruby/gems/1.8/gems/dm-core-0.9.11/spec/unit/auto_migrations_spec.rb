require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

require DataMapper.root / 'lib' / 'dm-core' / 'repository'
require DataMapper.root / 'lib' / 'dm-core' / 'resource'
require DataMapper.root / 'lib' / 'dm-core' / 'auto_migrations'

describe DataMapper::AutoMigrations do

  before :all do
    @cow = Class.new do
      include DataMapper::Resource

      property :name, String, :key => true
      property :age, Integer
    end
  end

  before(:each) do
    DataMapper::Resource.descendants.clear
  end

  after(:each) do
    DataMapper::Resource.descendants.clear
  end

  it "should add the resource class to AutoMigrator's models on a mixin" do
    @class = Class.new do
      include DataMapper::Resource
    end

    DataMapper::Resource.descendants.should include(@class)
  end

  it "should add the #auto_migrate! method on a mixin" do
    @cat = Class.new do
      include DataMapper::Resource

      property :name, String, :key => true
      property :age, Integer
    end

    @cat.should respond_to(:auto_migrate!)
  end

  it "should add the #auto_upgrade! method on a mixin" do
    @cat = Class.new do
      include DataMapper::Resource

      property :name, String, :key => true
      property :age, Integer
    end

    @cat.should respond_to(:auto_upgrade!)
  end

  it "should not conflict with other Migrators on a mixin" do
    migrator_class = Class.new(DataMapper::Migrator)

    included_proc = lambda { |model| migrator_class.models << model }

    migrator_mixin = Module.new do
      self.class.send(:define_method, :included, &included_proc)
    end

    model_class = Class.new do
      include DataMapper::Resource
      include migrator_mixin

      property :name, String
      property :age, String
    end

    DataMapper::Resource.descendants.should include(model_class)
    migrator_class.models.should include(model_class)
  end

  describe "#auto_migrate" do
    before do
      @repository_name = mock('repository name')
    end

    it "should call each model's auto_migrate! method" do
      models = [:cat, :dog, :fish, :cow].map {|m| mock(m)}

      models.each do |model|
        DataMapper::Resource.descendants << model
        model.should_receive(:auto_migrate_down!).with(@repository_name)
        model.should_receive(:auto_migrate_up!).with(@repository_name)
      end

      DataMapper::AutoMigrator.auto_migrate(@repository_name)
    end
  end
  describe "#auto_upgrade" do
    before do
      @repository_name = mock('repository name')
    end

    it "should call each model's auto_upgrade! method" do
      models = [:cat, :dog, :fish, :cow].map {|m| mock(m)}

      models.each do |model|
        DataMapper::Resource.descendants << model
        model.should_receive(:auto_upgrade!).with(@repository_name)
      end

      DataMapper::AutoMigrator.auto_upgrade(@repository_name)
    end
  end
end
