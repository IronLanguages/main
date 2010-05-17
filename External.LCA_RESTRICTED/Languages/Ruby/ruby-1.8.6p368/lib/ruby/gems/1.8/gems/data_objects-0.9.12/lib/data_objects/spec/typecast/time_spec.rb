share_examples_for 'supporting Time' do

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

  describe 'reading a Time' do

    describe 'with manual typecasting' do

      before  do
        @command = @connection.create_command("SELECT release_date FROM widgets WHERE ad_description = ?")
        @command.set_types(Time)
        @reader = @command.execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(Time)
      end

      it 'should return the correct result' do
        @values.first.should == Time.local(2008, 2, 14)
      end

    end

  end

  describe 'writing an Time' do

    before  do
      @reader = @connection.create_command("SELECT id FROM widgets WHERE release_datetime = ?").execute_reader(Time.utc(2008, 2, 14, 00, 31, 12))
      @reader.next!
      @values = @reader.values
    end

    after do
      @reader.close
    end

    it 'should return the correct entry' do
       #Some of the drivers starts autoincrementation from 0 not 1
       @values.first.should satisfy { |val| val == 1 or val == 0 }
    end

  end

end
