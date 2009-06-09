require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::IPAddress do
  before(:all) do
    class ::IPAddressTest
      include DataMapper::Resource

      property :id, Serial
      property :ip, IPAddress
    end
    IPAddressTest.auto_migrate!
  end

  it "should work" do
    repository(:default) do
      IPAddressTest.create(:ip => '127.0.0.1')
    end

    IPAddressTest.first.ip.should == IPAddr.new('127.0.0.1')
  end

  it 'should immediately typecast supplied values' do
    IPAddressTest.new(:ip => '10.0.0.1').ip.should == IPAddr.new('10.0.0.1')
  end
end
