require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe DataMapper::Type do

  before(:each) do
    class TestType < DataMapper::Type
      primitive String
      size 10
    end

    class TestType2 < DataMapper::Type
      primitive String
      size 10

      def self.load(value, property)
        value.reverse
      end

      def self.dump(value, property)
        value.reverse
      end
    end

    class TestResource
      include DataMapper::Resource
    end

    class TestType3 < DataMapper::Type
      primitive String
      size 10
      attr_accessor :property, :value

      def self.load(value, property)
        type = self.new
        type.property = property
        type.value    = value
        type
      end

      def self.dump(value, property)
        value.value
      end
    end
  end

  it "should have the same PROPERTY_OPTIONS array as DataMapper::Property" do
    DataMapper::Type::PROPERTY_OPTIONS.should == DataMapper::Property::PROPERTY_OPTIONS
  end

  it "should create a new type based on String primitive" do
    TestType.primitive.should == String
  end

  it "should have size of 10" do
    TestType.size.should == 10
  end

  it "should have options hash exactly equal to options specified in custom type" do
    #ie. it should not include null elements
    TestType.options.should == { :size => 10, :length => 10 }
  end

  it "should have length aliased to size" do
    TestType.length.should == TestType.size
  end

  it "should pass through the value if load wasn't overridden" do
    TestType.load("test", nil).should == "test"
  end

  it "should pass through the value if dump wasn't overridden" do
    TestType.dump("test", nil).should == "test"
  end

  it "should not raise NotImplementedException if load was overridden" do
    TestType2.dump("helo", nil).should == "oleh"
  end

  it "should not raise NotImplementedException if dump was overridden" do
    TestType2.load("oleh", nil).should == "helo"
  end

  describe "using a custom type" do
    before do
      @property = DataMapper::Property.new TestResource, :name, TestType3, {}
    end

    it "should return a object of the same type" do
      TestType3.load("helo", @property).class.should == TestType3
    end

    it "should contain the property" do
      TestType3.load("helo", @property).property.should == @property
    end

    it "should contain the value" do
      TestType3.load("helo", @property).value.should == "helo"
    end

    it "should return the value" do
      obj = TestType3.load("helo", @property)
      TestType3.dump(obj, @property).should == "helo"
    end
  end

  describe "using def Type" do
    before do
      @class = Class.new(DataMapper::Type(String, :size => 20))
    end

    it "should be of the specified type" do
      @class.primitive.should == String
    end

    it "should have the right options set" do
      @class.size.should == 20
    end
  end
end
