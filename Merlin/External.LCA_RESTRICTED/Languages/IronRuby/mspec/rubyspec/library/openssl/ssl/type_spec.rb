require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../shared/constants'
require 'openssl'

describe 'OpenSSL::SSL' do
  it 'is an OpenSSL::SSL' do
    OpenSSL::SSL.should == OpenSSL::SSL
  end
  
  it 'is a Module' do
    OpenSSL::SSL.class.should == Module
  end
end