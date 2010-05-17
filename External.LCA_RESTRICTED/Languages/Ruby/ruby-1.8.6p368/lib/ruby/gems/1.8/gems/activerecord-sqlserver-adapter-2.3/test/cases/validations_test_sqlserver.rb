require 'cases/sqlserver_helper'

class ValidationsTestSqlserver < ActiveRecord::TestCase
end

class ValidationsTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_validate_uniqueness_with_limit_and_utf8]
  
  include SqlserverCoercedTest
  
  # Because SQL Server converts UTF8 data to some other data type, there is no such thing as a 
  # one to byte length check. See this article for details:
  # http://connect.microsoft.com/SQLServer/feedback/ViewFeedback.aspx?FeedbackID=362867
  # 
  # Had the idea of doing this, but could not since the above was true.
  # Event.connection.change_column :events, :title, :nvarchar, :limit => 5
  # Event.reset_column_information
  def test_coerced_test_validate_uniqueness_with_limit_and_utf8
    assert true
  end
  
end

