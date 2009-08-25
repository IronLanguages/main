require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

if ADAPTER
  class Zebra
    include DataMapper::Resource

    def self.default_repository_name
      ADAPTER
    end

    property :id, Serial
    property :name, String
    property :age, Integer
    property :notes, Text

    has n, :stripes
  end

  class Stripe
    include DataMapper::Resource

    def self.default_repository_name
      ADAPTER
    end

    property :id, Serial
    property :name, String
    property :age,  Integer
    property :zebra_id, Integer

    belongs_to :zebra

    def self.sort_by_name
      all(:order => [ :name ])
    end
  end

  class CollectionSpecParty
    include DataMapper::Resource

    def self.default_repository_name
      ADAPTER
    end

    property :name, String, :key => true
    property :type, Discriminator
  end

  class CollectionSpecUser < CollectionSpecParty
    def self.default_repository_name
      ADAPTER
    end

    property :username, String
    property :password, String
  end

  module CollectionSpecHelper
    def setup
      Zebra.auto_migrate!(ADAPTER)
      Stripe.auto_migrate!(ADAPTER)

      repository(ADAPTER) do
        @nancy  = Zebra.create(:name => 'Nancy',  :age => 11, :notes => 'Spotted!')
        @bessie = Zebra.create(:name => 'Bessie', :age => 10, :notes => 'Striped!')
        @steve  = Zebra.create(:name => 'Steve',  :age => 8,  :notes => 'Bald!')

        @babe     = Stripe.create(:name => 'Babe')
        @snowball = Stripe.create(:name => 'snowball')

        @nancy.stripes << @babe
        @nancy.stripes << @snowball
        @nancy.save
      end
    end
  end

  describe DataMapper::Collection do
    include CollectionSpecHelper

    before do
      setup
    end

    before do
      @repository = repository(ADAPTER)
      @model      = Zebra
      @query      = DataMapper::Query.new(@repository, @model, :order => [ :id ])
      @collection = @repository.read_many(@query)
      @other      = @repository.read_many(@query.merge(:limit => 2))
    end

    it "should return the correct repository" do
      repository = repository(:legacy)
      query      = DataMapper::Query.new(repository, @model)
      DataMapper::Collection.new(query){}.repository.object_id.should == repository.object_id
    end

    it "should be able to add arbitrary objects" do
      properties = @model.properties(:default)

      collection = DataMapper::Collection.new(@query) do |c|
        c.load([ 4, 'Bob',   10 ])
        c.load([ 5, 'Nancy', 11 ])
      end

      collection.should respond_to(:reload)

      results = collection.entries
      results.should have(2).entries

      results.each do |cow|
        cow.attribute_loaded?(:name).should == true
        cow.attribute_loaded?(:age).should == true
      end

      bob, nancy = results[0], results[1]

      bob.name.should eql('Bob')
      bob.age.should eql(10)
      bob.should_not be_a_new_record

      nancy.name.should eql('Nancy')
      nancy.age.should eql(11)
      nancy.should_not be_a_new_record

      results.first.should == bob
    end

    it 'should be serializable with Marshal' do
      Marshal.load(Marshal.dump(Zebra.all)).should == Zebra.all
    end

    describe 'model proxying' do
      it 'should delegate to a model method' do
        stripes = @model.first.stripes
        stripes.should respond_to(:sort_by_name)
        stripes.sort_by_name.should == [ @babe, @snowball ]
      end
    end

    describe 'association proxying' do
      it "should provide a Query" do
        repository(ADAPTER) do
          zebras = Zebra.all(:order => [ :name ])
          zebras.query.order.should == [DataMapper::Query::Direction.new(Zebra.properties(ADAPTER)[:name])]
        end
      end

      it "should proxy the relationships of the model" do
        repository(ADAPTER) do
          zebras = Zebra.all
          zebras.should have(3).entries
          zebras.find { |zebra| zebra.name == 'Nancy' }.stripes.should have(2).entries
          zebras.should respond_to(:stripes)
          zebras.stripes.should == [@babe, @snowball]
        end
      end

      it "should preserve it's order on reload" do
        repository(ADAPTER) do |r|
          zebras = Zebra.all(:order => [ :name ])

          order = %w{ Bessie Nancy Steve }

          zebras.map { |z| z.name }.should == order

          # Force a lazy-load call:
          zebras.first.notes

          # The order should be unaffected.
          zebras.map { |z| z.name }.should == order
        end
      end
    end

    describe '.new' do
      describe 'with non-index keys' do
        it 'should instantiate read-only resources' do
          @collection = DataMapper::Collection.new(DataMapper::Query.new(@repository, @model, :fields => [ :age ])) do |c|
            c.load([ 1 ])
          end

          @collection.size.should == 1

          resource = @collection.entries[0]

          resource.should be_kind_of(@model)
          resource.collection.object_id.should == @collection.object_id
          resource.should_not be_new_record
          resource.should be_readonly
          resource.age.should == 1
        end
      end

      describe 'with inheritance property' do
        before do
          CollectionSpecUser.auto_migrate!
          CollectionSpecUser.create(:name => 'John')

          properties = CollectionSpecParty.properties(:default)
        end

        it 'should instantiate resources using the inheritance property class' do
          query = DataMapper::Query.new(@repository, CollectionSpecParty)
          collection = @repository.read_many(query)
          collection.should have(1).entries
          collection.first.model.should == CollectionSpecUser
        end
      end
    end

    [ true, false ].each do |loaded|
      describe " (#{loaded ? '' : 'not '}loaded) " do
        if loaded
          before do
            @collection.to_a
          end
        end

        describe '#<<' do
          it 'should relate each new resource to the collection' do
            # resource is orphaned
            @nancy.collection.object_id.should_not == @collection.object_id

            @collection << @nancy

            # resource is related
            @nancy.collection.object_id.should == @collection.object_id
          end

          it 'should return self' do
            @collection.<<(@steve).object_id.should == @collection.object_id
          end
        end

        describe '#all' do
          describe 'with no arguments' do
            it 'should return self' do
              @collection.all.object_id.should == @collection.object_id
            end
          end

          describe 'with query arguments' do
            describe 'should return a Collection' do
              before do
                @query.update(:offset => 10, :limit => 10)
                query = DataMapper::Query.new(@repository, @model)
                @unlimited = DataMapper::Collection.new(query) {}
              end

              it 'has an offset equal to 10' do
                @collection.all.query.offset.should == 10
              end

              it 'has a cumulative offset equal to 11 when passed an offset of 1' do
                @collection.all(:offset => 1).query.offset.should == 11
              end

              it 'has a cumulative offset equal to 19 when passed an offset of 9' do
                @collection.all(:offset => 9).query.offset.should == 19
              end

              it 'is empty when passed an offset that is out of range' do
                pending do
                  empty_collection = @collection.all(:offset => 10)
                  empty_collection.should == []
                  empty_collection.should be_loaded
                end
              end

              it 'has an limit equal to 10' do
                @collection.all.query.limit.should == 10
              end

              it 'has a limit equal to 5' do
                @collection.all(:limit => 5).query.limit.should == 5
              end

              it 'has a limit equal to 10 if passed a limit greater than 10' do
                @collection.all(:limit => 11).query.limit.should == 10
              end

              it 'has no limit' do
                @unlimited.all.query.limit.should be_nil
              end

              it 'has a limit equal to 1000 when passed a limit of 1000' do
                @unlimited.all(:limit => 1000).query.limit.should == 1000
              end
            end
          end
        end

        describe '#at' do
          it 'should return a Resource' do
            resource_at = @collection.at(1)
            resource_at.should be_kind_of(DataMapper::Resource)
            resource_at.id.should == @bessie.id
          end

          it 'should return a Resource when using a negative index' do
            resource_at = @collection.at(-1)
            resource_at.should be_kind_of(DataMapper::Resource)
            resource_at.id.should == @steve.id
          end
        end

        describe '#build' do
          it 'should build a new resource' do
            resource = @collection.build(:name => 'John')
            resource.should be_kind_of(@model)
            resource.should be_new_record
          end

          it 'should append the new resource to the collection' do
            resource = @collection.build(:name => 'John')
            resource.should be_new_record
            resource.collection.object_id.should == @collection.object_id
            @collection.should include(resource)
          end

          it 'should use the query conditions to set default values' do
            resource = @collection.build
            resource.should be_new_record
            resource.name.should be_nil

            @collection.query.update(:name => 'John')

            resource = @collection.build
            resource.name.should == 'John'
          end
        end

        describe '#clear' do
          it 'should orphan the resource from the collection' do
            entries = @collection.entries

            # resources are related
            entries.each { |r| r.collection.object_id.should == @collection.object_id }

            @collection.should have(3).entries
            @collection.clear
            @collection.should be_empty

            # resources are orphaned
            entries.each { |r| r.collection.object_id.should_not == @collection.object_id }
          end

          it 'should return self' do
            @collection.clear.object_id.should == @collection.object_id
          end
        end

        describe '#collect!' do
          it 'should return self' do
            @collection.collect! { |resource| resource }.object_id.should == @collection.object_id
          end
        end

        describe '#concat' do
          it 'should return self' do
            @collection.concat(@other).object_id.should == @collection.object_id
          end
        end

        describe '#create' do
          it 'should create a new resource' do
            resource = @collection.create(:name => 'John')
            resource.should be_kind_of(@model)
            resource.should_not be_new_record
          end

          it 'should append the new resource to the collection' do
            resource = @collection.create(:name => 'John')
            resource.should_not be_new_record
            resource.collection.object_id.should == @collection.object_id
            @collection.should include(resource)
          end

          it 'should not append the resource if it was not saved' do
            @repository.should_receive(:create).and_return(false)
            Zebra.should_receive(:repository).at_least(:once).and_return(@repository)

            resource = @collection.create(:name => 'John')
            resource.should be_new_record

            resource.collection.object_id.should_not == @collection.object_id
            @collection.should_not include(resource)
          end

          it 'should use the query conditions to set default values' do
            resource = @collection.create
            resource.should_not be_new_record
            resource.name.should be_nil

            @collection.query.update(:name => 'John')

            resource = @collection.create
            resource.name.should == 'John'
          end
        end

        describe '#delete' do
          it 'should orphan the resource from the collection' do
            collection = @nancy.collection

            # resource is related
            @nancy.collection.object_id.should == collection.object_id

            collection.should have(1).entries
            collection.delete(@nancy)
            collection.should be_empty

            # resource is orphaned
            @nancy.collection.object_id.should_not == collection.object_id
          end

          it 'should return a Resource' do
            collection = @nancy.collection

            resource = collection.delete(@nancy)

            resource.should be_kind_of(DataMapper::Resource)
            resource.object_id.should == @nancy.object_id
          end
        end

        describe '#delete_at' do
          it 'should orphan the resource from the collection' do
            collection = @nancy.collection

            # resource is related
            @nancy.collection.object_id.should == collection.object_id

            collection.should have(1).entries
            collection.delete_at(0).object_id.should == @nancy.object_id
            collection.should be_empty

            # resource is orphaned
            @nancy.collection.object_id.should_not == collection.object_id
          end

          it 'should return a Resource' do
            collection = @nancy.collection

            resource = collection.delete_at(0)

            resource.should be_kind_of(DataMapper::Resource)
            resource.object_id.should == @nancy.object_id
          end
        end

        describe '#destroy!' do
          before do
            @ids = [ @nancy.id, @bessie.id, @steve.id ]
          end

          it 'should destroy the resources in the collection' do
            @collection.map { |r| r.id }.should == @ids
            @collection.destroy!.should == true
            @model.all(:id => @ids).should == []
            @collection.reload.should == []
          end

          it 'should clear the collection' do
            @collection.map { |r| r.id }.should == @ids
            @collection.destroy!.should == true
            @collection.should == []
          end
        end

        describe '#each' do
          it 'should return self' do
            @collection.each { |resource| }.object_id.should == @collection.object_id
          end
        end

        describe '#each_index' do
          it 'should return self' do
            @collection.each_index { |resource| }.object_id.should == @collection.object_id
          end
        end

        describe '#eql?' do
          it 'should return true if for the same collection' do
            @collection.object_id.should == @collection.object_id
            @collection.should be_eql(@collection)
          end

          it 'should return true for duplicate collections' do
            dup = @collection.dup
            dup.should be_kind_of(DataMapper::Collection)
            dup.object_id.should_not == @collection.object_id
            dup.entries.should == @collection.entries
            dup.should be_eql(@collection)
          end

          it 'should return false for different collections' do
            @collection.should_not be_eql(@other)
          end
        end

        describe '#fetch' do
          it 'should return a Resource' do
            @collection.fetch(0).should be_kind_of(DataMapper::Resource)
          end
        end

        describe '#first' do
          describe 'with no arguments' do
            it 'should return a Resource' do
              first = @collection.first
              first.should_not be_nil
              first.should be_kind_of(DataMapper::Resource)
              first.id.should == @nancy.id
            end
          end

          describe 'with limit specified' do
            it 'should return a Collection' do
              collection = @collection.first(2)

              collection.should be_kind_of(DataMapper::Collection)
              collection.object_id.should_not == @collection.object_id

              collection.query.order.size.should == 1
              collection.query.order.first.property.should == @model.properties[:id]
              collection.query.order.first.direction.should == :asc

              collection.query.offset.should == 0
              collection.query.limit.should == 2

              collection.length.should == 2

              collection.entries.map { |r| r.id }.should == [ @nancy.id, @bessie.id ]
            end

            it 'should return a Collection if limit is 1' do
              collection = @collection.first(1)

              collection.should be_kind_of(DataMapper::Collection)
              collection.object_id.should_not == @collection.object_id
            end
          end
        end

        describe '#freeze' do
          it 'should freeze the underlying array' do
            @collection.should_not be_frozen
            @collection.freeze
            @collection.should be_frozen
          end
        end

        describe '#get' do
          it 'should find a resource in a collection by key' do
            resource = @collection.get(*@nancy.key)
            resource.should be_kind_of(DataMapper::Resource)
            resource.id.should == @nancy.id
          end

          it "should find a resource in a collection by typecasting the key" do
            resource = @collection.get(*@nancy.key)
            resource.should be_kind_of(DataMapper::Resource)
            resource.id.should == @nancy.id
          end

          it 'should not find a resource not in the collection' do
            @query.update(:offset => 0, :limit => 3)
            @david = Zebra.create(:name => 'David', :age => 15,  :notes => 'Albino')
            @collection.get(*@david.key).should be_nil
          end
        end

        describe '#get!' do
          it 'should find a resource in a collection by key' do
            resource = @collection.get!(*@nancy.key)
            resource.should be_kind_of(DataMapper::Resource)
            resource.id.should == @nancy.id
          end

          it 'should raise an exception if the resource is not found' do
            @query.update(:offset => 0, :limit => 3)
            @david = Zebra.create(:name => 'David', :age => 15,  :notes => 'Albino')
            lambda {
              @collection.get!(@david.key)
            }.should raise_error(DataMapper::ObjectNotFoundError)
          end
        end

        describe '#insert' do
          it 'should return self' do
            @collection.insert(1, @steve).object_id.should == @collection.object_id
          end
        end

        describe '#last' do
          describe 'with no arguments' do
            it 'should return a Resource' do
              last = @collection.last
              last.should_not be_nil
              last.should be_kind_of(DataMapper::Resource)
              last.id.should == @steve.id
            end
          end

          describe 'with limit specified' do
            it 'should return a Collection' do
              collection = @collection.last(2)

              collection.should be_kind_of(DataMapper::Collection)
              collection.object_id.should_not == @collection.object_id

              collection.query.order.size.should == 1
              collection.query.order.first.property.should == @model.properties[:id]
              collection.query.order.first.direction.should == :desc

              collection.query.offset.should == 0
              collection.query.limit.should == 2

              collection.length.should == 2

              collection.entries.map { |r| r.id }.should == [ @bessie.id, @steve.id ]
            end

            it 'should return a Collection if limit is 1' do
              collection = @collection.last(1)

              collection.class.should == DataMapper::Collection  # should be_kind_of(DataMapper::Collection)
              collection.object_id.should_not == @collection.object_id
            end
          end
        end

        describe '#load' do
          it 'should load resources from the identity map when possible' do
            @steve.collection = nil
            @repository.identity_map(@model).should_receive(:get).with([ @steve.id ]).and_return(@steve)

            collection = @repository.read_many(@query.merge(:id => @steve.id))

            collection.size.should == 1
            collection.map { |r| r.object_id }.should == [ @steve.object_id ]

            @steve.collection.object_id.should == collection.object_id
          end

          it 'should return a Resource' do
            @collection.load([ @steve.id, @steve.name, @steve.age ]).should be_kind_of(DataMapper::Resource)
          end
        end

        describe '#loaded?' do
          if loaded
            it 'should return true for an initialized collection' do
              @collection.should be_loaded
            end
          else
            it 'should return false for an uninitialized collection' do
              @collection.should_not be_loaded
              @collection.to_a  # load collection
              @collection.should be_loaded
            end
          end
        end

        describe '#pop' do
          it 'should orphan the resource from the collection' do
            collection = @steve.collection

            # resource is related
            @steve.collection.object_id.should == collection.object_id

            collection.should have(1).entries
            collection.pop.object_id.should == @steve.object_id
            collection.should be_empty

            # resource is orphaned
            @steve.collection.object_id.should_not == collection.object_id
          end

          it 'should return a Resource' do
            @collection.pop.key.should == @steve.key
          end
        end

        describe '#properties' do
          it 'should return a PropertySet' do
            @collection.properties.should be_kind_of(DataMapper::PropertySet)
          end

          it 'should contain same properties as query.fields' do
            properties = @collection.properties
            properties.entries.should == @collection.query.fields
          end
        end

        describe '#push' do
          it 'should relate each new resource to the collection' do
            # resource is orphaned
            @nancy.collection.object_id.should_not == @collection.object_id

            @collection.push(@nancy)

            # resource is related
            @nancy.collection.object_id.should == @collection.object_id
          end

          it 'should return self' do
            @collection.push(@steve).object_id.should == @collection.object_id
          end
        end

        describe '#relationships' do
          it 'should return a Hash' do
            @collection.relationships.should be_kind_of(Hash)
          end

          it 'should contain same properties as query.model.relationships' do
            relationships = @collection.relationships
            relationships.should == @collection.query.model.relationships
          end
        end

        describe '#reject' do
          it 'should return a Collection with resources that did not match the block' do
            rejected = @collection.reject { |resource| false }
            rejected.class.should == Array
            rejected.should == [ @nancy, @bessie, @steve ]
          end

          it 'should return an empty Array if resources matched the block' do
            rejected = @collection.reject { |resource| true }
            rejected.class.should == Array
            rejected.should == []
          end
        end

        describe '#reject!' do
          it 'should return self if resources matched the block' do
            @collection.reject! { |resource| true }.object_id.should == @collection.object_id
          end

          it 'should return nil if no resources matched the block' do
            @collection.reject! { |resource| false }.should be_nil
          end
        end

        describe '#reload' do
          it 'should return self' do
            @collection.reload.object_id.should == @collection.object_id
          end

          it 'should replace the collection' do
            original = @collection.dup
            @collection.reload.should == @collection
            @collection.should == original
          end

          it 'should reload lazily initialized fields' do
            pending 'Move to unit specs'

            @repository.should_receive(:all) do |model,query|
              model.should == @model

              query.should be_instance_of(DataMapper::Query)
              query.reload.should     == true
              query.offset.should     == 0
              query.limit.should      == 10
              query.order.should      == []
              query.fields.should     == @model.properties.defaults
              query.links.should      == []
              query.includes.should   == []
              query.conditions.should == [ [ :eql, @model.properties[:id], [ 1, 2, 3 ] ] ]

              @collection
            end

            @collection.reload
          end
        end

        describe '#replace' do
          it "should orphan each existing resource from the collection if loaded?" do
            entries = @collection.entries

            # resources are related
            entries.each { |r| r.collection.object_id.should == @collection.object_id }

            @collection.should have(3).entries
            @collection.replace([]).object_id.should == @collection.object_id
            @collection.should be_empty

            # resources are orphaned
            entries.each { |r| r.collection.object_id.should_not == @collection.object_id }
          end

          it 'should relate each new resource to the collection' do
            # resource is orphaned
            @nancy.collection.object_id.should_not == @collection.object_id

            @collection.replace([ @nancy ])

            # resource is related
            @nancy.collection.object_id.should == @collection.object_id
          end

          it 'should replace the contents of the collection' do
            other = [ @nancy ]
            @collection.should_not == other
            @collection.replace(other)
            @collection.should == other
            @collection.object_id.should_not == @other.object_id
          end
        end

        describe '#reverse' do
          [ true, false ].each do |loaded|
            describe "on a collection where loaded? == #{loaded}" do
              before do
                @collection.to_a if loaded
              end

              it 'should return a Collection with reversed entries' do
                reversed = @collection.reverse
                reversed.should be_kind_of(DataMapper::Collection)
                reversed.object_id.should_not == @collection.object_id
                reversed.entries.should == @collection.entries.reverse

                reversed.query.order.size.should == 1
                reversed.query.order.first.property.should == @model.properties[:id]
                reversed.query.order.first.direction.should == :desc
              end
            end
          end
        end

        describe '#reverse!' do
          it 'should return self' do
            @collection.reverse!.object_id.should == @collection.object_id
          end
        end

        describe '#reverse_each' do
          it 'should return self' do
            @collection.reverse_each { |resource| }.object_id.should == @collection.object_id
          end
        end

        describe '#select' do
          it 'should return an Array with resources that matched the block' do
            selected = @collection.select { |resource| true }
            selected.class.should == Array
            selected.should == @collection
          end

          it 'should return an empty Array if no resources matched the block' do
            selected = @collection.select { |resource| false }
            selected.class.should == Array
            selected.should == []
          end
        end

        describe '#shift' do
          it 'should orphan the resource from the collection' do
            collection = @nancy.collection

            # resource is related
            @nancy.collection.object_id.should == collection.object_id

            collection.should have(1).entries
            collection.shift.object_id.should == @nancy.object_id
            collection.should be_empty

            # resource is orphaned
            @nancy.collection.object_id.should_not == collection.object_id
          end

          it 'should return a Resource' do
            @collection.shift.key.should == @nancy.key
          end
        end

        [ :slice, :[] ].each do |method|
          describe '#slice' do
            describe 'with an index' do
              it 'should return a Resource' do
                resource = @collection.send(method, 0)
                resource.should be_kind_of(DataMapper::Resource)
                resource.id.should == @nancy.id
              end
            end

            describe 'with a start and length' do
              it 'should return a Collection' do
                sliced = @collection.send(method, 0, 1)
                sliced.should be_kind_of(DataMapper::Collection)
                sliced.object_id.should_not == @collection.object_id
                sliced.length.should == 1
                sliced.map { |r| r.id }.should == [ @nancy.id ]
              end
            end

            describe 'with a Range' do
              it 'should return a Collection' do
                sliced = @collection.send(method, 0..1)
                sliced.should be_kind_of(DataMapper::Collection)
                sliced.object_id.should_not == @collection.object_id
                sliced.length.should == 2
                sliced.map { |r| r.id }.should == [ @nancy.id, @bessie.id ]
              end
            end
          end
        end

        describe '#slice!' do
          describe 'with an index' do
            it 'should return a Resource' do
              resource = @collection.slice!(0)
              resource.should be_kind_of(DataMapper::Resource)
            end
          end

          describe 'with a start and length' do
            it 'should return an Array' do
              sliced = @collection.slice!(0, 1)
              sliced.class.should == Array
              sliced.map { |r| r.id }.should == [ @nancy.id ]
            end
          end

          describe 'with a Range' do
            it 'should return a Collection' do
              sliced = @collection.slice(0..1)
              sliced.should be_kind_of(DataMapper::Collection)
              sliced.object_id.should_not == @collection.object_id
              sliced.length.should == 2
              sliced[0].id.should == @nancy.id
              sliced[1].id.should == @bessie.id
            end
          end
        end

        describe '#sort' do
          it 'should return an Array' do
            sorted = @collection.sort { |a,b| a.age <=> b.age }
            sorted.class.should == Array
          end
        end

        describe '#sort!' do
          it 'should return self' do
            @collection.sort! { |a,b| 0 }.object_id.should == @collection.object_id
          end
        end

        describe '#unshift' do
          it 'should relate each new resource to the collection' do
            # resource is orphaned
            @nancy.collection.object_id.should_not == @collection.object_id

            @collection.unshift(@nancy)

            # resource is related
            @nancy.collection.object_id.should == @collection.object_id
          end

          it 'should return self' do
            @collection.unshift(@steve).object_id.should == @collection.object_id
          end
        end

        describe '#update!' do
          it 'should update the resources in the collection' do
            pending do
              # this will not pass with new update!
              # update! should never loop through and set attributes
              # even if it is loaded, and it will not reload the
              # changed objects (even with reload=true, as objects
              # are created is not in any identity map)
              names = [ @nancy.name, @bessie.name, @steve.name ]
              @collection.map { |r| r.name }.should == names
              @collection.update!(:name => 'John')
              @collection.map { |r| r.name }.should_not == names
              @collection.map { |r| r.name }.should == %w[ John ] * 3
            end
          end

          it 'should not update loaded resources unless forced' do
            repository(ADAPTER) do
              nancy = Zebra.first
              nancy.name.should == "Nancy"

              collection = Zebra.all(:name => ["Nancy","Bessie"])
              collection.update!(:name => "Stevie")

              nancy.name.should == "Nancy"
            end
          end

          it 'should update loaded resources if forced' do
            repository(ADAPTER) do
              nancy = Zebra.first
              nancy.name.should == "Nancy"

              collection = Zebra.all(:name => ["Nancy","Bessie"])
              collection.update!({:name => "Stevie"},true)

              nancy.name.should == "Stevie"
            end
          end

          it 'should update collection-query when updating' do
            repository(ADAPTER) do
              collection = Zebra.all(:name => ["Nancy","Bessie"])
              collection.query.conditions.first[2].should == ["Nancy","Bessie"]
              collection.length.should == 2
              collection.update!(:name => "Stevie")
              collection.length.should == 2
              collection.query.conditions.first[2].should == "Stevie"
            end
          end
        end

        describe '#keys' do
          it 'should return a hash of keys' do
            keys = @collection.send(:keys)
            keys.length.should == 1
            keys.each{|property,values| values.should == [1,2,3]}
          end

          it 'should return an empty hash if collection is empty' do
            keys = Zebra.all(:id.gt => 10000).send(:keys)
            keys.should == {}
          end
        end

        describe '#values_at' do
          it 'should return an Array' do
            values = @collection.values_at(0)
            values.class.should == Array
          end

          it 'should return an Array of the resources at the index' do
            @collection.values_at(0).entries.map { |r| r.id }.should == [ @nancy.id ]
          end
        end

        describe 'with lazy loading' do
          it "should take a materialization block" do
            collection = DataMapper::Collection.new(@query) do |c|
              c.should == []
              c.load([ 1, 'Bob',   10 ])
              c.load([ 2, 'Nancy', 11 ])
            end

            collection.should_not be_loaded
            collection.length.should == 2
            collection.should be_loaded
          end

          it "should load lazy columns when using offset" do
            repository(ADAPTER) do
              zebras = Zebra.all(:offset => 1, :limit => 2)
              zebras.first.notes.should_not be_nil
            end
          end
        end
      end
    end
  end
end
