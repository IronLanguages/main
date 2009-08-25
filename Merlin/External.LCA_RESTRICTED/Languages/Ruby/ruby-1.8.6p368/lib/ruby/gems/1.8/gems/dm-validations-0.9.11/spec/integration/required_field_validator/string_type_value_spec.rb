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

    describe "without explicitly specified committer name" do
      before :each do
        # no specific actions for this case! yay!
      end

      it "is valid for committing (because default value jumps in)" do
        @operation.should be_valid_for_committing
        @operation.should be_valid(:committing)
      end

      it "is not valid in default context" do
        # context here is :default
        @operation.should_not be_valid
      end

      it "has default value set" do
        # this is more of a sanity check since
        # this sort of functionality clearly needs to be
        # tested in
        @operation.committer_name.should == "Just another Ruby hacker"
      end
    end # describe "without explicitly specified committer name"

    describe "WITH explicitly specified committer name" do
      before :each do
        @operation.committer_name = "Core Team Guy"
      end

      it "is valid for committing" do
        @operation.should be_valid_for_committing
        @operation.should be_valid(:committing)
      end

      it "is not valid in default context" do
        @operation.should_not be_valid
        @operation.should_not be_valid(:default)
      end

      it "has value set" do
        # this is more of a sanity check since
        # this sort of functionality clearly needs to be
        # tested in
        @operation.committer_name.should == "Core Team Guy"
      end
    end # describe "with explicitly specified committer name"



    describe "without explicitly specified author name" do
      before :each do
        # no specific actions for this case! yay!
      end

      it "is valid for committing (because default value jumps in)" do
        @operation.should be_valid_for_committing
        @operation.should be_valid(:committing)
      end

      it "is not valid in default context" do
        # context here is :default
        @operation.should_not be_valid
        @operation.should_not be_valid(:default)
      end

      it "has default value set" do
        @operation.author_name.should == "Just another Ruby hacker"
      end
    end # describe "without explicitly specified author name"

    describe "WITH explicitly specified author name" do
      before :each do
        @operation.author_name = "Random contributor"
      end

      it "is valid for committing" do
        @operation.should be_valid_for_committing
      end

      it "is not valid in default context" do
        # context here is :default
        @operation.should_not be_valid
      end

      it "has value set" do
        @operation.author_name.should == "Random contributor"
      end
    end # describe "with explicitly specified author name"

    describe "with empty committer name" do
      before(:each) do
        @operation.committer_name = ""
      end

      it "is NOT valid for committing" do
        # empty string is not considered present for
        # a String value
        @operation.should_not be_valid_for_committing

        # sanity check since this empty vs blank vs nil
        # thing is a shaky ground
        @operation.committer_name = "l33t k0dr"
        @operation.should be_valid_for_committing
      end

      it "IS valid for pushing" do
        @operation.should be_valid_for_pushing
      end

      it "IS valid for pulling" do
        @operation.should be_valid_for_pulling
      end

      it "is not valid in default context" do
        @operation.should_not be_valid
      end
    end # describe "with empty committer field"


    describe "with empty author name" do
      before(:each) do
        @operation.author_name = ""
      end

      it "is NOT valid for committing" do
        # empty string is not considered present for
        # a String value
        @operation.should_not be_valid_for_committing

        # sanity check since this empty vs blank vs nil
        # thing is a shaky ground
        @operation.author_name = "l33t k0dr"
        @operation.should be_valid_for_committing
      end

      it "IS valid for pushing" do
        @operation.should be_valid_for_pushing
      end

      it "IS valid for pulling" do
        @operation.should be_valid_for_pulling
      end

      it "is not valid in default context" do
        @operation.should_not be_valid
      end
    end # describe "with empty author field"
  end # describe GitOperation
end # if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
