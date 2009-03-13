require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe DataMapper::Property do
  before :each do
    Object.send(:remove_const, :Zoo) if defined?(Zoo)
    class ::Zoo
      include DataMapper::Resource

      property :id, DataMapper::Types::Serial
    end

    Object.send(:remove_const, :Name) if defined?(Name)
    class ::Name < DataMapper::Type
      primitive String
      track :hash

      def self.load(value, property)
        value.split(", ").reverse
      end

      def self.dump(value, property)
        value && value.reverse.join(", ")
      end

      def self.typecast(value, property)
        value
      end
    end

    Object.send(:remove_const, :Tomato) if defined?(Tomato)
    class ::Tomato
      include DataMapper::Resource
    end
  end

  describe '.new' do
    [ Float, BigDecimal ].each do |primitive|
      describe "with a #{primitive} primitive" do
        it 'should raise an ArgumentError if precision is 0' do
          lambda {
            Zoo.class_eval <<-RUBY
              property :test, #{primitive}, :precision => 0
            RUBY
          }.should raise_error(ArgumentError)
        end

        it "raises an ArgumentError if precision is less than 0" do
          lambda {
            Zoo.class_eval <<-RUBY
              property :test, #{primitive}, :precision => -1
            RUBY
          }.should raise_error(ArgumentError)
        end

        it 'should raise an ArgumentError if scale is less than 0' do
          lambda {
            Zoo.class_eval <<-RUBY
              property :test, #{primitive}, :scale => -1
            RUBY
          }.should raise_error(ArgumentError)
        end

        it 'should raise an ArgumentError if precision is less than scale' do
          lambda {
            Zoo.class_eval <<-RUBY
              property :test, #{primitive}, :precision => 1, :scale => 2
            RUBY
          }.should raise_error(ArgumentError)
        end
      end
    end
  end

  describe '#field' do
    before(:each) do
      Zoo.class_eval do
        property :location, String, :field => "City"

        repository(:mock) do
          property :location, String, :field => "MockCity"
        end
      end
    end

    it 'should accept a custom field' do
      Zoo.properties[:location].field.should == 'City'
    end

    # How is this supposed to work?
    it 'should use repository name if passed in' do
      pending
      Zoo.properties[:location].field(:mock).should == 'MockCity'
    end
  end

  describe '#get' do
    before do
      Zoo.class_eval do
        property :name, String, :default => "San Diego"
        property :address, String
      end
      @resource        = Zoo.new
    end

    describe 'when setting the default on initial access' do
      it 'should set the ivar to the default' do
        @resource.name.should == 'San Diego'
      end

      it 'should set the original value to nil' do
        @resource.original_values[:name].should == nil
      end
    end

    it "should not reload the default if you set the property to nil" do
      @resource.name = nil
      @resource.name.should == nil
    end
  end

  describe '#get, when tracking via :hash' do
    before do
      Zoo.class_eval do
        property :name, String, :lazy => true, :track => :hash
      end
      Zoo.auto_migrate!
      @resource = Zoo.create(:name => "San Diego")
    end

    describe 'when setting the default on initial access' do
      it 'should set the ivar to the default' do
        @resource.name.should == "San Diego"
      end

      it 'should set the original value to nil' do
        @resource.name
        @resource.original_values[:name].should == "San Diego".hash
      end

      it "should know it's dirty if a change was made to the object" do
        @resource.name.upcase!
        @resource.should be_dirty
      end
    end
  end

  describe '#get, when tracking via :get' do
    before do
      Zoo.class_eval do
        property :name, String
      end
      Zoo.auto_migrate!
      @resource = Zoo.create(:name => "San Diego")
    end

    describe 'when setting the default on initial access' do
      it 'should set the ivar to the default' do
        @resource.name.should == "San Diego"
      end

      it 'should set the original value to "San Diego"' do
        @resource.name
        @resource.original_values[:name].should == "San Diego"
      end
    end

    it "should know it's dirty if a change was made to the object" do
      @resource.name.upcase!
      @resource.name
      @resource.should be_dirty
      @resource.original_values[:name].should == "San Diego"
    end
  end

  describe 'with Proc defaults' do
    it "calls the proc" do
      Zoo.class_eval do
        property :name, String, :default => proc {|r,p| "San Diego"}
        property :address, String
      end

      Zoo.new.name.should == "San Diego"
    end

    it "provides the resource to the proc" do
      Zoo.class_eval do
        property :name, String, :default => proc {|r,p| r.address}
        property :address, String
      end

      zoo = Zoo.new
      zoo.address = "San Diego"
      zoo.name.should == "San Diego"
    end

    it "provides the property to the proc" do
      Zoo.class_eval do
        property :name, String, :default => proc {|r,p| p.name.to_s}
      end

      zoo = Zoo.new
      zoo.name.should == "name"
    end
  end


  describe '#get!' do
    it 'should get the resource' do
      Zoo.class_eval do
        property :name, String
      end

      resource = Zoo.new(:name => "Portland Zoo")
      resource.name.should == "Portland Zoo"
    end
  end

  describe '#set' do
    before(:each) do
      Zoo.class_eval do
        property :name, String
        property :age, Integer
        property :description, String, :lazy => true
      end
      Zoo.auto_migrate!
      Zoo.create(:name => "San Diego Zoo", :age => 888,
        :description => "Great Zoo")
      @resource = Zoo.new
    end

    it 'should typecast the value' do
      @resource.age = "888"
      @resource.age.should == 888
    end

    it "should lazy load itself first" do
      resource = Zoo.first
      resource.description = "Still a Great Zoo"
      resource.original_values[:description].should == "Great Zoo"
    end

    it "should only set original_values once" do
      resource = Zoo.first
      resource.description = "Still a Great Zoo"
      resource.description = "What can I say. This is one great Zoo"
      resource.original_values[:description].should == "Great Zoo"
    end
  end

  describe '#set!' do
    before do
      Zoo.class_eval do
        property :name, String
        property :age, Integer
      end
    end

    it 'should set the resource' do
      resource = Zoo.new
      resource.name = "Seattle Zoo"
      resource.name.should == "Seattle Zoo"
    end
  end

  # What is this for?
  # ---
  # it "should evaluate two similar properties as equal" do
  #   p1 = DataMapper::Property.new(Zoo, :name, String, { :size => 30 })
  #   p2 = DataMapper::Property.new(Zoo, :name, String, { :size => 30 })
  #   p3 = DataMapper::Property.new(Zoo, :title, String, { :size => 30 })
  #   p1.eql?(p2).should == true
  #   p1.hash.should == p2.hash
  #   p1.eql?(p3).should == false
  #   p1.hash.should_not == p3.hash
  # end

  it "should create a String property" do
    Zoo.class_eval do
      property :name, String, :size => 30
    end

    resource = Zoo.new
    resource.name = 100
    resource.name.should == "100"
  end

  it "should not have key that is lazy" do
    Zoo.class_eval do
      property :id, String, :lazy => true, :key => true
      property :name, String, :lazy => true
    end
    Zoo.auto_migrate!

    Zoo.create(:id => "100", :name => "San Diego Zoo")
    zoo = Zoo.first

    # Do we mean for attribute_loaded? to be public?
    zoo.attribute_loaded?(:id).should == true
  end

  it "should lazily load other non-loaded, non-lazy fields" do
    # This somewhat contorted setup is to successfully test that
    # the list of eager properties to be loaded when it's initially
    # missing is, in fact, repository-scoped
    Zoo.class_eval do
      property :id, DataMapper::Types::Serial
      property :name, String, :lazy => true
      property :address, String, :lazy => true

      repository(:default2) do
        property :name, String
        property :address, String
      end
    end

    repository(:default2) do
      Zoo.auto_migrate!
      Zoo.create(:name => "San Diego Zoo", :address => "San Diego")
    end
    repository(:default2) do
      zoo = Zoo.first(:fields => [:id])

      zoo.attribute_loaded?(:name).should == false
      zoo.attribute_loaded?(:address).should == false
      zoo.name
      zoo.attribute_loaded?(:name).should == true
      zoo.attribute_loaded?(:address).should == true
    end
  end

  it "should use a custom type Name property" do
    Zoo.class_eval do
      property :name, Name
    end

    Zoo.auto_migrate!

    zoo = Zoo.create(:name => %w(Zoo San\ Diego))
    Zoo.first.name.should == %w(Zoo San\ Diego)
  end

  it "should override type options with property options" do
    Zoo.class_eval do
      property :name, Name, :track => :get
    end

    Zoo.auto_migrate!

    Zoo.create(:name => %w(Awesome Person\ Dude))
    zoo = Zoo.first
    zoo.name = %w(Awesome Person\ Dude)

    # If we were tracking by hash, this would cause zoo to be dirty,
    # as its hash would not match the original. Since we've overridden
    # and are tracking by :get, it won't be dirty
    zoo.name.stub!(:hash).and_return(1)
    zoo.should_not be_dirty
  end

  describe "public details" do
    before do
      Zoo.class_eval do
        property :botanical_name, String, :nullable => true, :lazy => true
        property :colloquial_name, DataMapper::Types::Text, :default => "Tomato"
      end
      Zoo.auto_migrate!
    end

    it "should determine nullness" do
      Zoo.properties[:botanical_name].options[:nullable].should be_true
    end

    it "should determine its name"  do
      Zoo.properties[:botanical_name].name.should == :botanical_name
    end

    # lazy? is not exposed to or used by the adapters, so it should be tested indirectly
    it "should determine laziness" do
      Zoo.create(:botanical_name => "Calystegia sepium")
      Zoo.first.attribute_loaded?(:botanical_name).should be_false
    end

    it "should automatically set laziness to true on text fields" do
      Zoo.create(:colloquial_name => "American hedge bindweed")
      Zoo.first.attribute_loaded?(:colloquial_name).should be_false
    end

    it "should determine whether it is a key" do
      zoo = Zoo.create(:botanical_name => "Calystegia sepium")
      id = zoo.id
      Zoo.first.id.should == id
    end

    it "should determine whether it is serial" do
      zoo = Zoo.create(:botanical_name => "Calystegia sepium")
      zoo.id.should_not be_nil
    end

    it "should determine a default value" do
      zoo = Zoo.new
      zoo.colloquial_name.should == "Tomato"
    end
  end

  describe "reader and writer visibility" do
    # parameter passed to Property.new                    # reader | writer visibility
    {
      {}                                                 => [:public,    :public],
      { :accessor => :public }                           => [:public,    :public],
      { :accessor => :protected }                        => [:protected, :protected],
      { :accessor => :private }                          => [:private,   :private],
      { :reader => :public }                             => [:public,    :public],
      { :reader => :protected }                          => [:protected, :public],
      { :reader => :private }                            => [:private,   :public],
      { :writer => :public }                             => [:public,    :public],
      { :writer => :protected }                          => [:public,    :protected],
      { :writer => :private }                            => [:public,    :private],
      { :reader => :public, :writer => :public }         => [:public,    :public],
      { :reader => :public, :writer => :protected }      => [:public,    :protected],
      { :reader => :public, :writer => :private }        => [:public,    :private],
      { :reader => :protected, :writer => :public }      => [:protected, :public],
      { :reader => :protected, :writer => :protected }   => [:protected, :protected],
      { :reader => :protected, :writer => :private }     => [:protected, :private],
      { :reader => :private, :writer => :public }        => [:private,   :public],
      { :reader => :private, :writer => :protected }     => [:private,   :protected],
      { :reader => :private, :writer => :private }       => [:private,   :private],
    }.each do |input, output|
      it "#{input.inspect} should make reader #{output[0]} and writer #{output[1]}" do
        Tomato.class_eval <<-RUBY
          property :botanical_name, String, #{input.inspect}
        RUBY
        Tomato.send("#{output[0]}_instance_methods").map { |m| m.to_s }.should include("botanical_name")
        Tomato.send("#{output[1]}_instance_methods").map { |m| m.to_s }.should include("botanical_name=")
      end
    end

    [
      { :accessor => :junk },
      { :reader   => :junk },
      {                          :writer => :junk },
      { :reader   => :public,    :writer => :junk },
      { :reader   => :protected, :writer => :junk },
      { :reader   => :private,   :writer => :junk },
      { :reader   => :junk,      :writer => :public },
      { :reader   => :junk,      :writer => :protected },
      { :reader   => :junk,      :writer => :private },
      { :reader   => :junk,      :writer => :junk },
      { :reader   => :junk,      :writer => :junk },
      { :reader   => :junk,      :writer => :junk },
    ].each do |input|
      it "#{input.inspect} should raise ArgumentError" do
        lambda {
          Tomato.class_eval <<-RUBY
            property :family, String, #{input.inspect}
          RUBY
        }.should raise_error(ArgumentError)
      end
    end
  end

  # This is handled by get!
  # ---
  # it "should return an instance variable name" do
  #  DataMapper::Property.new(Tomato, :flavor, String, {}).instance_variable_name.should == '@flavor'
  #  DataMapper::Property.new(Tomato, :ripe, TrueClass, {}).instance_variable_name.should == '@ripe' #not @ripe?
  # end

  it "should append ? to TrueClass property reader methods" do
    class ::Potato
      include DataMapper::Resource
      property :id, Integer, :key => true
      property :fresh, TrueClass
      property :public, TrueClass
    end

    Potato.new(:fresh => true).should be_fresh
  end

  it "should move unknown options into Property#extra_options" do
    Tomato.class_eval do
      property :botanical_name, String, :foo => :bar
    end
    Tomato.properties[:botanical_name].extra_options.should == {:foo => :bar}
  end

  it 'should provide #custom?' do
    Zoo.class_eval do
      property :name, Name, :size => 50
      property :state, String, :size => 2
    end
    Zoo.properties[:name].should be_custom
    Zoo.properties[:state].should_not be_custom
  end

  it "should set the field to the correct field_naming_convention" do
    Zoo.class_eval { property :species, String }
    Tomato.class_eval { property :genetic_history, DataMapper::Types::Text }

    Zoo.properties[:species].field.should == "species"
    Tomato.properties[:genetic_history].field.should == "genetic_history"
  end

  it "should provide the primitive mapping" do
    Zoo.class_eval do
      property :poverty, String
      property :fortune, DataMapper::Types::Text
    end

    Zoo.properties[:poverty].primitive.should == String
    Zoo.properties[:fortune].primitive.should == String
  end

  it "should make it possible to define an integer size" do
    Zoo.class_eval { property :cleanliness, String, :size => 100 }
    Zoo.properties[:cleanliness].size.should == 100
  end

  it "should make it possible to define an integer length (which defines size)" do
    Zoo.class_eval { property :cleanliness, String, :length => 100 }
    Zoo.properties[:cleanliness].size.should == 100
  end

  it "should make it possible to define a range size" do
    Zoo.class_eval { property :cleanliness, String, :size => 0..100 }
    Zoo.properties[:cleanliness].size.should == 100
  end

  it "should make it possible to define a range length (which defines size)" do
    Zoo.class_eval { property :cleanliness, String, :length => 0..100 }
    Zoo.properties[:cleanliness].size.should == 100
  end

  describe '#typecast' do
    def self.format(value)
      case value
        when BigDecimal             then "BigDecimal(#{value.to_s('F').inspect})"
        when Float, Integer, String then "#{value.class}(#{value.inspect})"
        else value.inspect
      end
    end

    it 'should pass through the value if it is the same type when typecasting' do
      Zoo.class_eval do
        property :name, String
      end
      zoo = Zoo.new
      value = "San Diego"
      def value.to_s() "San Francisco" end
      zoo.name = value
      zoo.name.should == "San Diego"
    end

    it 'should pass through the value nil when typecasting' do
      Zoo.class_eval do
        property :name, String
      end

      zoo = Zoo.new
      zoo.name = nil
      zoo.name.should == nil
    end

    it 'should pass through the value for an Object property' do
      value = Object.new
      Zoo.class_eval do
        property :object, Object
      end

      zoo = Zoo.new
      zoo.object = value
      zoo.object.object_id.should == value.object_id
    end

    [ true, 'true', 'TRUE', 1, '1', 't', 'T' ].each do |value|
      it "should typecast #{value.inspect} to true for a TrueClass property" do
        Zoo.class_eval do
          property :boolean, TrueClass
        end

        zoo = Zoo.new
        zoo.boolean = value
        zoo.boolean.should == true
      end
    end

    [ false, 'false', 'FALSE', 0, '0', 'f', 'F' ].each do |value|
      it "should typecast #{value.inspect} to false for a Boolean property" do
        Zoo.class_eval do
          property :boolean, TrueClass
        end

        zoo = Zoo.new
        zoo.boolean = value
        zoo.boolean.should == false
      end
    end

    it 'should typecast nil to nil for a Boolean property' do
      Zoo.class_eval do
        property :boolean, TrueClass
      end

      zoo = Zoo.new
      zoo.boolean = nil
      zoo.boolean.should == nil
    end

    it 'should typecast "0" to "0" for a String property' do
      Zoo.class_eval do
        property :string, String
      end

      zoo = Zoo.new
      zoo.string = "0"
      zoo.string.should == "0"
    end

    { '0' => 0.0, '0.0' => 0.0, 0 => 0.0, 0.0 => 0.0, BigDecimal('0.0') => 0.0 }.each do |value,expected|
      it "should typecast #{format(value)} to #{format(expected)} for a Float property" do
        Zoo.class_eval do
          property :float, Float
        end

        zoo = Zoo.new
        zoo.float = value
        zoo.float.should == expected
      end
    end

    { '-8' => -8, '-8.0' => -8, -8 => -8, -8.0 => -8, BigDecimal('8.0') => 8,
      '0' => 0, '0.0' => 0, 0 => 0, 0.0 => 0, BigDecimal('0.0') => 0,
      '5' => 5, '5.0' => 5, 5 => 5, 5.0 => 5, BigDecimal('5.0') => 5,
      'none' => nil, 'almost 5' => nil, '-3 change' => -3, '9 items' => 9}.each do |value,expected|
      it "should typecast #{format(value)} to #{format(expected)} for an Integer property" do
        Zoo.class_eval do
          property :int, Integer
        end

        zoo = Zoo.new
        zoo.int = value
        zoo.int.should == expected
      end
    end

    { '0' => BigDecimal('0'), '0.0' => BigDecimal('0.0'), 0.0 => BigDecimal('0.0'), BigDecimal('0.0') => BigDecimal('0.0') }.each do |value,expected|
      it "should typecast #{format(value)} to #{format(expected)} for a BigDecimal property" do
        Zoo.class_eval do
          property :big_decimal, BigDecimal
        end

        zoo = Zoo.new
        zoo.big_decimal = value
        zoo.big_decimal.should == expected
      end
    end

    it 'should typecast value for a DateTime property' do
      Zoo.class_eval { property :date_time, DateTime }
      zoo = Zoo.new
      zoo.date_time = '2000-01-01 00:00:00'
      zoo.date_time.should == DateTime.new(2000, 1, 1, 0, 0, 0)
    end

    it 'should typecast value for a Date property' do
      Zoo.class_eval { property :date, Date }
      zoo = Zoo.new
      zoo.date = '2000-01-01'
      zoo.date.should == Date.new(2000, 1, 1)
    end

    it 'should typecast value for a Time property' do
      Zoo.class_eval { property :time, Time }
      zoo = Zoo.new
      zoo.time = '2000-01-01 01:01:01.123456'
      zoo.time.should == Time.local(2000, 1, 1, 1, 1, 1, 123456)
    end

    it 'should typecast Hash for a Time property' do
      Zoo.class_eval { property :time, Time }
      zoo = Zoo.new
      zoo.time = {:year => 2002, "month" => 1, :day => 1, "hour" => 12, :min => 0, :sec => 0}
      zoo.time.should == Time.local(2002, 1, 1, 12, 0, 0)
    end

    it 'should typecast Hash for a Date property' do
      Zoo.class_eval { property :date, Date }
      zoo = Zoo.new
      zoo.date = {:year => 2002, "month" => 1, :day => 1}
      zoo.date.should == Date.new(2002, 1, 1)
    end

    it 'should typecast Hash for a DateTime property' do
      Zoo.class_eval { property :date_time, DateTime }
      zoo = Zoo.new
      zoo.date_time = {:year => 2002, :month => 1, :day => 1, "hour" => 12, :min => 0, "sec" => 0}
      zoo.date_time.should == DateTime.new(2002, 1, 1, 12, 0, 0)
    end

    it 'should use now as defaults for missing parts of a Hash to Time typecast' do
      now = Time.now
      Zoo.class_eval { property :time, Time }
      zoo = Zoo.new
      zoo.time = {:month => 1, :day => 1}
      zoo.time.should == Time.local(now.year, 1, 1, now.hour, now.min, now.sec)
    end

    it 'should use now as defaults for missing parts of a Hash to Date typecast' do
      now = Time.now
      Zoo.class_eval { property :date, Date }
      zoo = Zoo.new
      zoo.date = {:month => 1, :day => 1}
      zoo.date.should == Date.new(now.year, 1, 1)
    end

    it 'should use now as defaults for missing parts of a Hash to DateTime typecast' do
      now = Time.now
      Zoo.class_eval { property :date_time, DateTime }
      zoo = Zoo.new
      zoo.date_time = {:month => 1, :day => 1}
      zoo.date_time.should == DateTime.new(now.year, 1, 1, now.hour, now.min, now.sec)
    end

    it 'should rescue after trying to typecast an invalid Date value from a hash' do
      now = Time.now
      Zoo.class_eval { property :date, Date }
      zoo = Zoo.new
      zoo.date = {:year => 2002, :month => 2, :day => 31}
      zoo.date.should == Date.new(2002, 3, 3)
    end

    it 'should rescue after trying to typecast an invalid DateTime value from a hash' do
      now = Time.now
      Zoo.class_eval { property :date_time, DateTime }
      zoo = Zoo.new
      zoo.date_time = {
        :year => 2002, :month => 2, :day => 31, :hour => 12, :min => 0, :sec => 0
      }
      zoo.date_time.should == DateTime.new(2002, 3, 3, 12, 0, 0)
    end

    it 'should typecast value for a Class property' do
      Zoo.class_eval { property :klass, Class }
      zoo = Zoo.new
      zoo.klass = "Zoo"
      zoo.klass.should == Zoo
    end
  end

  it 'should return an abbreviated representation of the property when inspected' do
    Zoo.class_eval { property :name, String }
    Zoo.properties[:name].inspect.should == '#<Property:Zoo:name>'
  end
end
