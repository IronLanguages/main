class Tag
  include DataMapper::Resource

  property :id, Integer, :key => true, :serial => true
  property :name, String, :unique => true, :nullable => false
end
