require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe "OpenSSL::X509::Certificate#subject" do

  it 'is a OpenSSL::X509::Name' do
    x509_cert = OpenSSL::X509::Certificate.new
    x509_cert.subject.class.should == OpenSSL::X509::Name
  end

  it 'is empty by default' do
    x509_cert = OpenSSL::X509::Certificate.new
    x509_cert.subject.to_s.should be_empty
  end
  
  it 'returns the subject' do
    x509_cert = OpenSSL::X509::Certificate.new(X509Constants::X509CERT)
    x509_cert.subject.to_s.should == X509Constants::X509Subject
  end
end

describe "OpenSSL::X509::Certificate#subject=" do
  before :each do
    @x509_cert = OpenSSL::X509::Certificate.new
  end
  
  it 'returns the argument' do
    # TODO: Initialize Name with value
    n = OpenSSL::X509::Name.new
    (@x509_cert.subject=(n)).should equal(n)
  end
  
  it 'raises TypeError if argument is nil' do
    lambda { @x509_cert.subject = nil }.should raise_error(TypeError)
  end
  
  it 'raises TypeError if argument is not a OpenSSL::X509::Name' do
    lambda { @x509_cert.subject = "subject" }.should raise_error(TypeError)
  end
end
