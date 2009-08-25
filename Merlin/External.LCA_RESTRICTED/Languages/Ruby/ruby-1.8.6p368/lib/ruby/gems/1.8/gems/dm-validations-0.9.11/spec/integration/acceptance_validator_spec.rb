require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Validate::AcceptanceValidator do
  describe "with standard options" do
    before(:all) do
      class ::SkimBat
        include DataMapper::Resource
        property :id,        Integer, :serial => true
        property :sailyness, Boolean
        validates_is_accepted :sailyness
      end
      @s = SkimBat.new
    end

    it "should validate if a resource instance has been accepted" do
      @s.sailyness = "1"
      @s.valid?.should == true
    end

    it "should not validate if a resource instance has not been accepted" do
      @s.sailyness = "0"
      @s.valid?.should == false
    end

    it "should allow nil acceptance" do
      @s.sailyness = nil
      @s.valid?.should == true
    end

    it "should add the default message when invalid" do
      @s.sailyness = "0"
      @s.valid?.should == false
      @s.errors.on(:sailyness).should include('Sailyness is not accepted')
    end
  end
  describe "with :allow_nil => false" do
    before(:all) do
      SkimBat.class_eval do
        validators.clear!
        validates_is_accepted :sailyness, :allow_nil => false
      end
      @s = SkimBat.new
    end

    it "should not allow nil acceptance" do
      @s.sailyness = nil
      @s.valid?.should == false
    end
  end

  describe "with custom :accept" do
    before(:all) do
      SkimBat.class_eval do
        validators.clear!
        validates_is_accepted :sailyness, :accept => true
      end
      @s = SkimBat.new
    end

    it "should validate if a resource instance has been accepted" do
      @s.sailyness = "true"
      @s.valid?.should == true
    end

    it "should not validate if a resource instance has not been accepted" do
      @s.sailyness = "false"
      @s.valid?.should == false
    end
  end

  describe "with custom message" do
    before(:all) do
      SkimBat.class_eval do
        validators.clear!
        validates_is_accepted :sailyness, :message => "hehu!"
      end
      @s = SkimBat.new
    end

    it "should append the custom message when invalid" do
      @s.sailyness = "0"
      @s.valid?.should == false
      @s.errors.on(:sailyness).should include('hehu!')
    end
  end
end
