if defined?(RUBY_ENGINE) && RUBY_ENGINE == 'ironruby'

require File.expand_path('helpers', File.dirname(__FILE__))
require File.dirname(__FILE__) + '/testrequest'
require File.dirname(__FILE__) + '/../lib/rack/deploy/aspnet'

context "Rack::Handler::ASPNET" do
  include JSONTestRequest::Helpers
  include IronRubyRackTest::Helpers

  before(:all) do
    $host = 'localhost'
    $cassini = start_server $port = 9202, '/test', File.join(TestApp, '1'), %Q{
      $rackpath = "#{RackPath}"
      $thispath = "#{File.expand_path('.', File.dirname(__FILE__))}"
      $:.unshift File.join($rackpath, 'lib')
      $:.unshift $thispath
      require 'rack/lint'
      require 'testrequest'
      use Rack::Lint
      run JSONTestRequest.new
    }
  end
  
  setup do
    @host = $host
    @port = $port
  end

  after(:all) do
    stop_server $cassini
    $host = nil
    $port = nil
  end

  specify "should respond" do
    lambda {
      GET("/test")
    }.should.not.raise
  end

  specify "should be a Cassini" do
    GET("/test")
    status.should.be 200
    response["SERVER_SOFTWARE"].should =~ /Cassini/
    response["HTTP_VERSION"].should.equal "HTTP/1.1"
    response["SERVER_PROTOCOL"].should.equal "HTTP/1.1"
    response["SERVER_PORT"].should.equal "9202"
    response["SERVER_NAME"].should.equal "localhost"
  end

  specify "should have rack headers" do
    GET("/test")
    response["rack.version"].should.equal [1,1]
    response["rack.multithread"].should.be true
    response["rack.multiprocess"].should.be false
    response["rack.run_once"].should.be false
  end

  specify "should have CGI headers on GET" do
    GET("/test")
    response["REQUEST_METHOD"].should.equal "GET"
    response["SCRIPT_NAME"].should.equal "/test"
    response["REQUEST_PATH"].should.equal "/"
    response["PATH_INFO"].should.be.equal ""
    response["QUERY_STRING"].should.equal ""
    response["test.postdata"].should.equal ""

    GET("/test/foo?quux=1")
    response["REQUEST_METHOD"].should.equal "GET"
    response["SCRIPT_NAME"].should.equal "/test"
    response["REQUEST_PATH"].should.equal "/"
    response["PATH_INFO"].should.equal "/foo"
    response["QUERY_STRING"].should.equal "quux=1"

    GET("/test/foo%25encoding?quux=1")
    response["REQUEST_METHOD"].should.equal "GET"
    response["SCRIPT_NAME"].should.equal "/test"
    response["REQUEST_PATH"].should.equal "/"
    response["PATH_INFO"].should.equal "/foo%25encoding"
    response["QUERY_STRING"].should.equal "quux=1"
  end

  specify "should have CGI headers on POST" do
    POST("/test", {"rack-form-data" => "23"}, {'X-test-header' => '42'})
    status.should.equal 200
    response["REQUEST_METHOD"].should.equal "POST"
    response["SCRIPT_NAME"].should.equal "/test"
    response["REQUEST_PATH"].should.equal "/"
    response["QUERY_STRING"].should.equal ""
    response["HTTP_X_TEST_HEADER"].should.equal "42"
    response["test.postdata"].should.equal "rack-form-data=23"
  end

  specify "should support HTTP auth" do
    GET("/test", {:user => "ruth", :passwd => "secret"})
    response["HTTP_AUTHORIZATION"].should.equal "Basic cnV0aDpzZWNyZXQ="
  end

  specify "should set status" do
    GET("/test?secret")
    status.should.equal 403
    response["rack.url_scheme"].should.equal "http"
  end

  specify "should correctly set cookies" do
    cookietest = start_server port = 9203, "/cookie-test", File.join(TestApp, '2'), %Q{
      $rackpath = "#{RackPath}"
      $:.unshift File.join($rackpath, 'lib')
      require 'rack/lint'
      require 'rack/response'
      use Rack::Lint
      run lambda { |req|
        res = Rack::Response.new
        res.set_cookie "one", "1"
        res.set_cookie "two", "2"
        res.finish
      }
    }
    Net::HTTP.start($host, port) { |http|
      res = http.get("/cookie-test")
      res.code.to_i.should.equal 200
      res.get_fields("set-cookie").should.equal ["one=1", "two=2"]
    }
    stop_server cookietest
  end

  specify "should provide a .run" do

    # TODO need to implement the run method!

    #block_ran = false
    #catch(:done) {
    #  Rack::Handler::ASPNET.run(lambda {},
    #                             {:Port => 9210,
    #                               :Logger => ASPNET::Log.new(nil, ASPNET::BasicLog::WARN),
    #                               :AccessLog => []}) { |server|
    #    block_ran = true
    #    server.should.be.kind_of ASPNET::HTTPServer
    #    @s = server
    #    throw :done
    #  }
    #}
    #block_ran.should.be true
    #@s.shutdown
    
    true.should == false
  end

end

else
  $stderr.puts "Skipping Rack::Handler::ASPNET tests (IronRuby is required). http://ironruby.net/download."
end