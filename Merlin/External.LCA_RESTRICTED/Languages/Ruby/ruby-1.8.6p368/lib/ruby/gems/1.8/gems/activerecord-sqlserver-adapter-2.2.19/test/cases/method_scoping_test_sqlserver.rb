require 'cases/sqlserver_helper'
require 'models/developer'

class MethodScopingTestSqlServer < ActiveRecord::TestCase
end

class NestedScopingTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_merged_scoped_find]
  
  include SqlserverCoercedTest
  
  fixtures :developers

  def test_coerced_test_merged_scoped_find
    poor_jamis = developers(:poor_jamis)
    Developer.with_scope(:find => { :conditions => "salary < 100000" }) do
      Developer.with_scope(:find => { :offset => 1, :order => 'id asc' }) do
        assert_sql /ORDER BY id ASC/ do
          assert_equal(poor_jamis, Developer.find(:first, :order => 'id asc'))
        end
      end
    end
  end
  
end


