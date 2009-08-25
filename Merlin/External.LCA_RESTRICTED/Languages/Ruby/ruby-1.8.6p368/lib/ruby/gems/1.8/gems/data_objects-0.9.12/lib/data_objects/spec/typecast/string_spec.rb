share_examples_for 'supporting String' do

  include DataObjectsSpecHelpers

  before :all do
    setup_test_environment
  end

  before :each do
    @connection = DataObjects::Connection.new(CONFIG.uri)
  end

  after :each do
    @connection.close
  end

  describe 'reading a String' do

    describe 'with automatic typecasting' do

      before  do
        @reader = @connection.create_command("SELECT code FROM widgets WHERE ad_description = ?").execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(String)
      end

      it 'should return the correct result' do
        @values.first.should == "W0000001"
      end

    end

    describe 'with manual typecasting' do

      before  do
        @command = @connection.create_command("SELECT weight FROM widgets WHERE ad_description = ?")
        @command.set_types(String)
        @reader = @command.execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(String)
      end

      it 'should return the correct result' do
        @values.first.should == "13.4"
      end

    end

  end

  describe 'writing a String' do

    before  do
      @reader = @connection.create_command("SELECT id FROM widgets WHERE id = ?").execute_reader("2")
      @reader.next!
      @values = @reader.values
    end

    after do
      @reader.close
    end

    it 'should return the correct entry' do
      # Some of the drivers starts autoincrementation from 0 not 1
      @values.first.should satisfy { |val| val == 1 or val == 2 }
    end

  end

end
