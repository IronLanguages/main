require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

if HAS_SQLITE3
  describe DataMapper::Adapters::Sqlite3Adapter do
    before :all do
      @adapter = repository(:sqlite3).adapter
    end

    describe "auto migrating" do
      before :all do
        class ::Sputnik
          include DataMapper::Resource

          property :id, Serial
          property :name, DM::Text
        end
      end

      it "#upgrade_model should work" do
        @adapter.destroy_model_storage(repository(:sqlite3), Sputnik)
        @adapter.storage_exists?("sputniks").should == false
        Sputnik.auto_migrate!(:sqlite3)
        @adapter.storage_exists?("sputniks").should == true
        @adapter.field_exists?("sputniks", "new_prop").should == false
        Sputnik.property :new_prop, Integer
        Sputnik.auto_upgrade!(:sqlite3)
        @adapter.field_exists?("sputniks", "new_prop").should == true
      end
    end

    describe "querying metadata" do
      before :all do
        class ::Sputnik
          include DataMapper::Resource

          property :id, Serial
          property :name, DM::Text
        end
      end

      before do
        Sputnik.auto_migrate!(:sqlite3)
      end

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

    describe "database file handling" do
      it "should preserve the file path for file-based databases" do
        file = 'newfile.db'
        DataMapper.setup(:sqlite3file, "sqlite3:#{file}")
        adapter = repository(:sqlite3file).adapter
        adapter.uri.path.should == file
      end

      it "should have a path of just :memory: when using memory databases" do
        DataMapper.setup(:sqlite3memory, "sqlite3::memory:")
        adapter = repository(:sqlite3memory).adapter
        adapter.uri.path.should == ':memory:'
      end
    end

    describe "handling transactions" do
      before :all do
        class ::Sputnik
          include DataMapper::Resource

          property :id, Serial
          property :name, DM::Text
        end
      end

      before do
        Sputnik.auto_migrate!(:sqlite3)

        @transaction = DataMapper::Transaction.new(@adapter)
      end

      it "should rollback changes when #rollback_transaction is called" do
        @transaction.commit do |transaction|
          @adapter.execute("INSERT INTO sputniks (name) VALUES ('my pretty sputnik')")
          transaction.rollback
        end
        @adapter.query("SELECT * FROM sputniks WHERE name = 'my pretty sputnik'").empty?.should == true
      end

      it "should commit changes when #commit_transaction is called" do
        @transaction.commit do
          @adapter.execute("INSERT INTO sputniks (name) VALUES ('my pretty sputnik')")
        end
        @adapter.query("SELECT * FROM sputniks WHERE name = 'my pretty sputnik'").size.should == 1
      end
    end

    describe "reading & writing a database" do
      before :all do
        class ::User
          include DataMapper::Resource

          property :id, Serial
          property :name, DM::Text
        end
      end

      before do
        User.auto_migrate!(:sqlite3)

        @adapter.execute("INSERT INTO users (name) VALUES ('Paul')")
      end

      it 'should be able to #execute an arbitrary query' do
        result = @adapter.execute("INSERT INTO users (name) VALUES ('Sam')")

        result.affected_rows.should == 1
      end

      it 'should be able to #query' do
        result = @adapter.query("SELECT * FROM users")

        result.should be_kind_of(Array)
        row = result.first
        row.should be_kind_of(Struct)
        row.members.map { |m| m.to_s }.should == %w{id name}

        row.id.should == 1
        row.name.should == 'Paul'
      end

      it 'should return an empty array if #query found no rows' do
        @adapter.execute("DELETE FROM users")

        result = nil
        lambda { result = @adapter.query("SELECT * FROM users") }.should_not raise_error

        result.should be_kind_of(Array)
        result.size.should == 0
      end
    end

    describe "CRUD for serial Key" do
      before :all do
        class ::VideoGame
          include DataMapper::Resource

          property :id, Serial
          property :name, String
          property :object, Object
          property :notes, Text
        end
      end

      before do
        VideoGame.auto_migrate!(:sqlite3)
      end

      it 'should be able to create a record' do
        time = Time.now
        game = repository(:sqlite3) do
          game = VideoGame.new(:name => 'System Shock', :object => time, :notes => "Test")
          game.save
          game.should_not be_a_new_record
          game.should_not be_dirty
          game
        end
        repository(:sqlite3) do
          saved = VideoGame.first(:name => 'System Shock')
          saved.id.should == game.id
          saved.notes.should == game.notes
          saved.object.should == time
        end
      end

      it 'should be able to read a record' do
        name = 'Wing Commander: Privateer'
        id = @adapter.execute('INSERT INTO "video_games" ("name") VALUES (?)', name).insert_id

        game = repository(:sqlite3) do
          VideoGame.get(id)
        end

        game.name.should == name
        game.should_not be_dirty
        game.should_not be_a_new_record
      end

      it 'should be able to update a record' do
        name = 'Resistance: Fall of Mon'
        id = @adapter.execute('INSERT INTO "video_games" ("name") VALUES (?)', name).insert_id

        game = repository(:sqlite3) do
          VideoGame.get(id)
        end

        game.name = game.name.sub(/Mon/, 'Man')

        game.should_not be_a_new_record
        game.should be_dirty

        repository(:sqlite3) do
          game.save
        end

        game.should_not be_dirty

        clone = repository(:sqlite3) do
          VideoGame.get(id)
        end

        clone.name.should == game.name
      end

      it 'should be able to delete a record' do
        name = 'Zelda'
        id = @adapter.execute('INSERT INTO "video_games" ("name") VALUES (?)', name).insert_id

        game = repository(:sqlite3) do
          VideoGame.get(id)
        end

        game.name.should == name

        repository(:sqlite3) do
          game.destroy.should be_true
        end
        game.should be_a_new_record
        game.should be_dirty
      end

      it 'should respond to Resource#get' do
        name = 'Contra'
        id = @adapter.execute('INSERT INTO "video_games" ("name") VALUES (?)', name).insert_id

        contra = repository(:sqlite3) { VideoGame.get(id) }

        contra.should_not be_nil
        contra.should_not be_dirty
        contra.should_not be_a_new_record
        contra.id.should == id
      end
    end

    describe "CRUD for Composite Key" do
      before :all do
        class ::BankCustomer
          include DataMapper::Resource

          property :bank, String, :key => true
          property :account_number, String, :key => true
          property :name, String
        end
      end

      before do
        BankCustomer.auto_migrate!(:sqlite3)
      end

      it 'should be able to create a record' do
        customer = BankCustomer.new(:bank => 'Community Bank', :account_number => '123456', :name => 'David Hasselhoff')
        repository(:sqlite3) do
          customer.save
        end

        customer.should_not be_a_new_record
        customer.should_not be_dirty

        row = @adapter.query('SELECT "bank", "account_number" FROM "bank_customers" WHERE "name" = ?', customer.name).first
        row.bank.should == customer.bank
        row.account_number.should == customer.account_number
      end

      it 'should be able to read a record' do
        bank, account_number, name = 'Chase', '4321', 'Super Wonderful'
        @adapter.execute('INSERT INTO "bank_customers" ("bank", "account_number", "name") VALUES (?, ?, ?)', bank, account_number, name)

        repository(:sqlite3) do
          BankCustomer.get(bank, account_number).name.should == name
        end
      end

      it 'should be able to update a record' do
        bank, account_number, name = 'Wells Fargo', '00101001', 'Spider Pig'
        @adapter.execute('INSERT INTO "bank_customers" ("bank", "account_number", "name") VALUES (?, ?, ?)', bank, account_number, name)

        customer = repository(:sqlite3) do
          BankCustomer.get(bank, account_number)
        end

        customer.name = 'Bat-Pig'

        customer.should_not be_a_new_record
        customer.should be_dirty

        repository(:sqlite3) do
          customer.save
        end

        customer.should_not be_dirty

        clone = repository(:sqlite3) do
          BankCustomer.get(bank, account_number)
        end

        clone.name.should == customer.name
      end

      it 'should be able to delete a record' do
        bank, account_number, name = 'Megacorp', 'ABC', 'Flash Gordon'
        @adapter.execute('INSERT INTO "bank_customers" ("bank", "account_number", "name") VALUES (?, ?, ?)', bank, account_number, name)

        customer = repository(:sqlite3) do
          BankCustomer.get(bank, account_number)
        end

        customer.name.should == name

        repository(:sqlite3) do
          customer.destroy.should be_true
        end

        customer.should be_a_new_record
        customer.should be_dirty
      end

      it 'should respond to Resource#get' do
        bank, account_number, name = 'Conchords', '1100101', 'Robo Boogie'
        @adapter.execute('INSERT INTO "bank_customers" ("bank", "account_number", "name") VALUES (?, ?, ?)', bank, account_number, name)

        robots = repository(:sqlite3) { BankCustomer.get(bank, account_number) }

        robots.should_not be_nil
        robots.should_not be_dirty
        robots.should_not be_a_new_record
        robots.bank.should == bank
        robots.account_number.should == account_number
      end
    end
  end
end
