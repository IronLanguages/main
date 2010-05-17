$: << 'C:\M1\dlr\Languages\Ruby\Libs'

require 'socket'
require 'fcntl'
require 'thread'

class WebServer
  def initialize
    @tokens = SizedQueue.new(5)
    5.times{ @tokens.push(nil) }
  end

  def run
	thgroup = ThreadGroup.new
	@status = :Running
	@listeners = [TCPServer.new("127.0.0.1", 3000)]
	while @status == :Running
	    puts "#{Thread.current.inspect}: waiting"
		if svrs = IO.select(@listeners, nil, nil, 2.0)
		  puts "#{Thread.current.inspect}: selected #{svrs[0]}"
		  
		  svrs[0].each{|svr|
			@tokens.pop          # blocks while no token is there.
			if sock = accept_client(svr)
			  th = start_thread(sock)
			  th[:WEBrickThread] = true
			  thgroup.add(th)
			else
			  @tokens.push(nil)
			end
		  }
		end
	end
  end	
  
  def accept_client(svr)
      puts "#{Thread.current.inspect}: accept_client #{svr}"
      sock = nil
      sock = svr.accept
      puts "#{Thread.current.inspect}: accepted #{sock}"
      sock.sync = true
      return sock
  end
  
  
  def start_thread(sock, &block)
      Thread.start{
        Thread.current[:WEBrickSocket] = sock
        addr = sock.peeraddr
        puts "#{Thread.current.inspect}: working, peer = #{addr.inspect}"
        
        @tokens.push(nil)
        
        Thread.current[:WEBrickSocket] = nil
        
        puts "#{Thread.current.inspect}: close, peer = #{addr.inspect}"
        sock.close
      }
    end
end

WebServer.new.run
