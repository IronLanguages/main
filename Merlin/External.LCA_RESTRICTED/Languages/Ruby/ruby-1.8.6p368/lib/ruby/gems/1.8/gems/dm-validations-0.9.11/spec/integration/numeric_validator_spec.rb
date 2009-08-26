require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

class Bill # :nodoc:
  include DataMapper::Resource
  property :id, Integer, :serial => true
  property :amount_1, String, :auto_validation => false
  property :amount_2, Float, :auto_validation => false
  validates_is_number :amount_1, :amount_2
end

class Hillary # :nodoc:
  include DataMapper::Resource
  property :id, Integer, :serial => true
  property :amount_1, Float, :auto_validation => false, :default => 0.01
  validates_is_number :amount_1
end

describe DataMapper::Validate::NumericValidator do
  it "should validate a floating point value on the instance of a resource" do
    b = Bill.new
    b.should_not be_valid
    b.errors.on(:amount_1).should include('Amount 1 must be a number')
    b.errors.on(:amount_2).should include('Amount 2 must be a number')
    b.amount_1 = 'ABC'
    b.amount_2 = 27.343
    b.should_not be_valid
    b.errors.on(:amount_1).should include('Amount 1 must be a number')
    b.amount_1 = '34.33'
    b.should be_valid
  end

  it "should validate an integer value on the instance of a resource" do
    class ::Bill
      property :quantity_1, String, :auto_validation => false
      property :quantity_2, Integer, :auto_validation => false

      validators.clear!
      validates_is_number :quantity_1, :quantity_2, :integer_only => true
    end
    b = Bill.new
    b.valid?.should_not == true
    b.errors.on(:quantity_1).should include('Quantity 1 must be an integer')
    b.errors.on(:quantity_2).should include('Quantity 2 must be an integer')
    b.quantity_1 = '12.334'
    b.quantity_2 = 27.343
    b.valid?.should_not == true
    b.errors.on(:quantity_1).should include('Quantity 1 must be an integer')
    pending 'dm-core truncates float to integer' do
      # FIXME: The next line should pass, but :quantity_2 has no errors. This is
      #        because 27.343 has been truncated to 27 by the time it reaches the
      #        validation. Is this a bug?
      b.errors.on(:quantity_2).should include('Quantity 2 must be an integer')
    end
    b.quantity_1 = '34.33'
    b.quantity_2 = 22
    b.valid?.should_not == true
    b.errors.on(:quantity_1).should include('Quantity 1 must be an integer')
    b.quantity_1 = '34'
    b.valid?.should == true

  end

  it "should validate if a default fufills the requirements" do
    h = Hillary.new
    h.should be_valid
  end

  describe 'auto validation' do
    before :all do
      class ::Fish
        include DataMapper::Resource
        property :id,     Integer, :serial => true
        property :scales, Integer
      end
    end

    describe 'Float' do
      describe 'with default precision and scale' do
        before :all do
          class ::CloudFish < Fish
            property :average_weight, Float
          end
        end

        before do
          @cloud_fish = CloudFish.new
        end

        it 'should allow up to 10 digits before the decimal' do
          @cloud_fish.average_weight = 0
          @cloud_fish.should be_valid

          @cloud_fish.average_weight = 9_999_999_999
          @cloud_fish.should be_valid

          @cloud_fish.average_weight = 10_000_000_000
          @cloud_fish.should_not be_valid
        end

        it 'should allow 0 digits after the decimal' do
          @cloud_fish.average_weight = 0
          @cloud_fish.should be_valid
        end

        it 'should allow any digits after the decimal' do
          @cloud_fish.average_weight = 1.2
          @cloud_fish.should be_valid

          @cloud_fish.average_weight = 123.456
          @cloud_fish.should be_valid
        end

        it "should only allow up to 10 digits overall" do
          @cloud_fish.average_weight = 1.234567890
          @cloud_fish.should be_valid

          @cloud_fish.average_weight = 1.2345678901
          @cloud_fish.should_not be_valid
        end
      end

      describe 'with default precision and scaleof 0' do
        before :all do
          class ::RobotFish < Fish
            property :average_weight, Float, :scale => 0
          end
        end

        before do
          @robot_fish = RobotFish.new
        end

        it 'should allow up to 10 digits before the decimal' do
          @robot_fish.average_weight = 0
          @robot_fish.should be_valid

          @robot_fish.average_weight = 9_999_999_999
          @robot_fish.should be_valid

          @robot_fish.average_weight = 10_000_000_000
          @robot_fish.should_not be_valid
        end

        it 'should allow 0 digits after the decimal' do
          @robot_fish.average_weight = 0
          @robot_fish.should be_valid
        end

        it 'should allow 1 digit after the decimal if it is a zero' do
          @robot_fish.average_weight = 0.0
          @robot_fish.should be_valid

          @robot_fish.average_weight = 9_999_999_999.0
          @robot_fish.should be_valid

          @robot_fish.average_weight = 0.1
          @robot_fish.should_not be_valid
        end
      end

      describe 'with a precision of 4 and a scale of 2' do
        before :all do
          class ::GoldFish < Fish
            property :average_weight, Float, :precision => 4, :scale => 2
          end
        end

        before do
          @gold_fish = GoldFish.new
        end

        it "should have scale of 2" do
          @gold_fish.model.average_weight.scale.should == 2
        end

        it 'should allow up to 2 digits before the decimal' do
          @gold_fish.average_weight = 0
          @gold_fish.should be_valid

          @gold_fish.average_weight = 99
          @gold_fish.should be_valid

          @gold_fish.average_weight = -99
          @gold_fish.should be_valid

          @gold_fish.average_weight = 100
          @gold_fish.should_not be_valid

          @gold_fish.average_weight = -100
          @gold_fish.should_not be_valid
        end

        it 'should allow 2 digits after the decimal' do
          @gold_fish.average_weight = 99.99
          @gold_fish.should be_valid

          @gold_fish.average_weight = -99.99
          @gold_fish.should be_valid

          @gold_fish.average_weight = 99.999
          @gold_fish.should_not be_valid

          @gold_fish.average_weight = -99.999
          @gold_fish.should_not be_valid
        end
      end

      describe 'with a precision of 2 and a scale of 2' do
        before :all do
          class ::SilverFish < Fish
            property :average_weight, Float, :precision => 2, :scale => 2
          end
        end

        before do
          @silver_fish = SilverFish.new
        end

        it 'should allow a 0 before the decimal' do
          @silver_fish.average_weight = 0
          @silver_fish.should be_valid

          @silver_fish.average_weight = 0.1
          @silver_fish.should be_valid

          @silver_fish.average_weight = -0.1
          @silver_fish.should be_valid

          @silver_fish.average_weight = 1
          @silver_fish.should_not be_valid

          @silver_fish.average_weight = -1
          @silver_fish.should_not be_valid
        end

        it 'should allow 2 digits after the decimal' do
          @silver_fish.average_weight = 0.99
          @silver_fish.should be_valid

          @silver_fish.average_weight = -0.99
          @silver_fish.should be_valid

          @silver_fish.average_weight = 0.999
          @silver_fish.should_not be_valid

          @silver_fish.average_weight = -0.999
          @silver_fish.should_not be_valid
        end
      end
    end
  end
end
