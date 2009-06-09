require File.dirname(__FILE__) + '/../../../../spec_helper'
require File.dirname(__FILE__) + '/../../fixtures/classes'

describe :tcpsocket_new, :shared => true do
  before :each do
    @hostname = Socket.getaddrinfo("127.0.0.1", nil)[0][2]
    @thread = nil
  end
  
  after :each do
    @thread.join if @thread
  end
  
  it "requires a hostname and a port as arguments" do
    lambda { TCPSocket.new }.should raise_error(ArgumentError)
  end

  it "refuses the connection when there is no server to connect to" do
    lambda { TCPSocket.new('127.0.0.1', SocketSpecs.port) }.should raise_error(Errno::ECONNREFUSED)
  end

  it "connects to a listening server" do
    @thread = SocketSpecs.start_tcp_server
    lambda {
      sock = TCPSocket.new(@hostname, SocketSpecs.port)
      sock.close
    }.should_not raise_error(Errno::ECONNREFUSED)
  end

  it "has an address once it has connected to a listening server" do
    @thread = SocketSpecs.start_tcp_server('127.0.0.1')
    sock = TCPSocket.new('127.0.0.1', SocketSpecs.port)
    sock.addr[0].should == "AF_INET"
    sock.addr[1].should be_kind_of(Fixnum)
    # on some platforms (Mac), MRI
    # returns comma at the end. Other
    # platforms such as OpenBSD setup the
    # localhost as localhost.domain.com
    sock.addr[2].should =~ /^#{@hostname}/
    sock.addr[3].should == "127.0.0.1"
    sock.close
  end

  it "allows local_port to be 0 when local_host is not specified" do
    @thread = SocketSpecs.start_tcp_server('127.0.0.1')
    sock = TCPSocket.new('127.0.0.1', SocketSpecs.port, 0)
    sock.addr[0].should == "AF_INET"
    sock.close
  end

  it "requires local_port to be 0 when local_host is not specified" do
    # The requested address is not valid in its context
    lambda { sock = TCPSocket.new('127.0.0.1', SocketSpecs.port, 1) }.should raise_error(Errno::EADDRNOTAVAIL)
  end
end
