require 'cases/sqlserver_helper'
require 'models/person'

class MigrationTestSqlserver < ActiveRecord::TestCase
  
  def setup
    @connection = ActiveRecord::Base.connection
  end
  
  context 'For transactions' do
    
    setup do
      @trans_test_table1 = 'sqlserver_trans_table1'
      @trans_test_table2 = 'sqlserver_trans_table2'
      @trans_tables = [@trans_test_table1,@trans_test_table2]
    end
    
    teardown do
      @trans_tables.each do |table_name|
        ActiveRecord::Migration.drop_table(table_name) if @connection.tables.include?(table_name)
      end
    end
    
    should 'not create a tables if error in migrations' do
      begin
        ActiveRecord::Migrator.up(SQLSERVER_MIGRATIONS_ROOT+'/transaction_table')
      rescue Exception => e
        assert_match %r|this and all later migrations canceled|, e.message
      end
      assert_does_not_contain @trans_test_table1, @connection.tables
      assert_does_not_contain @trans_test_table2, @connection.tables
    end
    
  end
  
  context 'For changing column' do
    
    should 'not raise exception when column contains default constraint' do
      lock_version_column = Person.columns_hash['lock_version']
      assert_equal :integer, lock_version_column.type
      assert lock_version_column.default.present?
      assert_nothing_raised { @connection.change_column 'people', 'lock_version', :string }
      Person.reset_column_information
      lock_version_column = Person.columns_hash['lock_version']
      assert_equal :string, lock_version_column.type
      assert lock_version_column.default.nil?
    end
    
    should 'not drop the default contraint if just renaming' do
      find_default = lambda do 
        @connection.select_all("EXEC sp_helpconstraint 'defaults','nomsg'").select do |row|     
          row['constraint_type'] == "DEFAULT on column decimal_number"
        end.last
      end
      default_before = find_default.call
      @connection.change_column :defaults, :decimal_number, :decimal, :precision => 4
      default_after = find_default.call
      assert default_after
      assert_equal default_before['constraint_keys'], default_after['constraint_keys']
    end
    
  end
  
end


class MigrationTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_add_column_not_null_without_default]
  
  include SqlserverCoercedTest
  
  def test_coerced_test_add_column_not_null_without_default
    Person.connection.create_table :testings do |t| 
      t.column :foo, :string
      t.column :bar, :string, :null => false
    end
    assert_raises(ActiveRecord::StatementInvalid) do
      Person.connection.execute "INSERT INTO [testings] ([foo], [bar]) VALUES ('hello', NULL)"
    end
  ensure
    Person.connection.drop_table :testings rescue nil
  end
  
end

class ChangeTableMigrationsTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_string_creates_string_column]

  include SqlserverCoercedTest
  
  def setup
    @connection = Person.connection
    @connection.create_table :delete_me, :force => true do |t|
    end
  end

  def teardown
    @connection.drop_table :delete_me rescue nil
  end
  
  def test_coerced_string_creates_string_column
    with_sqlserver_change_table do |t|
      @connection.expects(:add_column).with(:delete_me, :foo, sqlserver_string_column, {})
      @connection.expects(:add_column).with(:delete_me, :bar, sqlserver_string_column, {})
      t.string :foo, :bar
    end
  end
  
  protected

  def with_sqlserver_change_table
    @connection.change_table :delete_me do |t|
      yield t
    end
  end
  
  def sqlserver_string_column
    "#{@connection.native_string_database_type}(255)"
  end
  
end
