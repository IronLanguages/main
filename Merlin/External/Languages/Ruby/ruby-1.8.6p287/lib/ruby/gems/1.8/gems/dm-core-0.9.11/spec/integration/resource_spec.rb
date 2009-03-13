require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

# ------------------------------------------------------------
# -----   Read SPECS for information about how to read   -----
# -----   and contribute to the DataMapper specs.        -----
# ------------------------------------------------------------

if ADAPTER
  describe "DataMapper::Resource with #{ADAPTER}" do

    load_models_for_metaphor :zoo
    load_models_for_metaphor :content

    before(:each) do
      DataMapper.auto_migrate!(ADAPTER)
      @zoo = Zoo.new(:name => "San Francisco")
      repository(ADAPTER) { @zoo.save }
    end

    it 'should be serializable with Marshal' do
      Marshal.load(Marshal.dump(@zoo)).should == @zoo
    end

    # --- Move somewhere ----
    it "should be able to destroy objects" do
      lambda { @zoo.destroy.should be_true }.should_not raise_error
    end

    it 'should not overwrite attributes when lazy loading' do
      zoo = Zoo.first
      zoo.name = 'San Diego'
      lambda { zoo.description }.should_not change(zoo, :name)
    end

    describe '#attribute_get' do
      it 'should provide #attribute_get' do
        Zoo.new.should respond_to(:attribute_get)
      end

      it 'should delegate to Property#get' do
        Zoo.properties[:name].should_receive(:get).with(zoo = Zoo.new)
        zoo.name
      end

      it "should return Property#get's return value"  do
        Zoo.properties[:name].should_receive(:get).and_return("San Francisco")
        Zoo.new.name.should == "San Francisco"
      end
    end

    describe '#attribute_set' do
      it "should provide #attribute_set" do
        Zoo.new.should respond_to(:attribute_set)
      end

      it 'should delegate to Property#set' do
        Zoo.properties[:name].should_receive(:set).with(zoo = Zoo.new, "San Francisco")
        zoo.name = "San Francisco"
      end
    end

    describe '#eql?' do

      it "should return true if the objects are the same instances" do
        z = Zoo.first
        z2 = z
        z.should be_eql(z2)
      end

      it "should return false if the other object is not an instance of the same model" do
        z = Zoo.first
        z2 = Zoo.create(:name => 'New York')
        z.should_not be_eql(z2)
      end

      it "should return false if the other object is a different class" do
        z = Zoo.first
        o = Content::Dialect.first
        z.should_not be_eql(o)
      end

      it "should return true if the repositories are the same and the primary key is the same"
      it "should return false if the repository is not the same and the primary key is the same"

      it "should return true if all the properties are the same" do
        z = Zoo.first
        z2 = Zoo.new(z.attributes.delete_if{|key, value| key == :mission})
        z.should be_eql(z2)
      end

      it "should return false if any of the properties are different" do
        z = Zoo.first
        z2 = Zoo.new(z.attributes.delete_if{|key, value| key == :mission}.merge(:description => 'impressive'))
        z.should_not be_eql(z2)
      end
    end

    describe '#hash' do
      it "should return the same hash values for unsaved objects that are equal" do
        e1 = Employee.new(:name => "John")
        e2 = Employee.new(:name => "John")
        e1.hash.should == e2.hash
      end

      it "should return the same hash values for saved objects that are equal" do
        # Make sure that the object_id's are not the same
        e1 = e2 = nil
        repository(ADAPTER) do
          e1 = Employee.create(:name => "John")
        end
        repository(ADAPTER) do
          e2 = Employee.get("John")
        end
        e1.hash.should == e2.hash
      end

      it "should return a different hash value for different objects of the same type" do
        repository(ADAPTER) do
          e1 = Employee.create(:name => "John")
          e2 = Employee.create(:name => "Dan")
          e1.hash.should_not == e2.hash
        end
      end

      it "should return a different hash value for different types of objects with the same key"
    end

    describe '#id' do
      it "should be awesome"
    end

    describe '#inspect' do
      it "should return a string representing the object"
    end

    describe '#key' do
      describe "original_value[:key]" do
        it "should be used when an existing resource's key changes" do
          repository(ADAPTER) do
            employee = Employee.create(:name => "John")
            employee.name = "Jon"
            employee.key.should == ["John"]
          end
        end

        it "should be used when saving an existing resource" do
          repository(ADAPTER) do
            employee = Employee.create(:name => "John")
            employee.name = "Jon"
            employee.save.should == true
            Employee.get("Jon").should == employee
          end
        end

        it "should not be used when a new resource's key changes" do
          employee = Employee.new(:name => "John")
          employee.name = "Jon"
          employee.key.should == ["Jon"]
        end
      end
    end

    describe '#pretty_print' do
      it "should display a pretty version of inspect"
    end

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

    describe '#repository' do
      it "should return the repository associated with the object if there is one"
      it "should return the repository associated with the model if the object doesn't have one"
    end
  end
