module Addressable
  include DataMapper::Resource

  is :remixable,
    :suffix => "address"

  property :id,         Integer, :key => true, :serial => true

  property :address1,   String, :length => 255
  property :address2,   String, :length => 255

  property :city,       String, :length => 128
  property :state,      String, :length => 2
  property :zip,        String, :length => 5..10
end
