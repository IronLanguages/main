RUBY_ENGINE = 'ruby' unless defined? RUBY_ENGINE

require 'benchmark'
require 'stringio'

usage = <<-EOS
ir test.rb [framework (rack)] [url (/)] [-tc|-trace_calls] [-tr|-trace_requires] [-w|-web_request] [-r|-response]
 
  framework                Can either be "rack" (default), "rails", or "sinatra"
  url                      URL to visit (defaults to "/")

  Options:

    -tc, -trace_calls      Traces all calls
    -tr, -trace_requires   Traces all calls to "require"
    -w, -web_request       Run actual IIS web requests
    -r, -response          Display the response
    -h, -?, -help          Shows this message

EOS

def trace_requires
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

colsize = 79

$fx = $web = $url = nil
puts '='*colsize if $DEBUG 
ARGV.clone.each do |arg|
  case arg
  when '-h', '-help', '-?'
    puts usage
    exit(0)
  when '-tr', '-trace_requires'
    puts 'trace requires' if $DEBUG
    trace_requires
    ARGV.shift
  when '-tc', '-trace_calls'
    puts 'trace method calls' if $DEBUG
    set_trace_func proc { |op, file, line, method, b, cls|
      if op == "call" 
        unless [:method_added, :inherited].include? method
          puts "#{cls}::#{method} (#{file.nil? ? nil : file.gsub('\\','/')}:#{line})"
        end  
      end
    }
    ARGV.shift
  when '-w', '-web_request'
    $web = true
    puts '$web = true' if $DEBUG
    ARGV.shift
  when '-r', '-response'
    $resp = true
    puts '$resp = true' if $DEBUG
    ARGV.shift
  else
    if $fx.nil?
      $fx = ARGV.shift
      puts "$fx = #{$fx.inspect}" if $DEBUG
    else
      $url = ARGV.shift
      $url = "/#{$url}" if $url[0] != ?/
      puts "$url = #{$url.inspect}" if $DEBUG
    end
  end
end
if $fx.nil?
  $fx = 'rack'
  puts "$fx = #{$fx.inspect}" if $DEBUG
end
if $url.nil? || $url.empty?
  $url = "/#{$url}" if $url && $url[0] != ?/
  $url ||= '/'
  puts "$url = #{$url.inspect}" if $DEBUG
end

puts '='*colsize, "#{$fx.capitalize} on #{RUBY_ENGINE} (http://localhost#{$url})", '='*colsize
ENV['RAILS_ENV'] = 'production' if $fx == 'rails'
ENV['RACK_ENV'] = 'production'
$app = "IronRuby.#{$fx.capitalize}.Example"
$app_root = File.dirname(__FILE__) + "/#{$app}"

env = {
  "APPL_PHYSICAL_PATH" => $app_root,
  "REMOTE_USER" => 'REDMOND\\jimmysch',
  "CONTENT_LENGTH" => 0,
  "LOCAL_ADDR" => '127.0.0.1',
  "PATH_INFO" => $url,
  "PATH_TRANSLATED" => "#{$app_root}#{$url}",
  "QUERY_STRING" => '',
  'REMOTE_ADDR' => '127.0.0.1',
  'REMOTE_HOST' => '127.0.0.1',
  'REMOTE_PORT' => '',
  'REQUEST_METHOD' => 'GET',
  'SCRIPT_NAME' => $url,
  'SERVER_NAME' => 'localhost',
  'SERVER_PORT' => 2873,
  'SERVER_PORT_SECURE' => 0,
  'SERVER_PROTOCOL' => 'HTTP/1.1',
  'SERVER_SOFTWARE' => '',
  'URL' => $url,
  'HTTP_CONNECTION' => 'Keep-Alive',
  'HTTP_ACCEPT' => 'image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-ms-application, application/vnd.ms-xpsdocument, application/xaml+xml, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-shockwave-flash, application/x-silverlight-2-b2, application/x-silverlight, */*',
  'HTTP_ACCEPT_ENCODING' => 'gzip, deflate',
  'HTTP_ACCEPT_LANGUAGE' => 'en-us',
  'HTTP_HOST' => 'localhost:2873',
  'HTTP_USER_AGENT' => 'Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; WOW64; SLCC1; .NET CLR 2.0.50727; .NET CLR 1.1.4322; InfoPath.2; .NET CLR 3.5.21022; MS-RTC LM 8; .NET CLR 3.5.30718; .NET CLR 3.0.30618)',
  'rack.version' => [1,1],
  'rack.url_scheme' => 'http',
  'rack.input' => StringIO.new(''),
  'rack.errors' => $stderr,
  'rack.multithread' => true,
  'rack.multiprocess' => false,
  'rack.run_once' => false
}

req = <<END
GET #{$url} HTTP/1.1
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
    (0..99).inject([]){|t,_| t << Thread.new{ app.call env } }.each{|t| t.join}
  }
}

# Output response
if $resp
  puts '='*colsize
  puts "Request:"

  status, headers, body = app.call env

  bbody = ''
  body.each {|b| bbody << b}

  puts "="*colsize, "Body:", bbody
  puts "-"*colsize, "Headers:", headers.inspect
  puts "-"*colsize, "Status:", status.inspect, "="*colsize
end

# Send 100 simultaneous web requests to IIS
if $web 
  require 'net/http'
  require 'uri'
  url = URI.parse "http://localhost/#{$app}#{$url}"
  def request(url)
    res = Net::HTTP.start(url.host, url.port) {|http| http.get url.path}
  end

  print "Send 1 request to warm-up ... "
  request(url)
  puts 'done'

  puts "100 simultaneous web requests to #{url}:"
  start = Time.now
  t = []
  100.times do
    t << Thread.new{ request url }
  end
  t.each{|tt| tt.join}
  finish = Time.now
  puts "#{finish - start} seconds"
  puts '-'*colsize
  puts "Body:"
  puts request(url).body
  puts '='*colsize
end
