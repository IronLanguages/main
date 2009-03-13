require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Validate::MethodValidator do
  before(:all) do
    class ::Ship
      include DataMapper::Resource
      property :id, Integer, :key => true
      property :name, String
      property :size, String

      validates_with_method :fail_validation, :when => [:testing_failure]
      validates_with_method :pass_validation, :when => [:testing_success]
      validates_with_method :first_validation, :second_validation, :when => [:multiple_validations]
      validates_with_method :name, :method => :name_validation, :when => [:testing_name_validation]

      def fail_validation
        return false, 'Validation failed'
      end

      def pass_validation
        return true
      end

      def first_validation
        return true
      end

      def second_validation
        return false, 'Second Validation was false'
      end

      def name_validation
        return false, 'Name is invalid'
      end
    end
  end

  it "should validate via a method on the resource" do
    Ship.new.valid_for_testing_failure?.should == false
    Ship.new.valid_for_testing_success?.should == true
    ship = Ship.new
    ship.valid_for_testing_failure?.should == false
    ship.errors.on(:fail_validation).should include('Validation failed')
  end

  it "should run multiple validation methods" do
    ship = Ship.new
    ship.valid_for_multiple_validations?.should == false
    ship.errors.on(:second_validation).should include('Second Validation was false')
  end

  it "should validate via a method and add error to field" do
    ship = Ship.new
    ship.should_not be_valid_for_testing_name_validation
    ship.errors.on(:name).should include('Name is invalid')
  end
end
