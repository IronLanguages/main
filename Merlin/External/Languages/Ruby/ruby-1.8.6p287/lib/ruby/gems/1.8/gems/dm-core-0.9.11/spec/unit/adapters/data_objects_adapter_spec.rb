require 'monitor'
require File.expand_path(File.join(File.dirname(__FILE__), '..', '..', 'spec_helper'))

require DataMapper.root / 'spec' / 'unit' / 'adapters' / 'adapter_shared_spec'

# TODO: make a shared adapter spec for all the DAO objects to adhere to

describe DataMapper::Adapters::DataObjectsAdapter do
  before :all do
    class ::Cheese
      include DataMapper::Resource
      property :id, Serial
      property :name, String, :nullable => false
      property :color, String, :default => 'yellow'
      property :notes, String, :length => 100, :lazy => true
    end
  end

  before do
    @uri     = Addressable::URI.parse('mock://localhost')
    @adapter = DataMapper::Adapters::DataObjectsAdapter.new(:default, @uri)
  end

  it_should_behave_like 'a DataMapper Adapter'

  describe "#find_by_sql" do

    before do
      class ::Plupp
        include DataMapper::Resource
        property :id, Integer, :key => true
        property :name, String
      end
    end

    it "should be added to DataMapper::Model" do
      DataMapper::Model.instance_methods.map { |m| m.to_s }.include?("find_by_sql").should == true
      Plupp.should respond_to(:find_by_sql)
    end

    describe "when called" do

      before do
        @reader = mock("reader")
        @reader.stub!(:next!).and_return(false)
        @reader.stub!(:close)
        @connection = mock("connection")
        @connection.stub!(:close)
        @command = mock("command")
        @adapter = Plupp.repository.adapter
        @repository = Plupp.repository
        @repository.stub!(:adapter).and_return(@adapter)
        @adapter.stub!(:create_connection).and_return(@connection)
        @adapter.should_receive(:is_a?).any_number_of_times.with(DataMapper::Adapters::DataObjectsAdapter).and_return(true)
      end

      it "should accept a single String argument with or without options hash" do
        @connection.should_receive(:create_command).twice.with("SELECT * FROM plupps").and_return(@command)
        @command.should_receive(:execute_reader).twice.and_return(@reader)
        Plupp.should_receive(:repository).any_number_of_times.and_return(@repository)
        Plupp.should_receive(:repository).any_number_of_times.with(:plupp_repo).and_return(@repository)
        Plupp.find_by_sql("SELECT * FROM plupps").to_a
        Plupp.find_by_sql("SELECT * FROM plupps", :repository => :plupp_repo).to_a
      end

      it "should accept an Array argument with or without options hash" do
        @connection.should_receive(:create_command).twice.with("SELECT * FROM plupps WHERE plur = ?").and_return(@command)
        @command.should_receive(:execute_reader).twice.with("my pretty plur").and_return(@reader)
        Plupp.should_receive(:repository).any_number_of_times.and_return(@repository)
        Plupp.should_receive(:repository).any_number_of_times.with(:plupp_repo).and_return(@repository)
        Plupp.find_by_sql(["SELECT * FROM plupps WHERE plur = ?", "my pretty plur"]).to_a
        Plupp.find_by_sql(["SELECT * FROM plupps WHERE plur = ?", "my pretty plur"], :repository => :plupp_repo).to_a
      end

      it "should accept a Query argument with or without options hash" do
        if ADAPTER == :mysql
          @connection.should_receive(:create_command).twice.with('SELECT `name` FROM `plupps` WHERE (`name` = ?) ORDER BY `id`').and_return(@command)
        else
          @connection.should_receive(:create_command).twice.with('SELECT "name" FROM "plupps" WHERE ("name" = ?) ORDER BY "id"').and_return(@command)
        end
        @command.should_receive(:execute_reader).twice.with('my pretty plur').and_return(@reader)
        Plupp.should_receive(:repository).any_number_of_times.and_return(@repository)
        Plupp.should_receive(:repository).any_number_of_times.with(:plupp_repo).and_return(@repository)
        Plupp.find_by_sql(DataMapper::Query.new(@repository, Plupp, "name" => "my pretty plur", :fields => ["name"])).to_a
        Plupp.find_by_sql(DataMapper::Query.new(@repository, Plupp, "name" => "my pretty plur", :fields => ["name"]), :repository => :plupp_repo).to_a
      end

      it "requires a Repository that is a DataObjectsRepository to work" do
        non_do_adapter = mock("non do adapter")
        non_do_repo = mock("non do repo")
        non_do_repo.stub!(:adapter).and_return(non_do_adapter)
        Plupp.should_receive(:repository).any_number_of_times.with(:plupp_repo).and_return(non_do_repo)
        Proc.new do
          Plupp.find_by_sql(:repository => :plupp_repo)
        end.should raise_error(Exception, /DataObjectsAdapter/)
      end

      it "requires some kind of query to work at all" do
        Plupp.should_receive(:repository).any_number_of_times.with(:plupp_repo).and_return(@repository)
        Proc.new do
          Plupp.find_by_sql(:repository => :plupp_repo)
        end.should raise_error(Exception, /requires a query/)
      end

    end

  end

  describe '#uri options' do
    it 'should transform a fully specified option hash into a URI' do
      options = {
        :adapter => 'mysql',
        :host => 'davidleal.com',
        :username => 'me',
        :password => 'mypass',
        :port => 5000,
        :database => 'you_can_call_me_al',
        :socket => 'nosock'
      }

      adapter = DataMapper::Adapters::DataObjectsAdapter.new(:spec, options)
      adapter.uri.should ==
        DataObjects::URI.parse("mysql://me:mypass@davidleal.com:5000/you_can_call_me_al?socket=nosock")
    end

    it 'should transform a minimal options hash into a URI' do
      options = {
        :adapter => 'mysql',
        :database => 'you_can_call_me_al'
      }

      adapter = DataMapper::Adapters::DataObjectsAdapter.new(:spec, options)
      adapter.uri.should == DataObjects::URI.parse("mysql:you_can_call_me_al")
    end

    it 'should accept the uri when no overrides exist' do
      uri = Addressable::URI.parse("protocol:///")
      DataMapper::Adapters::DataObjectsAdapter.new(:spec, uri).uri.should == DataObjects::URI.parse(uri)
    end
  end

  describe '#create' do
    before do
      @result = mock('result', :to_i => 1, :insert_id => 1)

      @adapter.stub!(:execute).and_return(@result)
      @adapter.stub!(:supports_returning?).and_return(false)

      @property    = mock('property', :kind_of? => true, :serial? => true, :name => :property, :field => 'property', :custom? => false, :typecast => 'bind value')
      @properties  = [ @property ]
      @bind_values = [ 'bind value' ]
      @attributes  = mock('attributes', :keys => @properties, :values => @bind_values)
      @model       = mock('model', :kind_of? => true, :key => [ @property ], :storage_name => 'models')
      @resource    = mock('resource', :model => @model, :dirty_attributes => @attributes)

      @property.stub!(:set!).and_return(@resource)

      @statement   = 'INSERT INTO "models" ("property") VALUES (?)'
    end

    def do_create
      @adapter.create([ @resource ])
    end

    it 'should use only dirty properties' do
      @resource.should_receive(:dirty_attributes).with(no_args).and_return(@attributes)
      do_create.should == 1
    end

    it 'should use the bind values' do
      @attributes.should_receive(:values).with(no_args).and_return(@bind_values)

      @adapter.should_receive(:execute).with(@statement, *@bind_values).and_return(@result)

      do_create.should == 1
    end

    it 'should generate an SQL statement when supports_returning? is true' do
      @property.should_receive(:serial?).with(no_args).and_return(true)
      @adapter.should_receive(:supports_returning?).with(no_args).and_return(true)

      @statement = 'INSERT INTO "models" ("property") VALUES (?) RETURNING "property"'
      @adapter.should_receive(:execute).with(@statement, 'bind value').and_return(@result)

      do_create.should == 1
    end

    it 'should generate an SQL statement when supports_default_values? is true' do
      @bind_values.clear
      @properties.clear
      @adapter.should_receive(:supports_default_values?).with(no_args).and_return(true)

      @statement = 'INSERT INTO "models" DEFAULT VALUES'
      @adapter.should_receive(:execute).with(@statement).and_return(@result)

      do_create.should == 1
    end

    it 'should generate an SQL statement when supports_default_values? is false' do
      @bind_values.clear
      @properties.clear
      @adapter.should_receive(:supports_default_values?).with(no_args).and_return(false)

      @statement = 'INSERT INTO "models" () VALUES ()'
      @adapter.should_receive(:execute).with(@statement).and_return(@result)

      do_create.should == 1
    end

    it 'should return 0 if no rows created' do
      @result.should_receive(:to_i).with(no_args).and_return(0)
      do_create.should == 0
    end

    it 'should return 1 if number of rows created is 1' do
      @result.should_receive(:to_i).with(no_args).and_return(1)
      do_create.should == 1
    end

    it 'should set the resource primary key if the model key size is 1 and the key is serial' do
      @model.key.size.should == 1
      @property.should_receive(:serial?).and_return(true)
      @result.should_receive(:insert_id).and_return(777)
      @property.should_receive(:set!).with(@resource, 777)
      do_create.should == 1
    end
  end

  [ :read_many, :read_one ].each do |method|
    describe "##{method}" do
      before do
        @key       = mock('key')
        @model     = mock('model', :key => @key, :storage_name => 'models', :relationships => {})
        @primitive = mock('primitive')
        @property  = mock('property', :kind_of? => true, :model => @model, :field => 'property', :primitive => @primitive)

        @child_model     = @model
        @parent_model    = mock('parent model', :storage_name => 'parents')
        @parent_property = mock('parent id', :kind_of? => true, :model => @parent_model, :field => 'id')

        @child_key    = [ @property ]
        @parent_key   = [ @parent_property ]
        @relationship = mock('relationship', :child_model => @child_model, :parent_model => @parent_model, :child_key => @child_key, :parent_key => @parent_key)
        @links        = [ @relationship ]

        @fields      = [ @property ]
        @bind_values = [ 'bind value' ]
        @conditions  = [ [ :eql, @property, @bind_values[0] ] ]

        @direction = mock('direction', :property => @property, :direction => :desc)
        @order     = [ @direction ]

        @query = mock('query', :model => @model, :kind_of? => true, :links => @links, :fields => @fields, :conditions => @conditions, :order => @order, :limit => 111, :offset => 222, :bind_values => @bind_values)
        @query.should_receive(:unique?).with(no_args).and_return(false)

        @reader     = mock('reader', :close => true, :next! => false)
        @command    = mock('command', :set_types => nil, :execute_reader => @reader)
        @connection = mock('connection', :close => true, :create_command => @command)

        DataObjects::Connection.stub!(:new).and_return(@connection)
        DataMapper::Query::Direction.stub!(:===).and_return(true)
      end

      if method == :read_one
        before do
          @query.should_receive(:limit).with(no_args).twice.and_return(1)

          @values = @bind_values.dup

          @reader.should_receive(:next!).with(no_args).and_return(true)
          @reader.should_receive(:values).with(no_args).and_return(@values)

          @resource = mock('resource')
          @resource.should_receive(:kind_of?).with(DataMapper::Resource).any_number_of_times.and_return(true)

          @model.should_receive(:load).with(@values, @query).and_return(@resource)

          @statement = 'SELECT "models"."property" FROM "models" INNER JOIN "parents" ON ("parents"."id" = "models"."property") WHERE ("models"."property" = ?) ORDER BY "models"."property" DESC LIMIT 1 OFFSET 222'
        end

        define_method(:do_read) do
          resource = @adapter.read_one(@query)
          resource.should == @resource
          resource
        end
      elsif method == :read_many
        before do
          @statement = 'SELECT "models"."property" FROM "models" INNER JOIN "parents" ON ("parents"."id" = "models"."property") WHERE ("models"."property" = ?) ORDER BY "models"."property" DESC LIMIT 111 OFFSET 222'
        end

        define_method(:do_read) do
          collection = @adapter.read_many(@query)
          collection.to_a
          collection
        end
      end

      it 'should use the bind values' do
        @command.should_receive(:execute_reader).with(*@bind_values).and_return(@reader)
        do_read
      end

      it 'should generate an SQL statement' do
        @connection.should_receive(:create_command).with(@statement).and_return(@command)
        do_read
      end

      it 'should generate an SQL statement with composite keys' do
        other_property = mock('other property', :kind_of? => true)
        other_property.should_receive(:field).with(:default).and_return('other')
        other_property.should_receive(:model).with(no_args).and_return(@model)

        other_value = 'other value'
        @bind_values << other_value
        @conditions << [ :eql, other_property, other_value ]

        @statement = %[SELECT "models"."property" FROM "models" INNER JOIN "parents" ON ("parents"."id" = "models"."property") WHERE ("models"."property" = ?) AND ("models"."other" = ?) ORDER BY "models"."property" DESC LIMIT #{method == :read_one ? '1' : '111'} OFFSET 222]
        @query.should_receive(:conditions).with(no_args).twice.and_return(@conditions)

        @connection.should_receive(:create_command).with(@statement).and_return(@command)

        do_read
      end

      it 'should set the return types to the property primitives' do
        @command.should_receive(:set_types).with([ @primitive ])
        do_read
      end

      it 'should close the reader' do
        @reader.should_receive(:close).with(no_args)
        do_read
      end

      it 'should close the connection' do
        @connection.should_receive(:close).with(no_args)
        do_read
      end

      if method == :read_one
        it 'should return a DataMapper::Resource' do
          do_read.should == be_kind_of(DataMapper::Resource)
        end
      else
        it 'should return a DataMapper::Collection' do
          do_read.should be_kind_of(DataMapper::Collection)
        end
      end
    end
  end

  describe '#update' do
    before do
      @result = mock('result', :to_i => 1)

      @adapter.stub!(:execute).and_return(@result)

      @values      = %w[ new ]
      @model       = mock('model', :storage_name => 'models')
      @property    = mock('property', :kind_of? => true, :field => 'property')
      @bind_values = [ 'bind value' ]
      @conditions  = [ [ :eql, @property, @bind_values[0] ] ]
      @attributes  = mock('attributes', :kind_of? => true, :empty? => false, :keys => [ @property ], :values => @values)
      @query       = mock('query', :kind_of? => true, :model => @model, :links => [], :conditions => @conditions, :bind_values => @bind_values)
      @statement   = 'UPDATE "models" SET "property" = ? WHERE ("property" = ?)'
    end

    def do_update
      @adapter.update(@attributes, @query)
    end

    it 'should use the bind values' do
      @attributes.should_receive(:values).with(no_args).and_return(@values)
      @query.should_receive(:bind_values).with(no_args).and_return(@bind_values)

      @adapter.should_receive(:execute).with(@statement, *@values + @bind_values).and_return(@result)

      do_update.should == 1
    end

    it 'should generate an SQL statement' do
      other_property = mock('other property', :kind_of? => true)
      other_property.should_receive(:field).with(:default).and_return('other')
      other_property.should_receive(:model).with(no_args).and_return(@model)

      other_value = 'other value'
      @bind_values << other_value
      @conditions << [ :eql, other_property, other_value ]

      @query.should_receive(:conditions).with(no_args).twice.and_return(@conditions)

      @statement   = 'UPDATE "models" SET "property" = ? WHERE ("property" = ?) AND ("other" = ?)'
      @adapter.should_receive(:execute).with(@statement, *%w[ new ] + @bind_values).and_return(@result)

      do_update.should == 1
    end

    it 'should return 0 if no rows updated' do
      @result.should_receive(:to_i).with(no_args).and_return(0)
      do_update.should == 0
    end

    it 'should return 1 if number of rows updated is 1' do
      @result.should_receive(:to_i).with(no_args).and_return(1)
      do_update.should == 1
    end
  end

  describe '#delete' do
    before do
      @result = mock('result', :to_i => 1)

      @adapter.stub!(:execute).and_return(@result)

      @model       = mock('model', :storage_name => 'models')
      @property    = mock('property', :kind_of? => true, :field => 'property')
      @bind_values = [ 'bind value' ]
      @conditions  = [ [ :eql, @property, @bind_values[0] ] ]
      @query       = mock('query', :kind_of? => true, :model => @model, :links => [], :conditions => @conditions, :bind_values => @bind_values)
      @resource    = mock('resource', :to_query => @query)
      @statement   = 'DELETE FROM "models" WHERE ("property" = ?)'
    end

    def do_delete
      @adapter.delete(@resource.to_query(@repository))
    end

    it 'should use the bind values' do
      @query.should_receive(:bind_values).with(no_args).and_return(@bind_values)

      @adapter.should_receive(:execute).with(@statement, *@bind_values).and_return(@result)

      do_delete.should == 1
    end

    it 'should generate an SQL statement' do
      other_property = mock('other property', :kind_of? => true)
      other_property.should_receive(:field).with(:default).and_return('other')
      other_property.should_receive(:model).with(no_args).and_return(@model)

      other_value = 'other value'
      @bind_values << other_value
      @conditions << [ :eql, other_property, other_value ]

      @query.should_receive(:conditions).with(no_args).twice.and_return(@conditions)

      @statement = 'DELETE FROM "models" WHERE ("property" = ?) AND ("other" = ?)'
      @adapter.should_receive(:execute).with(@statement, *@bind_values).and_return(@result)

      do_delete.should == 1
    end

    it 'should return 0 if no rows deleted' do
      @result.should_receive(:to_i).with(no_args).and_return(0)
      do_delete.should == 0
    end

    it 'should return 1 if number of rows deleted is 1' do
      @result.should_receive(:to_i).with(no_args).and_return(1)
      do_delete.should == 1
    end
  end

  describe "when upgrading tables" do
    it "should raise NotImplementedError when #storage_exists? is called" do
      lambda { @adapter.storage_exists?("cheeses") }.should raise_error(NotImplementedError)
    end

    describe "#upgrade_model_storage" do
      it "should call #create_model_storage" do
        @adapter.should_receive(:create_model_storage).with(repository, Cheese).and_return(true)
        @adapter.upgrade_model_storage(repository, Cheese).should == Cheese.properties
      end

      it "should check if all properties of the model have columns if the table exists" do
        @adapter.should_receive(:field_exists?).with("cheeses", "id").and_return(true)
        @adapter.should_receive(:field_exists?).with("cheeses", "name").and_return(true)
        @adapter.should_receive(:field_exists?).with("cheeses", "color").and_return(true)
        @adapter.should_receive(:field_exists?).with("cheeses", "notes").and_return(true)
        @adapter.should_receive(:storage_exists?).with("cheeses").and_return(true)
        @adapter.upgrade_model_storage(repository, Cheese).should == []
      end

      it "should create and execute add column statements for columns that dont exist" do
        @adapter.should_receive(:field_exists?).with("cheeses", "id").and_return(true)
        @adapter.should_receive(:field_exists?).with("cheeses", "name").and_return(true)
        @adapter.should_receive(:field_exists?).with("cheeses", "color").and_return(true)
        @adapter.should_receive(:field_exists?).with("cheeses", "notes").and_return(false)
        @adapter.should_receive(:storage_exists?).with("cheeses").and_return(true)
        connection = mock("connection")
        connection.should_receive(:close)
        @adapter.should_receive(:create_connection).and_return(connection)
        statement = mock("statement")
        command = mock("command")
        result = mock("result")
        command.should_receive(:execute_non_query).and_return(result)
        connection.should_receive(:create_command).with(statement).and_return(command)
        @adapter.should_receive(:alter_table_add_column_statement).with("cheeses",
                                                                             {
                                                                               :nullable? => true,
                                                                               :name => "notes",
                                                                               :serial? => false,
                                                                               :primitive => "VARCHAR",
                                                                               :size => 100
                                                                             }).and_return(statement)
        @adapter.upgrade_model_storage(repository, Cheese).should == [Cheese.notes]
      end
    end
  end

  describe '#execute' do
    before do
      @mock_command = mock('Command', :execute_non_query => nil)
      @mock_db = mock('DB Connection', :create_command => @mock_command, :close => true)

      @adapter.stub!(:create_connection).and_return(@mock_db)
    end

    it 'should #create_command from the sql passed' do
      @mock_db.should_receive(:create_command).with('SQL STRING').and_return(@mock_command)
      @adapter.execute('SQL STRING')
    end

    it 'should pass any additional args to #execute_non_query' do
      @mock_command.should_receive(:execute_non_query).with(:args)
      @adapter.execute('SQL STRING', :args)
    end

    it 'should return the result of #execute_non_query' do
      @mock_command.should_receive(:execute_non_query).and_return(:result_set)

      @adapter.execute('SQL STRING').should == :result_set
    end

    it 'should log any errors, then re-raise them' do
      @mock_command.should_receive(:execute_non_query).and_raise("Oh Noes!")
      DataMapper.logger.should_receive(:error)

      lambda { @adapter.execute('SQL STRING') }.should raise_error("Oh Noes!")
    end

    it 'should always close the db connection' do
      @mock_command.should_receive(:execute_non_query).and_raise("Oh Noes!")
      @mock_db.should_receive(:close)

      lambda { @adapter.execute('SQL STRING') }.should raise_error("Oh Noes!")
    end
  end

  describe '#query' do
    before do
      @mock_reader = mock('Reader', :fields => ['id', 'UserName', 'AGE'],
        :values => [1, 'rando', 27],
        :close => true)
      @mock_command = mock('Command', :execute_reader => @mock_reader)
      @mock_db = mock('DB Connection', :create_command => @mock_command, :close => true)

      #make the while loop run exactly once
      @mock_reader.stub!(:next!).and_return(true, nil)
      @adapter.stub!(:create_connection).and_return(@mock_db)
    end

    it 'should #create_command from the sql passed' do
      @mock_db.should_receive(:create_command).with('SQL STRING').and_return(@mock_command)
      @adapter.query('SQL STRING')
    end

    it 'should pass any additional args to #execute_reader' do
      @mock_command.should_receive(:execute_reader).with(:args).and_return(@mock_reader)
      @adapter.query('SQL STRING', :args)
    end

    describe 'returning multiple fields' do

      it 'should underscore the field names as members of the result struct' do
        @mock_reader.should_receive(:fields).and_return(['id', 'UserName', 'AGE'])

        result = @adapter.query('SQL STRING')

        result.first.members.map { |m| m.to_s }.should == %w[ id user_name age ]
      end

      it 'should convert each row into the struct' do
        @mock_reader.should_receive(:values).and_return([1, 'rando', 27])

        @adapter.query('SQL STRING')
      end

      it 'should add the row structs into the results array' do
        results = @adapter.query('SQL STRING')

        results.should be_kind_of(Array)

        row = results.first
        row.should be_kind_of(Struct)

        row.id.should == 1
        row.user_name.should == 'rando'
        row.age.should == 27
      end

    end

    describe 'returning a single field' do

      it 'should add the value to the results array' do
        @mock_reader.should_receive(:fields).and_return(['username'])
        @mock_reader.should_receive(:values).and_return(['rando'])

        results = @adapter.query('SQL STRING')

        results.should be_kind_of(Array)
        results.first.should == 'rando'
      end

    end

    it 'should log any errors, then re-raise them' do
      @mock_command.should_receive(:execute_non_query).and_raise("Oh Noes!")
      DataMapper.logger.should_receive(:error)

      lambda { @adapter.execute('SQL STRING') }.should raise_error("Oh Noes!")
    end

    it 'should always close the db connection' do
      @mock_command.should_receive(:execute_non_query).and_raise("Oh Noes!")
      @mock_db.should_receive(:close)

      lambda { @adapter.execute('SQL STRING') }.should raise_error("Oh Noes!")
    end
  end
end
