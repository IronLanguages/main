require File.dirname(__FILE__) + '/../../spec_helper'

describe Merb::Cache::CacheMixin do
  before(:all) do
    Merb::Cache.stores.clear
    Thread.current[:'merb-cache'] = nil

    Merb::Cache.register(:default, DummyStore)
    @dummy = Merb::Cache[:default]
  end

  describe ".cache!" do
    it "should add a before filter for all actions" do
      class CacheAllActionsBeforeCallController < Merb::Controller
        self.should_receive(:before).with(:_cache_before, :with =>{})

        cache!
      end
    end

    it "should add an after filter for all actions" do
      class CacheAllActionsAfterCallController < Merb::Controller
        self.should_receive(:after).with(:_cache_after, :with =>{})

        cache!
      end
    end
  end

  describe ".cache" do
    it "should call .cache_action with each method symbol" do
      class CacheActionsCallController < Merb::Controller
        actions = [:first, :second, :third]

        actions.each do |action|
          self.should_receive(:cache_action).with(action)
        end

        cache *actions
      end
    end

    it "should call .cache_action with the method symbol and conditions hash" do
      class CacheActionConditionsController < Merb::Controller
        self.should_receive(:cache_action).with(:action, :conditions => :hash)

        cache :action, :conditions => :hash
      end
    end
  end

  describe ".cache_action" do
    it "should add the before filter for only the action" do
      class CacheActionBeforeController < Merb::Controller
        self.should_receive(:before).with("_cache_action_before", :with => [{}], :only => :action)

        cache_action(:action)
      end
    end

    it "should add the after filter for only the action" do
      class CacheActionAfterController < Merb::Controller
        self.should_receive(:after).with("_cache_action_after", :with => [{}], :only => :action)

        cache_action(:action)
      end
    end
  end

  before(:each) do
    class TestController < Merb::Controller; def index; "Hello"; end end
    @controller = dispatch_to(TestController, :index)
  end

  describe "#_lookup_store" do
    it "should use the :store entry from the conditions hash" do
      @controller._lookup_store(:store => :foo_store).should == :foo_store
    end

    it "should use the :stores entry from the conditions hash" do
      @controller._lookup_store(:stores => [:foo_store, :bar_store]).should == [:foo_store, :bar_store]
    end

    it "should default store name if none is supplied in the conditions hash" do
      @controller._lookup_store.should == Merb::Cache.default_store_name
    end
    
    it "should request the default_cache_store" do
      @controller.should_receive(:default_cache_store).and_return(Merb::Cache.default_store_name)
      @controller._lookup_store
    end
  end

  describe "#_parameters_and_conditions" do
    it "should remove the :params entry from the conditions hash" do
      @controller._parameters_and_conditions(:params => [:foo, :bar]).last.should_not have_key(:params)
    end

    it "should remove the :store entry from the conditions hash" do
      @controller._parameters_and_conditions(:store => :foo_store).last.should_not have_key(:store)
    end

    it "should remove the :stores entry from the conditions hash" do
      @controller._parameters_and_conditions(:stores => [:foo_store, :bar_store]).last.should_not have_key(:stores)
    end

    it "should keep an :expires_in entry in the conditions hash" do
      @controller._parameters_and_conditions(:expire_in => 10).last.should have_key(:expire_in)
    end

    it "should move the :params entry to the parameters array" do
      @controller._parameters_and_conditions(:params => :foo).first.should have_key(:foo)
    end
  end

  describe "#skip_cache!" do
    it "should set @_skip_cache = true" do
      lambda { @controller.skip_cache! }.should change { @controller.instance_variable_get(:@_skip_cache) }.to(true)
    end
  end

  describe "force_cache!" do
    class AController < Merb::Controller
      
      cache :start
      
      def start
        "START"
      end      
    end
    
    before(:each) do
      @mock_store = mock("store", :null_object => true)
      Merb::Cache.stub!(:[]).and_return(@mock_store)
      @mock_store.stub!(:read).and_return("CACHED")
    end
        
    it "should try to hit the cache if force_cache is not set" do
      @mock_store.should_receive(:read).and_return("CACHED")
      controller = dispatch_to(AController, :start)
      controller.body.should == "CACHED"
    end
    
    it "should not try to hit the cache if force_cache is set" do
      @mock_store.should_not_receive(:read)
      controller = dispatch_to(AController, :start){|c| c.force_cache!}
      controller.body.should == "START"
    end
  end

  describe "#eager_cache" do
    before(:each) do
      Object.send(:remove_const, :EagerCacher) if defined?(EagerCacher)

      class EagerCacher < Merb::Controller
        def index
          "index"
        end
      end
    end



    it "should accept a block with an arity of 1" do
      class EagerCacher
        eager_cache(:index) {|params|}
      end

      lambda { dispatch_to(EagerCacher, :index) }.should_not raise_error
    end

    it "should accept a block with an arity greater than 1" do
      class EagerCacher
        eager_cache(:index) {|params, env|}
      end

      lambda { dispatch_to(EagerCacher, :index) }.should_not raise_error
    end

    it "should accept a block with an arity of -1" do
      class EagerCacher
        eager_cache(:index) {|*args|}
      end

      lambda { dispatch_to(EagerCacher, :index) }.should_not raise_error
    end

    it "should accept a block with an arity of 0" do
      class EagerCacher
        eager_cache(:index) {||}
      end

      lambda { dispatch_to(EagerCacher, :index) }.should_not raise_error
    end

    it "should allow the block to return nil" do
      class EagerCacher
        eager_cache(:index) {}
      end

      lambda { dispatch_to(EagerCacher, :index) }.should_not raise_error
    end

    it "should allow the block to return a params hash" do
      class EagerCacher
        eager_cache(:index) {|params| params.merge(:foo => :bar)}
      end

      lambda { dispatch_to(EagerCacher, :index) }.should_not raise_error(ArgumentError)
    end

    it "should allow the block to return a Merb::Request object" do
      class EagerCacher
        eager_cache(:index) {|params| build_request('/')}
      end

      lambda { dispatch_to(EagerCacher, :index) }.should_not raise_error(ArgumentError)
    end

    it "should allow the block to return a Merb::Controller object" do
      class EagerCacher
        eager_cache(:index) {|params, env| EagerCacher.new(env)}
      end

      lambda { dispatch_to(EagerCacher, :index) }.should_not raise_error(ArgumentError)
    end
  end

  describe "eager_cache (Instance Method)" do
    class HasRun
      cattr_accessor :has_run
    end
    
    def dispatch_and_wait(*args)
      @controller = nil
      Timeout.timeout(2) do
        until HasRun.has_run do
          @controller ||= dispatch_to(*args){ |c| 
            yield c if block_given?
          }
        end
      end
      @controller
    end
    
    class MyController < Merb::Controller
      after nil, :only => :start do
        eager_cache :stop
      end
      
      def start
        "START"
      end
      
      def stop
        "STOP"
      end
      
      def inline_call
        eager_cache :stop
      end
    end
    
    before(:each) do
      HasRun.has_run = false
    end
    
    after(:each) do
      HasRun.has_run = false
    end
        
    it "should run the stop action from a new instance as an after filter" do
      new_controller = MyController.new(fake_request)
      dispatch_and_wait(MyController, :start) do |c|
        MyController.should_receive(:new).and_return(new_controller)
        new_controller.should_receive(:stop).and_return(HasRun.has_run = true)
      end
    end
    
    it "should run the stop action from a new instance called inline" do
      new_controller = MyController.new(fake_request)
      dispatch_and_wait(MyController, :inline_call) do |c|
        MyController.should_receive(:new).and_return(new_controller)
        new_controller.should_receive(:stop).and_return(HasRun.has_run = true)
      end
    end
    
    it "should not run the stop action if the _set_skip_cache is set to true" do
      new_controller = MyController.new(fake_request)
      dispatch_and_wait(MyController, :start) do |c|
        MyController.stub!(:new).and_return(new_controller)
        c.skip_cache!
        new_controller.should_not_receive(:stop)
        HasRun.has_run = true        
      end    
    end
    
    it "should set the cache to be forced" do
      new_controller = MyController.new(fake_request)
      dispatch_and_wait(MyController, :start) do |c|
        MyController.should_receive(:new).and_return(new_controller)
        new_controller.should_receive(:force_cache!)
        new_controller.should_receive(:stop).and_return(HasRun.has_run = true)   
      end
    end
  end

  describe "#fetch_partial" do
    it "should pass the template argument to the partial method" do
      new_controller = MyController.new(fake_request)
      new_controller.should_receive(:partial).with(:foo, {})
      new_controller.stub!(:concat)

      new_controller.fetch_partial(:foo)
    end

    it "should pass the options to the partial method " do
      new_controller = MyController.new(fake_request)
      new_controller.should_receive(:partial).with(:foo, :bar => :baz)
      new_controller.stub!(:concat)

      new_controller.fetch_partial(:foo, :bar => :baz)
    end

    it "should contain only alpha-numeric characters in the template key" do
      new_controller = MyController.new(fake_request)
      new_controller.stub!(:partial)
      new_controller.stub!(:concat)

      @dummy.should_receive(:fetch).and_return do |template_key, opts, conditions, block|
        template_key.should =~ /^(?:[a-zA-Z0-9_\/\.-])+/
      end

      new_controller.fetch_partial('path/to/foo')
    end
  end

  describe "#fetch_fragment" do
    it "should include the filename that defines the fragment proc in the fragment key" do
      new_controller = MyController.new(fake_request)
      new_controller.stub!(:concat)

      @dummy.should_receive(:fetch).and_return do |fragment_key, opts, conditions, block|
        fragment_key.should include(__FILE__)
      end

      new_controller.fetch_fragment {}
    end

    it "should include the line number that defines the fragment proc in the fragment key" do
      new_controller = MyController.new(fake_request)
      new_controller.stub!(:concat)

      @dummy.should_receive(:fetch).and_return do |fragment_key, opts, conditions, block|
        fragment_key.should =~ %r{[\d+]}
      end

      new_controller.fetch_fragment {}
    end
  end

  describe ".build_request" do
    it "should create a CacheRequest" do
      Merb::Controller.build_request({}).class.should == Merb::Cache::CacheRequest
    end

    it "should use nil if the path is not supplied" do
      Merb::Controller.build_request({}).path.should be_nil
    end

    it "should allow the params to be specified" do
      Merb::Controller.build_request(:foo => :bar).params[:foo].should == :bar
    end

    it "should allow the env to be specified" do
      Merb::Controller.build_request({}, :foo => :bar).env[:foo].should == :bar
    end
  end
end