share_examples_for 'supporting Array' do

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

  describe 'passing an Array as a parameter in execute_reader' do

      before  do
        @reader = @connection.create_command("SELECT * FROM widgets WHERE id in ?").execute_reader([2,3,4,5])
      end

      after do
        @reader.close
      end

      it 'should return correct number of rows' do
        counter  = 0
        while(@reader.next!) do
          counter += 1
        end
        counter.should == 4
      end

  end
end
