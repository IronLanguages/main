require 'cases/sqlserver_helper'
require 'models/order'

class TableNameTestSqlserver < ActiveRecord::TestCase
  
  self.use_transactional_fixtures = false
  
  def setup
    Order.table_name = '[orders]'
    Order.reset_column_information
  end
  
  should 'load columns with escaped table name for model' do
    assert_equal 4, Order.columns.length
  end
  
  should 'not re-escape table name if it is escaped already for SQL queries' do
    assert_sql(/SELECT \* FROM \[orders\]/) { Order.all }
  end
  
  
end
