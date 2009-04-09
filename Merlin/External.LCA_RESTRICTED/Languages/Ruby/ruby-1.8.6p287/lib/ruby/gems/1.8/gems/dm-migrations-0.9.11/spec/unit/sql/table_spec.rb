require 'pathname'
require Pathname(__FILE__).dirname + '../../spec_helper'

require Pathname(__FILE__).dirname + '../../../lib/sql/table'

describe SQL::Table do
  before do
    @table = SQL::Table.new
  end

  %w{name columns}.each do |meth|
    it "should have a ##{meth} attribute" do
      @table.should respond_to(meth.intern)
    end
  end

  it 'should #to_s as the name' do
    @table.name = "table_name"
    @table.to_s.should == "table_name"
  end

  it 'should find a column by name' do
    column_a = mock('column', :name => 'id')
    column_b = mock('column', :name => 'login')
    @table.columns = [column_a, column_b]

    @table.column('id').should == column_a
  end


end
