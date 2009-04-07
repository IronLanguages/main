require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

if ADAPTER
  module QuerySpec
    class SailBoat
      include DataMapper::Resource

      property :id,      Serial
      property :name,    String
      property :port,    String
      property :captain, String

      def self.default_repository_name
        ADAPTER
      end
    end

    class Permission
      include DataMapper::Resource

      property :id,            Serial
      property :user_id,       Integer
      property :resource_id,   Integer
      property :resource_type, String
      property :token,         String

      def self.default_repository_name
        ADAPTER
      end
    end

    class Region
      include DataMapper::Resource

      property :id,   Serial
      property :name, String
   property :type, String

      def self.default_repository_name
        ADAPTER
      end
    end

    class Factory
      include DataMapper::Resource

      property :id,        Serial
      property :region_id, Integer
      property :name,      String

      repository(:mock) do
        property :land, String
      end

      belongs_to :region

      def self.default_repository_name
        ADAPTER
      end
    end

    class Vehicle
      include DataMapper::Resource

      property :id,         Serial
      property :factory_id, Integer
      property :name,       String

      belongs_to :factory

      def self.default_repository_name
        ADAPTER
      end
    end

    class Group
      include DataMapper::Resource
      property :id, Serial
      property :name, String
    end
  end

  module Namespace
    class Region
      include DataMapper::Resource

      property :id,   Serial
      property :name, String

      def self.default_repository_name
        ADAPTER
      end
    end

    class Factory
      include DataMapper::Resource

      property :id,        Serial
      property :region_id, Integer
      property :name,      String

      repository(:mock) do
        property :land, String
      end

      belongs_to :region

      def self.default_repository_name
        ADAPTER
      end
    end

    class Vehicle
      include DataMapper::Resource
      property :id,         Serial
      property :factory_id, Integer
      property :name,       String

      belongs_to :factory

      def self.default_repository_name
        ADAPTER
      end
    end
  end

  describe DataMapper::Query, "with #{ADAPTER}" do
    before do
      @query = DataMapper::Query.new(repository(ADAPTER), QuerySpec::SailBoat)
    end

    it 'should be serializable with Marshal' do
      Marshal.load(Marshal.dump(@query)).should == @query
    end

    describe '#unique' do
      include LoggingHelper

      before(:each) do
        QuerySpec::SailBoat.auto_migrate!

        QuerySpec::SailBoat.create(:name => 'A', :port => 'C')
        QuerySpec::SailBoat.create(:name => 'B', :port => 'B')
        QuerySpec::SailBoat.create(:name => 'C', :port => 'A')
      end

      def parse_statement(log)
        log.readlines.join.chomp.split(' ~ ').last.sub(/\A\(\d+\.\d+\)\s+/, '')
      end

      describe 'when true' do
        if [ :postgres, :sqlite3, :mysql ].include?(ADAPTER)
          it 'should add a GROUP BY to the SQL query' do
            logger do |log|
              QuerySpec::SailBoat.all(:unique => true, :fields => [ :id ]).to_a

              case ADAPTER
                when :postgres, :sqlite3
                  parse_statement(log).should == 'SELECT "id" FROM "query_spec_sail_boats" GROUP BY "id" ORDER BY "id"'
                when :mysql
                  parse_statement(log).should == 'SELECT `id` FROM `query_spec_sail_boats` GROUP BY `id` ORDER BY `id`'
              end
            end
          end

          it 'should not add a GROUP BY to the SQL query if no field is a Property' do
            operator = DataMapper::Query::Operator.new(:thing, :test)

            # make the operator act like a Property
            class << operator
              property = QuerySpec::SailBoat.properties[:id]
              (property.methods - (public_instance_methods - %w[ type ])).each do |method|
                define_method(method) do |*args|
                  property.send(method, *args)
                end
              end
            end

            operator.should_not be_kind_of(DataMapper::Property)

            logger do |log|
              QuerySpec::SailBoat.all(:unique => true, :fields => [ operator ]).to_a

              case ADAPTER
                when :postgres, :sqlite3
                  parse_statement(log).should == 'SELECT "id" FROM "query_spec_sail_boats" ORDER BY "id"'
                when :mysql
                  parse_statement(log).should == 'SELECT `id` FROM `query_spec_sail_boats` ORDER BY `id`'
              end
            end
          end
        end
      end

      describe 'when false' do
        if [ :postgres, :sqlite3, :mysql ].include?(ADAPTER)
          it 'should not add a GROUP BY to the SQL query' do
            logger do |log|
              QuerySpec::SailBoat.all(:unique => false, :fields => [ :id ]).to_a

              case ADAPTER
                when :postgres, :sqlite3
                  parse_statement(log).should == 'SELECT "id" FROM "query_spec_sail_boats" ORDER BY "id"'
                when :mysql
                  parse_statement(log).should == 'SELECT `id` FROM `query_spec_sail_boats` ORDER BY `id`'
              end
            end
          end
        end
      end
    end

    describe 'when ordering' do
      before(:each) do
        QuerySpec::SailBoat.auto_migrate!

        QuerySpec::SailBoat.create(:name => 'A', :port => 'C')
        QuerySpec::SailBoat.create(:name => 'B', :port => 'B')
        QuerySpec::SailBoat.create(:name => 'C', :port => 'A')
      end

      it "should find by conditions" do
        lambda do
          repository(ADAPTER) do
            QuerySpec::SailBoat.first(:conditions => [ 'name = ?', 'B' ])
          end
        end.should_not raise_error

        lambda do
          repository(ADAPTER) do
            QuerySpec::SailBoat.first(:conditions => [ 'name = ?', 'A' ])
          end
        end.should_not raise_error
      end

      it "should find by conditions passed in as hash" do
        repository(ADAPTER) do
          QuerySpec::SailBoat.create(:name => "couldbe@email.com", :port => 'wee')

          find = QuerySpec::SailBoat.first(:name => 'couldbe@email.com')
          find.name.should == 'couldbe@email.com'

          find = QuerySpec::SailBoat.first(:name => 'couldbe@email.com', :port.not => nil)
          find.should_not be_nil
          find.port.should_not be_nil
          find.name.should == 'couldbe@email.com'
        end
      end

      it "should find by conditions passed in a range" do
        repository(ADAPTER) do
          find = QuerySpec::SailBoat.all(:id => 0..2)
          find.should_not be_nil
          find.should have(2).entries

          find = QuerySpec::SailBoat.all(:id.not => 0..2)
          find.should have(1).entries
        end
      end

      it "should find by conditions passed in as an array" do
        repository(ADAPTER) do
          find = QuerySpec::SailBoat.all(:id => [1,2])
          find.should_not be_nil
          find.should have(2).entries

          find = QuerySpec::SailBoat.all(:id.not => [1,2])
          find.should have(1).entries
        end
      end

      describe "conditions passed in as an empty array" do
        it "should work when id is an empty Array" do
          repository(ADAPTER) do
            find = QuerySpec::SailBoat.all(:id => [])
            find.should have(0).entries
          end
        end

        it "should work when id is NOT an empty Array" do
          repository(ADAPTER) do
            find = QuerySpec::SailBoat.all(:id.not => [])
            find.should have(3).entries
          end
        end

        it "should work when id is an empty Array and other conditions are specified" do
          repository(ADAPTER) do
            find = QuerySpec::SailBoat.all(:id => [], :name => "A")
            find.should have(0).entries
          end
        end

        it "should work when id is NOT an empty Array and other conditions are specified" do
          repository(ADAPTER) do
            find = QuerySpec::SailBoat.all(:id.not => [], :name => "A")
            find.should have(1).entries
          end
        end

        it "should work when id is NOT an empty Array and other Array conditions are specified" do
          repository(ADAPTER) do
            find = QuerySpec::SailBoat.all(:id.not => [], :name => ["A", "B"])
            find.should have(2).entries
          end
        end
      end

      it "should order results" do
        repository(ADAPTER) do
          result = QuerySpec::SailBoat.all(:order => [
            DataMapper::Query::Direction.new(QuerySpec::SailBoat.properties[:name], :asc)
          ])
          result[0].id.should == 1

          result = QuerySpec::SailBoat.all(:order => [
            DataMapper::Query::Direction.new(QuerySpec::SailBoat.properties[:port], :asc)
          ])
          result[0].id.should == 3

          result = QuerySpec::SailBoat.all(:order => [
            DataMapper::Query::Direction.new(QuerySpec::SailBoat.properties[:name], :asc),
            DataMapper::Query::Direction.new(QuerySpec::SailBoat.properties[:port], :asc)
          ])
          result[0].id.should == 1

          result = QuerySpec::SailBoat.all(:order => [
            QuerySpec::SailBoat.properties[:name],
            DataMapper::Query::Direction.new(QuerySpec::SailBoat.properties[:port], :asc)
          ])
          result[0].id.should == 1

          result = QuerySpec::SailBoat.all(:order => [ :name ])
          result[0].id.should == 1

          result = QuerySpec::SailBoat.all(:order => [ :name.desc ])
          result[0].id.should == 3
        end
      end
    end

    describe 'when sub-selecting' do
      before(:each) do
        [ QuerySpec::SailBoat, QuerySpec::Permission ].each { |m| m.auto_migrate! }

        QuerySpec::SailBoat.create(:id => 1, :name => "Fantasy I",      :port => "Cape Town", :captain => 'Joe')
        QuerySpec::SailBoat.create(:id => 2, :name => "Royal Flush II", :port => "Cape Town", :captain => 'James')
        QuerySpec::SailBoat.create(:id => 3, :name => "Infringer III",  :port => "Cape Town", :captain => 'Jason')

        #User 1 permission -- read boat 1 & 2
        QuerySpec::Permission.create(:id => 1, :user_id => 1, :resource_id => 1, :resource_type => 'SailBoat', :token => 'READ')
        QuerySpec::Permission.create(:id => 2, :user_id => 1, :resource_id => 2, :resource_type => 'SailBoat', :token => 'READ')

        #User 2 permission  -- read boat 2 & 3
        QuerySpec::Permission.create(:id => 3, :user_id => 2, :resource_id => 2, :resource_type => 'SailBoat', :token => 'READ')
        QuerySpec::Permission.create(:id => 4, :user_id => 2, :resource_id => 3, :resource_type => 'SailBoat', :token => 'READ')
      end

      it 'should accept a DM::Query as a value of a condition' do
        # User 1
        acl = DataMapper::Query.new(repository(ADAPTER), QuerySpec::Permission, :user_id => 1, :resource_type => 'SailBoat', :token => 'READ', :fields => [ :resource_id ])
        query = { :port => 'Cape Town', :id => acl, :captain.like => 'J%', :order => [ :id ] }
        boats = repository(ADAPTER) { QuerySpec::SailBoat.all(query) }
        boats.should have(2).entries
        boats.entries[0].id.should == 1
        boats.entries[1].id.should == 2

        # User 2
        acl = DataMapper::Query.new(repository(ADAPTER), QuerySpec::Permission, :user_id => 2, :resource_type => 'SailBoat', :token => 'READ', :fields => [ :resource_id ])
        query = { :port => 'Cape Town', :id => acl, :captain.like => 'J%', :order => [ :id ] }
        boats = repository(ADAPTER) { QuerySpec::SailBoat.all(query) }

        boats.should have(2).entries
        boats.entries[0].id.should == 2
        boats.entries[1].id.should == 3
      end

      it 'when value is NOT IN another query' do
        # Boats that User 1 Cannot see
        acl = DataMapper::Query.new(repository(ADAPTER), QuerySpec::Permission, :user_id => 1, :resource_type => 'SailBoat', :token => 'READ', :fields => [ :resource_id ])
        query = { :port => 'Cape Town', :id.not => acl, :captain.like => 'J%' }
        boats = repository(ADAPTER) { QuerySpec::SailBoat.all(query) }
        boats.should have(1).entries
        boats.entries[0].id.should == 3
      end
    end  # describe sub-selecting

    describe 'when linking associated objects' do
      before(:each) do
        [ QuerySpec::Region, QuerySpec::Factory, QuerySpec::Vehicle ].each { |m| m.auto_migrate! }

        QuerySpec::Region.create(:id => 1, :name => 'North West', :type => 'commercial')
        QuerySpec::Factory.create(:id => 2000, :region_id => 1, :name => 'North West Plant')
        QuerySpec::Vehicle.create(:id => 1, :factory_id => 2000, :name => '10 ton delivery truck')

        Namespace::Region.auto_migrate!
        Namespace::Factory.auto_migrate!
        Namespace::Vehicle.auto_migrate!

        Namespace::Region.create(:id => 1, :name => 'North West')
        Namespace::Factory.create(:id => 2000, :region_id => 1, :name => 'North West Plant')
        Namespace::Vehicle.create(:id => 1, :factory_id => 2000, :name => '10 ton delivery truck')
      end

      it 'should require that all properties in :fields and all :links come from the same repository' #do
      #  land = QuerySpec::Factory.properties(:mock)[:land]
      #  fields = []
      #  QuerySpec::Vehicle.properties(ADAPTER).map do |property|
      #    fields << property
      #  end
      #  fields << land
      #
      #  lambda{
      #    begin
      #      results = repository(ADAPTER) { QuerySpec::Vehicle.all(:links => [ :factory ], :fields => fields) }
      #    rescue RuntimeError
      #      $!.message.should == "Property QuerySpec::Factory.land not available in repository #{ADAPTER}"
      #      raise $!
      #    end
      #  }.should raise_error(RuntimeError)
      #end

      it 'should accept a DM::Assoc::Relationship as a link' do
        factory = DataMapper::Associations::Relationship.new(
          :factory,
          ADAPTER,
          QuerySpec::Vehicle,
          QuerySpec::Factory,
          { :child_key => [ :factory_id ], :parent_key => [ :id ] }
        )
        results = repository(ADAPTER) { QuerySpec::Vehicle.all(:links => [ factory ]) }
        results.should have(1).entries
      end

      it 'should accept a symbol of an association name as a link' do
        results = repository(ADAPTER) { QuerySpec::Vehicle.all(:links => [ :factory ]) }
        results.should have(1).entries
      end

      it 'should accept a string of an association name as a link' do
        results = repository(ADAPTER) { QuerySpec::Vehicle.all(:links => [ 'factory' ]) }
        results.should have(1).entries
      end

      it 'should accept a mixture of items as a set of links' do
        region = DataMapper::Associations::Relationship.new(
          :region,
          ADAPTER,
          QuerySpec::Factory,
          QuerySpec::Region,
          { :child_key => [ :region_id ], :parent_key => [ :id ] }
        )
        results = repository(ADAPTER) { QuerySpec::Vehicle.all(:links => [ 'factory', region ]) }
        results.should have(1).entries
      end

      it 'should only accept a DM::Assoc::Relationship, String & Symbol as a link' do
        lambda{
          DataMapper::Query.new(repository(ADAPTER), QuerySpec::Vehicle, :links => [1])
        }.should raise_error(ArgumentError)
      end

      it 'should have a association by the name of the Symbol or String' do
        lambda{
          DataMapper::Query.new(repository(ADAPTER), QuerySpec::Vehicle, :links => [ 'Sailing' ])
        }.should raise_error(ArgumentError)

        lambda{
          DataMapper::Query.new(repository(ADAPTER), QuerySpec::Vehicle, :links => [ :sailing ])
        }.should raise_error(ArgumentError)
      end

      it 'should create an n-level query path' do
        QuerySpec::Vehicle.factory.region.model.should == QuerySpec::Region
        QuerySpec::Vehicle.factory.region.name.property.should == QuerySpec::Region.properties(QuerySpec::Region.repository.name)[ :name ]
      end

      it 'should accept a DM::QueryPath as the key to a condition' do
        vehicle = QuerySpec::Vehicle.first(QuerySpec::Vehicle.factory.region.name => 'North West')
        vehicle.name.should == '10 ton delivery truck'

        vehicle = Namespace::Vehicle.first(Namespace::Vehicle.factory.region.name => 'North West')
        vehicle.name.should == '10 ton delivery truck'
      end

      it "should accept a string representing a DM::QueryPath as they key to a condition" do
        vehicle = QuerySpec::Vehicle.first("factory.region.name" => 'North West')
        vehicle.name.should == '10 ton delivery truck'
      end

   it "should accept 'id' and 'type' as endpoints on ah DM::QueryPath" do
    vehicle = QuerySpec::Vehicle.first( QuerySpec::Vehicle.factory.region.type => 'commercial' )
    vehicle.name.should == '10 ton delivery truck'
    vehicle = QuerySpec::Vehicle.first( QuerySpec::Vehicle.factory.region.id => 1 )
    vehicle.name.should == '10 ton delivery truck'
   end

      it 'should auto generate the link if a DM::Property from a different resource is in the :fields option'

      it 'should create links with composite keys'

      it 'should eager load associations' do
        repository(ADAPTER) do
          vehicle = QuerySpec::Vehicle.first(:includes => [ QuerySpec::Vehicle.factory ])
        end
      end

      it "should behave when using mocks" do
        QuerySpec::Group.should_receive(:all).with(:order => [ :id.asc ])
        QuerySpec::Group.all(:order => [ :id.asc ])
      end
    end   # describe links
  end # DM::Query
end
