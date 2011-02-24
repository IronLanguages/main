class UnitTestSetup
  def initialize
    @name = "Rack"
    super
  end
  
  VERSION = '1.1.0'
  
  def ironruby?
    defined?(RUBY_ENGINE) and RUBY_ENGINE == "ironruby"
  end
  
  def require_files
    if ironruby?
      require 'test/ispec'
    end
    require 'rubygems'
    gem 'rack', "=#{VERSION}"
  end

  def gather_files
    @lib_tests_dir = File.expand_path("Languages/Ruby/Tests/Libraries/rack-#{VERSION}", ENV["DLR_ROOT"])
    @irk_tests_dir = File.expand_path("Hosts/IronRuby.Rack/ironruby-rack/test", ENV["DLR_ROOT"])
    @all_test_files = Dir.glob("#{@lib_tests_dir}/test/test*.rb") + Dir.glob("#{@lib_tests_dir}/test/spec*.rb")
    @all_test_files.concat Dir.glob("#{@irk_tests_dir}/spec*.rb") if ironruby?

    %W(lib test).each{|i| $:.unshift i}
  end

  def sanity
    # Some tests load data assuming the current folder
    Dir.chdir(@lib_tests_dir)
  end

  def exclude_critical_files
	  @all_test_files = @all_test_files.delete_if{|i| i =~ /camping/}
  end
  
  def disable_mri_only_failures
    # NotImplementedError: fork() function is unimplemented on this machine
    disable_spec 'rackup',
      'rackup',
      'rackup --help',
      'rackup --port',
      'rackup --debug',
      'rackup --eval',
      'rackup --warn',
      'rackup --include',
      'rackup --require',
      'rackup --server',
      'rackup --host',
      'rackup --daemonize --pid',
      'rackup --pid',
      'rackup --version',
      'rackup --env development includes lint',
      'rackup --env deployment does not include lint',
      'rackup --env none does not include lint',
      'rackup --env deployment does log',
      'rackup --env none does not log'
    
    disable_spec 'Rack::Utils::Multipart',
	    #EOFError: bad content body
	    "can parse fields that end at the end of the buffer"

    # <"/foo/bar/hello.txt"> expected but was
    # <"d:/tmp/hello.txt">.
    disable_spec 'Rack::Sendfile', 
      'sets X-Accel-Redirect response header and discards body',
      'sets X-Lighttpd-Send-File response header and discards body',
      'sets X-Sendfile response header and discards body'
    
    disable_spec 'Rack::Handler::FastCGI',
      'startup',
      # NotImplementedError: fork() function is unimplemented on this machine
      
      'should respond',
      # Exception raised:
      # Class: <Errno::ECONNREFUSED>
      # Message: <"No connection could be made because the target machine actively refused it. - connect(2)">
      
      'should respond via rackup server',
      'should be a lighttpd',
      'should have rack headers',
      'should have CGI headers on GET',
      'should support HTTP auth',
      'should set status',
      # Errno::ECONNREFUSED: No connection could be made because the target machine actively refused it. - connect(2)
      
      'shutdown'
      #TypeError: wrong argument type nil (expected Fixnum)
    
    disable_spec 'Rack::Handler::CGI',
      'startup',
      # NotImplementedError: fork() function is unimplemented on this machine
      
      'should respond',
      # Exception raised:
      # Class: <Errno::ECONNREFUSED>
      # Message: <"No connection could be made because the target machine actively refused it. - connect(2)">
      
      'should be a lighttpd',
      'should have rack headers',
      'should have CGI headers on GET',
      'should have CGI headers on POST',
      'should support HTTP auth',
      'should set status',
      # Errno::ECONNREFUSED: No connection could be made because the target machine actively refused it. - connect(2)
      
      'shutdown'
      #TypeError: wrong argument type nil (expected Fixnum)
      
  end
  
  def disable_mri_failures
    if valid_context? 'Rack::Handler::Mongrel'
      disable_spec "Rack::Handler::Mongrel",
        # Errno::ECONNREFUSED: No connection could be made because the target machine actively refused it.
        'should respond',
        'should be a Mongrel',
        'should have rack headers',
        'should have CGI headers on GET',
        'should have CGI headers on POST',
        'should support HTTP auth',
        'should set status',
    
        # empty? expected to be false.
        'should stream #each part of the response'
    end
    
    if valid_context? 'Rack::Handler::FastCGI'
      disable_spec 'Rack::Handler::FastCGI', 
        'should have CGI headers on POST'
    end
    
    disable_spec 'Rack::Handler',
	    #LoadError: no such file to load -- fcgi
	    "has registered default handlers"
    
    # <0.10001> expected to be
    # >
    # <0.10001>.
    disable_spec "Rack::Runtime", 'should allow multiple timers to be set'
  end
  
  def disable_tests
    disable_spec "Rack::Handler::ASPNET",
      # TODO implement .run
      'should provide a .run',

      # <"/"> expected but was
      # <"/test">.
      'should have CGI headers on POST',

      # <"/"> expected but was
      # <"/test/">
      'should have CGI headers on GET'

    disable_spec "rackup",
      'rackup --debug',
      'rackup --daemonize --pid'

    disable_spec 'Rack::Cascade',
	    #NameError: uninitialized constant Errno::EPERM
	    "should append new app",
	    "should dispatch onward on 404 by default"

    disable_spec 'Rack::Handler::CGI',
	    "should respond",
	    #ArgumentError: IPv4 address 0.0.0.0 and IPv6 address ::0 are unspecified addresses that cannot be use...
	    "should support HTTP auth",
	    "should have CGI headers on POST",
	    "should set status",
	    "should have CGI headers on GET",
	    "should have rack headers",
	    "should be a lighttpd",
	    #NotImplementedError: Signals are not currently implemented. Signal.trap just pretends to work
	    "shutdown",
	    #NoMethodError: undefined method `fork' for #<:0x0002198 @method_name="test_spec {Rack::Handler::CGI} ...
	    "startup"

    disable_spec 'Rack::Handler::FastCGI',
	    "should respond",
	    #ArgumentError: IPv4 address 0.0.0.0 and IPv6 address ::0 are unspecified addresses that cannot be use...
	    "should have CGI headers on POST",
	    "should have CGI headers on GET",
	    "should support HTTP auth",
	    "should set status",
	    "should respond via rackup server",
	    "should have rack headers",
	    "should be a lighttpd",
	    #NotImplementedError: Signals are not currently implemented. Signal.trap just pretends to work
	    "shutdown",
	    #NoMethodError: undefined method `fork' for #<:0x00021fe @method_name="test_spec {Rack::Handler::FastC...
	    "startup"

    if valid_context? 'Rack::Handler::Mongrel'
      disable_spec 'Mongrel',
        'should provide a .run', 
        'should provide a .run that maps a hash'
    end

    disable_spec 'Rack::Directory',
	    #NameError: uninitialized constant Errno::ELOOP
	    "404s if it can't find the file"

    disable_spec 'Rack::File',
	    #NameError: uninitialized constant Errno::EPERM
	    "detects SystemCallErrors",
	    "404s if it can't find the file"

    disable_spec 'Rack::Handler',
	    #LoadError: no such file to load -- fcgi
	    "has registered default handlers"

    disable_spec 'Rack::RewindableInput	given an IO object that is not rewindable',
	    #NoMethodError: undefined method `close' for nil:NilClass
	    "should buffer into a Tempfile when data has been consumed for the first time",
	    "should not buffer into a Tempfile if no data has been read yet",
	    "should be able to handle each",
	    "should be possible to call #close multiple times",
	    "should be possibel to call #close when no data has been buffered yet",
	    "should close the underlying tempfile upon calling #close",
	    "should be able to handle gets",
	    "should be able to handle to read(length)",
	    "should be able to handle to read(nil)",
	    "should be able to handle to read()",
	    "should rewind to the beginning when #rewind is called",
	    "should be able to handle to read(nil, buffer)",
	    "should be able to handle to read(length, buffer)"

    disable_spec 'Rack::Session::Cookie',
	    #NameError: uninitialized constant OpenSSL::Digest::SHA1
	    "loads from a cookie wih integrity hash",
	    "ignores tampered with session cookies"

    disable_spec 'Rack::Static',
	    #NameError: uninitialized constant Errno::EPERM
	    "404s if url root is known but it can't find the file"

    disable_spec 'Rack::Utils::Multipart',
	    #EOFError: bad content body
	    "can parse fields that end at the end of the buffer"

    disable_spec 'Rack::ETag',
	    "sets ETag if none is set"

    disable_spec 'Rack::MockRequest',
	    "should accept params and build multipart encoded params for POST requests"

    disable_spec 'Rack::Sendfile',
	    "sets X-Accel-Redirect response header and discards body",
	    "sets X-Sendfile response header and discards body",
	    "sets X-Lighttpd-Send-File response header and discards body"

    disable_spec 'Rack::Utils',
	    "should figure out which encodings are acceptable"
  end
end
