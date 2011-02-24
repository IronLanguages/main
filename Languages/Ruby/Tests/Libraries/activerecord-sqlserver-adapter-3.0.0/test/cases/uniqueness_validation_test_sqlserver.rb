# encoding: utf-8
require 'cases/sqlserver_helper'
require 'models/event'

class Event < ActiveRecord::Base
  before_validation :strip_mb_chars_for_sqlserver
  protected
  def strip_mb_chars_for_sqlserver
    self.title = title.mb_chars.to(4).to_s if title && title.is_utf8?
  end
end

class UniquenessValidationTestSqlserver < ActiveRecord::TestCase
end

class UniquenessValidationTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [:test_validate_uniqueness_with_limit_and_utf8]
  
  include SqlserverCoercedTest
  
  # I guess most databases just truncate a string when inserting. To pass this test we do a few things.
  # First, we make sure the type is unicode safe, second we extend the limit to well beyond what is 
  # needed. At the top we make sure to auto truncate the :title string like other databases would do 
  # automatically.
  # 
  #   "一二三四五".mb_chars.size           # => 5 
  #   "一二三四五六七八".mb_chars.size        # => 8 
  #   "一二三四五六七八".mb_chars.to(4).to_s  # => "一二三四五"
  
  def test_coerced_validate_uniqueness_with_limit_and_utf8
    with_kcode('UTF8') do
      Event.connection.change_column :events, :title, :nvarchar, :limit => 30
      Event.reset_column_information
      # Now the actual test copied from core.
      e1 = Event.create(:title => "一二三四五")
      assert e1.valid?, "Could not create an event with a unique, 5 character title"
      e2 = Event.create(:title => "一二三四五六七八")
      assert !e2.valid?, "Created an event whose title, with limit taken into account, is not unique"
    end
  end
  
end

