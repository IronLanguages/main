share_examples_for 'a Result' do

  include DataObjectsSpecHelpers

  before :all do
    setup_test_environment
  end

  before :each do
    @connection = DataObjects::Connection.new(CONFIG.uri)
    @result    = @connection.create_command("INSERT INTO users (name) VALUES (?)").execute_non_query("monkey")
  end

  after :each do
    @connection.close
  end

  it { @result.should respond_to(:affected_rows) }

  describe 'affected_rows' do

    it 'should return the number of affected rows' do
      @result.affected_rows.should == 1
    end

  end

end

share_examples_for 'a Result which returns inserted keys' do

  include DataObjectsSpecHelpers

  before :all do
    setup_test_environment
  end

  before :each do
    @connection = DataObjects::Connection.new(CONFIG.uri)
    @result    = @connection.create_command("INSERT INTO users (name) VALUES (?)").execute_non_query("monkey")
  end

  after :each do
    @connection.close
  end

  it { @result.should respond_to(:affected_rows) }

  describe 'insert_id' do

    it 'should return the number of affected rows' do
      # This is actually the 2nd record inserted
      @result.insert_id.should == 2
    end

  end

end
