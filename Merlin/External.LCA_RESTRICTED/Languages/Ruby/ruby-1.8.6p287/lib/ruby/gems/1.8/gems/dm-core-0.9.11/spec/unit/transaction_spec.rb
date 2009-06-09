require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe DataMapper::Transaction do

  before :all do
    class ::Smurf
      include DataMapper::Resource
      property :id, Integer, :key => true
    end
  end

  before :each do
    @adapter = mock("adapter", :name => 'mock_adapter')
    @repository = mock("repository")
    @repository_adapter = mock("repository adapter", :name => 'mock_repository_adapter')
    @resource = Smurf.new
    @transaction_primitive = mock("transaction primitive")
    @repository_transaction_primitive = mock("repository transaction primitive")
    @array = [@adapter, @repository]

    @adapter.should_receive(:is_a?).any_number_of_times.with(Array).and_return(false)
    @adapter.should_receive(:is_a?).any_number_of_times.with(DataMapper::Adapters::AbstractAdapter).and_return(true)
    @adapter.should_receive(:transaction_primitive).any_number_of_times.and_return(@transaction_primitive)
    @repository.should_receive(:is_a?).any_number_of_times.with(Array).and_return(false)
    @repository.should_receive(:is_a?).any_number_of_times.with(DataMapper::Adapters::AbstractAdapter).and_return(false)
    @repository.should_receive(:is_a?).any_number_of_times.with(DataMapper::Repository).and_return(true)
    @repository.should_receive(:adapter).any_number_of_times.and_return(@repository_adapter)
    @repository_adapter.should_receive(:is_a?).any_number_of_times.with(Array).and_return(false)
    @repository_adapter.should_receive(:is_a?).any_number_of_times.with(DataMapper::Adapters::AbstractAdapter).and_return(true)
    @repository_adapter.should_receive(:transaction_primitive).any_number_of_times.and_return(@repository_transaction_primitive)
    @transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:close).and_return(true)
    @transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:begin).and_return(true)
    @transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:prepare).and_return(true)
    @transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:rollback).and_return(true)
    @transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:rollback_prepared).and_return(true)
    @transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:commit).and_return(true)
    @repository_transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:close).and_return(true)
    @repository_transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:begin).and_return(true)
    @repository_transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:prepare).and_return(true)
    @repository_transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:rollback).and_return(true)
    @repository_transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:rollback_prepared).and_return(true)
    @repository_transaction_primitive.should_receive(:respond_to?).any_number_of_times.with(:commit).and_return(true)
  end

  it "should be able to initialize with an Array" do
    DataMapper::Transaction.new(@array)
  end
  it "should be able to initialize with DataMapper::Adapters::AbstractAdapters" do
    DataMapper::Transaction.new(@adapter)
  end
  it "should be able to initialize with DataMapper::Repositories" do
    DataMapper::Transaction.new(@repository)
  end
  it "should be able to initialize with DataMapper::Resource subclasses" do
    DataMapper::Transaction.new(Smurf)
  end
  it "should be able to initialize with DataMapper::Resources" do
    DataMapper::Transaction.new(Smurf.new)
  end
  it "should initialize with no transaction_primitives" do
    DataMapper::Transaction.new.transaction_primitives.empty?.should == true
  end
  it "should initialize with state :none" do
    DataMapper::Transaction.new.state.should == :none
  end
  it "should initialize the adapters given on creation" do
    DataMapper::Transaction.new(Smurf).adapters.should == {Smurf.repository.adapter => :none}
  end
  it "should be able receive multiple adapters on creation" do
    DataMapper::Transaction.new(Smurf, @resource, @adapter, @repository)
  end
  it "should be able to initialize with a block" do
    p = Proc.new do end
    @transaction_primitive.stub!(:begin)
    @transaction_primitive.stub!(:prepare)
    @transaction_primitive.stub!(:commit)
    @adapter.stub!(:push_transaction)
    @adapter.stub!(:pop_transaction)
    @transaction_primitive.stub!(:close)
    DataMapper::Transaction.new(@adapter, &p)
  end
  it "should accept new adapters after creation" do
    t = DataMapper::Transaction.new(@adapter, @repository)
    t.adapters.should == {@adapter => :none, @repository_adapter => :none}
    t.link(@resource)
    t.adapters.should == {@adapter => :none, @repository_adapter => :none, Smurf.repository.adapter => :none}
  end
  it "should not accept new adapters after state is changed" do
    t = DataMapper::Transaction.new(@adapter, @repository)
    @transaction_primitive.stub!(:begin)
    @repository_transaction_primitive.stub!(:begin)
    t.begin
    lambda do t.link(@resource) end.should raise_error(Exception, /Illegal state/)
  end
  describe "#begin" do
    before :each do
      @transaction = DataMapper::Transaction.new(@adapter, @repository)
    end
    it "should raise error if state is changed" do
      @transaction_primitive.stub!(:begin)
      @repository_transaction_primitive.stub!(:begin)
      @transaction.begin
      lambda do @transaction.begin end.should raise_error(Exception, /Illegal state/)
    end
    it "should try to connect each adapter (or log fatal error), then begin each adapter (or rollback and close)" do
      @transaction.should_receive(:each_adapter).with(:connect_adapter, [:log_fatal_transaction_breakage])
      @transaction.should_receive(:each_adapter).with(:begin_adapter, [:rollback_and_close_adapter_if_begin, :close_adapter_if_none])
      @transaction.begin
    end
    it "should leave with state :begin" do
      @transaction_primitive.stub!(:begin)
      @repository_transaction_primitive.stub!(:begin)
      @transaction.begin
      @transaction.state.should == :begin
    end
  end
  describe "#rollback" do
    before :each do
      @transaction = DataMapper::Transaction.new(@adapter, @repository)
    end
    it "should raise error if state is :none" do
      lambda do @transaction.rollback end.should raise_error(Exception, /Illegal state/)
    end
    it "should raise error if state is :commit" do
      @transaction_primitive.stub!(:begin)
      @repository_transaction_primitive.stub!(:begin)
      @transaction_primitive.stub!(:prepare)
      @repository_transaction_primitive.stub!(:prepare)
      @transaction_primitive.stub!(:commit)
      @repository_transaction_primitive.stub!(:commit)
      @transaction_primitive.stub!(:close)
      @repository_transaction_primitive.stub!(:close)
      @transaction.begin
      @transaction.commit
      lambda do @transaction.rollback end.should raise_error(Exception, /Illegal state/)
    end
    it "should try to rollback each adapter (or rollback and close), then then close (or log fatal error)" do
      @transaction.should_receive(:each_adapter).with(:connect_adapter, [:log_fatal_transaction_breakage])
      @transaction.should_receive(:each_adapter).with(:begin_adapter, [:rollback_and_close_adapter_if_begin, :close_adapter_if_none])
      @transaction.should_receive(:each_adapter).with(:rollback_adapter_if_begin, [:rollback_and_close_adapter_if_begin, :close_adapter_if_none])
      @transaction.should_receive(:each_adapter).with(:close_adapter_if_open, [:log_fatal_transaction_breakage])
      @transaction.should_receive(:each_adapter).with(:rollback_prepared_adapter_if_prepare, [:rollback_prepared_and_close_adapter_if_begin, :close_adapter_if_none])
      @transaction.begin
      @transaction.rollback
    end
    it "should leave with state :rollback" do
      @transaction_primitive.stub!(:begin)
      @repository_transaction_primitive.stub!(:begin)
      @transaction_primitive.stub!(:rollback)
      @repository_transaction_primitive.stub!(:rollback)
      @transaction_primitive.stub!(:close)
      @repository_transaction_primitive.stub!(:close)
      @transaction.begin
      @transaction.rollback
      @transaction.state.should == :rollback
    end
  end
  describe "#commit" do
    describe "without a block" do
      before :each do
        @transaction = DataMapper::Transaction.new(@adapter, @repository)
      end
      it "should raise error if state is :none" do
        lambda do @transaction.commit end.should raise_error(Exception, /Illegal state/)
      end
      it "should raise error if state is :commit" do
        @transaction_primitive.stub!(:begin)
        @repository_transaction_primitive.stub!(:begin)
        @transaction_primitive.stub!(:prepare)
        @repository_transaction_primitive.stub!(:prepare)
        @transaction_primitive.stub!(:commit)
        @repository_transaction_primitive.stub!(:commit)
        @transaction_primitive.stub!(:close)
        @repository_transaction_primitive.stub!(:close)
        @transaction.begin
        @transaction.commit
        lambda do @transaction.commit end.should raise_error(Exception, /Illegal state/)
      end
      it "should raise error if state is :rollback" do
        @transaction_primitive.stub!(:begin)
        @repository_transaction_primitive.stub!(:begin)
        @transaction_primitive.stub!(:rollback)
        @repository_transaction_primitive.stub!(:rollback)
        @transaction_primitive.stub!(:close)
        @repository_transaction_primitive.stub!(:close)
        @transaction.begin
        @transaction.rollback
        lambda do @transaction.commit end.should raise_error(Exception, /Illegal state/)
      end
      it "should try to prepare each adapter (or rollback and close), then commit each adapter (or log fatal error), then close (or log fatal error)" do
        @transaction.should_receive(:each_adapter).with(:connect_adapter, [:log_fatal_transaction_breakage])
        @transaction.should_receive(:each_adapter).with(:begin_adapter, [:rollback_and_close_adapter_if_begin, :close_adapter_if_none])
        @transaction.should_receive(:each_adapter).with(:prepare_adapter, [:rollback_and_close_adapter_if_begin, :rollback_prepared_and_close_adapter_if_prepare])
        @transaction.should_receive(:each_adapter).with(:commit_adapter, [:log_fatal_transaction_breakage])
        @transaction.should_receive(:each_adapter).with(:close_adapter, [:log_fatal_transaction_breakage])
        @transaction.begin
        @transaction.commit
      end
      it "should leave with state :commit" do
        @transaction_primitive.stub!(:begin)
        @repository_transaction_primitive.stub!(:begin)
        @transaction_primitive.stub!(:prepare)
        @repository_transaction_primitive.stub!(:prepare)
        @transaction_primitive.stub!(:commit)
        @repository_transaction_primitive.stub!(:commit)
        @transaction_primitive.stub!(:close)
        @repository_transaction_primitive.stub!(:close)
        @transaction.begin
        @transaction.commit
        @transaction.state.should == :commit
      end
    end
    describe "with a block" do
      before :each do
        @transaction = DataMapper::Transaction.new(@adapter, @repository)
      end
      it "should raise if state is not :none" do
        @transaction_primitive.stub!(:begin)
        @repository_transaction_primitive.stub!(:begin)
        @transaction.begin
        lambda do @transaction.commit do end end.should raise_error(Exception, /Illegal state/)
      end
      it "should begin, yield and commit if the block raises no exception" do
        @repository_transaction_primitive.should_receive(:begin)
        @repository_transaction_primitive.should_receive(:prepare)
        @repository_transaction_primitive.should_receive(:commit)
        @repository_transaction_primitive.should_receive(:close)
        @transaction_primitive.should_receive(:begin)
        @transaction_primitive.should_receive(:prepare)
        @transaction_primitive.should_receive(:commit)
        @transaction_primitive.should_receive(:close)
        p = Proc.new do end
        @transaction.should_receive(:within).with(&p)
        @transaction.commit(&p)
      end
      it "should rollback if the block raises an exception" do
        @repository_transaction_primitive.should_receive(:begin)
        @repository_transaction_primitive.should_receive(:rollback)
        @repository_transaction_primitive.should_receive(:close)
        @transaction_primitive.should_receive(:begin)
        @transaction_primitive.should_receive(:rollback)
        @transaction_primitive.should_receive(:close)
        p = Proc.new do end
        @transaction.should_receive(:within).with(&p).and_raise(Exception.new('test exception, never mind me'))
        lambda { @transaction.commit(&p) }.should raise_error(Exception, /test exception, never mind me/)
      end
    end
  end
  describe "#within" do
    before :each do
      @transaction = DataMapper::Transaction.new(@adapter, @repository)
    end
    it "should raise if no block is provided" do
      lambda do @transaction.within end.should raise_error(Exception, /No block/)
    end
    it "should raise if state is not :begin" do
      lambda do @transaction.within do end end.should raise_error(Exception, /Illegal state/)
    end
    it "should push itself on the per thread transaction context of each adapter and then pop itself out again" do
      @repository_transaction_primitive.should_receive(:begin)
      @transaction_primitive.should_receive(:begin)
      @repository_adapter.should_receive(:push_transaction).with(@transaction)
      @adapter.should_receive(:push_transaction).with(@transaction)
      @repository_adapter.should_receive(:pop_transaction)
      @adapter.should_receive(:pop_transaction)
      @transaction.begin
      @transaction.within do end
    end
    it "should push itself on the per thread transaction context of each adapter and then pop itself out again even if an exception was raised" do
      @repository_transaction_primitive.should_receive(:begin)
      @transaction_primitive.should_receive(:begin)
      @repository_adapter.should_receive(:push_transaction).with(@transaction)
      @adapter.should_receive(:push_transaction).with(@transaction)
      @repository_adapter.should_receive(:pop_transaction)
      @adapter.should_receive(:pop_transaction)
      @transaction.begin
      lambda do @transaction.within do raise "test exception, never mind me" end end.should raise_error(Exception, /test exception, never mind me/)
    end
  end
  describe "#method_missing" do
    before :each do
      @transaction = DataMapper::Transaction.new(@adapter, @repository)
      @adapter.should_receive(:is_a?).any_number_of_times.with(any_args).and_return(false)
      @adapter.should_receive(:is_a?).any_number_of_times.with(no_args).and_return(false)
      @adapter.should_receive(:is_a?).any_number_of_times.with(Regexp).and_return(false)
    end
    it "should delegate calls to [a method we have]_if_[state](adapter) to [a method we have](adapter) if state of adapter is [state]" do
      @transaction.should_receive(:state_for).with(@adapter).and_return(:begin)
      @transaction.should_receive(:connect_adapter).with(@adapter)
      @transaction.connect_adapter_if_begin(@adapter)
    end
    it "should not delegate calls to [a method we have]_if_[state](adapter) to [a method we have](adapter) if state of adapter is not [state]" do
      @transaction.should_receive(:state_for).with(@adapter).and_return(:commit)
      @transaction.should_not_receive(:connect_adapter).with(@adapter)
      @transaction.connect_adapter_if_begin(@adapter)
    end
    it "should delegate calls to [a method we have]_unless_[state](adapter) to [a method we have](adapter) if state of adapter is not [state]" do
      @transaction.should_receive(:state_for).with(@adapter).and_return(:none)
      @transaction.should_receive(:connect_adapter).with(@adapter)
      @transaction.connect_adapter_unless_begin(@adapter)
    end
    it "should not delegate calls to [a method we have]_unless_[state](adapter) to [a method we have](adapter) if state of adapter is [state]" do
      @transaction.should_receive(:state_for).with(@adapter).and_return(:begin)
      @transaction.should_not_receive(:connect_adapter).with(@adapter)
      @transaction.connect_adapter_unless_begin(@adapter)
    end
    it "should not delegate calls whose first argument is not a DataMapper::Adapters::AbstractAdapter" do
      lambda do @transaction.connect_adapter_unless_begin("plur") end.should raise_error
    end
    it "should not delegate calls that do not look like an if or unless followed by a state" do
      lambda do @transaction.connect_adapter_unless_hepp(@adapter) end.should raise_error
      lambda do @transaction.connect_adapter_when_begin(@adapter) end.should raise_error
    end
    it "should not delegate calls that we can not respond to" do
      lambda do @transaction.connect_adapters_unless_begin(@adapter) end.should raise_error
      lambda do @transaction.connect_adapters_if_begin(@adapter) end.should raise_error
    end
  end
  it "should be able to produce the connection for an adapter" do
    @transaction_primitive.stub!(:begin)
    @repository_transaction_primitive.stub!(:begin)
    @transaction = DataMapper::Transaction.new(@adapter, @repository)
    @transaction.begin
    @transaction.primitive_for(@adapter).should == @transaction_primitive
  end
  describe "#each_adapter" do
    before :each do
      @transaction = DataMapper::Transaction.new(@adapter, @repository)
      @adapter.should_receive(:is_a?).any_number_of_times.with(any_args).and_return(false)
      @adapter.should_receive(:is_a?).any_number_of_times.with(no_args).and_return(false)
      @adapter.should_receive(:is_a?).any_number_of_times.with(Regexp).and_return(false)
      @repository_adapter.should_receive(:is_a?).any_number_of_times.with(any_args).and_return(false)
      @repository_adapter.should_receive(:is_a?).any_number_of_times.with(no_args).and_return(false)
      @repository_adapter.should_receive(:is_a?).any_number_of_times.with(Regexp).and_return(false)
    end
    it "should send the first argument to itself once for each adapter" do
      @transaction.should_receive(:plupp).with(@adapter)
      @transaction.should_receive(:plupp).with(@repository_adapter)
      @transaction.instance_eval do each_adapter(:plupp, [:plur]) end
    end
    it "should stop sending if any call raises an exception, then send each element of the second argument to itself with each adapter as argument" do
      a1 = @repository_adapter
      a2 = @adapter
      @transaction.adapters.instance_eval do
        @a1 = a1
        @a2 = a2
        def each(&block)
          yield(@a1, :none)
          yield(@a2, :none)
        end
      end
      @transaction.should_receive(:plupp).with(@repository_adapter).and_throw(Exception.new("test error - dont mind me"))
      @transaction.should_not_receive(:plupp).with(@adapter)
      @transaction.should_receive(:plur).with(@adapter)
      @transaction.should_receive(:plur).with(@repository_adapter)
      lambda do @transaction.instance_eval do each_adapter(:plupp, [:plur]) end end.should raise_error(Exception, /test error - dont mind me/)
    end
    it "should send each element of the second argument to itself with each adapter as argument even if exceptions occur in the process" do
      a1 = @repository_adapter
      a2 = @adapter
      @transaction.adapters.instance_eval do
        @a1 = a1
        @a2 = a2
        def each(&block)
          yield(@a1, :none)
          yield(@a2, :none)
        end
      end
      @transaction.should_receive(:plupp).with(@repository_adapter).and_throw(Exception.new("test error - dont mind me"))
      @transaction.should_not_receive(:plupp).with(@adapter)
      @transaction.should_receive(:plur).with(@adapter).and_throw(Exception.new("another test error"))
      @transaction.should_receive(:plur).with(@repository_adapter).and_throw(Exception.new("yet another error"))
      lambda do @transaction.instance_eval do each_adapter(:plupp, [:plur]) end end.should raise_error(Exception, /test error - dont mind me/)
    end
  end
  it "should be able to return the state for a given adapter" do
    @transaction = DataMapper::Transaction.new(@adapter, @repository)
    a1 = @adapter
    a2 = @repository_adapter
    @transaction.instance_eval do state_for(a1) end.should == :none
    @transaction.instance_eval do state_for(a2) end.should == :none
    @transaction.instance_eval do @adapters[a1] = :begin end
    @transaction.instance_eval do state_for(a1) end.should == :begin
    @transaction.instance_eval do state_for(a2) end.should == :none
  end
  describe "#do_adapter" do
    before :each do
      @transaction = DataMapper::Transaction.new(@adapter, @repository)
      @adapter.should_receive(:is_a?).any_number_of_times.with(any_args).and_return(false)
      @adapter.should_receive(:is_a?).any_number_of_times.with(no_args).and_return(false)
      @adapter.should_receive(:is_a?).any_number_of_times.with(Regexp).and_return(false)
    end
    it "should raise if there is no connection for the adapter" do
      a1 = @adapter
      lambda do @transaction.instance_eval do do_adapter(a1, :ping, :pong) end end.should raise_error(Exception, /No primitive/)
    end
    it "should raise if the adapter has the wrong state" do
      @transaction_primitive.stub!(:begin)
      @repository_transaction_primitive.stub!(:begin)
      @transaction.begin
      a1 = @adapter
      @adapter.should_not_receive(:ping_transaction).with(@transaction)
      lambda do @transaction.instance_eval do do_adapter(a1, :ping, :pong) end end.should raise_error(Exception, /Illegal state/)
    end
    it "should delegate to the adapter if the connection exists and we have the right state" do
      @transaction_primitive.stub!(:begin)
      @repository_transaction_primitive.stub!(:begin)
      @transaction.begin
      a1 = @adapter
      @transaction_primitive.should_receive(:ping)
      @transaction.instance_eval do do_adapter(a1, :ping, :begin) end
    end
  end
  describe "#connect_adapter" do
    before :each do
      @other_adapter = mock("adapter")
      @other_adapter.should_receive(:is_a?).any_number_of_times.with(Array).and_return(false)
      @other_adapter.should_receive(:is_a?).any_number_of_times.with(DataMapper::Adapters::AbstractAdapter).and_return(true)
      @transaction = DataMapper::Transaction.new(@other_adapter)
    end
    it "should be able to connect an adapter" do
      a1 = @other_adapter
      @other_adapter.should_receive(:transaction_primitive).and_return(@transaction_primitive)
      @transaction.instance_eval do connect_adapter(a1) end
      @transaction.transaction_primitives[@other_adapter].should == @transaction_primitive
    end
  end
  describe "#close adapter" do
    before :each do
      @other_adapter = mock("adapter")
      @other_adapter.should_receive(:is_a?).any_number_of_times.with(Array).and_return(false)
      @other_adapter.should_receive(:is_a?).any_number_of_times.with(DataMapper::Adapters::AbstractAdapter).and_return(true)
      @transaction = DataMapper::Transaction.new(@other_adapter)
    end
    it "should be able to close the connection of an adapter" do
      a1 = @other_adapter
      @transaction_primitive.should_receive(:close)
      @other_adapter.should_receive(:transaction_primitive).and_return(@transaction_primitive)
      @transaction.instance_eval do connect_adapter(a1) end
      @transaction.transaction_primitives[@other_adapter].should == @transaction_primitive
      @transaction.instance_eval do close_adapter(a1) end
      @transaction.transaction_primitives[@other_adapter].should == nil
    end
  end
  describe "the transaction operation methods" do
    before :each do
      @other_adapter = mock("adapter")
      @other_adapter.should_receive(:is_a?).any_number_of_times.with(Array).and_return(false)
      @other_adapter.should_receive(:is_a?).any_number_of_times.with(DataMapper::Adapters::AbstractAdapter).and_return(true)
      @other_adapter.should_receive(:is_a?).any_number_of_times.with(any_args).and_return(false)
      @other_adapter.should_receive(:is_a?).any_number_of_times.with(no_args).and_return(false)
      @other_adapter.should_receive(:is_a?).any_number_of_times.with(Regexp).and_return(false)
      @transaction = DataMapper::Transaction.new(@other_adapter)
    end
    it "should only allow adapters in state :none to begin" do
      a1 = @other_adapter
      @transaction.should_receive(:do_adapter).with(@other_adapter, :begin, :none)
      @transaction.instance_eval do begin_adapter(a1) end
    end
    it "should only allow adapters in state :begin to prepare" do
      a1 = @other_adapter
      @transaction.should_receive(:do_adapter).with(@other_adapter, :prepare, :begin)
      @transaction.instance_eval do prepare_adapter(a1) end
    end
    it "should only allow adapters in state :prepare to commit" do
      a1 = @other_adapter
      @transaction.should_receive(:do_adapter).with(@other_adapter, :commit, :prepare)
      @transaction.instance_eval do commit_adapter(a1) end
    end
    it "should only allow adapters in state :begin to rollback" do
      a1 = @other_adapter
      @transaction.should_receive(:do_adapter).with(@other_adapter, :rollback, :begin)
      @transaction.instance_eval do rollback_adapter(a1) end
    end
    it "should only allow adapters in state :prepare to rollback_prepared" do
      a1 = @other_adapter
      @transaction.should_receive(:do_adapter).with(@other_adapter, :rollback_prepared, :prepare)
      @transaction.instance_eval do rollback_prepared_adapter(a1) end
    end
    it "should do delegate properly for rollback_and_close" do
      a1 = @other_adapter
      @transaction.should_receive(:rollback_adapter).with(@other_adapter)
      @transaction.should_receive(:close_adapter).with(@other_adapter)
      @transaction.instance_eval do rollback_and_close_adapter(a1) end
    end
    it "should do delegate properly for rollback_prepared_and_close" do
      a1 = @other_adapter
      @transaction.should_receive(:rollback_prepared_adapter).with(@other_adapter)
      @transaction.should_receive(:close_adapter).with(@other_adapter)
      @transaction.instance_eval do rollback_prepared_and_close_adapter(a1) end
    end
  end
end
