require 'monitor'
require File.expand_path(File.join(File.dirname(__FILE__), '..', '..', 'spec_helper'))
require File.expand_path(File.join(File.dirname(__FILE__), '..', 'adapters', 'adapter_shared_spec'))

describe DataMapper::Adapters::AbstractAdapter do

  before do
    @adapter = DataMapper::Adapters::AbstractAdapter.new(:default, 'mock_uri_string')
  end

  it_should_behave_like 'a DataMapper Adapter'

  describe "when handling transactions" do
    before :each do
      @transaction = DataMapper::Transaction.new(@adapter)
    end
    it "should be able to push and pop transactions on the current stack" do
      @adapter.current_transaction.should == nil
      @adapter.within_transaction?.should == false
      @adapter.push_transaction(@transaction)
      @adapter.current_transaction.should == @transaction
      @adapter.within_transaction?.should == true
      @adapter.push_transaction(@transaction)
      @adapter.current_transaction.should == @transaction
      @adapter.within_transaction?.should == true
      @adapter.pop_transaction
      @adapter.current_transaction.should == @transaction
      @adapter.within_transaction?.should == true
      @adapter.pop_transaction
      @adapter.current_transaction.should == nil
      @adapter.within_transaction?.should == false
    end
    it "should let each Thread have its own transaction stack" do
      lock = Monitor.new
      transaction2 = DataMapper::Transaction.new(@adapter)
      @adapter.within_transaction?.should == false
      @adapter.current_transaction.should == nil
      @adapter.push_transaction(transaction2)
      @adapter.within_transaction?.should == true
      @adapter.current_transaction.should == transaction2
      lock.synchronize do
        Thread.new do
          @adapter.within_transaction?.should == false
          @adapter.current_transaction.should == nil
          @adapter.push_transaction(@transaction)
          @adapter.within_transaction?.should == true
          @adapter.current_transaction.should == @transaction
          lock.synchronize do
            @adapter.within_transaction?.should == true
            @adapter.current_transaction.should == @transaction
            @adapter.pop_transaction
            @adapter.within_transaction?.should == false
            @adapter.current_transaction.should == nil
          end
        end
        @adapter.within_transaction?.should == true
        @adapter.current_transaction.should == transaction2
        @adapter.pop_transaction
        @adapter.within_transaction?.should == false
        @adapter.current_transaction.should == nil
      end
    end
  end

  it "should raise NotImplementedError when #create is called" do
    lambda { @adapter.create([ :resource ]) }.should raise_error(NotImplementedError)
  end

  it "should raise NotImplementedError when #read_many is called" do
    lambda { @adapter.read_many(:query) }.should raise_error(NotImplementedError)
  end

  it "should raise NotImplementedError when #read_one is called" do
    lambda { @adapter.read_one(:query) }.should raise_error(NotImplementedError)
  end

  it "should raise NotImplementedError when #update is called" do
    lambda { @adapter.update(:attributes, :query) }.should raise_error(NotImplementedError)
  end

  it "should raise NotImplementedError when #delete is called" do
    lambda { @adapter.delete(:query) }.should raise_error(NotImplementedError)
  end

  it "should raise NotImplementedError when #upgrade_model_storage is called" do
    lambda { @adapter.upgrade_model_storage(:repository, :resource) }.should raise_error(NotImplementedError)
  end

  it "should raise NotImplementedError when #storage_exists? is called" do
    lambda { @adapter.storage_exists?("hehu") }.should raise_error(NotImplementedError)
  end

  it "should raise NotImplementedError when #create_model_storage is called" do
    lambda { @adapter.create_model_storage(:repository, :resource) }.should raise_error(NotImplementedError)
  end

  it "should raise NotImplementedError when #destroy_model_storage is called" do
    lambda { @adapter.destroy_model_storage(:repository, :resource) }.should raise_error(NotImplementedError)
  end

  it "should raise NotImplementedError when #alter_model_storage is called" do
    lambda { @adapter.alter_model_storage(:repository, :resource) }.should raise_error(NotImplementedError)
  end

  it "should raise NotImplementedError when #create_property_storage is called" do
    lambda { @adapter.create_property_storage(:repository, :property) }
  end

  it "should raise NotImplementedError when #destroy_property_storage is called" do
    lambda { @adapter.destroy_property_storage(:repository, :property) }
  end

  it "should raise NotImplementedError when #alter_property_storage is called" do
    lambda { @adapter.alter_property_storage(:repository, :property) }
  end

  it "should raise NotImplementedError when #transaction_primitive is called" do
    lambda { @adapter.transaction_primitive }.should raise_error(NotImplementedError)
  end

  it "should clean out dead threads from @transactions" do
    @adapter.instance_eval do @transactions end.size.should == 0
    t = Thread.new do
      @adapter.push_transaction("plur")
    end
    while t.alive?
      sleep 0.1
    end
    @adapter.instance_eval do @transactions end.size.should == 1
    @adapter.push_transaction("ploj")
    @adapter.instance_eval do @transactions end.size.should == 1
  end
end
