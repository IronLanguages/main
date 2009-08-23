ironruby-rack
=============
Run Rack-based web applications on IIS with IronRuby

[Deploying on IIS](http://blog.jimmy.schementi.com/2009/05/ironruby-at-railsconf-2009.html#iis)

Setup
-----
1. Install Rack
> igem install rack

2. Install IIS (this set of features might be overkill)
   - Control Panel -> Programs -> Turn Windows Features on or off
   - Internet Information Services
     - World Wide Web Services, enable:
       - Application Development Features
       - Common HTTP Features -> Default Document, Directory Browsing, 
         HTTP Redirection and Static Content
       - Performance features -> Static Content Compression
       - Security -> Request Filtering and Windows Authentication
     - Web Management Tools, enable:
       - IIS 6 Management Compatibility -> IIS Metabase and IIS6 configuration
         compatibility
       - IIS Management Console
       - IIS Management Script and Tools
       - IIS Management Service

3. Give permissions to IIS
   - IIS needs to have permission to open files in this project, as well as in 
     the Ruby standard library. Grant the IIS_IUSRS group full permissions to this directory, 
     and grant read-only permissiong to the Ruby standard library (usually C:\ruby\lib\ruby).

4. Open IronRack.sln in Visual Studio
   - Click "OK" to prompts about creating virtual directories, otherwise not 
     all the project files will load.
   - Right-click on IronRuby.Rack.App and select "Set as StartUp Project".

5. Make sure GEM_PATH is set correctly
   - GEM_PATH is set in [Application.cs#42](http://github.com/jschementi/ironruby/tree/master/Merlin/Main/Hosts/IronRuby.Rack/Application.cs#L42). Make sure this reflects your environment. 

Building
--------
Simply build the solution in Visual Studio. It will build IronRuby, 
as well as IronRuby.Rack.dll.

Note: make sure your running Visual Studio as an administrator, and you have
IIS installed. Otherwise, the example app "IronRuby.Rack.Example.dll" won't work.

Running
-------
Click "Debug" -> "Start without Debugging" to run the Rack Application, which
will just navigate to http://localhost/IronRuby.Rack.Example

How it works
------------
- Uses ASP.NET's HttpHandlers to
   - Registering IronRuby.Rack in the Rack-based application's Web.config
   - Load a Rack-based application on startup (HttpHandlerFactory and 
     Application constructor).
     - Initializes Rack and runs the application's config.ru, which tells Rack
       what application (any Ruby object that responds to 'call')
   - Intercept web requests (HttpHandler.ProcessRequest)
     - Creates a Request and a Response, and passes it off to IIS.Handle which:
       - Set up the environment according to the Rack specification
       - Calls Application.Call with the prepared environment, which delegates 
         to the Rack application's "call" method (registered in the config.ru
         file). All C# <-> Ruby interaction happens in the RubyEngine.
       - Rack application does its thing (process Rails/Sinatra request, or 
         deal with things itself) and returns a response according to the Rack 
         specification.
       - Takes the Rack response and pass the appropriate data to the IIS 
         response (response body, status, headers)
