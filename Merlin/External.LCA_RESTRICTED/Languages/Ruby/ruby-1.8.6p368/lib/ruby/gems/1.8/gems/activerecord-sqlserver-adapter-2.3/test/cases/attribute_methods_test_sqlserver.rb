require 'cases/sqlserver_helper'
require 'models/topic'

class AttributeMethodsTestSqlserver < ActiveRecord::TestCase
end

class AttributeMethodsTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [
    :test_typecast_attribute_from_select_to_false,
    :test_typecast_attribute_from_select_to_true
  ]
  
  include SqlserverCoercedTest
  
  fixtures :topics
  
  
  def test_coerced_typecast_attribute_from_select_to_false
    topic = Topic.create(:title => 'Budget')
    topic = Topic.find(:first, :select => "topics.*, CASE WHEN 1=2 THEN 1 ELSE 0 END as is_test")
    assert !topic.is_test?
  end

  def test_coerced_typecast_attribute_from_select_to_true
    topic = Topic.create(:title => 'Budget')
    topic = Topic.find(:first, :select => "topics.*, CASE WHEN 2=2 THEN 1 ELSE 0 END as is_test")
    assert topic.is_test?
  end
  
  
end

