require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe DataMapper::Resource do

  load_models_for_metaphor :zoo

  describe '#attributes' do
    it 'should return a hash of attribute-names and values' do
      zoo = Zoo.new
      zoo.name = "San Francisco"
      zoo.description = "This is a pretty awesome zoo"
      zoo.attributes.should == {
        :name => "San Francisco", :description => "This is a pretty awesome zoo",
        :id => nil, :inception => nil, :open => false, :size => nil, :mission => nil
      }
    end

    it "should return a hash with all nil values if the instance is new and has no default values" do
      Species.new.attributes.should == { :id => nil, :name => nil }
    end

    it 'should not include private attributes' do
      Species.new.attributes.should == { :id => nil, :name => nil }
    end
  end

  describe "#attributes=" do
    before(:each) do
      @zoo = Zoo.new(:name => "San Francisco", :size => 10)
      @zoo.attributes = {:size => 12 }
    end
    it "should change a public property" do
      @zoo.size.should == 12
    end
    it "should raise when attempting to change a property with a non-public writer" do
      lambda { @zoo.attributes = {:mission => "Just keep some odd
        critters, y'know?" } }.should raise_error
    end
  end

# ---------- REPOSITORY WRITE METHODS ---------------

  describe '#save' do

    describe 'with a new resource' do
      it 'should set defaults before create'
      it 'should create when dirty'
      it 'should create when non-dirty, and it has a serial key'
    end

    describe 'with an existing resource' do
      it 'should update'
    end

  end
end

# ---------- Old specs... BOOOOOOOOOO ---------------

describe DataMapper::Resource do
  before(:each) do
    Object.send(:remove_const, :Planet) if defined?(Planet)
    class ::Planet
      include DataMapper::Resource

      storage_names[:legacy] = "dying_planets"

      property :id, Integer, :key => true
      property :name, String
      property :age, Integer
      property :core, String, :accessor => :private
      property :type, Discriminator
      property :data, Object, :track => :get

      repository(:legacy) do
        property :cowabunga, String
      end

      def age
        attribute_get(:age)
      end

      def to_s
        name
      end
    end

    Object.send(:remove_const, :Phone) if defined?(Phone)
    class ::Phone
      include DataMapper::Resource

      property :name, String, :key => true
      property :awesomeness, Integer
    end

    Object.send(:remove_const, :Fruit) if defined?(Fruit)
    class ::Fruit
      include DataMapper::Resource

      property :id, Integer, :key => true
      property :name, String
    end

    Object.send(:remove_const, :Grain) if defined?(Grain)
    class ::Grain
      include DataMapper::Resource

      property :id, Serial
      property :name, String, :default => 'wheat'
    end

    Object.send(:remove_const, :Vegetable) if defined?(Vegetable)
    class ::Vegetable
      include DataMapper::Resource

      property :id, Serial
      property :name, String
    end

    Object.send(:remove_const, :Banana) if defined?(Banana)
    class ::Banana < Fruit
      property :type, Discriminator
    end

    Object.send(:remove_const, :Cyclist) if defined?(Cyclist)
    class ::Cyclist
      include DataMapper::Resource
      property :id,         Serial
      property :victories,  Integer
    end

    Fruit.auto_migrate!
    Planet.auto_migrate!
    Cyclist.auto_migrate!
  end

  it 'should provide #save' do
    Planet.new.should respond_to(:save)
  end

  describe '#save' do
    before(:each) do
      @adapter = repository(:default).adapter
    end

    describe 'with a new resource' do
      it 'should set defaults before create' do
        resource = Grain.new

        resource.should_not be_dirty
        resource.should be_new_record
        resource.instance_variable_get('@name').should be_nil

        @adapter.should_receive(:create).with([ resource ]).and_return(1)

        resource.save.should be_true

        resource.instance_variable_get('@name').should == 'wheat'
      end

      it 'should create when dirty' do
        resource = Vegetable.new(:id => 1, :name => 'Potato')

        resource.should be_dirty
        resource.should be_new_record

        @adapter.should_receive(:create).with([ resource ]).and_return(1)

        resource.save.should be_true
      end

      it 'should create when non-dirty, and it has a serial key' do
        resource = Vegetable.new

        resource.should_not be_dirty
        resource.should be_new_record
        resource.model.key.any? { |p| p.serial? }.should be_true

        @adapter.should_receive(:create).with([ resource ]).and_return(1)

        resource.save.should be_true
      end

      it 'should not create when non-dirty, and is has a non-serial key' do
        resource = Fruit.new

        resource.should_not be_dirty
        resource.should be_new_record
        resource.model.key.any? { |p| p.serial? }.should be_false

        resource.save.should be_false
      end

      it 'should return true even if the object is not dirty' do
        resource = Cyclist.new
        resource.victories = "0 victories"
        resource.save.should be_true

        resource.should_not be_dirty
        resource.should_not be_new_record
        resource.save.should be_true
      end

      describe 'for integer fields' do

        it "should save strings without digits as nil" do
          resource = Cyclist.new
          resource.victories = "none"
          resource.save.should be_true
          resource.victories.should be_nil
        end

        it "should save strings beginning with non-digits as nil" do
          resource = Cyclist.new
          resource.victories = "almost 5"
          resource.save.should be_true
          resource.victories.should be_nil
        end

        it 'should save strings beginning with negative numbers as that number' do
          resource = Cyclist.new
          resource.victories = "-4 victories"
          resource.save.should be_true
          resource.victories.should == -4
        end

        it 'should save strings beginning with 0 as 0' do
          resource = Cyclist.new
          resource.victories = "0 victories"
          resource.save.should be_true
          resource.victories.should == 0
        end

        it 'should save strings beginning with positive numbers as that number' do
          resource = Cyclist.new
          resource.victories = "23 victories"
          resource.save.should be_true
          resource.victories.should == 23
        end

      end

    end

    describe 'with an existing resource' do
      it 'should update' do
        resource = Vegetable.new(:name => 'Potato')
        resource.instance_variable_set('@new_record', false)

        resource.should be_dirty
        resource.should_not be_new_record

        @adapter.should_receive(:update).with(resource.dirty_attributes, resource.to_query).and_return(1)

        resource.save.should be_true
      end
    end
  end

  it "should be able to overwrite to_s" do
    Planet.new(:name => 'Mercury').to_s.should == 'Mercury'
  end

  describe "storage names" do
    it "should use its class name by default" do
      Planet.storage_name.should == "planets"
    end

    it "should allow changing using #default_storage_name" do
      Planet.class_eval <<-EOF.margin
        @storage_names.clear
        def self.default_storage_name
          "Superplanet"
        end
      EOF

      Planet.storage_name.should == "superplanets"
      Planet.class_eval <<-EOF.margin
        @storage_names.clear
        def self.default_storage_name
          self.name
        end
      EOF
    end
  end

  it "should require a key" do
    lambda do
      DataMapper::Model.new("stuff") do
        property :name, String
      end.new
    end.should raise_error(DataMapper::IncompleteResourceError)
  end

  it "should hold repository-specific properties" do
    Planet.properties(:legacy).should have_property(:cowabunga)
    Planet.properties.should_not have_property(:cowabunga)
  end

  it "should track the classes that include it" do
    DataMapper::Resource.descendants.clear
    klass = Class.new { include DataMapper::Resource }
    DataMapper::Resource.descendants.should == Set.new([klass])
  end

  it "should return an instance of the created object" do
    Planet.create(:name => 'Venus', :age => 1_000_000, :id => 42).should be_a_kind_of(Planet)
  end

  it 'should provide persistance methods' do
    planet = Planet.new
    planet.should respond_to(:new_record?)
    planet.should respond_to(:save)
    planet.should respond_to(:destroy)
  end

  it "should have attributes" do
    attributes = { :name => 'Jupiter', :age => 1_000_000, :id => 42, :type => Planet, :data => nil }
    jupiter = Planet.new(attributes)
    jupiter.attributes.should == attributes
  end

  it "should be able to set attributes" do
    attributes = { :name => 'Jupiter', :age => 1_000_000, :id => 42, :type => Planet, :data => nil }
    jupiter = Planet.new(attributes)
    jupiter.attributes.should == attributes

    new_attributes = attributes.merge( :age => 2_500_000 )
    jupiter.attributes = new_attributes
    jupiter.attributes.should == new_attributes
  end

  it "should be able to set attributes using update_attributes" do
    attributes = { :name => 'Jupiter', :age => 1_000_000, :id => 42, :type => Planet, :data => nil }
    jupiter = Planet.new(attributes)
    jupiter.attributes.should == attributes

    new_age = { :age => 3_700_000 }
    jupiter.update_attributes(new_age).should be_true
    jupiter.age.should == 3_700_000
    jupiter.attributes.should == attributes.merge(new_age)
  end

  # Illustrates a possible controller situation, where an expected params
  # key does not exist.
  it "update_attributes(nil) should raise an exception" do
    hincapie = Cyclist.new
    params = {}
    lambda {
      hincapie.update_attributes(params[:does_not_exist])
    }.should raise_error(ArgumentError)
  end

  it "update_attributes(:not_a_hash) should raise an exception" do
    hincapie = Cyclist.new
    lambda {
      hincapie.update_attributes(:not_a_hash).should be_false
    }.should raise_error(ArgumentError)
  end

  # :core is a private accessor so Ruby should raise NameError
  it "should not be able to set private attributes" do
    lambda {
      jupiter = Planet.new({ :core => "Molten Metal" })
    }.should raise_error(ArgumentError)
  end

  it "should not mark attributes dirty if they are similar after update" do
    jupiter = Planet.new(:name => 'Jupiter', :age => 1_000_000, :id => 42, :data => { :a => "Yeah!" })
    jupiter.save.should be_true

    # discriminator will be set automatically
    jupiter.type.should == Planet

    jupiter.attributes = { :name => 'Jupiter', :age => 1_000_000, :data => { :a => "Yeah!" } }

    jupiter.attribute_dirty?(:name).should be_false
    jupiter.attribute_dirty?(:age).should be_false
    jupiter.attribute_dirty?(:core).should be_false
    jupiter.attribute_dirty?(:data).should be_false

    jupiter.dirty?.should be_false
  end

  it "should not mark attributes dirty if they are similar after typecasting" do
    jupiter = Planet.new(:name => 'Jupiter', :age => 1_000_000, :id => 42, :type => Planet)
    jupiter.save.should be_true
    jupiter.dirty?.should be_false

    jupiter.age = '1_000_000'
    jupiter.attribute_dirty?(:age).should be_false
    jupiter.dirty?.should be_false
  end

  it "should track attributes" do

    # So attribute tracking is a feature of the Resource,
    # not the Property. Properties are class-level declarations.
    # Instance-level operations like this happen in Resource with methods
    # and ivars it sets up. Like a @dirty_attributes Array for example to
    # track dirty attributes.

    mars = Planet.new :name => 'Mars'
    # #attribute_loaded? and #attribute_dirty? are a bit verbose,
    # but I like the consistency and grouping of the methods.

    # initialize-set values are dirty as well. DM sets ivars
    # directly when materializing, so an ivar won't exist
    # if the value wasn't loaded by DM initially. Touching that
    # ivar at all will declare it, so at that point it's loaded.
    # This means #attribute_loaded?'s implementation could be very
    # similar (if not identical) to:
    #   def attribute_loaded?(name)
    #     instance_variable_defined?("@#{name}")
    #   end
    mars.attribute_loaded?(:name).should be_true
    mars.attribute_dirty?(:id).should be_false
    mars.attribute_dirty?(:name).should be_true
    mars.attribute_loaded?(:age).should be_false
    mars.attribute_dirty?(:data).should be_false

    mars.age.should be_nil

    # So accessing a value should ensure it's loaded.
    # XXX: why?  if the @ivar isn't set, which it wouldn't be in this
    # case because mars is a new_record?, then perhaps it should return
    # false
    #    mars.attribute_loaded?(:age).should be_true

    # A value should be able to be both loaded and nil.
    mars.age.should be_nil

    # Unless you call #[]= it's not dirty.
    mars.attribute_dirty?(:age).should be_false

    mars.age = 30
    mars.data = { :a => "Yeah!" }

    # Obviously. :-)
    mars.attribute_dirty?(:age).should be_true
    mars.attribute_dirty?(:data).should be_true
  end

  it "should mark the key as dirty, if it is a natural key and has been set" do
    phone = Phone.new
    phone.name = 'iPhone'
    phone.attribute_dirty?(:name).should be_true
  end

  it 'should return the dirty attributes' do
    pluto = Planet.new(:name => 'Pluto', :age => 500_000)
    pluto.attribute_dirty?(:name).should be_true
    pluto.attribute_dirty?(:age).should be_true
  end

  it 'should overwite old dirty attributes with new ones' do
    pluto = Planet.new(:name => 'Pluto', :age => 500_000)
    pluto.dirty_attributes.size.should == 2
    pluto.attribute_dirty?(:name).should be_true
    pluto.attribute_dirty?(:age).should be_true
    pluto.name = "pluto"
    pluto.dirty_attributes.size.should == 2
    pluto.attribute_dirty?(:name).should be_true
    pluto.attribute_dirty?(:age).should be_true
  end

  it 'should provide a key' do
    Planet.new.should respond_to(:key)
  end

  it 'should store and retrieve default values' do
    Planet.property(:satellite_count, Integer, :default => 0)
    # stupid example but it's reliable and works
    Planet.property(:orbit_period, Float, :default => lambda { |r,p| p.name.to_s.length })
    earth = Planet.new(:name => 'Earth')
    earth.satellite_count.should == 0
    earth.orbit_period.should == 12
    earth.satellite_count = 2
    earth.satellite_count.should == 2
    earth.orbit_period = 365.26
    earth.orbit_period.should == 365.26
  end

  describe "#reload_attributes" do
    it 'should call collection.reload if not a new record' do
      planet = Planet.new(:name => 'Omicron Persei VIII')
      planet.stub!(:new_record?).and_return(false)

      collection = mock('collection')
      collection.should_receive(:reload).with(:fields => [:name]).once

      planet.stub!(:collection).and_return(collection)
      planet.reload_attributes(:name)
    end

    it 'should not call collection.reload if no attributes are provided to reload' do
      planet = Planet.new(:name => 'Omicron Persei VIII')
      planet.stub!(:new_record?).and_return(false)

      collection = mock('collection')
      collection.should_not_receive(:reload)

      planet.stub!(:collection).and_return(collection)
      planet.reload_attributes
    end

    it 'should not call collection.reload if the record is new' do
      lambda {
        Planet.new(:name => 'Omicron Persei VIII').reload_attributes(:name)
      }.should_not raise_error

      planet = Planet.new(:name => 'Omicron Persei VIII')
      planet.should_not_receive(:collection)
      planet.reload_attributes(:name)
    end
  end

  describe '#reload' do
    it 'should call #reload_attributes with the currently loaded attributes' do
      planet = Planet.new(:name => 'Omicron Persei VIII', :age => 1)
      planet.stub!(:new_record?).and_return(false)

      planet.should_receive(:reload_attributes).with(:name, :age).once

      planet.reload
    end

    it 'should call #reload on the parent and child associations' do
      planet = Planet.new(:name => 'Omicron Persei VIII', :age => 1)
      planet.stub!(:new_record?).and_return(false)

      child_association = mock('child assoc')
      child_association.should_receive(:reload).once.and_return(true)

      parent_association = mock('parent assoc')
      parent_association.should_receive(:reload).once.and_return(true)

      planet.stub!(:child_associations).and_return([child_association])
      planet.stub!(:parent_associations).and_return([parent_association])
      planet.stub!(:reload_attributes).and_return(planet)

      planet.reload
    end

    it 'should not do anything if the record is new' do
      planet = Planet.new(:name => 'Omicron Persei VIII', :age => 1)
      planet.should_not_receive(:reload_attributes)
      planet.reload
    end
  end

  describe 'when retrieving by key' do
    it 'should return the corresponding object' do
      m = mock("planet")
      Planet.should_receive(:get).with(1).and_return(m)

      Planet.get!(1).should == m
    end

    it 'should raise an error if not found' do
      Planet.should_receive(:get).and_return(nil)

      lambda do
        Planet.get!(1)
      end.should raise_error(DataMapper::ObjectNotFoundError)
    end
  end

  describe "inheritance" do
    before(:all) do
      class ::Media
        include DataMapper::Resource

        storage_names[:default] = 'media'
        storage_names[:west_coast] = 'm3d1a'

        property :name, String, :key => true
      end

      class ::NewsPaper < Media

        storage_names[:east_coast] = 'mother'

        property :rating, Integer
      end
    end

    it 'should inherit storage_names' do
      NewsPaper.storage_name(:default).should == 'media'
      NewsPaper.storage_name(:west_coast).should == 'm3d1a'
      NewsPaper.storage_name(:east_coast).should == 'mother'
      Media.storage_name(:east_coast).should == 'medium'
    end

    it 'should inherit properties' do
      Media.properties.should have(1).entries
      NewsPaper.properties.should have(2).entries
    end
  end

  describe "Single-table Inheritance" do
    before(:all) do
      class ::Plant
        include DataMapper::Resource

        property :id, Integer, :key => true
        property :length, Integer

        def calculate(int)
          int ** 2
        end

        def length=(len)
          attribute_set(:length, calculate(len))
        end
      end

      class ::HousePlant < Plant
        def calculate(int)
          int ** 3
        end
      end

      class ::PoisonIvy < Plant
        def length=(len)
          attribute_set(:length, len - 1)
        end
      end
    end

    it "should be able to overwrite getters" do
      @p = Plant.new
      @p.length = 3
      @p.length.should == 9
    end

    it "should pick overwritten methods" do
      @hp = HousePlant.new
      @hp.length = 3
      @hp.length.should == 27
    end

    it "should pick overwritten setters" do
      @pi = PoisonIvy.new
      @pi.length = 3
      @pi.length.should == 2
    end
  end
end
