module Commentable
  include DataMapper::Resource

  is :remixable,
    :suffix => "comment"

  property :id,         Integer, :key => true, :serial => true
  property :comment,    String
  property :created_at, DateTime

end
