require 'cases/sqlserver_helper'
require 'models/book'

class OffsetAndLimitTestSqlserver < ActiveRecord::TestCase
  
  class Account < ActiveRecord::Base; end
  
  def setup
    @connection = ActiveRecord::Base.connection
  end
  
  context 'When selecting with limit' do

    setup do
      @select_sql = 'SELECT * FROM schema'
    end

    should 'alter SQL to limit number of records returned' do
      options = { :limit => 10 }
      assert_equal('SELECT TOP 10 * FROM schema', @connection.add_limit_offset!(@select_sql, options))
    end

    should 'only allow integers for limit' do
      options = { :limit => 'ten' }
      assert_raise(ArgumentError) {@connection.add_limit_offset!(@select_sql, options) }
    end

    should 'convert strings which look like integers to integers' do
      options = { :limit => '42' }
      assert_nothing_raised(ArgumentError) {@connection.add_limit_offset!(@select_sql, options)}
    end

    should 'not allow sql injection via limit' do
      options = { :limit => '1 * FROM schema; DELETE * FROM table; SELECT TOP 10 *'}
      assert_raise(ArgumentError) { @connection.add_limit_offset!(@select_sql, options) }
    end

  end
  
  context 'When selecting with limit and offset' do

    setup do
      @select_sql = 'SELECT * FROM books'
      @subquery_select_sql = 'SELECT *, (SELECT TOP 1 id FROM books) AS book_id FROM books'
      @books = (1..10).map {|i| Book.create!}
    end
    
    teardown do
      @books.each {|b| b.destroy}
    end

    should 'have limit if offset is passed' do
      options = { :offset => 1 }
      assert_raise(ArgumentError) { @connection.add_limit_offset!(@select_sql, options) }
    end

    should 'only allow integers for offset' do
      options = { :limit => 10, :offset => 'five' }
      assert_raise(ArgumentError) { @connection.add_limit_offset!(@select_sql, options)}
    end

    should 'convert strings which look like integers to integers' do
      options = { :limit => 10, :offset => '5' }
      assert_nothing_raised(ArgumentError) {@connection.add_limit_offset!(@select_sql, options)}
    end

    should 'alter SQL to limit number of records returned offset by specified amount' do
      options = { :limit => 3, :offset => 5 }
      expected_sql = "SELECT * FROM (SELECT TOP 3 * FROM (SELECT TOP 8 * FROM books) AS tmp1) AS tmp2"
      assert_equal(expected_sql, @connection.add_limit_offset!(@select_sql, options))
    end
    
    should 'add locks to deepest sub select in limit offset sql that has a limited tally' do
      options = { :limit => 3, :offset => 5, :lock => 'WITH (NOLOCK)' }
      expected_sql = "SELECT * FROM (SELECT TOP 3 * FROM (SELECT TOP 8 * FROM books WITH (NOLOCK)) AS tmp1) AS tmp2"
      @connection.add_limit_offset! @select_sql, options
      assert_equal expected_sql, @connection.add_lock!(@select_sql,options)
    end

    # Not really sure what an offset sql injection might look like
    should 'not allow sql injection via offset' do
      options = { :limit => 10, :offset => '1 * FROM schema; DELETE * FROM table; SELECT TOP 10 *'}
      assert_raise(ArgumentError) { @connection.add_limit_offset!(@select_sql, options) }
    end

    should 'not create invalid SQL with subquery SELECTs with TOP' do
      options = { :limit => 5, :offset => 1 }
      expected_sql = "SELECT * FROM (SELECT TOP 5 * FROM (SELECT TOP 6 *, (SELECT TOP 1 id FROM books) AS book_id FROM books) AS tmp1) AS tmp2"
      assert_equal expected_sql, @connection.add_limit_offset!(@subquery_select_sql,options)
    end
    
    should 'add lock hints to tally sql if :lock option is present' do
      assert_sql %r|SELECT TOP 1000000000 \* FROM \[people\] WITH \(NOLOCK\)| do
        Person.all :limit => 5, :offset => 1, :lock => 'WITH (NOLOCK)'
      end
    end
    
    should 'not add lock hints to tally sql if there is no :lock option' do
      assert_sql %r|\(SELECT TOP 1000000000 \* FROM \[people\] \)| do
        Person.all :limit => 5, :offset => 1
      end
    end
    
  end
  
  
end

