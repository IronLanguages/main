require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

require __dir__.parent.parent + 'spec_helper'
require __dir__ + 'spec_helper'

describe Country do
  before(:each) do
    @country = Country.new(:name => "Italy", :area => "301318")
  end

  describe "with area as integer" do
    before(:each) do
      # no op in this case
    end

    it "is valid" do
      @country.should be_valid
    end
  end


  describe "with area as integer" do
    before(:each) do
      @country.area = 1603
    end

    it "is valid" do
      @country.should be_valid
    end
  end


  describe "with area as string containing only integers" do
    before(:each) do
      @country.area = "301318"
    end

    it "is valid" do
      @country.should be_valid
    end
  end


  describe "with area as string containing a float" do
    before(:each) do
      @country.area = "301318.6"
    end

    it "IS valid" do
      @country.should be_valid
    end
  end


  describe "with area as string containing random alphanumeric characters" do
    before(:each) do
      @country.area = "area=51"
    end

    it "IS NOT valid" do
      @country.should_not be_valid
    end
  end


  describe "with area as string containing random punctuation characters" do
    before(:each) do
      @country.area = "$$ * $?"
    end

    it "IS NOT valid" do
      @country.should_not be_valid
    end
  end


  describe "with unknown area" do
    before(:each) do
      @country.area = nil
    end

    it "is NOT valid" do
      @country.should_not be_valid
    end

    it "has a meaningful error message on for the property" do
      @country.valid?
      @country.errors.on(:area).should include("Please use integers to specify area")
    end
  end
end
