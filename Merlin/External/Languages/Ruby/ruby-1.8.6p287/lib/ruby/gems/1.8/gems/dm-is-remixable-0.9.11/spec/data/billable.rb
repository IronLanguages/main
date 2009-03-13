module Billable
  include DataMapper::Resource

  is :remixable,
    :suffix => "billing_account"

  property :id,         Integer, :key => true, :serial => true

  property :cc_type,    Enum.new("mastercard","amex","visa")
  property :cc_num,     String, :length => 12..20
  property :expiration, Date
end
