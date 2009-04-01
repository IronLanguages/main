require 'pathname'
require Pathname(__FILE__).dirname + '../spec_helper'

require Pathname(__FILE__).dirname + '../../lib/migration'

describe 'Migration' do

  before do
    @adapter = mock('adapter', :class => DataMapper::Adapters::Sqlite3Adapter)
    @repo = mock('DataMapper.repository', :adapter => @adapter)
    DataMapper.stub!(:repository).and_return(@repo)
    @m = DataMapper::Migration.new(1, :do_nothing, {}) {}
    @m.stub!(:write) # silence any output
  end

  [:position, :name, :database, :adapter].each do |meth|
    it "should respond to ##{meth}" do
      @m.should respond_to(meth)
    end
  end

  describe 'initialization' do
    it 'should set @position from the given position' do
      @m.instance_variable_get(:@position).should == 1
    end

    it 'should set @name from the given name' do
      @m.instance_variable_get(:@name).should == :do_nothing
    end

    it 'should set @options from the options hash' do
      @m.instance_variable_get(:@options).should == {}
    end

    it 'should set @database from the default repository if no :database option is given' do
      DataMapper.should_receive(:repository).with(:default).and_return(@repo)
      DataMapper::Migration.new(1, :do_nothing, {}) {}
    end

    it 'should set @database to the repository specified with the :database option' do
      DataMapper.should_receive(:repository).with(:foobar).and_return(@repo)
      DataMapper::Migration.new(1, :do_nothing, :database => :foobar) {}
    end

    it 'should determine the class of the adapter to be extended' do
      @adapter.should_receive(:class).and_return(DataMapper::Adapters::Sqlite3Adapter)
      DataMapper::Migration.new(1, :do_nothing, {}) {}
    end

    it 'should extend the adapter with the right module' do
      @adapter.should_receive(:extend).with(SQL::Sqlite3)
      DataMapper::Migration.new(1, :do_nothing, {}) {}
    end

    it 'should raise "Unsupported adapter" on an unknown adapter' do
      @adapter.should_receive(:class).any_number_of_times.and_return("InvalidAdapter")
      lambda {
        DataMapper::Migration.new(1, :do_nothing, {}) {}
      }.should raise_error
    end

    it 'should set @verbose from the options hash' do
      m = DataMapper::Migration.new(1, :do_nothing, :verbose => false) {}
      m.instance_variable_get(:@verbose).should be_false
    end

    it 'should set @verbose to true by default' do
      @m.instance_variable_get(:@verbose).should be_true
    end

    it 'should set the @up_action to an empty block' do
      @m.instance_variable_get(:@up_action).should be_kind_of(Proc)
    end

    it 'should set the @down_action to an empty block' do
      @m.instance_variable_get(:@down_action).should be_kind_of(Proc)
    end

    it 'should evaluate the given block'

  end

  it 'should set the @up_action when #up is called with a block' do
    action = lambda {}
    @m.up(&action)
    @m.instance_variable_get(:@up_action).should == action
  end

  it 'should set the @up_action when #up is called with a block' do
    action = lambda {}
    @m.down(&action)
    @m.instance_variable_get(:@down_action).should == action
  end

  describe 'perform_up' do
    before do
      @up_action = mock('proc', :call => true)
      @m.instance_variable_set(:@up_action, @up_action)
      @m.stub!(:needs_up?).and_return(true)
      @m.stub!(:update_migration_info)
    end

    it 'should call the action assigned to @up_action and return the result' do
      @up_action.should_receive(:call).and_return(:result)
      @m.perform_up.should == :result
    end

    it 'should output a status message with the position and name of the migration' do
      @m.should_receive(:write).with(/Performing Up Migration #1: do_nothing/)
      @m.perform_up
    end

    it 'should not run if it doesnt need to be' do
      @m.should_receive(:needs_up?).and_return(false)
      @up_action.should_not_receive(:call)
      @m.perform_up
    end

    it 'should update the migration info table' do
      @m.should_receive(:update_migration_info).with(:up)
      @m.perform_up
    end

    it 'should not update the migration info table if the migration does not need run' do
      @m.should_receive(:needs_up?).and_return(false)
      @m.should_not_receive(:update_migration_info)
      @m.perform_up
    end

  end

  describe 'perform_down' do
    before do
      @down_action = mock('proc', :call => true)
      @m.instance_variable_set(:@down_action, @down_action)
      @m.stub!(:needs_down?).and_return(true)
      @m.stub!(:update_migration_info)
    end

    it 'should call the action assigned to @down_action and return the result' do
      @down_action.should_receive(:call).and_return(:result)
      @m.perform_down.should == :result
    end

    it 'should output a status message with the position and name of the migration' do
      @m.should_receive(:write).with(/Performing Down Migration #1: do_nothing/)
      @m.perform_down
    end

    it 'should not run if it doesnt need to be' do
      @m.should_receive(:needs_down?).and_return(false)
      @down_action.should_not_receive(:call)
      @m.perform_down
    end

    it 'should update the migration info table' do
      @m.should_receive(:update_migration_info).with(:down)
      @m.perform_down
    end

    it 'should not update the migration info table if the migration does not need run' do
      @m.should_receive(:needs_down?).and_return(false)
      @m.should_not_receive(:update_migration_info)
      @m.perform_down
    end

  end

  describe 'methods used in the action blocks' do

    describe '#execute' do
      before do
        @adapter.stub!(:execute)
      end

      it 'should send the SQL it its executing to the adapter execute method' do
        @adapter.should_receive(:execute).with('SELECT SOME SQL')
        @m.execute('SELECT SOME SQL')
      end

      it 'should output the SQL it is executing' do
        @m.should_receive(:write).with(/SELECT SOME SQL/)
        @m.execute('SELECT SOME SQL')
      end
    end

    describe 'helpers' do
      before do
        @m.stub!(:execute) # don't actually run anything
      end

      describe '#create_table' do
        before do
          @tc = mock('TableCreator', :to_sql => 'CREATE TABLE')
          SQL::TableCreator.stub!(:new).and_return(@tc)
        end

        it 'should create a new TableCreator object' do
          SQL::TableCreator.should_receive(:new).with(@adapter, :users, {}).and_return(@tc)
          @m.create_table(:users) { }
        end

        it 'should convert the TableCreator object to an sql statement' do
          @tc.should_receive(:to_sql).and_return('CREATE TABLE')
          @m.create_table(:users) { }
        end

        it 'should execute the create table sql' do
          @m.should_receive(:execute).with('CREATE TABLE')
          @m.create_table(:users) { }
        end

      end

      describe '#drop_table' do
        it 'should quote the table name' do
          @adapter.should_receive(:quote_table_name).with('users')
          @m.drop_table :users
        end

        it 'should execute the DROP TABLE sql for the table' do
          @adapter.stub!(:quote_table_name).and_return("'users'")
          @m.should_receive(:execute).with(%{DROP TABLE 'users'})
          @m.drop_table :users
        end

      end

      describe '#modify_table' do
        before do
          @tm = mock('TableModifier', :statements => [])
          SQL::TableModifier.stub!(:new).and_return(@tm)
        end

        it 'should create a new TableModifier object' do
          SQL::TableModifier.should_receive(:new).with(@adapter, :users, {}).and_return(@tm)
          @m.modify_table(:users){ }
        end

        it 'should get the statements from the TableModifier object' do
          @tm.should_receive(:statements).and_return([])
          @m.modify_table(:users){ }
        end

        it 'should iterate over the statements and execute each one' do
          @tm.should_receive(:statements).and_return(['SELECT 1', 'SELECT 2'])
          @m.should_receive(:execute).with('SELECT 1')
          @m.should_receive(:execute).with('SELECT 2')
          @m.modify_table(:users){ }
        end

      end

      describe 'sorting' do
        it 'should order things by position' do
          m1 = DataMapper::Migration.new(1, :do_nothing){}
          m2 = DataMapper::Migration.new(2, :do_nothing_else){}

          (m1 <=> m2).should == -1
        end

        it 'should order things by name when they have the same position' do
          m1 = DataMapper::Migration.new(1, :do_nothing_a){}
          m2 = DataMapper::Migration.new(1, :do_nothing_b){}

          (m1 <=> m2).should == -1
        end

      end

      describe 'formatting output' do
        describe '#say' do
          it 'should output the message' do
            @m.should_receive(:write).with(/Paul/)
            @m.say("Paul")
          end

          it 'should indent the message with 4 spaces by default' do
            @m.should_receive(:write).with(/^\s{4}/)
            @m.say("Paul")
          end

          it 'should indext the message with a given number of spaces' do
            @m.should_receive(:write).with(/^\s{3}/)
            @m.say("Paul", 3)
          end
        end

        describe '#say_with_time' do
          before do
            @m.stub!(:say)
          end

          it 'should say the message with an indent of 2' do
            @m.should_receive(:say).with("Paul", 2)
            @m.say_with_time("Paul"){}
          end

          it 'should output the time it took' do
            @m.should_receive(:say).with(/\d+/, 2)
            @m.say_with_time("Paul"){}
          end
        end

        describe '#write' do
          before do
            # need a new migration object, because the main one had #write stubbed to silence output
            @m = DataMapper::Migration.new(1, :do_nothing) {}
          end

          it 'should puts the message' do
            @m.should_receive(:puts).with("Paul")
            @m.write("Paul")
          end

          it 'should not puts the message if @verbose is false' do
            @m.instance_variable_set(:@verbose, false)
            @m.should_not_receive(:puts)
            @m.write("Paul")
          end

        end

      end

      describe 'working with the migration_info table' do
        before do
          @adapter.stub!(:storage_exists?).and_return(true)
          @adapter.stub!(:quote_table_name).and_return(%{'users'})
          @adapter.stub!(:quote_column_name).and_return(%{'migration_name'})
        end

        describe '#update_migration_info' do
          it 'should add a record of the migration' do
            @m.should_receive(:execute).with(
              %Q{INSERT INTO 'users' ('migration_name') VALUES ('do_nothing')}
            )
            @m.update_migration_info(:up)
          end

          it 'should remove the record of the migration' do
            @m.should_receive(:execute).with(
              %Q{DELETE FROM 'users' WHERE 'migration_name' = 'do_nothing'}
            )
            @m.update_migration_info(:down)
          end

          it 'should try to create the migration_info table' do
            @m.should_receive(:create_migration_info_table_if_needed)
            @m.update_migration_info(:up)
          end
        end

        describe '#create_migration_info_table_if_needed' do
          it 'should create the migration info table' do
            @m.should_receive(:migration_info_table_exists?).and_return(false)
            @m.should_receive(:execute).with(
              %Q{CREATE TABLE 'users' ('migration_name' VARCHAR(255) UNIQUE)}
            )
            @m.create_migration_info_table_if_needed
          end

          it 'should not try to create the migration info table if it already exists' do
            @m.should_receive(:migration_info_table_exists?).and_return(true)
            @m.should_not_receive(:execute)
            @m.create_migration_info_table_if_needed
          end
        end

        it 'should quote the name of the migration for use in sql' do
          @m.quoted_name.should == %{'do_nothing'}
        end

        it 'should query the adapter to see if the migration_info table exists' do
          @adapter.should_receive(:storage_exists?).with('migration_info').and_return(true)
          @m.migration_info_table_exists?.should == true
        end

        describe '#migration_record' do
          it 'should query for the migration' do
            @adapter.should_receive(:query).with(
              %Q{SELECT 'migration_name' FROM 'users' WHERE 'migration_name' = 'do_nothing'}
            )
            @m.migration_record
          end

          it 'should not try to query if the table does not exist' do
            @m.stub!(:migration_info_table_exists?).and_return(false)
            @adapter.should_not_receive(:query)
            @m.migration_record
          end

        end

        describe '#needs_up?' do
          it 'should be true if there is no record' do
            @m.should_receive(:migration_record).and_return([])
            @m.needs_up?.should == true
          end

          it 'should be false if the record exists' do
            @m.should_receive(:migration_record).and_return([:not_empty])
            @m.needs_up?.should == false
          end

          it 'should be true if there is no migration_info table' do
            @m.should_receive(:migration_info_table_exists?).and_return(false)
            @m.needs_up?.should == true
          end

        end

        describe '#needs_down?' do
          it 'should be false if there is no record' do
            @m.should_receive(:migration_record).and_return([])
            @m.needs_down?.should == false
          end

          it 'should be true if the record exists' do
            @m.should_receive(:migration_record).and_return([:not_empty])
            @m.needs_down?.should == true
          end

          it 'should be false if there is no migration_info table' do
            @m.should_receive(:migration_info_table_exists?).and_return(false)
            @m.needs_down?.should == false
          end

        end

        it 'should have the adapter quote the migration_info table' do
          @adapter.should_receive(:quote_table_name).with('migration_info').and_return("'migration_info'")
          @m.migration_info_table.should == "'migration_info'"
        end

        it 'should have a quoted migration_name_column' do
          @adapter.should_receive(:quote_column_name).with('migration_name').and_return("'migration_name'")
          @m.migration_name_column.should == "'migration_name'"
        end

      end

    end

  end
end
