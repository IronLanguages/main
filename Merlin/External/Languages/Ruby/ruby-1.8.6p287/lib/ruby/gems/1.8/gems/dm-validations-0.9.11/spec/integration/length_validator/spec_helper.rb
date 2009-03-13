class MotorLaunch
  include DataMapper::Resource
  property :id, Integer, :serial => true
  property :name, String, :auto_validation => false
end

class BoatDock
  include DataMapper::Resource
  property :id, Integer, :serial => true
  property :name, String, :auto_validation => false, :default => "I'm a long string"
  validates_length :name, :min => 3
end
