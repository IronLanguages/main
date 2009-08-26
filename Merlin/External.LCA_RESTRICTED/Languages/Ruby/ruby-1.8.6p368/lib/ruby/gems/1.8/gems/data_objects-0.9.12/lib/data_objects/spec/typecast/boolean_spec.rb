share_examples_for 'supporting Boolean' do

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

  describe 'reading a Boolean' do

    describe 'with manual typecasting' do

      before  do
        @command = @connection.create_command("SELECT flags FROM widgets WHERE ad_description = ?")
        @command.set_types(TrueClass)
        @reader = @command.execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(FalseClass)
      end

      it 'should return the correct result' do
        @values.first.should == false
      end

    end

  end

  describe 'writing an Boolean' do

    before  do
      @reader = @connection.create_command("SELECT id FROM widgets WHERE flags = ?").execute_reader(true)
      @reader.next!
      @values = @reader.values
    end

    after do
      @reader.close
    end

    it 'should return the correct entry' do
      @values.first.should == 2
    end

  end

end

share_examples_for 'supporting Boolean autocasting' do

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

  describe 'reading a Boolean' do

    describe 'with automatic typecasting' do

      before  do
        @reader = @connection.create_command("SELECT flags FROM widgets WHERE ad_description = ?").execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(FalseClass)
      end

      it 'should return the correct result' do
        @values.first.should == false
      end

    end

  end

end
