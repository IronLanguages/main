require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe "OpenSSL::X509::Certificate.new" do

  it 'returns a x509 certificate' do
    x509_cert = OpenSSL::X509::Certificate.new
    x509_cert.subject.should == ""
    x509_cert.issuer.should == ""
  end
  
  it 'create a x509 certificate with data' do
    x509_cert = OpenSSL::X509::Certificate.new(HMACConstants::X509CERT)
    x509_cert.subject.should == HMACConstants::X509Subject
    x509_cert.issuer.should == HMACConstants::X509Issuer
  end
  
  it 'raises CertificateError if argument is supply not enought data' do
    lambda { OpenSSL::X509::Certificate.new("test") }.should raise_error(OpenSSL::X509::CertificateError)
  end
end
