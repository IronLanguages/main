require 'cases/sqlserver_helper'

class CalculationsTestSqlserver < ActiveRecord::TestCase
end

class CalculationsTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_should_sum_expression]
  
  include SqlserverCoercedTest
  
  fixtures :accounts
  
  def test_coerced_should_sum_expression
    assert_equal 636, Account.sum("2 * credit_limit")
  end
  
  
end
