# ==========================
# Used for Association specs
# ---
# These models will probably
# end up removed. So, I wouldn't
# use this metaphor
class Vehicle
  include DataMapper::Resource

  property :id, Serial
  property :name, String

  class << self
    attr_accessor :mock_relationship
  end
end

class Manufacturer
  include DataMapper::Resource

  property :id, Serial
  property :name, String

  class << self
    attr_accessor :mock_relationship
  end
end

class Supplier
  include DataMapper::Resource

  property :id, Serial
  property :name, String
end
