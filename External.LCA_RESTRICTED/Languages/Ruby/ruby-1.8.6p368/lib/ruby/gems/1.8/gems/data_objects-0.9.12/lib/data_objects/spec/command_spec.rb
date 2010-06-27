WINDOWS = Gem.win_platform?

share_examples_for 'a Command' do

  include DataObjectsSpecHelpers

  before :all do
    setup_test_environment
  end

  before :each do
    @connection = DataObjects::Connection.new(CONFIG.uri)
    @command    = @connection.create_command("INSERT INTO users (name) VALUES (?)")
    @reader     = @connection.create_command("SELECT code, name FROM widgets WHERE ad_description = ?")
  end

  after :each do
    @connection.close
  end

  it { @command.should be_kind_of(DataObjects::Command) }

  it { @command.should respond_to(:execute_non_query) }

  describe 'execute_non_query' do

    describe 'with an invalid statement' do

      before :each do
        @invalid_command = @connection.create_command("INSERT INTO non_existent_table (tester) VALUES (1)")
      end

      it 'should raise an error on an invalid query' do
        lambda { @invalid_command.execute_non_query }.should raise_error
      end

      it 'should raise an error with too few binding parameters' do
        lambda { @command.execute_non_query("Too", "Many") }.should raise_error(ArgumentError, "Binding mismatch: 2 for 1")
      end

      it 'should raise an error with too many binding parameters' do
        lambda { @command.execute_non_query }.should raise_error(ArgumentError, "Binding mismatch: 0 for 1")
      end

    end

    describe 'with a valid statement' do

      it 'should not raise an error with an explicit nil as parameter' do
        lambda { @command.execute_non_query(nil) }.should_not raise_error
      end

    end

  end

  it { @command.should respond_to(:execute_reader) }

  describe 'execute_reader' do

    describe 'with an invalid reader' do

      before :each do
        @invalid_reader = @connection.create_command("SELECT * FROM non_existent_widgets WHERE ad_description = ?")
      end

      it 'should raise an error on an invalid query' do
        lambda { @invalid_reader.execute_reader }.should raise_error
      end

      it 'should raise an error with too few binding parameters' do
        lambda { @reader.execute_reader("Too", "Many") }.should raise_error(ArgumentError, "Binding mismatch: 2 for 1")
      end

      it 'should raise an error with too many binding parameters' do
        lambda { @reader.execute_reader }.should raise_error(ArgumentError, "Binding mismatch: 0 for 1")
      end

    end

    describe 'with a valid reader' do

      it 'should not raise an error with an explicit nil as parameter' do
        lambda { @reader.execute_reader(nil) }.should_not raise_error
      end

    end

  end

  it { @command.should respond_to(:set_types) }

  describe 'set_types' do

    describe 'is invalid when used with a statement' do

      before :each do
        @command.set_types(String)
      end

      it 'should raise an error when types are set' do
        lambda { @command.execute_non_query }.should raise_error
      end

    end

    describe 'with an invalid reader' do

      it 'should raise an error with too few types' do
        @reader.set_types(String)
        lambda { @reader.execute_reader("One parameter") }.should raise_error(ArgumentError, "Field-count mismatch. Expected 1 fields, but the query yielded 2")
      end

      it 'should raise an error with too many types' do
        @reader.set_types(String, String, BigDecimal)
        lambda { @reader.execute_reader("One parameter") }.should raise_error(ArgumentError, "Field-count mismatch. Expected 3 fields, but the query yielded 2")
      end

    end

    describe 'with a valid reader' do

      it 'should not raise an error with correct number of types' do
        @reader.set_types(String, String)
        lambda { @result = @reader.execute_reader('Buy this product now!') }.should_not raise_error
        lambda { @result.next! }.should_not raise_error
        lambda { @result.values }.should_not raise_error
        @result.close
      end

      it 'should also support old style array argument types' do
        @reader.set_types([String, String])
        lambda { @result = @reader.execute_reader('Buy this product now!') }.should_not raise_error
        lambda { @result.next! }.should_not raise_error
        lambda { @result.values }.should_not raise_error
        @result.close
      end

    end

  end

  it { @command.should respond_to(:to_s) }

  describe 'to_s' do

  end


end

share_examples_for 'a Command with async' do

  include DataObjectsSpecHelpers

  before :all do
    setup_test_environment
  end

  describe 'running queries in parallel' do

    before :each do

      threads = []

      @start = Time.now
      4.times do |i|
        threads << Thread.new do
          connection = DataObjects::Connection.new(CONFIG.uri)
          command = connection.create_command(CONFIG.sleep)
          result = command.execute_non_query
          connection.close
        end
      end

      threads.each{|t| t.join }
      @finish = Time.now
    end

    after :each do
      @connection.close
    end

    it "should finish within 2 seconds" do
      pending_if("Ruby on Windows doesn't support asynchronious operations", WINDOWS) do
        (@finish - @start).should < 2
      end
    end

  end
end
