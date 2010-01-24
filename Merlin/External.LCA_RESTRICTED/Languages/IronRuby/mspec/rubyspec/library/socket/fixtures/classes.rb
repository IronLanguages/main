require 'socket'

module SocketSpecs
  # helper to get the hostname associated to 127.0.0.1
  def self.hostname
    # Calculate each time, without caching, since the result might
    # depend on things like do_not_reverse_lookup mode, which is
    # changing from test to test
    Socket.getaddrinfo("127.0.0.1", nil)[0][2]
  end

  def self.hostnamev6
    Socket.getaddrinfo("::1", nil)[0][2]
  end
  
  def self.port
    40001
  end

  def self.local_port
    40002
  end

  def self.sockaddr_in(port, host)
    Socket::SockAddr_In.new(Socket.sockaddr_in(port, host))
  end

  def self.socket_path
    tmp("unix_server_spec.socket")
  end
  
  def self.start_tcp_server(remote_host = nil)
    thread = Thread.new do
      if remote_host
        server = TCPServer.new(remote_host, SocketSpecs.port)
      else
        server = TCPServer.new(SocketSpecs.port)
      end
      server.accept
      server.close
    end
    Thread.pass until thread.status == 'sleep' or thread.status == nil
    thread.status.should_not be_nil
    thread
  end
end
