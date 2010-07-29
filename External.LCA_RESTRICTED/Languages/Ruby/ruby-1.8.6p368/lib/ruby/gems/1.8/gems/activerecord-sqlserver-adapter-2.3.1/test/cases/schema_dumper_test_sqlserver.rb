require 'cases/sqlserver_helper'

class SchemaDumperTestSqlserver < ActiveRecord::TestCase
  
  setup :find_all_tables
  
  context 'For primary keys' do

    should 'honor nonstandards' do
      table_dump('movies') do |output|
        match = output.match(%r{create_table "movies"(.*)do})
        assert_not_nil(match, "nonstandardpk table not found")
        assert_match %r(:primary_key => "movieid"), match[1], "non-standard primary key not preserved"
      end
    end
    
  end
  
  context 'For integers' do
    
    should 'include limit constraint that match logic for smallint and bigint in #extract_limit' do
      table_dump('integer_limits') do |output|
        assert_match %r{c_int_1.*:limit => 2}, output
        assert_match %r{c_int_2.*:limit => 2}, output
        assert_match %r{c_int_3.*}, output
        assert_match %r{c_int_4.*}, output
        assert_no_match %r{c_int_3.*:limit}, output
        assert_no_match %r{c_int_4.*:limit}, output
        assert_match %r{c_int_5.*:limit => 8}, output
        assert_match %r{c_int_6.*:limit => 8}, output
        assert_match %r{c_int_7.*:limit => 8}, output
        assert_match %r{c_int_8.*:limit => 8}, output
      end
    end
    
  end
  
  context 'For strings' do

    should 'have varchar(max) dumped as text' do
      table_dump('sql_server_strings') do |output|
        assert_match %r{t.text.*varchar_max}, output
      end
    end if sqlserver_2005? || sqlserver_2008?

  end
  
  
  
  
  private
  
  def find_all_tables
    @all_tables ||= ActiveRecord::Base.connection.tables
  end
  
  def standard_dump(ignore_tables = [])
    stream = StringIO.new
    ActiveRecord::SchemaDumper.ignore_tables = [*ignore_tables]
    ActiveRecord::SchemaDumper.dump(ActiveRecord::Base.connection, stream)
    stream.string
  end
  
  def table_dump(*table_names)
    stream = StringIO.new
    ActiveRecord::SchemaDumper.ignore_tables = @all_tables-table_names
    ActiveRecord::SchemaDumper.dump(ActiveRecord::Base.connection, stream)
    yield stream.string
    stream.string
  end
  
end
