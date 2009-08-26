require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe "OpenSSL::X509::Name.new" do

  it 'returns a x509 Name' do
    name = OpenSSL::X509::Name.new
    name.to_s.should be_empty
  end
end
