Chiron.exe - Silverlight Development Utility

Chiron is a command-line utility for creating Silverlight XAP files and
enabling package-less development of dynamic language applications.

1. Creating Silverlight XAP files

  Chiron can produce Silverlight application packages (XAP) for C#/VB
  applications, as well as any Dynamic Language Runtime (DLR) based language.
  The Silverlight SDK provides IronRuby, IronPython, and Managed JScript.

  a. XAP a C#/VB application

    The Silverlight Visual Studio Tools use Chiron in a post-build step to
    package a Silverlight application. However, if you had a un-packaged built
    Silverlight application, you could package it with the following:

    >Chiron.exe /directory:MyApp /xap:MyApp.xap

  b. XAP a DLR application

    Dynamic language applications require the DLR and language assemblies to be
    inside the package as well. To do this automatically:

    >Chiron.exe /directory:MyApp\app /zipdlr:app.xap

    This will XAP the app directory, generate an AppManifest.xaml if necessary, 
    and insert the DLR assemblies and the language assemblies of the languages 
    you usedin your app.

    Given this simple application:

    MyApp\
    MyApp\index.html
    MyApp\app\
    MyApp\app\app.rb

    Chiron will generate an app.xap (in the MyApp folder) with these contents:

    app.rb
    AppManifest.xaml
    IronRuby.dll
    IronRuby.Libraries.dll
    Microsoft.Dynamic.dll
    Microsoft.Scripting.dll
    Microsoft.Scripting.Silverlight.dll

2. Package-less development of DLR applications

  Generating a XAP file works great for compiled languages since they can do it
  as a post-build step. However, dynamic languages are traditionally not 
  compiled to disk, so Chiron automates the XAP file creation with every 
  web-request.

  So, with our same sample app:

  >cd MyApp
  >Chiron.exe /webserver

  Chiron will launch a localhost-only web-server at http://localhost:2060 with
  the webroot being the current working directory. When it receives a request 
  for "app.xap", it will look for a folder named "app", XAP the contents (same 
  behavior as the /zipdlr flag) in memory, and send it back to the client.

  This allows for the developer to edit a file and simply hit refresh on their
  browser to see the change.

3. Configuration

  Chiron allows you to configure what languages it knows about and the
  AppManifest.xaml template by modifying Chiron.exe.config.

  New languages can be added in the <languages> section as follows:

  <language extensions="myext"
            assemblies="MyLanguageRuntime.dll"
            languageContext="MyLanguage.Namespace.MyLanguageContextClass" />

  Customizations to the AppManifest template go in the <appManifestTemplate>
  section.

4. Documentation

  Running Chiron without any command-line arguments will show you the avaliable
  flags Chiron will accept.
  
  >Chiron.exe
  Microsoft(R) Silverlight(TM) Development Utility. Version 1.0.0.0
  Copyright (c) Microsoft Corporation.  All rights reserved.

  Usage: Chiron [<options>]

  General Options:

    /d[irectory]:<path>
      Specifies directory on disk (default: the current directory)

    /x[ap]:<file>
      Specifies XAP file to generate
      Does not start the web server, cannot be combined with /w or /b

    /n[ologo]
      Suppresses display of the logo banner

    /s[ilent]
      Suppresses display of all output

  Dynamic Language Options:

    /z[ipdlr]:<file>
      Like /x, but includes files needed for dyanmic language apps
      Does not start the web server, cannot be combined with /w or /b

    /w[ebserver][:<port number>]
      Launches a development web server that automatically creates
      XAP files for dynamic language applications
      Optionally specifies server port number (default: 2060)

    /b[rowser]
      Launches the default browser and starts the web server
      Implies /w, cannot be combined with /x or /z

    /r[efpath]:<path>
      Specifies the directory that contains dynamic language assemblies
      Only copies the assemblies for the languages used in the project
      (defaults to "dlr" subfolder under Chrion install location)

    /m[anifest]
      Saves the generated AppManifest.xaml file to disk
      Use /d to set the directory containing the sources
      Can only be combined with /d, /n and /s

