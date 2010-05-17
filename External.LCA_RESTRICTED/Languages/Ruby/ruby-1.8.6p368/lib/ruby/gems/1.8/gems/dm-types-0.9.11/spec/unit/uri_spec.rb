require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::URI do

  before(:each) do
    @uri_str = "http://example.com/path/to/resource/"
    @uri = Addressable::URI.parse(@uri_str)
  end

  describe ".dump" do
    it "should return the URI as a String" do
      DataMapper::Types::URI.dump(@uri, :property).should == @uri_str
    end

    it "should return nil if the String is nil" do
      DataMapper::Types::URI.dump(nil, :property).should be_nil
    end

    it "should return an empty URI if the String is empty" do
      DataMapper::Types::URI.dump("", :property).should == ""
    end
  end

  describe ".load" do
    it "should return the URI as Addressable" do
      DataMapper::Types::URI.load(@uri_str, :property).should == @uri
    end

    it "should return nil if given nil" do
      DataMapper::Types::URI.load(nil, :property).should be_nil
    end

    it "should return an empty URI if given an empty String" do
      DataMapper::Types::URI.load("", :property).should == Addressable::URI.parse("")
    end
  end

  describe '.typecast' do
    it 'should do nothing if an Addressable::URI is provided' do
      DataMapper::Types::URI.typecast(@uri, :property).should == @uri
    end

    it 'should defer to .load if a string is provided' do
      DataMapper::Types::URI.should_receive(:load).with(@uri_str, :property)
      DataMapper::Types::URI.typecast(@uri_str, :property)
    end
  end
end
