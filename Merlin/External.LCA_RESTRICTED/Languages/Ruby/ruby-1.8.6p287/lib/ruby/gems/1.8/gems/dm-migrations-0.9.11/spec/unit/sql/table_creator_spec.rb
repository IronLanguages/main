require 'pathname'
require Pathname(__FILE__).dirname + '../../spec_helper'

require Pathname(__FILE__).dirname + '../../../lib/sql/table_creator'

describe 'SQL module' do
  describe 'TableCreator' do
    before do
      @adapter = mock('adapter')
      @adapter.stub!(:quote_table_name).and_return(%{'users'})
      @tc = SQL::TableCreator.new(@adapter, 'users') { }
    end

    describe 'initialization' do
      it 'should set @adapter to the adapter' do
        @tc.instance_variable_get("@adapter").should == @adapter
      end

      it 'should set @table_name to the stringified table name' do
        @tc.instance_variable_get("@table_name").should == 'users'
      end

      it 'should set @opts to the options hash' do
        @tc.instance_variable_get("@opts").should == {}
      end

      it 'should set @columns to an empty array' do
        @tc.instance_variable_get("@columns").should == []
      end

      it 'should evaluate the given block' do
        block = lambda { column :foo, :bar }
        col = mock('column')
        SQL::TableCreator::Column.should_receive(:new).with(@adapter, :foo, :bar, {}).and_return(col)
        tc = SQL::TableCreator.new(@adapter, 'users', {}, &block)
        tc.instance_variable_get("@columns").should == [col]
      end
    end

    it 'should have a table_name' do
      @tc.should respond_to(:table_name)
      @tc.table_name.should == 'users'
    end

    it 'should use the adapter to quote the table name' do
      @adapter.should_receive(:quote_table_name).with('users').and_return(%{'users'})
      @tc.quoted_table_name.should == %{'users'}
    end

    it 'should initialze a new column and add it to the list of columns' do
      col = mock('column')
      SQL::TableCreator::Column.should_receive(:new).with(@adapter, :foo, :bar, {}).and_return(col)
      @tc.column(:foo, :bar)
      @tc.instance_variable_get("@columns").should == [col]
    end

    it 'should output an SQL CREATE statement to build itself' do
      @adapter.should_receive(:create_table_statement).with("'users'").and_return(%{CREATE TABLE 'users'})
      @tc.to_sql.should ==
        %{CREATE TABLE 'users' ()}
    end

    describe 'Column' do
      before do
        @adapter.stub!(:quote_column_name).and_return(%{'id'})
        @adapter.class.stub!(:type_map).and_return(Integer => {:type => 'int'})
        @adapter.stub!(:property_schema_statement).and_return("SOME SQL")
        @c = SQL::TableCreator::Column.new(@adapter, 'id', Integer, :serial => true)
      end

      describe 'initialization' do
        it 'should set @adapter to the adapter' do
          @c.instance_variable_get("@adapter").should == @adapter
        end

        it 'should set @name to the stringified name' do
          @c.instance_variable_get("@name").should == 'id'
        end

        # TODO make this really the type, not this sql bullshit
        it 'should set @type to the type' do
          @c.instance_variable_get("@type").should == "SOME SQL"
        end

        it 'should set @opts to the options hash' do
          @c.instance_variable_get("@opts").should == {:serial => true}
        end

      end

    end
  end

end
