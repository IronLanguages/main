require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/abstract_strategy_store_spec'

describe Merb::Cache::GzipStore do
  it_should_behave_like 'all strategy stores'

  before(:each) do
    @klass = Merb::Cache::GzipStore
    @store = Merb::Cache::GzipStore[DummyStore].new
  end

  describe "#writable?" do
    it "should always be true" do
      @store.writable?(:foo).should be_true
      @store.writable?('foo').should be_true
      @store.writable?(123).should be_true
    end
  end

  describe "#read" do
    it "should return nil if hash does not exist as a key in any context store" do
      @store.stores.each {|s| s.should_receive(:read).with(:foo, :bar => :baz).and_return nil}

      @store.read(:foo, :bar => :baz).should be_nil
    end

    it "should return the data from the context store when there is a hash key match" do
      @store.stores.first.should_receive(:read).with(:foo, {}).and_return @store.compress("bar")

      @store.read(:foo).should == "bar"
    end
  end

  describe "#write" do
    it "should pass the hashed key to the context store" do
      @store.stores.first.should_receive(:write).with(:foo,  @store.compress('body'), {}, {}).and_return true

      @store.write(:foo, 'body').should be_true
    end

    it "should use the parameters to create the hashed key" do
      @store.stores.first.should_receive(:write).with(:foo, @store.compress('body'), {:bar => :baz}, {}).and_return true

      @store.write(:foo, 'body', :bar => :baz).should be_true
    end
  end

  describe "#fetch" do
    # not sure how to spec this yet
  end
end