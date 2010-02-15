require 'cases/sqlserver_helper'
require 'models/task'

class QueryCacheTestSqlserver < ActiveRecord::TestCase
end

class QueryCacheTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_cache_does_not_wrap_string_results_in_arrays]
  
  include SqlserverCoercedTest
  
  fixtures :tasks
  

  def test_coerced_test_cache_does_not_wrap_string_results_in_arrays
    Task.cache do
      assert_instance_of Fixnum, Task.connection.select_value("SELECT count(*) AS count_all FROM tasks")
    end
  end
  
end


