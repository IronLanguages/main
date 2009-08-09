require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe "OpenSSL::PKey::RSA.new" do

  it 'new without params' do
    rsa = OpenSSL::PKey::RSA.new
    rsa.to_s.should == PrivateKeyConstants::RSABlank
  end
  
  it 'new with key size' do
    rsa = OpenSSL::PKey::RSA.new(2048)
    rsa.to_s.should_not be_empty
  end
  
  it 'new with data' do
    rsa = OpenSSL::PKey::RSA.new(PrivateKeyConstants::RSAKey)
    rsa.to_s.should == PrivateKeyConstants::RSAKey
  end
end
