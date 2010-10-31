require File.dirname(__FILE__) + '/../../../spec_helper'
require 'openssl'

describe "OpenSSL::SSL::SSLError" do

  it "inherits from OpenSSL::OpenSSLError" do
    OpenSSL::SSL::SSLError.superclass.should == OpenSSL::OpenSSLError
  end
end

describe "OpenSSL::SSL::SSLError.new" do
  it "can be called with no arguments" do
    OpenSSL::SSL::SSLError.new.should_not be_nil
  end

  it "can be called with one arguments" do
    OpenSSL::SSL::SSLError.new("hello").should_not be_nil
  end
end