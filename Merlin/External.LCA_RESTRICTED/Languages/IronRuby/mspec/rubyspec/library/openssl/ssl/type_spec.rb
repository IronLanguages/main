require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../shared/constants'
require 'openssl'

describe "OpenSSL::SSL" do
  it "is a Module" do
    OpenSSL::SSL.should be_kind_of(Module)
  end
end