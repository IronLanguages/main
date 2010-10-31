require 'cases/sqlserver_helper'
require 'models/book'
require 'models/post'

class OffsetAndLimitTestSqlserver < ActiveRecord::TestCase
  
  fixtures :posts
  
  setup     :create_10_books
  teardown  :destroy_all_books
  
  
  context 'When selecting with limit' do
  
    should 'alter sql to limit number of records returned' do
      assert_sql(/SELECT TOP \(10\)/) { Book.limit(10).all }
    end
  
    should 'only allow integers for limit' do
      assert_sql(/SELECT TOP \(20\)/) { Book.limit('20-twenty').all }
    end
  
  end
  
  context 'When selecting with offset' do

    should 'have no limit (top) if only offset is passed' do
      assert_sql(/SELECT \[__rnt\]\.\* FROM.*WHERE \[__rnt\]\.\[__rn\] > 1/) { Book.all(:offset=>1) }
    end

  end
  
  context 'When selecting with limit and offset' do
    
    should 'only allow integers for offset' do
      assert_sql(/WHERE \[__rnt\]\.\[__rn\] > 0/) { Book.limit(10).offset('five').all }
    end
    
    should 'convert strings which look like integers to integers' do
      assert_sql(/WHERE \[__rnt\]\.\[__rn\] > 5/) { Book.limit(10).offset('5').all }
    end

    should 'alter SQL to limit number of records returned offset by specified amount' do
      sql = %|SELECT TOP (3) [__rnt].* 
              FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY [books].[id]) AS [__rn], [books].* 
                FROM [books]
              ) AS [__rnt]
              WHERE [__rnt].[__rn] > 5|.squish
      assert_sql(sql) { Book.limit(3).offset(5).all }
    end
    
    should 'add locks to deepest sub select' do
      pattern = /FROM \[books\] WITH \(NOLOCK\)/
      assert_sql(pattern) { Book.all :limit => 3, :offset => 5, :lock => 'WITH (NOLOCK)' }
      assert_sql(pattern) { Book.count :limit => 3, :offset => 5, :lock => 'WITH (NOLOCK)' }
    end
    
    context 'with count' do

      should 'pass a gauntlet of window tests' do
        assert_equal 7, Post.count
        assert_equal 1, Post.limit(1).offset(1).size
        assert_equal 1, Post.limit(1).offset(5).size
        assert_equal 1, Post.limit(1).offset(6).size
        assert_equal 0, Post.limit(1).offset(7).size
        assert_equal 3, Post.limit(3).offset(4).size
        assert_equal 2, Post.limit(3).offset(5).size
        assert_equal 1, Post.limit(3).offset(6).size
        assert_equal 0, Post.limit(3).offset(7).size
        assert_equal 0, Post.limit(3).offset(8).size
      end

    end
    
  end
  
  
  protected
  
  def create_10_books
    Book.delete_all
    @books = (1..10).map {|i| Book.create!}
  end
  
  def destroy_all_books
    @books.each { |b| b.destroy }
  end
  
end

