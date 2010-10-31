require 'cases/sqlserver_helper'
require 'models/company'

class InheritanceTestSqlserver < ActiveRecord::TestCase
end

class InheritanceTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [
    :test_eager_load_belongs_to_primary_key_quoting
  ]
  
  include SqlserverCoercedTest
  
  def test_coerced_test_eager_load_belongs_to_primary_key_quoting
    assert_sql(/\(\[companies\].\[id\] = 1\)/) do
      Account.find(1, :include => :firm)
    end
  end
  
  
end
