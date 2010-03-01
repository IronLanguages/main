require 'cases/sqlserver_helper'

class UnicodeTestSqlserver < ActiveRecord::TestCase
  
  
  context 'Testing basic saves and unicode limits' do

    should 'save and reload simple nchar string' do
      assert nchar_data = SqlServerUnicode.create!(:nchar => 'A')
      assert_equal 'A', SqlServerUnicode.find(nchar_data.id).nchar
    end
    
    should 'save and reload simple nvarchar(max) string' do
      test_string = 'Ken Collins'
      assert nvarcharmax_data = SqlServerUnicode.create!(:nvarchar_max => test_string)
      assert_equal test_string, SqlServerUnicode.find(nvarcharmax_data.id).nvarchar_max
    end if sqlserver_2005? || sqlserver_2008?

    should 'enforce default nchar_10 limit of 10' do
      assert_raise(ActiveRecord::StatementInvalid) { SqlServerUnicode.create!(:nchar => '01234567891') }
    end

    should 'enforce default nvarchar_100 limit of 100' do
      assert_raise(ActiveRecord::StatementInvalid) { SqlServerUnicode.create!(:nvarchar_100 => '0123456789'*10+'1') }
    end

  end
  
  context 'Testing unicode data' do

    setup do
      @unicode_data = "\344\270\200\344\272\21434\344\272\224\345\205\255"
      @encoded_unicode_data = "\344\270\200\344\272\21434\344\272\224\345\205\255".force_encoding('UTF-8') if ruby_19?
    end

    should 'insert into nvarchar field' do
      assert data = SqlServerUnicode.create!(:nvarchar => @unicode_data)
      assert_equal @unicode_data, data.reload.nvarchar
    end
    
    should 're-encode data on DB reads' do
      assert data = SqlServerUnicode.create!(:nvarchar => @unicode_data)
      assert_equal @encoded_unicode_data, data.reload.nvarchar
    end if ruby_19?

  end
  
  
  
end
