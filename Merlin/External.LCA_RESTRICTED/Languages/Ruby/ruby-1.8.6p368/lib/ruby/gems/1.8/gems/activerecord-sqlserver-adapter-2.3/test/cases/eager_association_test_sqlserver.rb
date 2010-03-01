require 'cases/sqlserver_helper'
require 'models/post'
require 'models/author'
require 'models/comment'

class EagerAssociationTestSqlserver < ActiveRecord::TestCase
end

class EagerAssociationTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [
    :test_count_with_include,
    :test_eager_with_has_many_and_limit_and_high_offset_and_multiple_array_conditions,
    :test_eager_with_has_many_and_limit_and_high_offset_and_multiple_hash_conditions
  ]
  
  include SqlserverCoercedTest
  
  fixtures :authors, :posts, :comments
  
  def test_coerced_test_count_with_include
    assert_equal 3, authors(:david).posts_with_comments.count(:conditions => "len(comments.body) > 15")
  end
  
  def test_coerced_eager_with_has_many_and_limit_and_high_offset_and_multiple_array_conditions
    assert_queries(2) do
      posts = Post.find(:all, :include => [ :author, :comments ], :limit => 2, :offset => 10,
        :conditions => [ "authors.name = ? and comments.body = ?", 'David', 'go crazy' ])
      assert_equal 0, posts.size
    end
  end

  def test_coerced_eager_with_has_many_and_limit_and_high_offset_and_multiple_hash_conditions
    assert_queries(2) do
      posts = Post.find(:all, :include => [ :author, :comments ], :limit => 2, :offset => 10,
        :conditions => { 'authors.name' => 'David', 'comments.body' => 'go crazy' })
      assert_equal 0, posts.size
    end
  end unless active_record_2_point_2?
  
  
end
