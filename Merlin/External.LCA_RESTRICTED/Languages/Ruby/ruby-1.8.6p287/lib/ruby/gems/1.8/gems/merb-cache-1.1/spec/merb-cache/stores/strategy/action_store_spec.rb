require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/abstract_strategy_store_spec'

describe Merb::Cache::ActionStore do
  it_should_behave_like 'all strategy stores'

  # this could be cleaned up a bit
  before(:each) do
    Merb::Cache.stores.clear
    Thread.current[:'merb-cache'] = nil

    @klass = Merb::Cache::ActionStore[:dummy]
    Merb::Cache.register(:dummy, DummyStore)
    Merb::Cache.register(:default, @klass)

    @store = Merb::Cache[:default]
    @dummy = @store.stores.first

    class TestController < Merb::Controller; def action; end; end
    @controller = TestController.new(fake_request)
    @controller.stub!(:action_name).and_return :action
    @controller.stub!(:body).and_return 'body'
  end

  describe "#writable?" do
    it "should be false if the key argument is not an instance of a controller" do
      @store.writable?(:foo).should be_false
    end

    it "should be false if the fundamental store cannot write the normalized key" do
      @store.stores.first.should_receive(:writable?).and_return false

      @store.writable?(@controller).should be_false
    end

    it "should be true if the key is a controller instance and the fundamental store is writable" do
      @store.writable?(@controller).should be_true
    end
  end

  describe "#read" do
    it "should pass the normalized dispatch as the key to the context cache" do
      @dummy.should_receive(:read).with("TestController#action", {})

      @store.read(@controller)
    end
  end

  describe "#write" do
    it "should pass the normalized dispatch as the key to the fundamental store" do
      @dummy.should_receive(:write).with("TestController#action", @controller.body, {}, {})

      @store.write(@controller)
    end

    it "should not store to the context cache if the dispatch is not storable" do
      @dummy.should_not_receive(:write)

      @store.write(:foo).should be_nil
    end

    it "should use the controller instance's body as the data" do
      @controller.should_receive(:body)

      @store.write(@controller)
    end
  end

  describe "#write_all" do
    it "should pass the normalized dispatch as the key to the fundamental store" do
      @dummy.should_receive(:write_all).with("TestController#action", @controller.body, {}, {})

      @store.write_all(@controller)
    end

    it "should not store to the context cache if the dispatch is not storable" do
      @dummy.should_not_receive(:write_all)

      @store.write_all(:foo).should be_nil
    end

    it "should use the controller instance's body as the data" do
      @controller.should_receive(:body)

      @store.write_all(@controller)
    end
  end

  describe "examples" do
    class MLBSchedule < Merb::Controller
      cache :index

      def index
        "MLBSchedule index"
      end
    end

    class MLBScores < Merb::Controller
      cache :index, :show
      cache :overview
      cache :short, :params => :page
      cache :stats, :params => [:start_date, :end_date]
      cache :ticker, :expire_in => 10

      eager_cache :index, [MLBSchedule, :index]
      eager_cache :overview, :index
      eager_cache(:short, :params => :page) do |params, env|
        build_request(params.merge(:page => (params[:page].to_i + 1).to_s))
      end

      def index
        "MLBScores index"
      end

      def show(team)
        "MLBScores show(#{team})"
      end

      def overview(team = :all)
        "MLBScores overview(#{team})"
      end

      def short(team = :all)
        "MLBScores short(#{team})[#{params[:page]}]"
      end

      def stats(start_date, end_date, team = :all)
        "MLBScores stats(#{team}, #{start_date}, #{end_date})"
      end

      def ticker
        "MLBScores ticker"
      end
    end

    it "should cache the index action on the first request" do
      dispatch_to(MLBScores, :index)

      @dummy.data("MLBScores#index").should == "MLBScores index"
    end

    it "should cache the show action by the team parameter using the action arguments" do
      dispatch_to(MLBScores, :show, :team => :redsox)

      @dummy.data("MLBScores#show", :team => 'redsox').should == "MLBScores show(redsox)"
    end

    it "should cache the overview action by the default parameter if none is given" do
      dispatch_to(MLBScores, :overview)

      @dummy.data("MLBScores#overview", :team => :all).should == "MLBScores overview(all)"
    end

    it "should cache the short action by the team & page parameters" do
      dispatch_to(MLBScores, :short, :team => :bosux, :page => 4)

      @dummy.data("MLBScores#short", :team => 'bosux', :page => '4').should == "MLBScores short(bosux)[4]"
    end

    it "should cache the stats action by team, start_date & end_date parameters" do
      start_date, end_date = Time.today.to_s, Time.now.to_s
      dispatch_to(MLBScores, :stats, :start_date => start_date, :end_date => end_date)

      @dummy.data("MLBScores#stats", :team => :all, :start_date => start_date, :end_date => end_date).should == "MLBScores stats(all, #{start_date}, #{end_date})"
    end

    it "should cache the ticker action with an expire_in condition" do
      dispatch_to(MLBScores, :ticker)

      @dummy.conditions("MLBScores#ticker")[:expire_in].should == 10
    end

    it "should eager cache MLBSchedule#index after a request to MLBScores#index" do
      dispatch_to(MLBScores, :index)

      @dummy.data("MLBSchedule#index").should == "MLBSchedule index"
    end

    it "should eager cache :index after a request to :overview" do
      dispatch_to(MLBScores, :overview)

      @dummy.data("MLBScores#index").should == "MLBScores index"
    end

    it "should eager cache the next :short page" do
      dispatch_to(MLBScores, :short, :team => :bosux, :page => 4)

      @dummy.data("MLBScores#short", :team => 'bosux', :page => '5').should == "MLBScores short(bosux)[5]"
    end
  end
end