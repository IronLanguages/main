# Very simple test of TCPServer

# Currently this is a manual test, until we have threading
# Run the script in MRI or IronRuby, then connect to
# http://127.0.0.1:1234/

# To kill, use Ctrl+C then make a request
# (or kill your command window)

# BUG: currently socket is a builtin in IronRuby
# BUG: also, for some reason we cannot 'rescue' a failed require in IronRuby,
# so instead we try TCPSocket and if it fails require it

begin
  TCPSocket
rescue
  require 'socket'
end

TCPServer.open('127.0.0.1', 1234) do |server|
  reqnum = 0
  while true
    socket = server.accept    
    req = socket.recv(10000)
    reqnum += 1
    content = "Hello from Ruby TCPServer! Request number #{reqnum}"
    puts req
    puts
    socket.send("HTTP/1.1 200 OK\r
Content-Type: text/plain; charset=utf-8\r
Server: ruby-manual-socket-test/0.1\r
Cache-Control: no-cache\r
Pragma: no-cache\r
Expires: -1\r
Content-Length: #{content.length}\r
Connection: Close\r\n\r\n", 0)
    socket.send(content, 0)
    socket.shutdown
  end
end
