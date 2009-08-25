require File.expand_path(File.join(File.dirname(__FILE__), '..', "..", 'spec_helper'))

if HAS_POSTGRES
  describe DataMapper::Adapters::PostgresAdapter do
    before :all do
      @adapter = repository(:postgres).adapter
    end

    describe '#upgrade_model_storage' do
      before do
        @repository = mock('repository', :kind_of? => true, :name => :postgres)
        @model      = mock('model', :kind_of? => true, :storage_name => 'models')
        @property   = mock('property', :kind_of? => true, :model => @model, :serial? => true, :field => 'property')

        @model.should_receive(:properties).with(:postgres).any_number_of_times.and_return([@property])

        @command    = mock('command')
        @connection = mock('connection', :create_command => @command, :close => true)
        @result     = mock('result', :to_i => 0)

        DataObjects::Connection.stub!(:new).and_return(@connection)

        @adapter.stub!(:execute).and_return(@result)
        @adapter.stub!(:storage_exists?).and_return(true)
        @adapter.stub!(:query).and_return([ 0 ])

        @original_method = @adapter.class.superclass.instance_method(:upgrade_model_storage)
        @adapter.class.superclass.send(:define_method, :upgrade_model_storage) { |repository, model| }
      end

      after do
        method = @original_method
        @adapter.class.superclass.send(:define_method, :upgrade_model_storage) do |*args|
          method.bind(self).call(*args)
        end
      end

      it 'should check to make sure the sequences exist' do
        statement = %q[SELECT COUNT(*) FROM "information_schema"."sequences" WHERE "sequence_name" = ? AND "sequence_schema" = current_schema()]
        @adapter.should_receive(:query).with(statement, 'models_property_seq').and_return([ 0 ])
        @adapter.upgrade_model_storage(@repository, @model)
      end

      it 'should add sequences' do
        statement = %q[CREATE SEQUENCE "models_property_seq"]
        @adapter.should_receive(:execute).with(statement)
        @adapter.upgrade_model_storage(@repository, @model)
      end

      it 'should execute the superclass upgrade_model_storage' do
        rv = mock('inside super')
        @adapter.class.superclass.send(:define_method, :upgrade_model_storage) { |repository, model| rv }
        @adapter.upgrade_model_storage(@repository, @model).should == rv
      end
    end

    describe '#create_model_storage' do
      before do
        @repository = mock('repository', :kind_of? => true, :name => :postgres)
        @model      = mock('model', :kind_of? => true, :storage_name => 'models')
        @property   = mock('property', :kind_of? => true, :model => @model, :serial? => true, :field => 'property')

        @model.should_receive(:properties).with(:postgres).any_number_of_times.and_return([@property])

        @adapter.stub!(:execute).and_return(@result)
        @adapter.stub!(:storage_exists?).and_return(true)
        @adapter.stub!(:query).and_return([ 0 ])

        @original_method = @adapter.class.superclass.instance_method(:create_table_statement)
        @adapter.class.superclass.send(:define_method, :create_table_statement) {}
      end

      after do
        method = @original_method
        @adapter.class.superclass.send(:define_method, :create_table_statement) do |*args|
          method.bind(self).call(*args)
        end
      end

      it 'should check to make sure the sequences exist' do
        statement = %q[SELECT COUNT(*) FROM "information_schema"."sequences" WHERE "sequence_name" = ? AND "sequence_schema" = current_schema()]
        @adapter.should_receive(:query).with(statement, 'models_property_seq').and_return([ 0 ])
        @adapter.create_model_storage(@repository, @model)
      end

      it 'should add sequences' do
        statement = %q[CREATE SEQUENCE "models_property_seq"]
        @adapter.should_receive(:execute).with(statement)
        @adapter.create_model_storage(@repository, @model)
      end

      it 'should execute the superclass upgrade_model_storage' do
        rv = mock('inside super')
        @adapter.class.superclass.send(:define_method, :create_table_statement) { |repository, model| rv }
        @adapter.create_table_statement(@repository, @model).should == rv
      end
    end

    describe '#destroy_model_storage' do
      before do
        @repository = mock('repository', :kind_of? => true, :name => :postgres)
        @model      = mock('model', :kind_of? => true, :storage_name => 'models')
        @property   = mock('property', :kind_of? => true, :model => @model, :serial? => true, :field => 'property')

        @model.should_receive(:properties).with(:postgres).any_number_of_times.and_return([@property])

        @original_method = @adapter.class.superclass.instance_method(:destroy_model_storage)
        @adapter.class.superclass.send(:define_method, :destroy_model_storage) {}
      end

      after do
        method = @original_method
        @adapter.class.superclass.send(:define_method, :destroy_model_storage) do |*args|
          method.bind(self).call(*args)
        end
      end

      it 'should not execute the superclass destroy_model_storage if the storage does not exist' do
        rv = mock('inside super')
        @adapter.class.superclass.send(:define_method, :destroy_model_storage) { |repository, model| rv }
        @adapter.destroy_model_storage(@repository, @model).should_not == rv
      end

      it 'should execute the superclass destroy_model_storage if the storage exists' do
        rv = mock('inside super')
        @adapter.class.superclass.send(:define_method, :destroy_model_storage) { |repository, model| rv }
        @adapter.stub!(:storage_exists?).and_return(true)

        @adapter.destroy_model_storage(@repository, @model).should == rv
      end
    end
  end
end
