require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Validate do
  before :all do
    class ::Yacht
      include DataMapper::Resource
      property :id, Integer, :serial => true
      property :name, String, :auto_validation => false

      validates_present :name
    end
  end

  describe '#validations' do
      it 'should support more different validations of a different type' do
          number_of_validators_before = Yacht.validators.contexts[:default].length
            class ::Yacht
                validates_is_unique :name
            end
          number_of_validators_after = Yacht.validators.contexts[:default].length
          (number_of_validators_after - number_of_validators_before).should == 1
      end

  end

  it 'should respond to save' do
    Yacht.new.should respond_to(:save)
  end

  describe '#save' do
    before do
      Yacht.auto_migrate!
      @yacht = Yacht.new :name => 'The Gertrude'
    end

    describe 'without context specified' do
      it 'should validate using the default context' do
        @yacht.should_receive(:valid?).with(:default)
        @yacht.save
      end

      it 'should save if the object is valid for the default context' do
        @yacht.should be_valid
        @yacht.save.should be_true
        @yacht.should_not be_new_record
      end

      it 'should not save if the object is not valid for the default context' do
        @yacht.name = 'a'
        @yacht.should be_valid

        @yacht.name = nil
        @yacht.should_not be_valid
        @yacht.save.should be_false
        @yacht.should be_new_record
      end
    end

    describe 'with context specified' do
      before :all do
        class ::Yacht
          validates_length :name, :min => 2, :context => [ :strict_name ]
        end
      end

      it 'should validate using the specified context' do
        @yacht.should_receive(:valid?).with(:strict_name)
        @yacht.save(:strict_name)
      end

      it 'should save if the object is valid for the specified context' do
        @yacht.should be_valid(:strict_name)
        @yacht.save(:strict_name).should be_true
        @yacht.should_not be_new_record
      end

      it 'should not save if the object is not valid for the specified context' do
        @yacht.name = 'aa'
        @yacht.should be_valid(:strict_name)

        @yacht.name = 'a'
        @yacht.should_not be_valid(:strict_name)
        @yacht.save(:strict_name).should be_false
        @yacht.should be_new_record
      end
    end
  end

  describe '#save!' do
    before do
      Yacht.auto_migrate!
      @yacht = Yacht.new
    end

    it "should save object without running validations" do
      @yacht.should_not_receive(:valid?)
      @yacht.save!
      @yacht.should_not be_new_record
    end
  end

  describe '#create!' do
    before do
      Yacht.auto_migrate!
    end

    it "should save object without running validations" do
      Yacht.create!.should be_a_kind_of(Yacht)
    end
  end

  describe "#create" do
    before do
      Yacht.auto_migrate!
    end

    it "should run validations" do
      Yacht.create.new_record?.should be_true
    end
  end

  it "should respond to validatable? (for recursing associations)" do
    Yacht.new.should be_validatable
    Class.new.new.should_not be_validatable
  end

  it "should have a set of errors on the instance of the resource" do
    shamrock = Yacht.new
    shamrock.should respond_to(:errors)
  end

  it "should have a set of contextual validations on the class of the resource" do
    Yacht.should respond_to(:validators)
  end

  it "should execute all validators for a given context against the resource" do
    Yacht.validators.should respond_to(:execute)
  end

  it "should place a validator in the :default context if a named context is not provided" do
    Yacht.validators.context(:default).length.should == 2
  end

  it "should allow multiple user defined contexts for a validator" do
    class ::Yacht
      property :port, String, :auto_validation => false
      validates_present :port, :context => [:at_sea, :in_harbor]
    end

    Yacht.validators.context(:at_sea).length.should == 1
    Yacht.validators.context(:in_harbor).length.should == 1
    Yacht.validators.context(:no_such_context).length.should == 0
  end

  it "should alias :on and :when for :context" do
    class ::Yacht
      property :owner, String, :auto_validation => false
      property :bosun, String, :auto_validation => false

      validates_present :owner, :on => :owned_vessel
      validates_present :bosun, :when => [:under_way]
    end
    Yacht.validators.context(:owned_vessel).length.should == 1
    Yacht.validators.context(:under_way).length.should == 1
  end

  it "should alias :group for :context (backward compat with Validatable??)" do
    class ::Yacht
      property :captain, String, :auto_validation => false
      validates_present :captain, :group => [:captained_vessel]
    end
    Yacht.validators.context(:captained_vessel).length.should == 1
  end

  it "should add a method valid_for_<context_name>? for each context" do
    class ::Yacht
      property :engine_size, String, :auto_validation => false
      validates_present :engine_size, :when => :power_boat
    end

    cigaret = Yacht.new
    cigaret.valid_for_default?.should_not == true
    cigaret.should respond_to(:valid_for_power_boat?)
    cigaret.valid_for_power_boat?.should_not == true

    cigaret.engine_size = '4 liter V8'
    cigaret.valid_for_power_boat?.should == true
  end

  it "should add a method all_valid_for_<context_name>? for each context" do
    class ::Yacht
      property :mast_height, String, :auto_validation => false
      validates_present :mast_height, :when => :sailing_vessel
    end
    swift = Yacht.new
    swift.should respond_to(:all_valid_for_sailing_vessel?)
  end

  it "should be able to translate the error message" # needs String::translations

  it "should be able to get the error message for a given field" do
    class ::Yacht
      property :wood_type, String, :auto_validation => false
      validates_present :wood_type, :on => :wooden_boats
    end
    fantasy = Yacht.new
    fantasy.valid_for_wooden_boats?.should == false
    fantasy.errors.on(:wood_type).first.should == 'Wood type must not be blank'
    fantasy.wood_type = 'birch'
    fantasy.valid_for_wooden_boats?.should == true
  end

  it "should be able to specify a custom error message" do
    class ::Yacht
      property :year_built, String, :auto_validation => false
      validates_present :year_built, :when => :built, :message => 'Year built is a must enter field'
    end

    sea = Yacht.new
    sea.valid_for_built?.should == false
    sea.errors.full_messages.first.should == 'Year built is a must enter field'
  end

  it "should execute a Proc when provided in an :if clause and run validation if the Proc returns true" do
    class ::Dingy
      include DataMapper::Resource
      property :id, Integer, :serial => true
      property :owner, String, :auto_validation => false
      validates_present :owner, :if => Proc.new{|resource| resource.owned?}

      def owned?
        false
      end
    end

    Dingy.new.valid?.should == true

    class ::Dingy
      def owned?
        true
      end
    end

    Dingy.new.valid?.should_not == true
  end

  it "should execute a symbol or method name provided in an :if clause and run validation if the method returns true" do
    class ::Dingy
      validators.clear!
      validates_present :owner, :if => :owned?

      def owned?
        false
      end
    end

    Dingy.new.valid?.should == true

    class ::Dingy
      def owned?
        true
      end
    end

    Dingy.new.valid?.should_not == true
  end

  it "should execute a Proc when provided in an :unless clause and not run validation if the Proc returns true" do
    class ::RowBoat
      include DataMapper::Resource
      property :id, Integer, :serial => true
      validates_present :salesman, :unless => Proc.new{|resource| resource.sold?}

      def sold?
        false
      end
    end

    RowBoat.new.valid?.should_not == true

    class ::RowBoat
      def sold?
        true
      end
    end

    RowBoat.new.valid?.should == true
  end

  it "should execute a symbol or method name provided in an :unless clause and not run validation if the method returns true" do
    class ::Dingy
      validators.clear!
      validates_present :salesman, :unless => :sold?

      def sold?
        false
      end
    end

    Dingy.new.valid?.should_not == true  #not sold and no salesman

    class ::Dingy
      def sold?
        true
      end
    end

    Dingy.new.valid?.should == true    # sold and no salesman
  end

  it "should perform automatic recursive validation #all_valid? checking all instance variables (and ivar.each items if valid)" do
    class ::Invoice
      include DataMapper::Resource
      property :id, Integer, :serial => true
      property :customer, String, :auto_validation => false
      validates_present :customer

      def line_items
        @line_items || @line_items = []
      end

      def comment
        @comment || nil
      end

      def comment=(value)
        @comment = value
      end
    end

    class ::LineItem
      include DataMapper::Resource
      property :id, Integer, :serial => true
      property :price, String, :auto_validation => false
      validates_is_number :price

      def initialize(price)
        @price = price
      end
    end

    class ::Comment
      include DataMapper::Resource
      property :id, Integer, :serial => true
      property :note, String, :auto_validation => false

      validates_present :note
    end

    invoice = Invoice.new(:customer => 'Billy Bob')
    invoice.valid?.should == true

    for i in 1..6 do
      invoice.line_items << LineItem.new(i.to_s)
    end
    invoice.line_items[1].price = 'BAD VALUE'
    invoice.comment = Comment.new

    invoice.comment.valid?.should == false
    invoice.line_items[1].valid?.should == false

    invoice.all_valid?.should == false
    invoice.comment.note = 'This is a note'

    invoice.all_valid?.should == false
    invoice.line_items[1].price = '23.44'

    invoice.all_valid?.should == true
  end

  it "should retrieve private instance variables for validation" do
    class ::Raft
      include DataMapper::Resource
      property :length, Integer, :accessor => :private

      def initialize(length)
        @length = length
      end
    end

    Raft.new(10).validation_property_value("length").should == 10
  end

  it "should duplicate validations to STI models" do
    class ::Company
      include DataMapper::Resource

      validates_present :title, :message => "Company name is a required field"

      property :id,       Integer, :serial => true, :key => true
      property :title,    String
      property :type,     Discriminator
    end

    class ::ServiceCompany < Company
    end

    class ::ProductCompany < Company
    end
    company = ServiceCompany.new
    company.should_not be_valid
  end
end
