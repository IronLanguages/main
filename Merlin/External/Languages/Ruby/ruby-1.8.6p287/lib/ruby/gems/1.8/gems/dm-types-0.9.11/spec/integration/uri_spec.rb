require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::URI do
  before(:all) do
    class ::URITest
      include DataMapper::Resource

      property :id, Integer, :serial => true
      property :uri, DM::URI
    end
    URITest.auto_migrate!
  end

  it "should work" do
    repository(:default) do
      URITest.create(:uri => 'http://localhost')
    end

    URITest.first.uri.should == Addressable::URI.parse('http://localhost')
  end

  it 'should immediately typecast supplied values' do
    URITest.new(:uri => 'http://localhost').uri.should == Addressable::URI.parse('http://localhost')
  end

  it "should correctly typecast nil values" do
    URITest.new(:uri => nil).uri.should == nil
  end

end
