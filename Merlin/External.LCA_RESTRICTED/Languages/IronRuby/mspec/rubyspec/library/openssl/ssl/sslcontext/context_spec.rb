require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/constants'
require 'openssl'

describe 'OpenSSL::SSL::SSLContext' do
  it 'is an OpenSSL::SSL::SSLContext' do
    OpenSSL::SSL::SSLContext.should == OpenSSL::SSL::SSLContext
  end
  
  it 'class is a Class' do
    OpenSSL::SSL::SSLContext.class.should == Class
  end
end

describe 'OpenSSL::SSL::SSLContext#verify_mode' do
  before :each do
    @ctx = OpenSSL::SSL::SSLContext.new
  end

  it 'should be able to use nil to set/get verify_mode' do
    @ctx.verify_mode = nil
    @ctx.verify_mode.should == nil
  end
  
  it 'should be able to use VERIFY_NONE to set/get verify_mode' do
    @ctx.verify_mode = OpenSSL::SSL::VERIFY_NONE
    @ctx.verify_mode.should == OpenSSL::SSL::VERIFY_NONE
  end
end