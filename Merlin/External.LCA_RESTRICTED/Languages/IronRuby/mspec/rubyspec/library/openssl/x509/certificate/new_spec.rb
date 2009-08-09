require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe "OpenSSL::X509::Certificate.new" do

  it 'returns a x509 certificate' do
    x509_cert = OpenSSL::X509::Certificate.new
    x509_cert.subject.class.should == OpenSSL::X509::Name
    x509_cert.issuer.class.should == OpenSSL::X509::Name
  end
  
  it 'create a x509 certificate with data' do
    x509_cert = OpenSSL::X509::Certificate.new(X509Constants::X509CERT)
    x509_cert.subject.to_s.should == X509Constants::X509Subject
    x509_cert.issuer.to_s.should == X509Constants::X509Issuer
  end
  
  it 'raises CertificateError if argument is supply not enought data' do
    lambda { OpenSSL::X509::Certificate.new("test") }.should raise_error(OpenSSL::X509::CertificateError)
  end
end
