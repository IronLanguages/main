require 'cases/sqlserver_helper'
require 'models/company'

class InheritanceTestSqlserver < ActiveRecord::TestCase
end

class InheritanceTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [
    :test_eager_load_belongs_to_primary_key_quoting,
    :test_a_bad_type_column
  ]
  
  include SqlserverCoercedTest
  
  def test_coerced_test_eager_load_belongs_to_primary_key_quoting
    assert_sql(/\(\[companies\].\[id\] = 1\)/) do
      Account.find(1, :include => :firm)
    end
  end
  
  def test_coerced_test_a_bad_type_column
    Company.connection.insert "INSERT INTO [companies] ([id], #{QUOTED_TYPE}, [name]) VALUES(100, 'bad_class!', 'Not happening')"
    assert_raises(ActiveRecord::SubclassNotFound) { Company.find(100) }
  end
  
  
end
