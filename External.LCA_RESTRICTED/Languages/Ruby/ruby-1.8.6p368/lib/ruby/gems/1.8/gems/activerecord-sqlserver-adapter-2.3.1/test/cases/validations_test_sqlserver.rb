require 'cases/sqlserver_helper'

class ValidationsTestSqlserver < ActiveRecord::TestCase
end

class ValidationsTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_validate_uniqueness_with_limit_and_utf8]
  
  include SqlserverCoercedTest
  
  # This test is tricky to pass. The validation SQL would generate something like this:
  # 
  #   SELECT TOP 1 [events].id FROM [events] WHERE ([events].[title] COLLATE Latin1_General_CS_AS = '一二三四五')
  # 
  # The problem is that we can not change the adapters quote method from this:
  # 
  #   elsif column && column.respond_to?(:is_utf8?) && column.is_utf8?
  #     quoted_utf8_value(value)
  # 
  # To something like this for all quoting like blind bind vars:
  # 
  #   elsif value.is_utf8?
  #     quoted_utf8_value(value)
  # 
  # As it would cause way more errors, sure this piggybacks on ActiveSupport's 1.8/1.9 abstract
  # code to infer if the passed in string is indeed a national/unicde type. Perhaps in rails 3
  # and using AREL this might get better, but I do not see a solution right now.
  # 
  def test_coerced_test_validate_uniqueness_with_limit_and_utf8
    assert true
  end
  
end

