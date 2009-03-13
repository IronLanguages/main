require 'pathname'
require Pathname(__FILE__).dirname + '../../spec_helper'

require Pathname(__FILE__).dirname + '../../../lib/sql/sqlite3'

# a dummy class to include the module into
class Sqlite3Extension
  include SQL::Sqlite3
end

describe "SQLite3 Extensions" do
  before do
    @se = Sqlite3Extension.new
  end

  it 'should support schema-level transactions' do
    @se.supports_schema_transactions?.should be_true
  end

  it 'should support the serial column attribute' do
    @se.supports_serial?.should be_true
  end

  it 'should create a table object from the name' do
    table = mock('SQLite3 Table')
    SQL::Sqlite3::Table.should_receive(:new).with(@se, 'users').and_return(table)

    @se.table('users').should == table
  end

  describe 'recreating the database' do
    before do
      uri = mock('URI', :path => '/foo/bar.db')
      @se.instance_variable_set('@uri', uri)
    end

    it 'should rm the db file' do
      @se.should_receive(:system).with('rm /foo/bar.db')
      @se.recreate_database
    end

  end

  describe 'Table' do
    before do
      @cs1 = mock('Column Struct')
      @cs2 = mock('Column Struct')
      @adapter = mock('adapter')
      @adapter.stub!(:query_table).with('users').and_return([@cs1, @cs2])

      @col1 = mock('SQLite3 Column')
      @col2 = mock('SQLite3 Column')
    end

    it 'should initialize columns by querying the table' do
      SQL::Sqlite3::Column.should_receive(:new).with(@cs1).and_return(@col1)
      SQL::Sqlite3::Column.should_receive(:new).with(@cs2).and_return(@col2)
      @adapter.should_receive(:query_table).with('users').and_return([@cs1,@cs2])
      SQL::Sqlite3::Table.new(@adapter, 'users')
    end

    it 'should create SQLite3 Column objects from the returned column structs' do
      SQL::Sqlite3::Column.should_receive(:new).with(@cs1).and_return(@col1)
      SQL::Sqlite3::Column.should_receive(:new).with(@cs2).and_return(@col2)
      SQL::Sqlite3::Table.new(@adapter, 'users')
    end

    it 'should set the @columns to the looked-up columns' do
      SQL::Sqlite3::Column.should_receive(:new).with(@cs1).and_return(@col1)
      SQL::Sqlite3::Column.should_receive(:new).with(@cs2).and_return(@col2)
      t = SQL::Sqlite3::Table.new(@adapter, 'users')
      t.columns.should == [ @col1, @col2 ]
    end

  end

  describe 'Column' do
    before do
      @cs = mock('Struct',
                 :name       => 'id',
                 :type       => 'integer',
                 :dflt_value => 123,
                 :pk         => true,
                 :notnull    => 0)
      @c = SQL::Sqlite3::Column.new(@cs)
    end

    it 'should set the name from the name value' do
      @c.name.should == 'id'
    end

    it 'should set the type from the type value' do
      @c.type.should == 'integer'
    end

    it 'should set the default_value from the dflt_value value' do
      @c.default_value.should == 123
    end

    it 'should set the primary_key from the pk value' do
      @c.primary_key.should == true
    end

    it 'should set not_null based on the notnull value' do
      @c.not_null.should == true
    end

  end


end
