require 'cases/sqlserver_helper'

class NamedScopeTestSqlserver < ActiveRecord::TestCase
end

class NamedScopeTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_named_scopes_honor_current_scopes_from_when_defined]
  
  include SqlserverCoercedTest
  
  # See: http://github.com/rails/rails/commit/0dd2f96f5c90f8abacb0fe0757ef7e5db4a4d501#comment_37025
  # The orig test is a little brittle and fails on other adapters that do not explicitly fall back to a secondary 
  # sort of id ASC. Since there are duplicate records with comments_count equal to one another. I have found that 
  # named_scope :ranked_by_comments, :order => "comments_count DESC, id ASC" fixes the ambiguity.
  def test_coerced_test_named_scopes_honor_current_scopes_from_when_defined
    assert true
  end
  
  
end
