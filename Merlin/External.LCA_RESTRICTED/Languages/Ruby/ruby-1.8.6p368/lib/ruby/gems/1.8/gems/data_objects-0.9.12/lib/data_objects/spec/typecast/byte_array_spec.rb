share_examples_for 'supporting ByteArray' do

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

  describe 'reading a ByteArray' do

    describe 'with automatic typecasting' do

      before  do
        @reader = @connection.create_command("SELECT cad_drawing FROM widgets WHERE ad_description = ?").execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(::Extlib::ByteArray)
      end

      it 'should return the correct result' do
        @values.first.should == "CAD \001 \000 DRAWING"
      end

    end

    describe 'with manual typecasting' do

      before  do
        @command = @connection.create_command("SELECT cad_drawing FROM widgets WHERE ad_description = ?")
        @command.set_types(::Extlib::ByteArray)
        @reader = @command.execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(::Extlib::ByteArray)
      end

      it 'should return the correct result' do
        @values.first.should == "CAD \001 \000 DRAWING"
      end

    end

  end

  describe 'writing a ByteArray' do

    before  do
      @reader = @connection.create_command("SELECT id FROM widgets WHERE id = ?").execute_reader(::Extlib::ByteArray.new("2"))
      @reader.next!
      @values = @reader.values
    end

    after do
      @reader.close
    end

    it 'should return the correct entry' do
      #Some of the drivers starts autoincrementation from 0 not 1
      @values.first.should satisfy { |val| val == 2 or val == 1 }
    end

  end

end
