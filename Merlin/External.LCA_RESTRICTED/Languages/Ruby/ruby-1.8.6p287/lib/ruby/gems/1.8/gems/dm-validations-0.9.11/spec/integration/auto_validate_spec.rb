require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

module TypecastBypassSetter
  # Bypass typecasting so we can set values for specs
  def set(attributes)
    attributes.each do |k, v|
      instance_variable_set("@#{k}", v)
    end
  end
end

class SailBoat
  include DataMapper::Resource
  property :id,            Integer,    :key => true
  property :name,          String,                                :nullable => false,     :validates       => :presence_test
  property :description,   String,     :length => 10,                                     :validates       => :length_test_1
  property :notes,         String,     :length => 2..10,                                  :validates       => :length_test_2
  property :no_validation, String,                                                        :auto_validation => false
  property :salesman,      String,                                :nullable => false,     :validates       => [:multi_context_1, :multi_context_2]
  property :code,          String,     :format => Proc.new { |code| code =~ /A\d{4}\z/ }, :validates       => :format_test
  property :allow_nil,     String,     :size => 5..10,            :nullable => true,      :validates       => :nil_test
  property :build_date,    Date,                                                          :validates       => :primitive_test
  property :float,         Float,      :precision => 2, :scale => 1
  property :big_decimal,   BigDecimal, :precision => 2, :scale => 1

  include TypecastBypassSetter
end

class HasNullableBoolean
  include DataMapper::Resource
  property :id,   Integer, :key => true
  property :bool, Boolean # :nullable => true by default

  include TypecastBypassSetter
end

class HasNotNullableBoolean
  include DataMapper::Resource
  property :id,   Integer, :key => true
  property :bool, Boolean, :nullable => false

  include TypecastBypassSetter
end

class HasNotNullableParanoidBoolean
  include DataMapper::Resource
  property :id,   Integer,         :key => true
  property :bool, ParanoidBoolean, :nullable => false

  include TypecastBypassSetter
end

