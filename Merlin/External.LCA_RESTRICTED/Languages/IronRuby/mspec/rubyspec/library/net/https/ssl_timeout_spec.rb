require File.dirname(__FILE__) + '/../../../spec_helper'
require 'net/https'

describe "Net::HTTP#ssl_timeout=" do
  it "raises ArgumentError when set while use_ssl is false" do
    net = Net::HTTP.new('localhost')
    net.use_ssl = false
    lambda { net.ssl_timeout = 10 }.should raise_error(ArgumentError)
  end
end