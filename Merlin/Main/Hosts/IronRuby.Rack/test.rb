require 'benchmark'
require 'stringio'

usage = <<-EOS
ir test.rb framework [url] [-tc] [-tr]

framework .. sinatra or rack
url ........ URL to visit ('/' is default)
-tr ........ trace all requires 
-tc ........ trace all method calls
EOS

fx = ARGV[0]
unless fx
  puts "Framework must be the first arg"
  puts "Usage:", usage
  exit(1)
end

if !ARGV[1].nil? and ARGV[1][1] != ?- 
  url = ARGV[1]
else
  url = "/"
end  

def trace_requires
  puts 'Tracing requires'
  
  $REQUIRE_DEPTH = 0
  Kernel.module_eval do
    alias x_require require
    alias x_load load
  
    def require path
      $REQUIRE_DEPTH += 1
      puts "#{$REQUIRE_DEPTH}\t" + ('| ' * $REQUIRE_DEPTH) + "> #{path}"
      x_require path
    ensure
      $REQUIRE_DEPTH -= 1
    end
    
    def load path, *wrap
      $REQUIRE_DEPTH += 1
      puts "#{$REQUIRE_DEPTH}\t" + ('| ' * $REQUIRE_DEPTH) + "> #{path} (LOAD)"
      x_load path, *wrap
    ensure
      $REQUIRE_DEPTH -= 1
    end
  end
end

if ARGV.include? "-tr"
  trace_requires
end

if ARGV.include? "-tc"
  set_trace_func proc { |op, file, line, method, b, cls|
    if op == "call" 
      unless [:method_added, :inherited].include? method
        puts "#{cls}::#{method} (#{file.nil? ? nil : file.gsub('\\','/')}:#{line})"
      end  
    end
  }
end

require File.dirname(__FILE__) + '/bin/release/IronRuby.Rack.dll'

puts '='*79, fx.capitalize, url, '='*79

$app_root = File.dirname(__FILE__) + "/IronRuby.#{fx.capitalize}.App"

env = {
  "APPL_PHYSICAL_PATH" => $app_root,
  "REMOTE_USER" => 'REDMOND\\jimmysch',
  "CONTENT_LENGTH" => 0,
  "LOCAL_ADDR" => '127.0.0.1',
  "PATH_INFO" => url,
  "PATH_TRANSLATED" => "#{$app_root}#{url}",
  "QUERY_STRING" => '',
  'REMOTE_ADDR' => '127.0.0.1',
  'REMOTE_HOST' => '127.0.0.1',
  'REMOTE_PORT' => '',
  'REQUEST_METHOD' => 'GET',
  'SCRIPT_NAME' => url,
  'SERVER_NAME' => 'localhost',
  'SERVER_PORT' => 2873,
  'SERVER_PORT_SECURE' => 0,
  'SERVER_PROTOCOL' => 'HTTP/1.1',
  'SERVER_SOFTWARE' => '',
  'URL' => url,
  'HTTP_CONNECTION' => 'Keep-Alive',
  'HTTP_ACCEPT' => 'image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-ms-application, application/vnd.ms-xpsdocument, application/xaml+xml, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-shockwave-flash, application/x-silverlight-2-b2, application/x-silverlight, */*',
  'HTTP_ACCEPT_ENCODING' => 'gzip, deflate',
  'HTTP_ACCEPT_LANGUAGE' => 'en-us',
  'HTTP_HOST' => 'localhost:2873',
  'HTTP_USER_AGENT' => 'Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; WOW64; SLCC1; .NET CLR 2.0.50727; .NET CLR 1.1.4322; InfoPath.2; .NET CLR 3.5.21022; MS-RTC LM 8; .NET CLR 3.5.30718; .NET CLR 3.0.30618)',
  'rack.version' => [1,0],
  'rack.url_scheme' => 'http',
  'rack.input' => StringIO.new(''),
  'rack.errors' => $stderr,
  'rack.multithread' => true,
  'rack.multiprocess' => false,
  'rack.run_once' => false
}

req = <<END
GET #{url} HTTP/1.1
Connection: Keep-Alive
Accept: image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-ms-application, application/vnd.ms-xpsdocument, application/xaml+xml, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-shockwave-flash, application/x-silverlight-2-b2, application/x-silverlight, */*
Accept-Encoding: gzip, deflate
Accept-Language: en-us
Host: localhost:2873
User-Agent: Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; WOW64; SLCC1; .NET CLR 2.0.50727; .NET CLR 1.1.4322; InfoPath.2; .NET CLR 3.5.21022; MS-RTC LM 8; .NET CLR 3.5.30718; .NET CLR 3.0.30618)
UA-CPU: x86

END

app = nil

Benchmark.bm(16) { |x|
  x.report("Startup") {
    require 'rubygems'
    gem 'rack', '=0.9.1' if fx == 'sinatra'
    require 'rack'

    config = File.read $app_root + '/config.ru'
    $:.unshift $app_root
    app = eval "Rack::Builder.new { #{config} }.to_app"
  }
  x.report("1 request:") {
    app.call env
  }
  10.times {
    x.report("1 request:") {
      app.call env
    }
  }
  x.report("100 requests:") {
    100.times {
      app.call env
    }
  }
}

# Output response

puts "Request:"

status, headers, body = app.call env

bbody = ''
body.each {|b| bbody << b}

colsize = 79

puts "="*colsize, "Body:", bbody
puts "-"*colsize, "Headers:", headers.inspect
puts "-"*colsize, "Status:", status.inspect, "="*colsize

