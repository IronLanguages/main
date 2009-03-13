require Pathname(__FILE__).dirname / "rating"

class Topic
  include DataMapper::Resource

  property :id, Integer, :key => true, :serial => true

  property :name, String
  property :description, String

  remix n, My::Nested::Remixable::Rating,
    :as => :ratings_for_topic,
    :class_name => "Rating"

end
