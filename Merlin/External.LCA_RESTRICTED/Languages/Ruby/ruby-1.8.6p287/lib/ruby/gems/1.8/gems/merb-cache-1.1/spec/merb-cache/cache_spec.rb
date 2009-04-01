require File.dirname(__FILE__) + '/../spec_helper'

describe Merb::Cache do
  before(:each) do
    Merb::Cache.stores.clear
    Thread.current[:'merb-cache'] = nil
  end

  describe ".setup" do
    it "should have have access to the Merb::Cache.register method from the block" do
      Merb::Cache.setup do
        self.respond_to?(:register).should == true
      end
    end
  end

  describe ".register" do
    it "should add the store name and instance to the store hash" do
      Merb::Cache.stores.should_not have_key(:foo)
      Merb::Cache.register(:foo, DummyStore)
      Merb::Cache.stores.should have_key(:foo)
    end

    it "should use :default when no name is supplied" do
      Merb::Cache.stores.should_not have_key(:default)
      Merb::Cache.register(DummyStore)
      Merb::Cache.stores.should have_key(:default)
    end
    
    it "should not allow a store to be redefined" do
      Merb::Cache.register(DummyStore)
      lambda do
        Merb::Cache.register(DummyStore)
      end.should raise_error(Merb::Cache::StoreExists)
    end
  end
  
  describe ".exists?" do
    it "should return true if a repository is setup" do
      Merb::Cache.register(DummyStore)
      Merb::Cache.register(:store_that_exists, DummyStore)
      Merb::Cache.exists?(:default).should be_true
      Merb::Cache.exists?(:store_that_exists).should be_true
    end
    
    it "should return false if a repository is not setup" do
      Merb::Cache.exists?(:not_here).should be_false
    end
  end

  describe ".[]" do
    it "should clone the stores so to keep them threadsafe" do
      Merb::Cache.register(DummyStore)
      Merb::Cache[:default].should_not be_nil
      Merb::Cache[:default].should_not == Merb::Cache.stores[:default]
    end

    it "should cache the thread local stores in Thread.current" do
      Merb::Cache.register(DummyStore)
      Thread.current[:'merb-cache'].should be_nil
      Merb::Cache[:default]
      Thread.current[:'merb-cache'].should_not be_nil
    end

    it "should create an adhoc store if multiple store names are supplied" do
      Merb::Cache.register(DummyStore)
      Merb::Cache.register(:dummy, DummyStore)
      Merb::Cache[:default, :dummy].class.should == Merb::Cache::AdhocStore
    end
    
    it "should let you create new stores after accessing the old ones" do
      Merb::Cache.register(DummyStore)
      Merb::Cache.register(:one, DummyStore)
      Merb::Cache[:default].should_not be_nil
      Merb::Cache[:one].should_not be_nil
      Merb::Cache.register(:two, DummyStore)
      Merb::Cache[:two].should_not be_nil
    end
    
    it "should raise an error if the cache has not been setup" do
      Merb::Cache.register(DummyStore)
      Merb::Cache[:default].should_not be_nil      
      lambda do
        Merb::Cache[:does_not_exist]
      end.should raise_error(Merb::Cache::StoreNotFound)
    end
  end
end