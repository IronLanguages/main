require File.dirname(__FILE__) + '/../spec_helper'

describe Merb::Cache::CacheRequest do
  it "should subclass Merb::Request" do
    Merb::Cache::CacheRequest.superclass.should == Merb::Request
  end

  describe "#path" do
    it "can be specified without manipulating the env" do
      Merb::Cache::CacheRequest.new('/path/to/foo').path.should == '/path/to/foo'
    end
  end

  describe "#params" do
    it "can be specified without manipulating the env" do
      Merb::Cache::CacheRequest.new('/', 'foo' => 'bar').params.should == {'foo' => 'bar'}
    end
  end

  describe "#env" do
    it "can be specified in the constructor" do
      Merb::Cache::CacheRequest.new('', {}, 'foo' => 'bar').env['foo'].should == 'bar'
    end
  end

  it "should setup a default env" do
    Merb::Cache::CacheRequest.new('').env.should_not be_empty
  end
end