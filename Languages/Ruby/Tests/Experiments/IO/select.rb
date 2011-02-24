$: << 'C:\M0\dlr\Languages\Ruby\Libs'

require 'socket'

serv = TCPServer.new("127.0.0.1", 0)
af, port, host, addr = serv.addr


c = TCPSocket.new(addr, port)
s = serv.accept

t1 = Thread.new {
  sleep(3)
  c.send "aaa", 0
}

puts 'waiting'
IO.select([s])

p s.recv_nonblock(10)

t1.join

