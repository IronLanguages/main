class Zoo
  include DataMapper::Resource

  property :id,           Serial
  property :name,         String
  property :description,  Text
  property :inception,    DateTime
  property :open,         Boolean,  :default => false
  property :size,         Integer
  property :mission,      Text, :writer => :protected

  has n, :animals

  def to_s
    name
  end
end

class Species
  include DataMapper::Resource

  property :id,             Serial
  property :name,           String
  property :classification, String, :reader => :private

  has n, :animals
end

class Animal
  include DataMapper::Resource

  property :id,   Serial
  property :name, String

  belongs_to :zoo
  belongs_to :species
  belongs_to :keeper
end

class Employee
  include DataMapper::Resource

  property :name, String, :key => true
end

class Keeper < Employee
  has n, :animals
end
