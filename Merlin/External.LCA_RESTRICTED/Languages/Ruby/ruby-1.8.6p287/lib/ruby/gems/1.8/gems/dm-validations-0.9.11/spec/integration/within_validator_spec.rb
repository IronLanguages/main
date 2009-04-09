require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Validate::WithinValidator do
  before(:all) do
    class ::Telephone
      include DataMapper::Resource
      property :id, Integer, :serial => true
      property :type_of_number, String, :auto_validation => false
      validates_within :type_of_number, :set => ['Home', 'Work', 'Cell']
    end

    class ::Inf
      include DataMapper::Resource
      property :id, Integer, :serial => true
      property :gte, Integer, :auto_validation => false
      property :lte, Integer, :auto_validation => false
      property :between, Integer, :auto_validation => false
      validates_within :gte, :set => (10..n)
      validates_within :lte, :set => (-n..10)
      validates_within :between, :set => (10..20)
    end

    class ::Receiver
      include DataMapper::Resource
      property :id, Integer, :serial => true
      property :holder, String, :auto_validation => false, :default => 'foo'
      validates_within :holder, :set => ['foo', 'bar', 'bang']
    end

    class ::Nullable
      include DataMapper::Resource
      property :id, Integer, :serial => true
      property :nullable, Integer, :auto_validation => false
      validates_within :nullable, :set => (1..5), :allow_nil => true
    end
  end

  it "should validate a value on an instance of a resource within a predefined
      set of values" do
    tel = Telephone.new
    tel.valid?.should_not == true
    tel.errors.full_messages.first.should == 'Type of number must be one of [Home, Work, Cell]'

    tel.type_of_number = 'Cell'
    tel.valid?.should == true
  end

  it "should validate a value within range with infinity" do
    inf = Inf.new
    inf.should_not be_valid
    inf.errors.on(:gte).first.should == 'Gte must be greater than or equal to 10'
    inf.errors.on(:lte).first.should == 'Lte must be less than or equal to 10'
    inf.errors.on(:between).first.should == 'Between must be between 10 and 20'

    inf.gte = 10
    inf.lte = 10
    inf.between = 10
    inf.valid?.should == true
  end

  it "should validate a value by its default" do
    tel = Receiver.new
    tel.should be_valid
  end

  it "should allow a nil value if :allow_nil is true" do
    nullable = Nullable.new

    nullable.nullable = nil
    nullable.should be_valid

    nullable.nullable = 11
    nullable.should_not be_valid

    nullable.nullable = 3
    nullable.should be_valid
  end
end
