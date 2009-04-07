class Person
  include DataMapper::Resource
  is :viewable

  create_view :satan, :favorite_number => 666

  property :id, Serial
  property :name, String

  property :favorite_color, String
  property :favorite_number, Integer

  belongs_to :location
end
