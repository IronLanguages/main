require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

# global first, then local to length validators
require __dir__.parent.parent + "spec_helper"
require __dir__ + 'spec_helper'

class MotorLaunch
  validators.clear!
  validates_length :name, :in => (3..5)
end


describe MotorLaunch do
  before :each do
    @launch = MotorLaunch.new
  end

  describe "with a value that is out of range bounds (too long)" do
    before :each do
      @launch.name = 'a'
      @launch.valid?
    end

    it "is invalid" do
      @launch.should_not be_valid
    end
    it "includes range bounds and field name in error message" do
      @launch.errors.on(:name).should include('Name must be between 3 and 5 characters long')
    end
  end


  describe "with a value that is out of range bounds (too short)" do
    before :each do
      @launch.name = 'L'
      @launch.valid?
    end

    it "is invalid" do
      @launch.should_not be_valid
    end
    it "includes range bounds and field name in error message" do
      @launch.errors.on(:name).should include('Name must be between 3 and 5 characters long')
    end
  end


  # arguable but reasonable for 80% of cases
  # to treat nil as a 0 lengh value
  # reported in
  # http://datamapper.lighthouseapp.com/projects/20609/tickets/646
  describe "with a nil value" do
    before :each do
      @launch.name = nil
      @launch.valid?
    end

    it "is invalid" do
      @launch.should_not be_valid
    end
    it "includes range bounds and field name in error message" do
      @launch.errors.on(:name).should include('Name must be between 3 and 5 characters long')
    end
  end



  describe "with a value that is within range bounds" do
    before :each do
      @launch.name = 'Lisp'
      @launch.valid?
    end

    it "is valid" do
      @launch.should be_valid
    end
    it "has blank error message" do
      @launch.errors.on(:name).should be_blank
    end
  end



  it "aliases :within for :in" do
    class ::MotorLaunch
      validators.clear!
      validates_length :name, :within => (3..5)
    end

    launch = MotorLaunch.new
    launch.name = 'Ride'
    launch.valid?.should == true
  end
end
