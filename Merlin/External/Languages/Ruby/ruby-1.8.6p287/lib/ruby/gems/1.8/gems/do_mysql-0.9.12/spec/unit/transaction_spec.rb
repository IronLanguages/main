require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataObjects::Mysql::Transaction do

  before :each do
    @connection = mock("connection")
    DataObjects::Connection.should_receive(:new).with("mock://mock/mock").once.and_return(@connection)
    @transaction = DataObjects::Mysql::Transaction.new("mock://mock/mock")
    @transaction.id.replace("id")
    @command = mock("command")
  end

  {
    :begin => "XA START 'id'",
    :commit => "XA COMMIT 'id'",
    :rollback => ["XA END 'id'", "XA ROLLBACK 'id'"],
    :rollback_prepared => "XA ROLLBACK 'id'",
    :prepare => ["XA END 'id'", "XA PREPARE 'id'"]
  }.each do |method, commands|
    it "should execute #{commands.inspect} on ##{method}" do
      if commands.is_a?(String)
        @command.should_receive(:execute_non_query).once
        @connection.should_receive(:create_command).once.with(commands).and_return(@command)
        @transaction.send(method)
      elsif commands.is_a?(Array) && commands.size == 2
        @command.should_receive(:execute_non_query).twice
        @connection.should_receive(:create_command).once.with(commands.first).and_return(@command)
        @connection.should_receive(:create_command).once.with(commands.last).and_return(@command)
        @transaction.send(method)
      end
    end
  end

end
