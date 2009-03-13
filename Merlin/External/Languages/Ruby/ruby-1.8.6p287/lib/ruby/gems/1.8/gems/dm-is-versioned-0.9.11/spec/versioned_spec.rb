require File.dirname(__FILE__) + "/spec_helper"

class Story
  include DataMapper::Resource

  property :id, Integer, :serial => true
  property :title, String
  property :updated_at, DateTime

  before :save do
    # For the sake of testing, make sure the updated_at is always unique
    time = self.updated_at ? self.updated_at + 1 : Time.now
    self.updated_at = time if self.dirty?
  end

  is_versioned :on => :updated_at

end

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  describe 'DataMapper' do
    describe "#auto_migrate!" do
      before do
        # I *think* AutoMigrator breaks toplevel auto_migrate
        # into two calls to reduce redundancy, so auto_migrate!
        # never actually gets called on decendents after a
        # DataMapper.auto_migrate! call.
        Story::Version.should_receive(:auto_migrate_down!)
        Story::Version.should_receive(:auto_migrate_up!)
      end
      it "should get called on a versioned inner class" do
        DataMapper.auto_migrate!
      end
    end # #auto_migrate!

    describe "#auto_upgrade!" do
      before do
        # AutoMigrator collects all descendents, triggering once
        # but triggers a second time after Story gets upgraded.
        # Since it's non-destructive, shouldn't matter that it
        # gets called twice though.
        Story::Version.should_receive(:auto_upgrade!).twice
      end
      it "should get called on a versioned inner class" do
        DataMapper.auto_upgrade!
      end
    end # #auto_upgrade!
  end

  describe 'DataMapper::Is::Versioned' do
    describe "inner class" do
      it "should be present" do
        Story::Version.should be_a_kind_of(Class)
      end

      it "should have a default storage name" do
        Story::Version.storage_name.should == "story_versions"
      end

      it "should have its parent's properties" do
        Story.properties.each do |property|
          Story::Version.properties.should have_property(property.name)
        end
      end
    end # inner class

    describe "#auto_migrate!" do
      before do
        Story::Version.should_receive(:auto_migrate!)
      end
      it "should get called on the inner class" do
        Story.auto_migrate!
      end
    end # #auto_migrate!

    describe "#auto_upgrade!" do
      before do
        Story::Version.should_receive(:auto_upgrade!)
      end
      it "should get called on the inner class" do
        Story.auto_upgrade!
      end
    end # #auto_upgrade!

    describe "#create" do
      before do
        Story.auto_migrate!
        Story.create(:title => "A Very Interesting Article")
      end
      it "should not create a versioned copy" do
        Story::Version.all.size.should == 0
      end
    end # #create

    describe "#save" do
      before do
        Story.auto_migrate!
      end

      describe "(with new resource)" do
        before do
          @story = Story.new(:title => "A Story")
          @story.save
        end
        it "should not create a versioned copy" do
          Story::Version.all.size.should == 0
        end
      end

      describe "(with a clean existing resource)" do
        before do
          @story = Story.create(:title => "A Story")
          @story.save
        end

        it "should not create a versioned copy" do
          Story::Version.all.size.should == 0
        end
      end

      describe "(with a dirty existing resource)" do
        before do
          @story = Story.create(:title => "A Story")
          @story.title = "An Inner Update"
          @story.title = "An Updated Story"
          @story.save
        end

        it "should create a versioned copy" do
          Story::Version.all.size.should == 1
        end

        it "should not have the same value for the versioned field" do
          @story.updated_at.should_not == Story::Version.first.updated_at
        end

        it "should save the original value, not the inner update" do
          # changes to the story between saves shouldn't be updated.
          @story.versions.last.title.should == "A Story"
        end
      end

    end # #save

    describe "#pending_version_attributes" do
      before do
        @story = Story.create(:title => "A Story")
      end

      it "should be updated when a property changes" do
        @story.title = "A New Title"
        @story.pending_version_attributes[:title].should == "A Story"
      end

      it "should be cleared when a resource is saved" do
        @story.title = "A New Title"
        @story.save
        @story.pending_version_attributes.should be_empty
      end
    end # #pending_version_attributes

    describe "#versions" do
      before do
        Story.auto_migrate!
        @story = Story.create(:title => "A Story")
      end

      it "should return an empty array when there are no versions" do
        @story.versions.should == []
      end

      it "should return a collection when there are versions" do
        @story.versions.should == Story::Version.all(:id => @story.id)
      end

      it "should not return another object's versions" do
        @story2 = Story.create(:title => "A Different Story")
        @story2.title = "A Different Title"
        @story2.save
        @story.versions.should == Story::Version.all(:id => @story.id)
      end
    end # #versions
  end
end
