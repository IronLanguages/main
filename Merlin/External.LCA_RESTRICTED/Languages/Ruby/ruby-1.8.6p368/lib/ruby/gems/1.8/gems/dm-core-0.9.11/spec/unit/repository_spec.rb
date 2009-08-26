require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe DataMapper::Repository do
  before do
    @adapter       = mock('adapter')
    @identity_map  = mock('identity map', :[]= => nil)
    @identity_maps = mock('identity maps', :[] => @identity_map)

    @repository = repository(:mock)
    @repository.stub!(:adapter).and_return(@adapter)

    # TODO: stub out other external dependencies in repository
  end

  describe "managing transactions" do
    it "should create a new Transaction with itself as argument when #transaction is called" do
      transaction = mock('transaction')
      DataMapper::Transaction.should_receive(:new).with(@repository).and_return(transaction)
      @repository.transaction.should == transaction
    end
  end

  it 'should provide .storage_exists?' do
    @repository.should respond_to(:storage_exists?)
  end

  it '.storage_exists? should whether or not the storage exists' do
    @adapter.should_receive(:storage_exists?).with(:vegetable).and_return(true)

    @repository.storage_exists?(:vegetable).should == true
  end

  it "should provide persistance methods" do
    @repository.should respond_to(:create)
    @repository.should respond_to(:read_many)
    @repository.should respond_to(:read_one)
    @repository.should respond_to(:update)
    @repository.should respond_to(:delete)
  end

  it "should be reused in inner scope" do
    DataMapper.repository(:default) do |outer_repos|
      DataMapper.repository(:default) do |inner_repos|
        outer_repos.object_id.should == inner_repos.object_id
      end
    end
  end

  it 'should provide default_name' do
    DataMapper::Repository.should respond_to(:default_name)
  end

  it 'should return :default for default_name' do
    DataMapper::Repository.default_name.should == :default
  end

  describe "#migrate!" do
    it "should call DataMapper::Migrator.migrate with itself as the repository argument" do
      DataMapper::Migrator.should_receive(:migrate).with(@repository.name)

      @repository.migrate!
    end
  end

  describe "#auto_migrate!" do
    it "should call DataMapper::AutoMigrator.auto_migrate with itself as the repository argument" do
      DataMapper::AutoMigrator.should_receive(:auto_migrate).with(@repository.name)

      @repository.auto_migrate!
    end
  end

  describe "#auto_upgrade!" do
    it "should call DataMapper::AutoMigrator.auto_upgrade with itself as the repository argument" do
      DataMapper::AutoMigrator.should_receive(:auto_upgrade).with(@repository.name)

      @repository.auto_upgrade!
    end
  end

  describe "#map" do
    it "should call type_map.map with the arguments" do
      type_map = mock('type map')

      @adapter.class.should_receive(:type_map).and_return(type_map)
      DataMapper::TypeMap.should_receive(:new).with(type_map).and_return(type_map)

      type_map.should_receive(:map).with(:type, :arg)

      @repository.map(:type, :arg)
    end
  end
end
