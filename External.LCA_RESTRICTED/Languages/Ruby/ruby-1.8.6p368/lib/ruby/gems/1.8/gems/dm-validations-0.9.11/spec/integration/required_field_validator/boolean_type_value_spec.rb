require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

require __dir__.parent.parent + 'spec_helper'
require __dir__ + 'spec_helper'

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  # keep in mind any ScmOperation has a default value for brand property
  # so it is used
  describe GitOperation do
    before :each do
      @operation = GitOperation.new(:network_connection => true,
                                    :clean_working_copy => true,
                                    :message            => "I did it! I did it!! Hell yeah!!!")
    end

    describe "without operation name" do
      before(:each) do
        @operation.name = nil
      end
      it_should_behave_like "unnamed SCM operation"
    end



    describe "without network connection" do
      before(:each) do
        # now note that false make sense from readability
        # point of view but is incorrect from validator
        # point of view ;)
        @operation.network_connection = nil
      end

      it "is valid for committing" do
        @operation.should be_valid_for_committing
        @operation.errors.on(:network_connection).should be_blank
      end

      it "is not valid for pushing" do
        @operation.should_not be_valid_for_pushing
        @operation.errors.on(:network_connection).
          first[:pushing].should include("cannot push without network connectivity")
      end

      it "is not valid for pulling" do
        @operation.should_not be_valid_for_pulling
        @operation.errors.on(:network_connection).
          first[:pulling].should include("you must have network connectivity to pull from others")
      end

      it "is not valid in default context" do
        @operation.should_not be_valid
      end
    end # describe "without network connection"

    describe "with a network connection" do
      before(:each) do
        @operation.network_connection = false
      end

      it "is valid for committing" do
        @operation.should be_valid_for_committing
      end

      it "is valid for pushing" do
        @operation.should be_valid_for_pushing
      end

      it "is valid for pulling" do
        @operation.should be_valid_for_pulling
      end

      it "is not valid in default context" do
        @operation.should_not be_valid
      end
    end # describe "with a network connection"


    describe "WITHOUT a clean working copy" do
      before(:each) do
        @operation.clean_working_copy = nil
      end

      it "is valid for committing" do
        @operation.should be_valid_for_committing
      end

      it "is valid for pushing" do
        @operation.should be_valid_for_pushing
      end

      it "is not valid for pulling" do
        @operation.should_not be_valid_for_pulling
      end

      it "is not valid in default context" do
        @operation.should_not be_valid
      end
    end # describe "without network connection"

    describe "with a clean working copy" do
      before(:each) do
        @operation.clean_working_copy = true
      end

      it "is valid for committing" do
        @operation.should be_valid_for_committing
      end

      it "is valid for pushing" do
        @operation.should be_valid_for_pushing
      end

      it "is valid for pulling" do
        @operation.should be_valid_for_pulling
      end

      it "is not valid in default context" do
        @operation.should_not be_valid
      end
    end # describe "with a network connection"
  end # describe GitOperation


  describe SubversionOperation do
    before(:each) do
      @operation = SubversionOperation.new :name    => "ci", :network_connection => true,
                                           :message => "v1.5.8", :clean_working_copy => true
    end

    describe "without operation name" do
      before(:each) do
        @operation.name = nil
      end
      it_should_behave_like "unnamed SCM operation"
    end

    describe "without network connection" do
      before(:each) do
        @operation.network_connection = nil
      end

      it "virtually useless" do
        @operation.should_not be_valid_for_committing
        @operation.should_not be_valid_for_log_viewing
      end
    end # describe "without network connection"
  end
end # if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
