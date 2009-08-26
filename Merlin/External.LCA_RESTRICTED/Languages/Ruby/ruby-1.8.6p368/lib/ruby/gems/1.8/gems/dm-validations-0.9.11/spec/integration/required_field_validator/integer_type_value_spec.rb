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
  class HgCommit < ScmOperation
    #
    # Properties
    #

    property :local_repo_revision_num, Integer, :auto_validation => false

    #
    # Validations
    #

    validates_present :local_repo_revision_num
  end
  HgCommit.auto_migrate!


  describe HgCommit do
    before :each do
      @operation = HgCommit.new(:local_repo_revision_num => 90, :name => "ci")
      @operation.should be_valid
    end

    describe "with local revision number = 0" do
      before(:each) do
        @operation.local_repo_revision_num = 0
      end

      it "IS valid" do
        # yes, presence validator does not care
        @operation.should be_valid
      end
    end # describe "with local revision number = 0"



    describe "with local revision number = 100" do
      before(:each) do
        @operation.local_repo_revision_num = 100
      end

      it "IS valid" do
        @operation.should be_valid
      end
    end # describe "with local revision number = 100"


    describe "with local revision number = 100.0 (float!)" do
      before(:each) do
        @operation.local_repo_revision_num = 100.0
      end

      it "IS valid" do
        @operation.should be_valid
      end
    end # describe "with local revision number = 100.0 (float!)"


    describe "with local revision number = -1100" do
      before(:each) do
        # presence validator does not care
        @operation.local_repo_revision_num = -1100
      end

      it "IS valid" do
        @operation.should be_valid
      end
    end # describe "with local revision number = -1100"


    describe "with local revision number = nil" do
      before(:each) do
        @operation.local_repo_revision_num = nil
      end

      it "is NOT valid" do
        # nil = missing for integer value
        # and HgCommit only has default validation context
        @operation.should_not be_valid

        # sanity check
        @operation.local_repo_revision_num = 100
        @operation.should be_valid
      end
    end # describe "with local revision number = nil"
  end # describe HgCommit
end # if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
