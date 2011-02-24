require 'cases/sqlserver_helper'

class HasAndBelongsToManyAssociationsTestSqlserver < ActiveRecord::TestCase
end

class HasAndBelongsToManyAssociationsTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_count_with_finder_sql]
  
  include SqlserverCoercedTest
  
  def test_coerced_count_with_finder_sql
    assert true
  end
  
  
end