describe "Automatic Validation from Property Definition" do
  it "should have a hook for adding auto validations called from
      DataMapper::Property#new" do
    SailBoat.should respond_to(:auto_generate_validations)
  end

  it "should auto add a validates_is_present when property has option
      :nullable => false" do
    validator = SailBoat.validators.context(:presence_test).first
    validator.should be_kind_of(DataMapper::Validate::RequiredFieldValidator)
    validator.field_name.should == :name

    boat = SailBoat.new
    boat.valid_for_presence_test?.should == false
    boat.errors.on(:name).should include('Name must not be blank')
    boat.name = 'Float'
    boat.valid_for_presence_test?.should == true
  end

  it "should auto add a validates_length for maximum size on String properties" do
    # max length test max=10
    boat = SailBoat.new
    boat.valid_for_length_test_1?.should == true  #no minimum length
    boat.description = 'ABCDEFGHIJK' #11
    boat.valid_for_length_test_1?.should == false
    boat.errors.on(:description).should include('Description must be less than 10 characters long')
    boat.description = 'ABCDEFGHIJ' #10
    boat.valid_for_length_test_1?.should == true
  end

  it "should auto add validates_length within a range when option :length
      or :size is a range" do
    # Range test notes = 2..10
    boat = SailBoat.new
    boat.should be_valid_for_length_test_2
    boat.notes = 'AB' #2
    boat.should be_valid_for_length_test_2
    boat.notes = 'ABCDEFGHIJK' #11
    boat.should_not be_valid_for_length_test_2
    boat.errors.on(:notes).should include('Notes must be between 2 and 10 characters long')
    boat.notes = 'ABCDEFGHIJ' #10
    boat.should be_valid_for_length_test_2
  end

  it "should auto add a validates_format if the :format option is given" do
    # format test - format = /A\d{4}\z/   on code
    boat = SailBoat.new
    boat.should be_valid_for_format_test
    boat.code = 'A1234'
    boat.should be_valid_for_format_test
    boat.code = 'BAD CODE'
    boat.should_not be_valid_for_format_test
    boat.errors.on(:code).should include('Code has an invalid format')
  end

  it "should auto validate all strings for max length" do
    klass = Class.new do
      include DataMapper::Resource
      property :id, Integer, :serial => true
      property :name, String
    end
    t = klass.new(:id => 1)
    t.should be_valid
    t.name = 'a' * 51
    t.should_not be_valid
    t.errors.on(:name).should include('Name must be less than 50 characters long')
  end

  it "should auto validate the primitive type" do
    validator = SailBoat.validators.context(:primitive_test).first
    validator.should be_kind_of(DataMapper::Validate::PrimitiveValidator)
    boat = SailBoat.new
    boat.should be_valid_for_primitive_test
    boat.build_date = 'ABC'
    boat.should_not be_valid_for_primitive_test
    boat.errors.on(:build_date).should include('Build date must be of type Date')
  end

  it "should not auto add any validators if the option :auto_validation => false was given" do
    klass = Class.new do
      include DataMapper::Resource
      property :id,   Integer,                    :serial   => true,  :auto_validation => false
      property :name, String,                     :nullable => false, :auto_validation => false
      property :bool, DataMapper::Types::Boolean, :nullable => false, :auto_validation => false
    end
    klass.new.valid?.should == true
  end

  it "should auto add range checking the length of a string while still allowing null values" do
    boat = SailBoat.new
    boat.allow_nil = 'ABC'
    boat.should_not be_valid_for_nil_test
    boat.errors.on(:allow_nil).should include('Allow nil must be between 5 and 10 characters long')

    boat.allow_nil = 'ABCDEFG'
    boat.should be_valid_for_nil_test

    boat.allow_nil = 'ABCDEFGHIJKLMNOP'
    boat.should_not be_valid_for_nil_test
    boat.errors.on(:allow_nil).should include('Allow nil must be between 5 and 10 characters long')

    boat.allow_nil = nil
    boat.should be_valid_for_nil_test
  end

  describe 'for Integer properties' do
    before do
      @boat = SailBoat.new
    end

    it 'should allow integers' do
      @boat.set(:id => 1)
      @boat.should be_valid
    end

    it 'should not allow floats' do
      @boat.set(:id => 1.0)
      @boat.should_not be_valid
      @boat.errors.on(:id).should include('Id must be an integer')
    end

    it 'should not allow decimals' do
      @boat.set(:id => BigDecimal('1'))
      @boat.should_not be_valid
      @boat.errors.on(:id).should include('Id must be an integer')
    end
  end

  describe 'for nullable Boolean properties' do
    before do
      @boat = HasNullableBoolean.new(:id => 1)
    end

    it 'should allow true' do
      @boat.set(:bool => true)
      @boat.should be_valid
    end

    it 'should allow false' do
      @boat.set(:bool => false)
      @boat.should be_valid
    end

    it 'should allow nil' do
      @boat.set(:bool => nil)
      @boat.should be_valid
    end
  end

  describe 'for non-nullable Boolean properties' do
    before do
      @boat = HasNotNullableBoolean.new(:id => 1)
    end

    it 'should allow true' do
      @boat.set(:bool => true)
      @boat.should be_valid
    end

    it 'should allow false' do
      @boat.set(:bool => false)
      @boat.should be_valid
    end

    it 'should not allow nil' do
      @boat.set(:bool => nil)
      @boat.should_not be_valid
      @boat.errors.on(:bool).should include('Bool must not be nil')
    end
  end

  describe 'for non-nullable ParanoidBoolean properties' do
    before do
      @boat = HasNotNullableParanoidBoolean.new(:id => 1)
    end

    it 'should allow true' do
      @boat.set(:bool => true)
      @boat.should be_valid
    end

    it 'should allow false' do
      @boat.set(:bool => false)
      @boat.should be_valid
    end

    it 'should not allow nil' do
      @boat.set(:bool => nil)
      @boat.should_not be_valid
      @boat.errors.on(:bool).should include('Bool must not be nil')
    end
  end

  { :float => Float, :big_decimal => BigDecimal }.each do |column, type|
    describe "for #{type} properties" do
      before do
        @boat = SailBoat.new(:id => 1)
      end

      it 'should allow integers' do
        @boat.set(column => 1)
        @boat.should be_valid
      end

      it 'should allow floats' do
        @boat.set(column => '1.0')
        @boat.should be_valid
      end

      it 'should allow decimals' do
        @boat.set(column => BigDecimal('1'))
        @boat.should be_valid
      end
    end
  end

  describe 'for within validator' do
    before :all do
      class ::LimitedBoat
        include DataMapper::Resource
        property :id,       Integer,  :serial => true
        property :limited,  String,   :set => ['foo', 'bar', 'bang'], :default => 'foo'
      end
    end

    before do
      @boat = LimitedBoat.new
    end

    it 'should set default value' do
      @boat.should be_valid
    end

    it 'should not accept value not in range' do
      @boat.limited = "blah"
      @boat.should_not be_valid
      @boat.errors.on(:limited).should include('Limited must be one of [foo, bar, bang]')
    end

  end

  describe 'for custom messages' do
    it "should have correct error message" do
      custom_boat = Class.new do
        include DataMapper::Resource
        property :id,   Integer, :serial => true
        property :name, String,  :nullable => false, :message => "This boat must have name"
      end
      boat = custom_boat.new
      boat.should_not be_valid
      boat.errors.on(:name).should include('This boat must have name')
    end

    it "should have correct error messages" do
      custom_boat = Class.new do
        include DataMapper::Resource
        property :id,   Integer, :serial => true
        property :name, String,  :nullable => false, :length => 5..20, :format => /^[a-z]+$/,
                 :messages => {
                   :presence => "This boat must have name",
                   :length => "Name must have at least 4 and at most 20 chars",
                   :format => "Please use only small letters"
                 }
      end

      boat = custom_boat.new
      boat.should_not be_valid
      boat.errors.on(:name).should include("This boat must have name")
      boat.errors.on(:name).should include("Name must have at least 4 and at most 20 chars")
      boat.errors.on(:name).should include("Please use only small letters")

      boat.name = "%%"
      boat.should_not be_valid
      boat.errors.on(:name).should_not include("This boat must have name")
      boat.errors.on(:name).should include("Name must have at least 4 and at most 20 chars")
      boat.errors.on(:name).should include("Please use only small letters")

      boat.name = "%%asd"
      boat.should_not be_valid
      boat.errors.on(:name).should_not include("This boat must have name")
      boat.errors.on(:name).should_not include("Name must have at least 4 and at most 20 chars")
      boat.errors.on(:name).should include("Please use only small letters")

      boat.name = "superboat"
      boat.should be_valid
      boat.errors.on(:name).should be_nil
    end
  end
end
