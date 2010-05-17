require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Migrator do
  before(:each) do
    DataMapper::Migrator.subclasses.clear
  end

  after(:each) do
    DataMapper::Migrator.subclasses.clear
  end

  it "should keep track of subclasses" do
    lambda { Class.new(DataMapper::Migrator) }.should change{ DataMapper::Migrator.subclasses.size }.by(1)
  end

  it "should define a class level 'models' method for each subclass" do
    klass = Class.new(DataMapper::Migrator)

    klass.should respond_to(:models)
  end

  it "should keep subclass models seperated" do
    klass_a = Class.new(DataMapper::Migrator)
    klass_b = Class.new(DataMapper::Migrator)

    klass_a.models << :foo

    klass_b.models.should be_empty

    klass_a.models.should == [:foo]
  end
end
