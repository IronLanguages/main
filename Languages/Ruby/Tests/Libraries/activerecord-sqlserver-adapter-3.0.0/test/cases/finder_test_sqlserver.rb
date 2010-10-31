require 'cases/sqlserver_helper'
require 'models/event'

class FinderTestSqlserver < ActiveRecord::TestCase
end

class FinderTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_string_sanitation]
  
  include SqlserverCoercedTest
  
  def test_coerced_string_sanitation
    assert_not_equal "'something ' 1=1'", ActiveRecord::Base.sanitize("something ' 1=1")
    if quote_values_as_utf8?
      assert_equal "N'something; select table'", ActiveRecord::Base.sanitize("something; select table")
    else
      assert_equal "'something; select table'", ActiveRecord::Base.sanitize("something; select table")
    end
  end
  
end

