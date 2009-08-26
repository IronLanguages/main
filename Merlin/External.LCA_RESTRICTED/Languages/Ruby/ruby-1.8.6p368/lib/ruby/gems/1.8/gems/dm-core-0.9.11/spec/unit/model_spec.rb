require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe 'DataMapper::Model' do
  before do
    Object.send(:remove_const, :ModelSpec) if defined?(ModelSpec)
    module ModelSpec
      class Resource
        include DataMapper::Resource

        storage_names[:legacy] = 'legacy_resource'

        property :id,   Serial
        property :name, String
        property :type, Discriminator
      end
    end
  end

  it 'should provide .new' do
    meta_class = class << ModelSpec::Resource; self end
    meta_class.should respond_to(:new)
  end

  describe '.new' do
    it 'should require a default storage name and accept a block' do
      pluto = DataMapper::Model.new('planets') do
        property :name, String, :key => true
      end

      pluto.storage_name(:default).should == 'planets'
      pluto.storage_name(:legacy).should == 'planets'
      pluto.properties[:name].should_not be_nil
    end
  end

  it 'should provide #transaction' do
    ModelSpec::Resource.should respond_to(:transaction)
  end

  describe '#transaction' do
    it 'should return a new Transaction with Model as argument' do
      transaction = mock("transaction")
      DataMapper::Transaction.should_receive(:new).with(ModelSpec::Resource).and_return(transaction)
      ModelSpec::Resource.transaction.should == transaction
    end
  end

  it 'should provide #before' do
    ModelSpec::Resource.should respond_to(:before)
  end

  it 'should provide #after' do
    ModelSpec::Resource.should respond_to(:after)
  end

  it 'should provide #repository' do
    ModelSpec::Resource.should respond_to(:repository)
  end

  describe '#repository' do
    it 'should delegate to DataMapper.repository' do
      repository = mock('repository')
      DataMapper.should_receive(:repository).with(:legacy).and_return(repository)
      ModelSpec::Resource.repository(:legacy).should == repository
    end

    it 'should use default repository when not passed any arguments' do
      ModelSpec::Resource.repository.name.should == ModelSpec::Resource.repository(:default).name
    end
  end

  it 'should provide #storage_name' do
    ModelSpec::Resource.should respond_to(:storage_name)
  end

  describe '#storage_name' do
    it 'should map a repository to the storage location' do
      ModelSpec::Resource.storage_name(:legacy).should == 'legacy_resource'
    end

    it 'should use default repository when not passed any arguments' do
      ModelSpec::Resource.storage_name.object_id.should == ModelSpec::Resource.storage_name(:default).object_id
    end
  end

  it 'should provide #storage_names' do
    ModelSpec::Resource.should respond_to(:storage_names)
  end

  describe '#storage_names' do
    it 'should return a Hash mapping each repository to a storage location' do
      ModelSpec::Resource.storage_names.should be_kind_of(Hash)
      ModelSpec::Resource.storage_names.should == { :legacy => 'legacy_resource' }
    end
  end

  it 'should provide #property' do
    ModelSpec::Resource.should respond_to(:property)
  end

  describe '#property' do
    it 'should raise a SyntaxError when the name contains invalid characters' do
      lambda {
        ModelSpec::Resource.property(:"with space", TrueClass)
      }.should raise_error(SyntaxError)
    end
  end

  it 'should provide #properties' do
    ModelSpec::Resource.should respond_to(:properties)
  end

  describe '#properties' do
    it 'should return an PropertySet' do
      ModelSpec::Resource.properties(:legacy).should be_kind_of(DataMapper::PropertySet)
      ModelSpec::Resource.properties(:legacy).should have(3).entries
    end

    it 'should use default repository when not passed any arguments' do
      ModelSpec::Resource.properties.object_id.should == ModelSpec::Resource.properties(:default).object_id
    end
  end

  it 'should provide #key' do
    ModelSpec::Resource.should respond_to(:key)
  end

  describe '#key' do
    it 'should return an Array of Property objects' do
      ModelSpec::Resource.key(:legacy).should be_kind_of(Array)
      ModelSpec::Resource.key(:legacy).should have(1).entries
      ModelSpec::Resource.key(:legacy).first.should be_kind_of(DataMapper::Property)
    end

    it 'should use default repository when not passed any arguments' do
      ModelSpec::Resource.key.should == ModelSpec::Resource.key(:default)
    end

    it 'should not cache the key value' do
      class GasGiant < ModelSpec::Resource
      end

      GasGiant.key.object_id.should_not == ModelSpec::Resource.key(:default)

      # change the key and make sure the Array changes
      GasGiant.key == GasGiant.properties.slice(:id)
      GasGiant.property(:new_prop, String, :key => true)
      GasGiant.key.object_id.should_not == ModelSpec::Resource.key(:default)
      GasGiant.key == GasGiant.properties.slice(:id, :new_prop)
    end
  end

  it 'should provide #get' do
    ModelSpec::Resource.should respond_to(:get)
  end

  it 'should provide #first' do
    ModelSpec::Resource.should respond_to(:first)
  end

  it 'should provide #all' do
    ModelSpec::Resource.should respond_to(:all)
  end

  it 'should provide #storage_exists?' do
    ModelSpec::Resource.should respond_to(:storage_exists?)
  end

  describe '#storage_exists?' do
    it 'should return whether or not the storage exists' do
      ModelSpec::Resource.storage_exists?.should == false
    end
  end

  it 'should provide #default_order' do
    ModelSpec::Resource.should respond_to(:default_order)
  end

  describe '#default_order' do
    it 'should be equal to #key by default' do
      ModelSpec::Resource.default_order.should == [ DataMapper::Query::Direction.new(ModelSpec::Resource.properties[:id], :asc) ]
    end
  end

  describe '#append_inclusions' do
    before(:all) do
      @standard_resource_inclusions = DataMapper::Resource.instance_variable_get('@extra_inclusions')
      @standard_model_extensions = DataMapper::Model.instance_variable_get('@extra_extensions')
    end

    before(:each) do
      DataMapper::Resource.instance_variable_set('@extra_inclusions', [])
      DataMapper::Model.instance_variable_set('@extra_extensions', [])

      @module = Module.new do
        def greet
          hi_mom!
        end
      end

      @another_module = Module.new do
        def hello
          hi_dad!
        end
      end

      @class = Class.new

      @class_code = %{
        include DataMapper::Resource
        property :id, Serial
      }
    end

    after(:each) do
      DataMapper::Resource.instance_variable_set('@extra_inclusions', @standard_resource_inclusions)
      DataMapper::Model.instance_variable_set('@extra_extensions', @standard_model_extensions)
    end

    it "should append the module to be included in resources" do
      DataMapper::Resource.append_inclusions @module
      @class.class_eval(@class_code)

      instance = @class.new
      instance.should_receive(:hi_mom!)
      instance.greet
    end

    it "should append the module to all resources" do
      DataMapper::Resource.append_inclusions @module

      objects = (1..5).map do
        the_class = Class.new
        the_class.class_eval(@class_code)

        instance = the_class.new
        instance.should_receive(:hi_mom!)
        instance
      end

      objects.each { |obj| obj.greet }
    end

    it "should append multiple modules to be included in resources" do
      DataMapper::Resource.append_inclusions @module, @another_module
      @class.class_eval(@class_code)

      instance = @class.new
      instance.should_receive(:hi_mom!)
      instance.should_receive(:hi_dad!)
      instance.greet
      instance.hello
    end

    it "should include the appended modules in order" do
      module_one = Module.new do
        def self.included(base); base.hi_mom!; end;
      end

      module_two = Module.new do
        def self.included(base); base.hi_dad!; end;
      end

      DataMapper::Resource.append_inclusions module_two, module_one

      @class.should_receive(:hi_dad!).once.ordered
      @class.should_receive(:hi_mom!).once.ordered

      @class.class_eval(@class_code)
    end

    it "should append the module to extend resources with" do
      DataMapper::Model.append_extensions @module
      @class.class_eval(@class_code)

      @class.should_receive(:hi_mom!)
      @class.greet
    end

    it "should extend all resources with the module" do
      DataMapper::Model.append_extensions @module

      classes = (1..5).map do
        the_class = Class.new
        the_class.class_eval(@class_code)
        the_class.should_receive(:hi_mom!)
        the_class
      end

      classes.each { |cla| cla.greet }
    end

    it "should append multiple modules to extend resources with" do
      DataMapper::Model.append_extensions @module, @another_module
      @class.class_eval(@class_code)

      @class.should_receive(:hi_mom!)
      @class.should_receive(:hi_dad!)
      @class.greet
      @class.hello
    end

    it "should extend the resource in the order that the modules were appended" do
      @module.class_eval do
        def self.extended(base); base.hi_mom!; end;
      end

      @another_module.class_eval do
        def self.extended(base); base.hi_dad!; end;
      end

      DataMapper::Model.append_extensions @another_module, @module

      @class.should_receive(:hi_dad!).once.ordered
      @class.should_receive(:hi_mom!).once.ordered

      @class.class_eval(@class_code)
    end

  end
end
