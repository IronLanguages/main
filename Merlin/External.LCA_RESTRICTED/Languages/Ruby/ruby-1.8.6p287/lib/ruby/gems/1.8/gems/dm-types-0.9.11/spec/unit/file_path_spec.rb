require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::FilePath do

  before(:each) do
    @path_str = "/usr/bin/ruby"
    @path = Pathname.new(@path_str)
  end

  describe ".dump" do
    it "should return the file path as a String" do
      DataMapper::Types::FilePath.dump(@path_str, :property).should == @path_str
    end

    it "should return nil if the String is nil" do
      DataMapper::Types::FilePath.dump(nil, :property).should be_nil
    end

    it "should return an empty file path if the String is empty" do
      DataMapper::Types::FilePath.dump("", :property).should == ""
    end
  end

  describe ".load" do
    it "should return the file path as a Pathname" do
      DataMapper::Types::FilePath.load(@uri_str, :property).should == @uri
    end

    it "should return nil if given nil" do
      DataMapper::Types::FilePath.load(nil, :property).should be_nil
    end

    it "should return an empty Pathname if given an empty String" do
      DataMapper::Types::FilePath.load("", :property).should == Pathname.new("")
    end
  end

  describe '.typecast' do
    it 'should do nothing if a Pathname is provided' do
      DataMapper::Types::FilePath.typecast(@path, :property).should == @path
    end

    it 'should defer to .load if a string is provided' do
      DataMapper::Types::FilePath.should_receive(:load).with(@path_str, :property)
      DataMapper::Types::FilePath.typecast(@path_str, :property)
    end
  end
end
