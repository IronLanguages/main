require File.dirname(__FILE__) + '/../../../spec_helper'
require 'net/https'
require File.dirname(__FILE__) + '/../../net/http/http/fixtures/http_server'

describe "Net::HTTP#use_ssl=" do
  before(:all) do
    NetHTTPSpecs.start_server
  end
  
  after(:all) do
    NetHTTPSpecs.stop_server
  end
  
  it "raises IOError when changed after start" do
    net = Net::HTTP.start("localhost", 3333)
    lambda { net.use_ssl = true }.should raise_error(IOError)
  end
end