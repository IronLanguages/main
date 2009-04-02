class Location
  include DataMapper::Resource
  is :viewable

  has n, :people, :class_name => 'Person'

  property :id, Serial
  property :name, String
  property :description, String
end
