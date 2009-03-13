require File.expand_path(File.join(File.dirname(__FILE__), 'spec_helper'))

describe DataObjects::Connection do
  before do
    @connection = DataObjects::Connection.new('mock://localhost')
  end

  after do
    @connection.release
  end

  %w{dispose create_command}.each do |meth|
    it "should respond to ##{meth}" do
      @connection.should respond_to(meth.intern)
    end
  end

  it "should have #to_s that returns the connection uri string" do
    @connection.to_s.should == 'mock://localhost'
  end

  describe "initialization" do
    it "should accept a regular connection uri as a String" do
      c = DataObjects::Connection.new('mock://localhost/database')
      # relying on the fact that mock connection sets @uri
      uri = c.instance_variable_get("@uri")

      uri.scheme.should == 'mock'
      uri.host.should == 'localhost'
      uri.path.should == '/database'
    end

    it "should accept a connection uri as a Addressable::URI" do
      c = DataObjects::Connection.new(Addressable::URI::parse('mock://localhost/database'))
      # relying on the fact that mock connection sets @uri
      c.to_s.should == 'mock://localhost/database'
    end

    it "should return the Connection specified by the scheme" do
      c = DataObjects::Connection.new(Addressable::URI.parse('mock://localhost/database'))
      c.should be_kind_of(DataObjects::Mock::Connection)

      c = DataObjects::Connection.new(Addressable::URI.parse('mock:jndi://jdbc/database'))
      #c.should be_kind_of(DataObjects::Mock::Connection)
    end

    it "should return the Connection using username" do
      c = DataObjects::Connection.new(Addressable::URI.parse('mock://root@localhost/database'))
      c.instance_variable_get(:@uri).user.should == 'root'
      c.instance_variable_get(:@uri).password.should be_nil

      c = DataObjects::Connection.new(Addressable::URI.parse('mock://root:@localhost/database'))
      c.instance_variable_get(:@uri).user.should == 'root'
      c.instance_variable_get(:@uri).password.should == ''

      c = DataObjects::Connection.new(Addressable::URI.parse('mock://root:pwd@localhost/database'))
      c.instance_variable_get(:@uri).user.should == 'root'
      c.instance_variable_get(:@uri).password.should == 'pwd'
    end
  end
end
