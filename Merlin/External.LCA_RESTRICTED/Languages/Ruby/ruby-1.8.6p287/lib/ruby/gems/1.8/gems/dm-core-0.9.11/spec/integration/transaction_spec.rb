require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

# transaction capable adapters
ADAPTERS = []
ADAPTERS << :postgres if HAS_POSTGRES
ADAPTERS << :mysql    if HAS_MYSQL
ADAPTERS << :sqlite3  if HAS_SQLITE3

if ADAPTERS.any?
  class Sputnik
    include DataMapper::Resource

    property :id, Serial
    property :name, DM::Text
  end

  describe DataMapper::Transaction do
    before :all do
      @repositories = []

      ADAPTERS.each do |name|
        @repositories << repository(name)
      end
    end

    before :each do
      ADAPTERS.each do |name|
        Sputnik.auto_migrate!(name)
      end
    end

    it "should commit changes to all involved adapters on a two phase commit" do
      DataMapper::Transaction.new(*@repositories) do
        ADAPTERS.each do |name|
          repository(name) { Sputnik.create(:name => 'hepp') }
        end
      end

      ADAPTERS.each do |name|
        repository(name) { Sputnik.all.size.should == 1 }
      end
    end

    it "should not commit any changes if the block raises an exception" do
      lambda do
        DataMapper::Transaction.new(*@repositories) do
          ADAPTERS.each do |name|
            repository(name) { Sputnik.create(:name => 'hepp') }
          end
          raise "plur"
        end
      end.should raise_error(Exception, /plur/)

      ADAPTERS.each do |name|
        repository(name) { Sputnik.all.size.should == 0 }
      end
    end

    it "should not commit any changes if any of the adapters doesnt prepare properly" do
      lambda do
        DataMapper::Transaction.new(*@repositories) do |transaction|
          ADAPTERS.each do |name|
            repository(name) { Sputnik.create(:name => 'hepp') }
          end

          transaction.primitive_for(@repositories.last.adapter).should_receive(:prepare).and_throw(Exception.new("I am the famous test exception"))
        end
      end.should raise_error(Exception, /I am the famous test exception/)

      ADAPTERS.each do |name|
        repository(name) { Sputnik.all.size.should == 0 }
      end
    end
  end
end
