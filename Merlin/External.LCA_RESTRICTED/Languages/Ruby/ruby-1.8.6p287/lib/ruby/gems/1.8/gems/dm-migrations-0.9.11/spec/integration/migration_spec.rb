require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

ADAPTERS.each do |adapter|
  describe "Using Adapter #{adapter}, " do
    describe DataMapper::Migration, 'interface' do
      before do
        @migration = DataMapper::Migration.new(1, :create_people_table, :verbose => false) { }
      end

      it "should have a postition attribute" do
        @migration.should respond_to(:position)
        @migration.should respond_to(:position=)
        @migration.position.should == 1
      end

      it "should have a name attribute" do
        @migration.should respond_to(:name)
        @migration.should respond_to(:name=)
        @migration.name.should == :create_people_table
      end

      it "should have a :database option" do
        DataMapper.setup(:other, "sqlite3://#{Dir.pwd}/migration_other.db")

        m = DataMapper::Migration.new(2, :create_dogs_table, :database => :other) {}
        m.instance_variable_get(:@database).name.should == :other
      end

      it "should use the default database by default" do
        @migration.instance_variable_get(:@database).name.should == :default
      end

      it "should have a verbose option" do
        m = DataMapper::Migration.new(2, :create_dogs_table, :verbose => false) {}
        m.instance_variable_get(:@verbose).should == false
      end

      it "should be verbose by default" do
        m = DataMapper::Migration.new(2, :create_dogs_table) {}
        m.instance_variable_get(:@verbose).should == true
      end

      it "should be sortable, first by position, then name" do
        m1 = DataMapper::Migration.new(1, :create_people_table) {}
        m2 = DataMapper::Migration.new(2, :create_dogs_table) {}
        m3 = DataMapper::Migration.new(2, :create_cats_table) {}
        m4 = DataMapper::Migration.new(4, :create_birds_table) {}

        [m1, m2, m3, m4].sort.should == [m1, m3, m2, m4]
      end

      expected_module = {
        :sqlite3  => lambda { SQL::Sqlite3 },
        :mysql    => lambda { SQL::Mysql },
        :postgres => lambda { SQL::Postgresql }
      }[adapter][]

      it "should extend with #{expected_module} when adapter is #{adapter}" do
        migration = DataMapper::Migration.new(1, :"#{adapter}_adapter_test", :database => adapter) { }
        (class << migration.adapter; self; end).included_modules.should include(expected_module)
      end
    end

    describe DataMapper::Migration, 'defining actions' do
      before do
        @migration = DataMapper::Migration.new(1, :create_people_table, :verbose => false) { }
      end

      it "should have an #up method" do
        @migration.should respond_to(:up)
      end

      it "should save the block passed into the #up method in @up_action" do
        action = lambda {}
        @migration.up(&action)

        @migration.instance_variable_get(:@up_action).should == action
      end

      it "should have a #down method" do
        @migration.should respond_to(:down)
      end

      it "should save the block passed into the #down method in @down_action" do
        action = lambda {}
        @migration.down(&action)

        @migration.instance_variable_get(:@down_action).should == action
      end

      it "should make available an #execute method" do
        @migration.should respond_to(:execute)
      end

      it "should run the sql passed into the #execute method"
      # TODO: Find out how to stub the DataMapper::database.execute method
    end

    describe DataMapper::Migration, "output" do
      before do
        @migration = DataMapper::Migration.new(1, :create_people_table) { }
        @migration.stub!(:write) # so that we don't actually write anything to the console!
      end

      it "should #say a string with an indent" do
        @migration.should_receive(:write).with("   Foobar")
        @migration.say("Foobar", 2)
      end

      it "should #say with a default indent of 4" do
        @migration.should_receive(:write).with("     Foobar")
        @migration.say("Foobar")
      end

      it "should #say_with_time the running time of a block" do
        @migration.should_receive(:write).with(/Block/)
        @migration.should_receive(:write).with(/-> [\d]+/)

        @migration.say_with_time("Block"){ }
      end

    end
  end
end
