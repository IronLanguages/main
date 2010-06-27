share_examples_for 'supporting Class' do

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

  describe 'reading a Class' do

    describe 'with manual typecasting' do

      before  do
        @command = @connection.create_command("SELECT whitepaper_text FROM widgets WHERE ad_description = ?")
        @command.set_types(Class)
        @reader = @command.execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(Class)
      end

      it 'should return the correct result' do
        @values.first.should == String
      end

    end

  end

  describe 'writing a Class' do

    before  do
      @reader = @connection.create_command("SELECT whitepaper_text FROM widgets WHERE whitepaper_text = ?").execute_reader(String)
      @reader.next!
      @values = @reader.values
    end

    after do
      @reader.close
    end

    it 'should return the correct entry' do
      @values.first.should == "String"
    end

  end

end
