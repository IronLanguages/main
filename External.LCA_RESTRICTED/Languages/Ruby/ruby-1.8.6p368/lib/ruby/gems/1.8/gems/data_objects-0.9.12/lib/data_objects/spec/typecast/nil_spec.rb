share_examples_for 'supporting Nil' do

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

  describe 'reading a Nil' do

    describe 'with manual typecasting' do

      before  do
        @command = @connection.create_command("SELECT flags FROM widgets WHERE ad_description = ?")
        @command.set_types(NilClass)
        @reader = @command.execute_reader('Buy this product now!')
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(NilClass)
      end

      it 'should return the correct result' do
        @values.first.should == nil
      end

    end

  end

end

share_examples_for 'supporting writing an Nil' do

  describe 'supporting writing an Nil' do

    describe 'as a parameter' do

        before  do
          @reader = @connection.create_command("SELECT id FROM widgets WHERE ad_description IS ?").execute_reader(nil)
          @reader.next!
          @values = @reader.values
        end

        after do
          @reader.close
        end

        it 'should return the correct entry' do
          #Some of the drivers starts autoincrementation from 0 not 1
          @values.first.should satisfy { |val| val == 3 or val == 2 }
        end

    end

  end

end

share_examples_for 'supporting Nil autocasting' do

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

  describe 'reading a Nil' do

    describe 'with automatic typecasting' do

      before  do
        @reader = @connection.create_command("SELECT ad_description FROM widgets WHERE id = ?").execute_reader(3)
        @reader.next!
        @values = @reader.values
      end

      after do
        @reader.close
      end

      it 'should return the correctly typed result' do
        @values.first.should be_kind_of(NilClass)
      end

      it 'should return the correct result' do
        @values.first.should == nil
      end

    end

  end

end
