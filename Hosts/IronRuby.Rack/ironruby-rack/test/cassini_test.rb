require 'rbconfig'
$: << RbConfig::CONFIG['bindir']

require 'Cassini'
server = Cassini::Server.new(9202, '/test', "#{File.expand_path(File.dirname(__FILE__)).gsub('/', '\\')}")

Thread.new { server.start }
trap(:INT) { server.stop }

require 'net/http'
require 'uri'

url = URI.parse('http://localhost:9202/test/')
res = Net::HTTP.start(url.host, url.port) {|http|
  http.get('/test/')
}
puts res.body

server.stop