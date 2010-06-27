require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

require __dir__.parent.parent + 'spec_helper'
require __dir__ + 'spec_helper'

describe BasketballPlayer do
  before(:each) do
    @mj = BasketballPlayer.new(:name => "Michael Jordan", :height => 198.1, :weight => 97.2)
  end

  describe "with height as float" do
    before(:each) do
      # no op in this case
    end

    it "is valid" do
      @mj.should be_valid
    end
  end


  describe "with height as integer" do
    before(:each) do
      @mj.height = 198
    end

    it "is valid" do
      @mj.should be_valid
    end
  end


  describe "with height as string containing only integers" do
    before(:each) do
      @mj.height = "198"
    end

    it "is valid" do
      @mj.should be_valid
    end
  end


  describe "with height as string containing a float" do
    before(:each) do
      @mj.height = "198.1"
    end

    it "is valid" do
      @mj.should be_valid
    end
  end


  describe "with height as string containing random alphanumeric characters" do
    before(:each) do
      @mj.height = "height=198.1"
    end

    it "is set to 0.0" do
      @mj.height.should == 0.0
    end

    it "IS  valid" do
      # float property is set to 0.0 here
      @mj.should be_valid
    end
  end


  describe "with height as string containing random punctuation characters" do
    before(:each) do
      @mj.height = "$$ * $?"
    end

    it "is set to 0.0" do
      @mj.height.should == 0.0
    end

    it "IS  valid" do
      # float property is set to 0.0 here
      @mj.should be_valid
    end
  end


  describe "with nil height" do
    before(:each) do
      @mj.height = nil
    end

    it "is NOT valid" do
      @mj.should_not be_valid
    end

    it "has a meaningful error message on for the property" do
      @mj.valid?
      @mj.errors.on(:height).should include("Height must be a number")
    end
  end
end
