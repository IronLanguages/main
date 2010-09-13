require 'profile/helper'
require 'models/topic'
require 'models/reply'

class FinderProfileCase < ActiveRecord::TestCase
  
  fixtures :topics
  
  def test_find_all
    ruby_profile :finder_find_all do
      1000.times { Topic.all }
    end
  end
  
  
end


