# -*- coding: utf-8 -*-
class BasketballPlayer
  #
  # Behaviors
  #

  include DataMapper::Resource

  #
  # Properties
  #

  property :id,     Serial
  property :name,   String

  property :height, Float, :auto_validation => false
  property :weight, Float, :auto_validation => false

  #
  # Validations
  #

  validates_is_number :height, :weight
end
BasketballPlayer.auto_migrate!



class City
  #
  # Behaviors
  #

  include DataMapper::Resource

  #
  # Properties
  #

  property :id,         Serial
  property :name,       String

  property :founded_in, Integer, :auto_validation => false

  #
  # Validations
  #

  validates_is_number :founded_in, :message => "Foundation year must be an integer"
end
City.auto_migrate!



class Country
  #
  # Behaviors
  #

  include DataMapper::Resource

  #
  # Properties
  #

  property :id,         Serial
  property :name,       String

  property :area,       String, :integer_only => true

  #
  # Validations
  #

  validates_is_number :area, :message => "Please use integers to specify area"
end
Country.auto_migrate!
