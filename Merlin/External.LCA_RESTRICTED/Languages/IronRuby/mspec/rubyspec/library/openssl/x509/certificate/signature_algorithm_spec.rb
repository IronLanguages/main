require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe "OpenSSL::X509::Certificate#signature_algorithm" do
  it 'is string "NULL" by default' do
    x509_cert = OpenSSL::X509::Certificate.new
    x509_cert.signature_algorithm.should == "NULL"
  end
  
  it 'returns the not before datetime' do
    x509_cert = OpenSSL::X509::Certificate.new(X509Constants::X509CERT)
    x509_cert.signature_algorithm.should == X509Constants::X509Signature
  end
end
