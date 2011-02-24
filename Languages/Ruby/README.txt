Content description
-------------------

ClassInitGenerator
  - A tool used to generate Initializer.Generated.cs files in libraries.

Console
  - ir.exe, ir64.exe

Docs
  - Implementation documents.

IronRuby.Tests
  - Targetted compiler and runtime unit tests hosted in a C# test harness.

Libraries
  - Builtin modules (IronRuby.Libraries.dll).

Libraries.Yaml
  - YAML standard library (IronRuby.Libraries.Yaml.dll).

Libs 
  - Includes IronRuby specific libraries.

Public
  - Misc files included in IronRuby.msi.

Ruby
  - Core compiler and runtime (IronRuby.dll)

Samples
  - samples included in IronRuby.msi

Scripts
  - Infrastructure scripts and helpers.

StdLib
  - Ruby standard library distributed in IronRuby.msi.
  - The content is a copy of MRI 1.9.2 standard library with the following changes:

    i386-mingw32 directory removed
      - *.so files need to be reimplemented in IronRuby.Libraries.dll
      - rbconfig.rb is replaced by the one in Languages\Ruby\Libs

    complex18.rb and rational18.rb are taken from MRI 1.8.6 
      - this should be replaced by IronRuby's own implementation of Complex and Rational builtins in future 

    gem_prelude.rb taked from Ruby 1.9.2 source distribution and adapted
      - should be replaced by an implementation in IronRuby.Libraries.dll

Tests 
  - Test suites.

  

