IronRack
========

Running Rack applications on IIS

Setup
-----
0. Install Rack

> igem install rack

1. Install IIS (this set of features might be overkill)
   - Control Panel -> Programs -> Turn Windows Features on or off
   - Internet Information Services
     - World Wide Web Services, enable:
       - Application Development Features
       - Common HTTP Features -> Default Document, Directory Browsing, 
         HTTP Redirection and Static Content
       - Performance features -> Static Content Compression
       - Security -> Request Filtering and Windows Authentication
     - Web Management Tools, enable:
       - IIS 6 Management Compatibility -> IIS Metabase and IIS6 configuration compatibility
       - IIS Management Console
       - IIS Management Script and Tools
       - IIS Management Service

2. Give permissions to IIS
   - IIS needs to have permission to open files in this project, as well
     as in the Ruby standard library. Grant IIS_IUSER permission to this directory,
     as well as the Ruby standard library (usually C:\ruby\lib\ruby)

2. Open IronRack.sln in Visual Studio
   - Click "OK" to prompts about creating virtual directories, otherwise not all 
     the project files will load.
   - Right-click on IronRuby.Rack.App and select "Set as StartUp Project".

Building
--------
Simply build the solution in Visual Studio. It will build IronRuby, as well as IronRuby.Rack.dll.

Running
-------
Click "Debug" -> "Start without Debugging" to run the Rack Application, which will 
just navigate to http://localhost/RackApp.
