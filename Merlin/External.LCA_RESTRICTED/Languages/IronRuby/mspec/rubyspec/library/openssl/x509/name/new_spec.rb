require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe "OpenSSL::X509::Name.new" do

  it 'returns a x509 Name' do
    x509_cert = OpenSSL::X509::Name.new
    x509_cert.subject.should == ""
    x509_cert.issuer.should == ""
  end
end