end




# ---------- Old specs... BOOOOOOOOOO ---------------
if ADAPTER
  describe "DataMapper::Resource with #{ADAPTER}" do
    before :all do
      class ::Orange
        include DataMapper::Resource

        def self.default_repository_name
          ADAPTER
        end

        property :name, String, :key => true
        property :color, String
      end

      class ::Apple
        include DataMapper::Resource

        def self.default_repository_name
          ADAPTER
        end

        property :id, Serial
        property :color, String, :default => 'green', :nullable => true
      end

      class ::FortunePig
        include DataMapper::Resource

        def self.default_repository_name
          ADAPTER
        end

        property :id, Serial
        property :name, String

        def to_s
          name
        end

        after :create do
          @created_id = self.id
        end

        after :save do
          @save_id = self.id
        end
      end

      class ::Car
        include DataMapper::Resource

        def self.default_repository_name
          ADAPTER
        end

        property :brand, String, :key => true
        property :color, String
        property :created_on, Date
        property :touched_on, Date
        property :updated_on, Date

        before :save do
          self.touched_on = Date.today
        end

        before :create do
          self.created_on = Date.today
        end

        before :update do
          self.updated_on = Date.today
        end
      end

      class ::Male
        include DataMapper::Resource

        def self.default_repository_name
          ADAPTER
        end

        property :id, Serial
        property :name, String
        property :iq, Integer, :default => 100
        property :type, Discriminator
        property :data, Object

        def iq=(i)
          attribute_set(:iq, i - 1)
        end
      end

      class ::Bully < Male; end

      class ::Mugger < Bully; end

      class ::Maniac < Bully; end

      class ::Psycho < Maniac; end

      class ::Geek < Male
        property :awkward, Boolean, :default => true

        def iq=(i)
          attribute_set(:iq, i + 30)
        end
      end

      class ::Flanimal
        include DataMapper::Resource

        def self.default_repository_name
          ADAPTER
        end

        property :id, Serial
        property :type, Discriminator
        property :name, String
      end

      class ::Sprog < Flanimal; end

      Orange.auto_migrate!(ADAPTER)
      Apple.auto_migrate!(ADAPTER)
      FortunePig.auto_migrate!(ADAPTER)

      orange = Orange.new(:color => 'orange')
      orange.name = 'Bob' # Keys are protected from mass-assignment by default.
      repository(ADAPTER) { orange.save }
    end

    it "should be able to overwrite Resource#to_s" do
      repository(ADAPTER) do
        ted = FortunePig.create(:name => "Ted")
        FortunePig.get!(ted.id).to_s.should == 'Ted'
      end
    end

    it "should be able to destroy objects" do
      apple = Apple.create(:color => 'Green')
      lambda do
        apple.destroy.should be_true
      end.should_not raise_error
    end

    it 'should return false to #destroy if the resource is new' do
      Apple.new.destroy.should be_false
    end

    it "should be able to reload objects" do
      orange = repository(ADAPTER) { Orange.get!('Bob') }
      orange.color.should == 'orange'
      orange.color = 'blue'
      orange.color.should == 'blue'
      orange.reload
      orange.color.should == 'orange'
    end

    it "should be able to reload new objects" do
      repository(ADAPTER) do
        Orange.create(:name => 'Tom').reload
      end
    end

    it "should be able to find first or create objects" do
      repository(ADAPTER) do
        orange = Orange.create(:name => 'Naval')

        Orange.first_or_create(:name => 'Naval').should == orange

        purple = Orange.first_or_create(:name => 'Purple', :color => 'Fuschia')
        oranges = Orange.all(:name => 'Purple')
        oranges.size.should == 1
        oranges.first.should == purple
      end
    end

    it "should be able to override a default with a nil" do
      repository(ADAPTER) do
        apple = Apple.new
        apple.color = nil
        apple.save
        apple.color.should be_nil

        apple = Apple.create(:color => nil)
        apple.color.should be_nil
      end
    end

    it "should be able to respond to create hooks" do
      bob = repository(ADAPTER) { FortunePig.create(:name => 'Bob') }
      bob.id.should_not be_nil
      bob.instance_variable_get("@created_id").should == bob.id

      fred = FortunePig.new(:name => 'Fred')
      repository(ADAPTER) { fred.save }
      fred.id.should_not be_nil
      fred.instance_variable_get("@save_id").should == fred.id
    end

    it "should be dirty when Object properties are changed" do
      # pending "Awaiting Property#track implementation"
      repository(ADAPTER) do
        Male.auto_migrate!
      end
      repository(ADAPTER) do
        bob = Male.create(:name => "Bob", :data => {})
        bob.dirty?.should be_false
        bob.data.merge!(:name => "Bob")
        bob.dirty?.should be_true
        bob = Male.first
        bob.data[:name] = "Bob"
        bob.dirty?.should be_true
      end
    end

    describe "hooking" do
      before :all do
        Car.auto_migrate!(ADAPTER)
      end

      it "should execute hooks before creating/updating objects" do
        repository(ADAPTER) do
          c1 = Car.new(:brand => 'BMW', :color => 'white')

          c1.new_record?.should == true
          c1.created_on.should == nil

          c1.save

          c1.new_record?.should == false
          c1.touched_on.should == Date.today
          c1.created_on.should == Date.today
          c1.updated_on.should == nil

          c1.color = 'black'
          c1.save

          c1.updated_on.should == Date.today
        end

      end

    end

    describe "inheritance" do
      before :all do
        Geek.auto_migrate!(ADAPTER)

        repository(ADAPTER) do
          Male.create(:name => 'John Dorian')
          Bully.create(:name => 'Bob', :iq => 69)
          Geek.create(:name => 'Steve', :awkward => false, :iq => 132)
          Geek.create(:name => 'Bill', :iq => 150)
          Bully.create(:name => 'Johnson')
          Mugger.create(:name => 'Frank')
          Maniac.create(:name => 'William')
          Psycho.create(:name => 'Norman')
        end

        Flanimal.auto_migrate!(ADAPTER)

      end

      it "should test bug ticket #302" do
        repository(ADAPTER) do
          Sprog.create(:name => 'Marty')
          Sprog.first(:name => 'Marty').should_not be_nil
        end
      end

      it "should select appropriate types" do
        repository(ADAPTER) do
          males = Male.all
          males.should have(8).entries

          males.each do |male|
            male.class.name.should == male.type.name
          end

          Male.first(:name => 'Steve').should be_a_kind_of(Geek)
          Bully.first(:name => 'Bob').should be_a_kind_of(Bully)
          Geek.first(:name => 'Steve').should be_a_kind_of(Geek)
          Geek.first(:name => 'Bill').should be_a_kind_of(Geek)
          Bully.first(:name => 'Johnson').should be_a_kind_of(Bully)
          Male.first(:name => 'John Dorian').should be_a_kind_of(Male)
        end
      end

      it "should not select parent type" do
        repository(ADAPTER) do
          Male.first(:name => 'John Dorian').should be_a_kind_of(Male)
          Geek.first(:name => 'John Dorian').should be_nil
          Geek.first.iq.should > Bully.first.iq
        end
      end

      it "should select objects of all inheriting classes" do
        repository(ADAPTER) do
          Male.all.should have(8).entries
          Geek.all.should have(2).entries
          Bully.all.should have(5).entries
          Mugger.all.should have(1).entries
          Maniac.all.should have(2).entries
          Psycho.all.should have(1).entries
        end
      end

      it "should inherit setter method from parent" do
        repository(ADAPTER) do
          Bully.first(:name => "Bob").iq.should == 68
        end
      end

      it "should be able to overwrite a setter in a child class" do
        repository(ADAPTER) do
          Geek.first(:name => "Bill").iq.should == 180
        end
      end
    end
  end
end
