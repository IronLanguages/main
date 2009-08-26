require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::IPAddress do

  before(:each) do
    @ip_str = "81.20.130.1"
    @ip = IPAddr.new(@ip_str)
  end

  describe ".dump" do
    it "should return the IP address as a string" do
      DataMapper::Types::IPAddress.dump(@ip, :property).should == @ip_str
    end

    it "should return nil if the string is nil" do
      DataMapper::Types::IPAddress.dump(nil, :property).should be_nil
    end

    it "should return an empty IP address if the string is empty" do
      DataMapper::Types::IPAddress.dump("", :property).should == ""
    end
  end

  describe ".load" do
    it "should return the IP address string as IPAddr" do
      DataMapper::Types::IPAddress.load(@ip_str, :property).should == @ip
    end

    it "should return nil if given nil" do
      DataMapper::Types::IPAddress.load(nil, :property).should be_nil
    end

    it "should return an empty IP address if given an empty string" do
      DataMapper::Types::IPAddress.load("", :property).should == IPAddr.new("0.0.0.0")
    end

    it 'should raise an ArgumentError if given something else' do
      lambda {
        DataMapper::Types::IPAddress.load([], :property)
      }.should raise_error(ArgumentError, '+value+ must be nil or a String')
    end
  end

  describe '.typecast' do
    it 'should do nothing if an IpAddr is provided' do
      DataMapper::Types::IPAddress.typecast(@ip, :property).should == @ip
    end

    it 'should defer to .load if a string is provided' do
      DataMapper::Types::IPAddress.should_receive(:load).with(@ip_str, :property)
      DataMapper::Types::IPAddress.typecast(@ip_str, :property)
    end
  end

end
