require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

require __dir__.parent.parent + 'spec_helper'
require __dir__ + 'spec_helper'

describe City do
  before(:each) do
    @city = City.new(:name => "Tokyo", :founded_in => 1603)
  end

  describe "with foundation year as integer" do
    before(:each) do
      # no op in this case
    end

    it "is valid" do
      @city.should be_valid
    end
  end


  describe "with foundation year as integer" do
    before(:each) do
      @city.founded_in = 1603
    end

    it "is valid" do
      @city.should be_valid
    end
  end


  describe "with foundation year as string containing only integers" do
    before(:each) do
      @city.founded_in = "1603"
    end

    it "is valid" do
      @city.should be_valid
    end
  end


  describe "with foundation year as string containing a float" do
    before(:each) do
      @city.founded_in = "1603.6"
    end

    it "is valid" do
      @city.should be_valid
    end
  end


  describe "with foundation year as string containing random alphanumeric characters" do
    before(:each) do
      @city.founded_in = "founded-in=1603"
    end

    it "is set to nil" do
      @city.founded_in.should be(nil)
    end

    it "IS NOT valid" do
      @city.should_not be_valid
    end
  end


  describe "with foundation year as string containing random punctuation characters" do
    before(:each) do
      @city.founded_in = "$$ * $?"
    end

    it "is set to nil" do
      @city.founded_in.should be(nil)
    end

    it "IS NOT valid" do
      @city.should_not be_valid
    end
  end


  describe "with unknown foundation date" do
    before(:each) do
      @city.founded_in = nil
    end

    it "is NOT valid" do
      @city.should_not be_valid
    end

    it "has a meaningful error message on for the property" do
      @city.valid?
      @city.errors.on(:founded_in).should include("Foundation year must be an integer")
    end
  end
end
