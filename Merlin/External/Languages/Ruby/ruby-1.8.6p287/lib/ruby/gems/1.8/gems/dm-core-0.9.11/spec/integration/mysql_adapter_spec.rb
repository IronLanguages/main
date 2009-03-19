require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

if HAS_MYSQL
  describe DataMapper::Adapters::MysqlAdapter do
    before :all do
      @adapter = repository(:mysql).adapter
    end

    before :all do
      class ::Sputnik
        include DataMapper::Resource

        property :id, Serial
        property :name, DM::Text
        property :object, Object

        auto_migrate!(:mysql)
      end
    end

    it "should handle Object type" do
      time = Time.now
      repository(:mysql) do
        Sputnik.create(:name => "Sputnik", :object => time)
        Sputnik.first.object.should == time
      end
    end

    describe "auto migrating" do
      it "#upgrade_model should work" do
        @adapter.destroy_model_storage(repository(:mysql), Sputnik)
        @adapter.storage_exists?("sputniks").should == false
        Sputnik.auto_migrate!(:mysql)
        @adapter.storage_exists?("sputniks").should == true
        @adapter.field_exists?("sputniks", "new_prop").should == false
        Sputnik.property :new_prop, Integer
        Sputnik.auto_upgrade!(:mysql)
        @adapter.field_exists?("sputniks", "new_prop").should == true
      end
    end

    describe "querying metadata" do
      it "#storage_exists? should return true for tables that exist" do
        @adapter.storage_exists?("sputniks").should == true
      end

      it "#storage_exists? should return false for tables that don't exist" do
        @adapter.storage_exists?("space turds").should == false
      end

      it "#field_exists? should return true for columns that exist" do
        @adapter.field_exists?("sputniks", "name").should == true
      end

      it "#storage_exists? should return false for tables that don't exist" do
        @adapter.field_exists?("sputniks", "plur").should == false
      end
    end

    describe "handling transactions" do
      before do
        @transaction = DataMapper::Transaction.new(@adapter)
      end

      it "should rollback changes when #rollback_transaction is called" do
        repository(:mysql) do
          @transaction.commit do |trans|
            Sputnik.create(:name => 'my pretty sputnik')
            trans.rollback
          end
          Sputnik.all(:name => 'my pretty sputnik').should be_empty
        end
      end

      it "should commit changes when #commit_transaction is called" do
        repository(:mysql) do
          @transaction.commit do
            Sputnik.create(:name => 'my pretty sputnik')
          end
          Sputnik.all(:name => 'my pretty sputnik').size.should == 1
        end
      end
    end
  end
end
