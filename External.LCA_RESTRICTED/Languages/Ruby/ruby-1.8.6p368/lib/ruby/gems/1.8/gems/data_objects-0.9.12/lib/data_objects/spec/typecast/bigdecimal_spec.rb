share_examples_for 'supporting BigDecimal' do

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

  describe 'reading a BigDecimal' do

    describe 'with manual typecasting' do

      before  do
        @command = @connection.create_command("SELECT cost1 FROM widgets WHERE ad_description = ?")
        @command.set_types(BigDecimal)
        @reader = @command.execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(BigDecimal)
      end

      it 'should return the correct result' do
        @values.first.should == 10.23
      end

    end

  end

  describe 'writing an Integer' do

    before  do
      @reader = @connection.create_command("SELECT id FROM widgets WHERE id = ?").execute_reader(BigDecimal("2.0"))
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

share_examples_for 'supporting BigDecimal autocasting' do

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

  describe 'reading a BigDecimal' do

    describe 'with automatic typecasting' do

      before  do
        @reader = @connection.create_command("SELECT cost2 FROM widgets WHERE ad_description = ?").execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(BigDecimal)
      end

      it 'should return the correct result' do
        @values.first.should == 50.23
      end

    end

  end

end
