require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../shared/constants'
require 'openssl'

describe "OpenSSL::SSL constants" do

  it "returns VERIFY_NONE" do
    OpenSSL::SSL::VERIFY_NONE.should == SSLConstants::VERIFY_NONE
  end

  it "returns VERIFY_PEER" do
    OpenSSL::SSL::VERIFY_PEER.should == SSLConstants::VERIFY_PEER
  end
end