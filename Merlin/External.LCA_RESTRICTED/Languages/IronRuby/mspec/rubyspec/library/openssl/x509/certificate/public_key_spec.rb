require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe "OpenSSL::X509::Certificate#public_key" do
  it 'raises OpenSSL::X509::CertificateError by default' do
    x509_cert = OpenSSL::X509::Certificate.new
    lambda { x509_cert.public_key }.should raise_error(OpenSSL::X509::CertificateError)
  end
  
  it 'returns the public_key' do
    x509_cert = OpenSSL::X509::Certificate.new(X509Constants::X509CERT)
    x509_cert.public_key.to_s.should == X509Constants::X509PublicKey
  end
end

describe "OpenSSL::X509::Certificate#public_key=" do
  before :each do
    @x509_cert = OpenSSL::X509::Certificate.new
  end
  
  it 'raises TypeError if argument is nil' do
    lambda { @x509_cert.public_key = nil }.should raise_error(TypeError)
  end
  
  it 'raises TypeError if argument is not a OpenSSL::PKey::PKey' do
    lambda { @x509_cert.public_key = 10 }.should raise_error(TypeError)
  end
end
