require File.join(File.dirname(__FILE__), 'spec_helper.rb')

if COUCHDB_AVAILABLE
  class ::User
    include DataMapper::CouchResource
    def self.default_repository_name
      :couch
    end

    # regular properties
    property :name, String
    property :age, Integer
    property :wealth, Float
    property :created_at, DateTime
    property :created_on, Date
    property :location, JsonObject

    # creates methods for accessing stored/indexed views in the CouchDB database
    view(:by_name) {{ "map" => "function(doc) { if (#{couchdb_types_condition}) { emit(doc.name, doc); } }" }}
    view(:by_age)  {{ "map" => "function(doc) { if (#{couchdb_types_condition}) { emit(doc.age, doc); } }" }}
    view(:count)   {{ "map" => "function(doc) { if (#{couchdb_types_condition}) { emit(null, 1); } }",
                      "reduce" => "function(keys, values) { return sum(values); }" }}

    belongs_to :company

    before :create do
      self.created_at = DateTime.now
      self.created_on = Date.today
    end
  end

  class ::Company
    include DataMapper::CouchResource
    def self.default_repository_name
      :couch
    end

    # This class happens to have similar properties
    property :name, String
    property :age, Integer

    has n, :users
  end

  class ::Person
    include DataMapper::CouchResource
    def self.default_repository_name
      :couch
    end

    property :name, String
  end

  class ::Employee < Person
    property :rank, String
  end

  class ::Broken
    include DataMapper::CouchResource
    def self.default_repository_name
      :couch
    end

    property :couchdb_type, Discriminator
    property :name, String
  end

  describe DataMapper::Adapters::CouchdbAdapter do

    describe "resource functions" do

      before(:each) do
        @user = User.new(:name => "Jamie", :age => 67, :wealth => 11.5)
        @user.save.should be_true
      end

      after(:each) do
        @user.destroy.should be_true
      end

      it "should create a record with a specified id" do
        user_with_id = User.new(:name => 'user with id')
        user_with_id.id = 'user_id'
        user_with_id.save.should == true
        User.get!('user_id', :repository => :couch).should == user_with_id
        user_with_id.destroy.should be_true
      end

      it "should get a record" do
        user = User.get!(@user.id)
        user.id.should_not be_nil
        user.name.should == "Jamie"
        user.age.should == 67
      end

      it "should not get records of the wrong type by id" do
        Company.get(@user.id).should == nil
        lambda { Company.get!(@user.id) }.should raise_error(DataMapper::ObjectNotFoundError)
      end

      it "should update a record" do
        user = User.get!(@user.id)
        user.name = "Janet"
        user.save
        user.name.should_not == @user.name
        user.rev.should_not == @user.rev
        user.age.should == @user.age
        user.id.should == @user.id
        user.destroy.should be_true
      end

      it "should get all records" do
        User.all.length.should == 1
      end

      it "should set total_rows on collection" do
        User.all.total_rows.should == 1
      end
    end

    describe "ad_hoc queries" do

      before(:each) do
        @user = User.new({ :name => "Jamie", :age => 67, :wealth => 11.5 })
        @user.save.should be_true
      end

      after(:each) do
        @user.destroy.should be_true
      end

      it "should get records by eql matcher" do
        User.all(:name => "Jamie").size.should == 1
        User.all(:age => 50).size.should == 0
      end

      it "should get records by not matcher" do
        User.all(:age.not => 50).size.should == 1
      end

      it "should get records by gt matcher" do
        User.all(:age.gt => 67).size.should == 0
      end

      it "should get records by gte matcher" do
        User.all(:age.gte => 67).size.should == 1
      end

      it "should get records by lt matcher" do
        User.all(:age.lt => 67).size.should == 0
      end

      it "should get records by lte matcher" do
        User.all(:age.lte => 67).size.should == 1
      end

      it "should get records by the like matcher" do
        User.all(:name.like => "Jo").size.should == 0
        User.all(:name.like => "Ja%").size.should == 1
        User.all(:name.like => "%J%m%").size.should == 1
        User.all(:name.like => /^Jam/).size.should == 1
      end

      it "should get records with multiple matchers" do
        User.all(:name => "Jamie", :age.lt => 80).size.should == 1
      end

      it "should order records" do
        user = User.new(:name => "Aaron", :age => 30)
        user.save
        users = User.all(:order => [:age])
        users[0].age.should == 30
        users = User.all(:order => [:name, :age])
        users[0].age.should == 30
        users[1].age.should == 67
        user.destroy
      end

    end

    describe "view queries" do

      before(:all) do
        User.auto_migrate!
      end

      before(:each) do
        @jamie = User.create(:name => "Jamie", :age => 67, :wealth => 11.5)
        @aaron = User.create(:name => "Aaron", :age => 30, :wealth => 20)
      end

      after(:each) do
        [@jamie, @aaron].each { |user| user.destroy }
      end

      it "should be able to call stored views" do
        User.by_name.first.should == User.all(:order => [:name]).first
        User.by_age.first.should == User.all(:order => [:age]).first
      end

      it "should be able to call stored views with keys" do
        User.by_name("Aaron").first == User.all(:name => "Aaron").first
        User.by_age(30).first == User.all(:age => 30).first
        User.by_name("Aaron").first == User.by_name(:key => "Aaron").first
        User.by_age(30).first == User.by_age(:key => 30).first
      end

      it "should return a value from a view with reduce defined" do
        User.count.should == [ { "value" => User.all.length, "key" => nil } ]
      end

      it "should be able to perform ordered multi-key fetch on a view" do
        User.by_name(:keys => ["Aaron", "Jamie"]).should == [@aaron, @jamie]
        User.by_name(:keys => ["Jamie", "Aaron"]).should == [@jamie, @aaron]
      end

    end

    describe "associations" do
      before(:all) do
        @company = Company.create(:name => "ExCorp")
        @user = User.create(:name => 'John', :company => @company)
      end
      after(:all) do
        @company.destroy
        @user.destroy
      end

      it "should work with belongs_to associations" do
        User.get(@user.id).company.should == @company
      end

      it "should work with has n associations" do
        @company.users.should include(@user)
      end
    end

    describe 'STI' do

      before(:all) do
        Person.auto_migrate!
      end

      it "should override default type" do
        person = Person.new(:name => 'Bob')
        person.save.should be_true
        Person.first.couchdb_type.should == Person
        person.destroy.should be_true
      end

      it "should load descendents on parent.all" do
        employee = Employee.new(:name => 'Bob', :rank => 'Peon')
        employee.save.should be_true
        Person.all.include?(employee).should be_true
        employee.destroy.should be_true
      end

      it "should be able to get children from parent.get" do
        employee = Employee.new(:name => 'Bob', :rank => 'Peon')
        employee.save.should be_true
        Person.get(employee.id).should_not be_nil
        employee.destroy.should be_true
      end

      it "should load descendents on parent.by_name" do
        employee = Employee.new(:name => 'Bob', :rank => 'Peon')
        employee.save.should be_true
        Person.by_name(:key => 'Bob').include?(employee).should be_true
        employee.destroy.should be_true
      end
    end

    describe 'JSON serialization' do
      if DMSERIAL_AVAILABLE
        before(:all) do
          @moe = User.create(:name => "Moe", :age => 46, :wealth => 20)
          @larry = User.create(:name => "Larry", :age => 42, :wealth => 10)
          @curly = User.create(:name => "Curly", :age => 44, :wealth => 1)
        end

        after(:all) do
          [@moe, @larry, @curly].each { |stooge| stooge.destroy }
        end

        it "should properly serialize a single resource" do
          moe_serial = JSON.parse(@moe.to_json)
          moe_serial['name'].should == "Moe"
          moe_serial['age'].should == 46
          moe_serial['wealth'].should == 20
        end

        it "should properly serialize a resource collection" do
          stooges = JSON.parse(User.all.to_json)
          stooges.length.should == 3
          stooges.each { |stooge| stooge['name'].should_not be_blank }
        end
      else
        it "requires dm-serializer to run serialization tests"
      end
    end

  end
end
