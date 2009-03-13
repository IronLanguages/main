require File.expand_path(File.join(File.dirname(__FILE__), 'spec_helper'))

describe DataObjects::Command do
  before do
    @connection = DataObjects::Connection.new('mock://localhost')
    @command = DataObjects::Command.new(@connection, 'SQL STRING')
  end

  after do
    @connection.close
  end

  it "should assign the connection object to @connection" do
    @command.instance_variable_get("@connection").should == @connection
  end

  it "should assign the sql text to @text" do
    @command.instance_variable_get("@text").should == 'SQL STRING'
  end

  %w{connection execute_non_query execute_reader set_types to_s}.each do |meth|
    it "should respond to ##{meth}" do
      @command.should respond_to(meth.intern)
    end
  end

  %w{execute_non_query execute_reader set_types}.each do |meth|
    it "should raise NotImplementedError on ##{meth}" do
      lambda { @command.send(meth.intern, nil) }.should raise_error(NotImplementedError)
    end
  end

  it "should make the connection object available in #connection" do
    @command.connection.should == @command.instance_variable_get("@connection")
  end

  it "should make the SQL text available in #to_s" do
    @command.to_s.should == @command.instance_variable_get("@text")
  end

end
