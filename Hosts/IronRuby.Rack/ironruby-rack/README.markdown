ironruby-rack
=============
Deploy and run Rack-based web applications on IIS with IronRuby ([announcement blog-post](http://blog.jimmy.schementi.com/2009/05/ironruby-at-railsconf-2009.html#iis))

Setup
-----
TODO need to make a gem/binary for this, and automate IIS/Cassini installation
and setup. The first binary release of ironruby-rack (as a gem) will be soon after
ironruby-1.0rc4.

Install Rack

    igem install rack

Install IIS 6 or 7 (this set of features might be overkill)

*  Internet Information Services
   *  World Wide Web Services
      *  Application Development Features
      *  Common HTTP Features -> Default Document, Directory Browsing, 
         HTTP Redirection and Static Content
      *  Performance features -> Static Content Compression
      *  Security -> Request Filtering and Windows Authentication
   *  Web Management Tools
      *  IIS 6 Management Compatibility -> IIS Metabase and IIS6 configuration
         compatibility
      *  IIS Management Console
      *  IIS Management Script and Tools
      *  IIS Management Service

Copy web.config and config.ru (if your app does't already have one) to your
application's root directory, and tweak any settings in web.config as necessary.

Add your application as a "Web Application" in IIS's control panel.

* Also, I suggest to create a seperate app-pool for all your Ruby applications, 
  make sure your application is assigned to it. Also make sure the app is 
  running as a user that has (at least) read-only permission to Ruby's
  standard libraries and RubyGems (the path's defined in web.config).

Navigate to your application in a browser, and Rails will start on first load.

For developers
--------------
First see the general Setup instruction above.

* Open IronRuby.Rack.sln in Visual Studio 2008 (or IronRuby.Rack.4.sln 
  for Visual Studio 2010).
* Click "OK" to prompts about creating virtual directories, otherwise not 
  all the project files will load. This also requires you run Visual Studio
  as an Administrator. This installs the test apps in IIS.
* IIS usually runs as IIS_IUSRS, which means that IIS won't have access
  to your application or the Ruby/IronRuby libraries unless you either grant
  read access to IIS_IUSRS (and write access to your application so it can
  write the log files). You can also run the application in an AppPool which
  has the identity of another user, if you don't want to give IIS free range.
* Right-click on IronRuby.Rack.Example and select "Set as StartUp Project".
* press F5 (Compile and Debug), and the Rack application will start.

Running tests
-------------
To run the tests, just launch the test.bat file:

    test.bat

This will test that IronRuby and Ruby can run a simple Rack, Sinatra, and Rails
application, and benchmarks under various loads (first 10 requests, and then
100 simultaneous requests).

To run an actual test of ironruby-rack, use the -w flag:

    ir test.rb rack -w

This will run just the rack tests in IronRuby, and then do 100 simultaneous
requests against the IIS hosted version (TODO you must have set this up first).

See the help for more info

    ir test.rb -h

How it works
------------
* Uses ASP.NET's HttpHandlers to
   * Registering IronRuby.Rack in the Rack-based application's Web.config
   * Load a Rack-based application on startup (HttpHandlerFactory and 
     Application constructor).
     * Initializes Rack and runs the application's config.ru, which tells Rack
       what application (any Ruby object that responds to 'call')
   * Intercept web requests (HttpHandler.ProcessRequest)
     * Creates a Request and a Response, and passes it off to IIS.Handle which:
       * Set up the environment according to the Rack specification
       * Calls Application.Call with the prepared environment, which delegates 
         to the Rack application's "call" method (registered in the config.ru
         file). All C# <-> Ruby interaction happens in the RubyEngine.
       * Rack application does its thing (process Rails/Sinatra request, or 
         deal with things itself) and returns a response according to the Rack 
         specification.
       * Takes the Rack response and pass the appropriate data to the IIS 
         response (response body, status, headers)

TODO
----
* Support Rack::Handler::IIS.run as a way to launch cassini, so
  "rackup" works as well (without depending on webrick).
* Should rename to AspDotNet or something like that, as it's not IIS-specific
  (works on cassini).
* Run actual non-webserver-specific rack tests, to verify that this adapter
  is a good rack-citizen.
* Add ASP.NET specific tests to rack, and try to get this merged in entirely?
