require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../shared/constants'
require 'openssl'

describe "OpenSSL::SSL::SSLContext" do
  it "is a Class" do
    OpenSSL::SSL::SSLContext.should be_kind_of(Class)
  end
  
  describe "OpenSSL::SSL::SSLContext#verify_mode" do
    before :each do
      @ctx = OpenSSL::SSL::SSLContext.new
    end
	
    it "returns nil by default" do
      @ctx.verify_mode.should be_nil
    end
	
    it "returns the value when set to VERIFY_NONE" do
      @ctx.verify_mode = OpenSSL::SSL::VERIFY_NONE
      @ctx.verify_mode.should == SSLConstants::VERIFY_NONE
    end
  end
end