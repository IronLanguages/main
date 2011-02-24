require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe "OpenSSL::SSL::SSLContext" do
  it "is a Class" do
    OpenSSL::SSL::SSLContext.should be_kind_of(Class)
  end
end