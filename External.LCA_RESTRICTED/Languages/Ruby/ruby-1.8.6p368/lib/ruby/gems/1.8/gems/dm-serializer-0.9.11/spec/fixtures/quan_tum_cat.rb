# Yes, this crazy capitalization is intentional,
# to test xml root element name generation
module QuanTum
  class Cat
    include DataMapper::Resource

    property :id, Serial
    property :name, String
    property :location, String

    repository(:alternate) do
      property :is_dead, Boolean
    end
  end
end
