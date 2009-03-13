require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

# if RUBY_VERSION >= '1.9.0'
#   require 'csv'
# else
#   gem 'fastercsv', '~>1.4.0'
#   require 'fastercsv'
# end

if ADAPTER
  module ::TypeTests
    class Impostor < DataMapper::Type
      primitive String
    end

    class Coconut
      include DataMapper::Resource

      storage_names[ADAPTER] = 'coconuts'

      def self.default_repository_name
        ADAPTER
      end

      property :id, Serial
      property :faked, Impostor
      property :active, Boolean
      property :note, Text
    end
  end

  class ::Lemon
    include DataMapper::Resource

    def self.default_repository_name
      ADAPTER
    end

    property :id, Serial
    property :color, String
    property :deleted_at, DataMapper::Types::ParanoidDateTime
  end

  class ::Lime
    include DataMapper::Resource

    def self.default_repository_name
      ADAPTER
    end

    property :id, Serial
    property :color, String
    property :deleted_at, DataMapper::Types::ParanoidBoolean
  end

  describe DataMapper::Type, "with #{ADAPTER}" do
    before do
      TypeTests::Coconut.auto_migrate!(ADAPTER)

      @document = <<-EOS.margin
        NAME, RATING, CONVENIENCE
        Freebird's, 3, 3
        Whataburger, 1, 5
        Jimmy John's, 3, 4
        Mignon, 5, 2
        Fuzi Yao's, 5, 1
        Blue Goose, 5, 1
      EOS

      @stuff = YAML::dump({ 'Happy Cow!' => true, 'Sad Cow!' => false })

      @active = true
      @note = "This is a note on our ol' guy bob"
    end

    it "should instantiate an object with custom types" do
      coconut = TypeTests::Coconut.new(:faked => 'bob', :active => @active, :note => @note)
      coconut.faked.should == 'bob'
      coconut.active.should be_a_kind_of(TrueClass)
      coconut.note.should be_a_kind_of(String)
    end

    it "should CRUD an object with custom types" do
      repository(ADAPTER) do
        coconut = TypeTests::Coconut.new(:faked => 'bob', :active => @active, :note => @note)
        coconut.save.should be_true
        coconut.id.should_not be_nil

        fred = TypeTests::Coconut.get!(coconut.id)
        fred.faked.should == 'bob'
        fred.active.should be_a_kind_of(TrueClass)
        fred.note.should be_a_kind_of(String)

        note = "Seems like bob is just mockin' around"
        fred.note = note

        fred.save.should be_true

        active = false
        fred.active = active

        fred.save.should be_true

        # Can't call coconut.reload since coconut.collection isn't setup.
        mac = TypeTests::Coconut.get!(fred.id)
        mac.active.should == active
        mac.note.should == note
      end
    end

    it "should respect paranoia with a datetime" do
      Lemon.auto_migrate!(ADAPTER)

      lemon = nil

      repository(ADAPTER) do |repository|
        lemon = Lemon.new
        lemon.color = 'green'

        lemon.save
        lemon.destroy

        lemon.deleted_at.should be_kind_of(DateTime)
      end

      repository(ADAPTER) do |repository|
        Lemon.all.should be_empty
        Lemon.get(lemon.id).should be_nil
      end
    end

    it "should provide access to paranoid items with DateTime" do
      Lemon.auto_migrate!(ADAPTER)

      lemon = nil

      repository(ADAPTER) do |repository|
        %w(red green yellow blue).each do |color|
          Lemon.create(:color => color)
        end

        Lemon.all.size.should == 4
        Lemon.first.destroy
        Lemon.all.size.should == 3
        Lemon.with_deleted{Lemon.all.size.should == 1}
      end
    end

    it "should set paranoid datetime to a date time" do
      tmp = (DateTime.now - 0.5)
      dt = DateTime.now
      DateTime.stub!(:now).and_return(tmp)

      repository(ADAPTER) do |repository|
        lemon = Lemon.new
        lemon.color = 'green'
        lemon.save
        lemon.destroy
        lemon.deleted_at.should == tmp
      end
    end

    it "should respect paranoia with a boolean" do
      Lime.auto_migrate!(ADAPTER)

      lime = nil

      repository(ADAPTER) do |repository|
        lime = Lime.new
        lime.color = 'green'

        lime.save
        lime.destroy

        lime.deleted_at.should be_kind_of(TrueClass)
      end

      repository(ADAPTER) do |repository|
        Lime.all.should be_empty
        Lime.get(lime.id).should be_nil
      end
    end

    it "should provide access to paranoid items with Boolean" do
      Lime.auto_migrate!(ADAPTER)

      lemon = nil

      repository(ADAPTER) do |repository|
        %w(red green yellow blue).each do |color|
          Lime.create(:color => color)
        end

        Lime.all.size.should == 4
        Lime.first.destroy
        Lime.all.size.should == 3
        Lime.with_deleted{Lime.all.size.should == 1}
      end
    end

    describe "paranoid types across repositories" do
      before(:all) do
        DataMapper::Repository.adapters[:alternate_paranoid] = repository(ADAPTER).adapter.dup

        Object.send(:remove_const, :Orange) if defined?(Orange)
        class ::Orange
          include DataMapper::Resource

          def self.default_repository_name
            ADAPTER
          end

          property :id, Serial
          property :color, String

          repository(:alternate_paranoid) do
            property :deleted,    DataMapper::Types::ParanoidBoolean
            property :deleted_at, DataMapper::Types::ParanoidDateTime
          end
        end

        repository(:alternate_paranoid){Orange.auto_migrate!}
      end

      before(:each) do
        %w(red orange blue green).each{|color| o = Orange.create(:color => color)}
      end

      after(:each) do
        Orange.repository.adapter.execute("DELETE FROM oranges")
      end

      it "should setup the correct objects for the spec" do
        repository(:alternate_paranoid){Orange.all.should have(4).items}
      end

      it "should allow access the the default repository" do
        Orange.all.should have(4).items
      end

      it "should mark the objects as deleted in the alternate_paranoid repository" do
        repository(:alternate_paranoid) do
          Orange.first.destroy
          Orange.all.should have(3).items
          Orange.find_by_sql("SELECT * FROM oranges").should have(4).items
        end
      end

      it "should mark the objects as deleted in the alternate_paranoid repository but ignore it in the #{ADAPTER} repository" do
        repository(:alternate_paranoid) do
          Orange.first.destroy
        end
        Orange.all.should have(4).items
      end

      it "should raise an error when trying to destroy from a repository that is not paranoid" do
        lambda do
          Orange.first.destroy
        end.should raise_error(ArgumentError)
      end

      it "should set all paranoid attributes on delete" do
        repository(:alternate_paranoid) do
          orange = Orange.first
          orange.deleted.should be_false
          orange.deleted_at.should be_nil
          orange.destroy

          orange.deleted.should be_true
          orange.deleted_at.should be_a_kind_of(DateTime)
        end
      end
    end
  end
end
