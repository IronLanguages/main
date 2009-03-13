module Viewable
  include DataMapper::Resource

  is :remixable,
    :suffix => "view"

  property :id, Integer, :key => true, :serial => true

  property :created_at, DateTime
  property :ip, String
end
