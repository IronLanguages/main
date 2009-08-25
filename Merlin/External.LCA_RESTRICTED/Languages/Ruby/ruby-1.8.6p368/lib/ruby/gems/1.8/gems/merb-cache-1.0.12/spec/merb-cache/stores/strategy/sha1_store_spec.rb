require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/abstract_strategy_store_spec'

describe Merb::Cache::SHA1Store do
  it_should_behave_like 'all strategy stores'

  before(:each) do
    @klass = Merb::Cache::SHA1Store
    @store = Merb::Cache::SHA1Store[DummyStore].new
  end

  describe "#writable?" do
    it "should be true if the key is a string, symbol, or number" do
      @store.writable?(:foo).should be_true
      @store.writable?('foo').should be_true
      @store.writable?(123).should be_true
    end

    it "should be false if none of the context caches are writable" do
      @store.stores.each {|s| s.should_receive(:writable?).and_return false}

      @store.writable?(:foo).should be_false
    end
  end

  describe "#read" do
    it "should return nil if hash does not exist as a key in any context store" do
      @store.stores.each {|s| s.should_receive(:read).with(@store.digest(:foo, :bar => :baz)).and_return nil}

      @store.read(:foo, :bar => :baz).should be_nil
    end

    it "should return the data from the context store when there is a hash key match" do
      @store.stores.first.should_receive(:read).with(@store.digest(:foo)).and_return :bar

      @store.read(:foo).should == :bar
    end
  end

  describe "#write" do
    it "should pass the hashed key to the context store" do
      @store.stores.first.should_receive(:write).with(@store.digest(:foo), 'body', {}, {}).and_return true

      @store.write(:foo, 'body').should be_true
    end

    it "should use the parameters to create the hashed key" do
      @store.stores.first.should_receive(:write).with(@store.digest(:foo, :bar => :baz), 'body', {}, {}).and_return true

      @store.write(:foo, 'body', :bar => :baz).should be_true
    end
  end

  describe "#fetch" do
    it "should return nil if the arguments are not storable" do
      @store.fetch(mock(:request)) {'body'}.should be_nil
    end

    
  end

  describe "#digest" do
    it "should produce the same digest for the exact same key and parameters" do
      @store.digest(:foo, :bar => :baz).should == @store.digest(:foo, :bar => :baz)
    end

    it "should use the string result of the parameter arguments to_param in the hash" do
      @store.digest(:foo, :bar => :baz).should_not == @store.digest(:foo)
    end

    it "should use the key argument in the hash" do
      @store.digest('', :bar => :baz).should_not == @store.digest('')
    end
  end
end