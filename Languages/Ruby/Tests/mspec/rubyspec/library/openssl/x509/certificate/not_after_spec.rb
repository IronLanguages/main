require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe "OpenSSL::X509::Certificate#not_after" do
  it 'is nil by default' do
    x509_cert = OpenSSL::X509::Certificate.new
    x509_cert.not_after.should be_nil
  end
  
  it 'returns the not before datetime' do
    x509_cert = OpenSSL::X509::Certificate.new(X509Constants::X509CERT)
    x509_cert.not_after.to_s.should == X509Constants::X509NotAfter
  end
end

describe "OpenSSL::X509::Certificate#not_after=" do
  
  it 'returns the argument' do
    n = Time.now
    x509_cert = OpenSSL::X509::Certificate.new
    x509_cert.not_after=(n).should equal(n)
  end
end
