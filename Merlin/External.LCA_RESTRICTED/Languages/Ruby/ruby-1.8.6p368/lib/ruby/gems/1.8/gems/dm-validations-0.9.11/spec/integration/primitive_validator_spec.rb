require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

class Monica # :nodoc:
  include DataMapper::Resource
  property :id, Integer, :serial => true
  property :birth_date, Date, :auto_validation => false
  property :happy, Boolean
  validates_is_primitive :birth_date
end

describe DataMapper::Validate::PrimitiveValidator do
  it "should validate a property to check for the type" do
    b = Monica.new
    b.should be_valid

    b.birth_date = 'ABC'
    b.should_not be_valid
    b.errors.on(:birth_date).should include('Birth date must be of type Date')
    b.birth_date.should eql('ABC')
    b.birth_date = '2008-01-01'
    b.should be_valid
    b.birth_date.should eql(Date.civil(2008, 1, 1))
  end
  it "should accept FalseClass even when the property type is TrueClass" do
    b = Monica.new
    b.happy = false
    b.valid?.should == true
  end
end
