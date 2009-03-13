require File.expand_path(File.join(File.dirname(__FILE__), 'spec_helper'))

describe DataObjects::URI do
  before do
    @uri = DataObjects::URI.parse('mock://username:password@localhost:12345/path?encoding=utf8#fragment')
  end

  it "should parse the scheme part" do
    @uri.scheme.should == "mock"
  end

  it "should parse the user part" do
    @uri.user.should == "username"
  end

  it "should parse the password part" do
    @uri.password.should == "password"
  end

  it "should parse the host part" do
    @uri.host.should == "localhost"
  end

  it "should parse the port part" do
    @uri.port.should == 12345
  end

  it "should parse the path part" do
    @uri.path.should == "/path"
  end

  it "should parse the query part" do
    @uri.query.should == { "encoding" => "utf8" }
  end

  it "should parse the fragment part" do
    @uri.fragment.should == "fragment"
  end

  it "should provide a correct string representation" do
    @uri.to_s.should == 'mock://username:password@localhost:12345/path?encoding=utf8#fragment'
  end

end
