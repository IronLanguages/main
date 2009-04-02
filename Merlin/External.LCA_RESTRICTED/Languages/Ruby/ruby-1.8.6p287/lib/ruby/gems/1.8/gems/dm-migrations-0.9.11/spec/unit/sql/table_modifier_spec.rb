require 'pathname'
require Pathname(__FILE__).dirname + '../../spec_helper'

require Pathname(__FILE__).dirname + '../../../lib/sql/table_modifier'

describe 'SQL module' do
  describe 'TableModifier' do
    before do
      @adapter = mock('adapter')
      @adapter.stub!(:quote_table_name).and_return(%{'users'})
      @tc = SQL::TableModifier.new(@adapter, :users) { }
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

      it 'should set @statements to an empty array' do
        @tc.instance_variable_get("@statements").should == []
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

  end

end
