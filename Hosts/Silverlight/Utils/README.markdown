Utilities
=========
chiron-here.reg
---------------
Runs Chiron on any directory just by right-clicking on it.

1. UPDATE SDLSDK LOCATION!

  chiron-here.reg assumes that you unzipped sdl-sdk.zip into C:\sdl-sdk. If this
  is not the case, open chiron-here.reg in your editor of choice (notepad will do)
  and change the two paths to point to your sdl-sdk location. 

2. INSTALL

  Double click on chiron-here.reg file. It will prompt you to make sure you trust
  the source of the .reg file; click *Yes* if you want to install it. That's it! No
  reboot required.

3. USAGE

  You will now have two extra options when you right-click on a folder: "Start
  Chiron Here" and "Generate XAP Here". 

  "Start Chiron Here" will run Chiron on port 2060at the directory you right-clicked 
  on. Then navigate to http://localhost:2060 with your web-browser.

  "Generate XAP Here" will generate a XAP file to disk to deployment. The
  directory you right-clicked on will appear as a .xap file in the same parent
  directory.

XAP HTTP Handler
================

Chiron.exe contains the XapHttpHandler type, which is a HttpHandler to enable 
IIS or the ASP.NET Development WebServer (Cassini) to auto-xap any directory
when requested with the .xap extension, just like Chiron does.

xap-http-handler\XapHttpHandler.sln is a example of this working in action. Make
sure to run xap-http-handler\update.bat first, so the DLR binaries are copied to
the website.

To set it up for your own project:

Visual Studio ASP.NET Development WebServer (Cassini)
-----------------------------------------------------
1. Create a ASP.NET Website project
2. Add the binaries in the bin folder to a Bin folder inside the website.
3. Copy xap-http-handler\XapHttpHandler.SampleSite\web.config to your app

IIS
---
Works the same as above; just move your project to IIS.
