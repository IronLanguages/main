require 'cases/sqlserver_helper'
require 'models/developer'

class BasicsTestSqlserver < ActiveRecord::TestCase
end

class BasicsTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_read_attributes_before_type_cast_on_datetime]
  
  include SqlserverCoercedTest
  
  fixtures :developers
  
  def test_coerced_test_read_attributes_before_type_cast_on_datetime
    developer = Developer.find(:first)
    assert_equal developer.created_at.to_s(:db)+'.000' , developer.attributes_before_type_cast["created_at"]
  end
  
  
end
