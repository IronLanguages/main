require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

require __dir__.parent.parent + 'spec_helper'
require __dir__ + 'spec_helper'

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  #
  # Especially stupid example since Hg adds local repository revision
  # to each new commit, but lets roll on with this SCM-ish classes and
  # still show how Integer type values are validated for presence
  #
  class CpuConsumption
    #
    # Behaviors
    #

    include DataMapper::Resource

    #
    # Properties
    #

    property :id,      Integer, :serial          => true
    property :percent, Float,   :auto_validation => false

    #
    # Validations
    #

    validates_present :percent
  end
  CpuConsumption.auto_migrate!


  describe CpuConsumption do
    before :each do
      @metric = CpuConsumption.new(:percent => 20.0)
      @metric.should be_valid
    end

    describe "with percentage = 0.0" do
      before(:each) do
        @metric.percent = 0.0
      end

      it "IS valid" do
        # yes, presence validator does not care
        @metric.should be_valid
      end
    end # describe "with percentage = 0.0"



    describe "with percentage = 0" do
      before(:each) do
        @metric.percent = 0
      end

      it "IS valid" do
        # yes, presence validator does not care
        @metric.should be_valid
      end
    end # describe "with percentage = 0"



    describe "with percentage = 100" do
      before(:each) do
        @metric.percent = 100
      end

      it "IS valid" do
        @metric.should be_valid
      end
    end # describe "with percentage = 100"


    describe "with percentage = 100.0" do
      before(:each) do
        @metric.percent = 100.0
      end

      it "IS valid" do
        @metric.should be_valid
      end
    end # describe "with percentage = 100.0"


    describe "with percentage = -1100" do
      before(:each) do
        # presence validator does not care
        @metric.percent = -1100
      end

      it "IS valid" do
        @metric.should be_valid
      end
    end # describe "with percentage = -1100"


    describe "with percentage = -1100.5" do
      before(:each) do
        # presence validator does not care
        @metric.percent = -1100.5
      end

      it "IS valid" do
        @metric.should be_valid
      end
    end # describe "with percentage = -1100.5"


    describe "with percentage = nil" do
      before(:each) do
        @metric.percent = nil
      end

      it "is NOT valid" do
        # nil = missing for float value
        # and CpuConsumption only has default validation context
        @metric.should_not be_valid

        # sanity check
        @metric.percent = 100
        @metric.should be_valid
      end
    end # describe "with percentage = nil"
  end # describe CpuConsumption
end # if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
