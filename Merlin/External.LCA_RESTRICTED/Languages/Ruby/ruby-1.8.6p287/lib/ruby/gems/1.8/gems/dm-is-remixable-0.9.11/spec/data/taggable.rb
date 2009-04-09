module Taggable
  def self.included(base)
    base.extend Taggable::ClassMethods
  end

  include DataMapper::Resource

  is :remixable

  property :id, Integer, :key => true, :serial => true
  property :tag_id, Integer




  module ClassMethods

    def related_tags
      puts "should work"
    end

  end

end
