require 'cases/sqlserver_helper'

class ExecuteProcedureTestSqlserver < ActiveRecord::TestCase
  
  def setup
    @klass = ActiveRecord::Base
  end
  
  should 'execute a simple procedure' do
    tables = @klass.execute_procedure :sp_tables
    assert_instance_of Array, tables
    assert_instance_of HashWithIndifferentAccess, tables.first
  end
  
  should 'take parameter arguments' do
    tables = @klass.execute_procedure :sp_tables, 'sql_server_chronics'
    table_info = tables.first
    assert_equal 1, tables.size
    assert_equal (ENV['ARUNIT_DB_NAME'] || 'activerecord_unittest'), table_info[:TABLE_QUALIFIER], "Table Info: #{table_info.inspect}"
    assert_equal 'TABLE', table_info[:TABLE_TYPE], "Table Info: #{table_info.inspect}"
  end
  
  should 'quote bind vars correctly' do
    assert_sql(/EXEC sp_tables '%sql_server%', NULL, NULL, NULL, 1/) do
      @klass.execute_procedure :sp_tables, '%sql_server%', nil, nil, nil, true
    end if sqlserver_2005? || sqlserver_2008?
    assert_sql(/EXEC sp_tables '%sql_server%', NULL, NULL, NULL/) do
      @klass.execute_procedure :sp_tables, '%sql_server%', nil, nil, nil
    end if sqlserver_2000?
  end
  
  should 'allow multiple result sets to be returned' do
    results1, results2 = @klass.execute_procedure('sp_helpconstraint','accounts')
    assert_instance_of Array, results1
    assert_instance_of HashWithIndifferentAccess, results1.first
    assert results1.first['Object Name']
    assert_instance_of Array, results2
    assert_instance_of HashWithIndifferentAccess, results2.first
    assert results2.first['constraint_name']
    assert results2.first['constraint_type']
  end
  
  
end
