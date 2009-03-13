require File.expand_path(File.join(File.dirname(__FILE__), 'spec_helper'))

describe DataObjects::Transaction do

  before :each do
    @connection = mock("connection")
    DataObjects::Connection.should_receive(:new).with("mock://mock/mock").once.and_return(@connection)
    @transaction = DataObjects::Transaction.new("mock://mock/mock")
  end

  it "should have a HOST constant" do
    DataObjects::Transaction::HOST.should_not == nil?
  end

  describe "#initialize" do
    it "should provide a connection" do
      @transaction.connection.should == @connection
    end
    it "should provide an id" do
      @transaction.id.should_not == nil
    end
    it "should provide a unique id" do
      DataObjects::Connection.should_receive(:new).with("mock://mock/mock2").once.and_return(@connection)
      @transaction.id.should_not == DataObjects::Transaction.new("mock://mock/mock2").id
    end
  end
  describe "#close" do
    it "should close its connection" do
      @connection.should_receive(:close).once
      @transaction.close
    end
  end
  [:begin, :commit, :rollback, :rollback_prepared, :prepare].each do |meth|
    it "should raise NotImplementedError on #{meth}" do
      lambda do @transaction.send(meth) end.should raise_error(NotImplementedError)
    end
  end

end
