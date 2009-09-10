# The filename begins with "aaaa" to ensure this is the first test.
require 'cases/sqlserver_helper'

class AAAACreateTablesTestSqlserver < ActiveRecord::TestCase
  self.use_transactional_fixtures = false
  
  should 'load activerecord schema' do
    schema_file = "#{ACTIVERECORD_TEST_ROOT}/schema/schema.rb"
    eval(File.read(schema_file))
    assert true
  end
  
  should 'load sqlserver specific schema' do
    sqlserver_specific_schema_file = "#{SQLSERVER_SCHEMA_ROOT}/sqlserver_specific_schema.rb"
    eval(File.read(sqlserver_specific_schema_file))
    assert true
  end
  
end
