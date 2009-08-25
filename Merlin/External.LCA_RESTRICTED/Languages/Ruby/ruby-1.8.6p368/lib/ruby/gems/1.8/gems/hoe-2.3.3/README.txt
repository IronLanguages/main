= Hoe

* http://rubyforge.org/projects/seattlerb/
* http://seattlerb.rubyforge.org/hoe/
* http://www.zenspider.com/~ryand/hoe.pdf
* mailto:ryand-ruby@zenspider.com

== DESCRIPTION:

Hoe is a rake/rubygems helper for project Rakefiles. It helps generate
rubygems and includes a dynamic plug-in system allowing for easy
extensibility. Hoe ships with plug-ins for all your usual project
tasks including rdoc generation, testing, packaging, and deployment.

Plug-ins Provided:

* Hoe::Clean
* Hoe::Debug
* Hoe::Deps
* Hoe::Flay
* Hoe::Flog
* Hoe::Inline
* Hoe::Package
* Hoe::Publish
* Hoe::RCov
* Hoe::Signing
* Hoe::Test

See class rdoc for help. Hint: ri Hoe

== FEATURES/PROBLEMS:

* Includes a dynamic plug-in system allowing for easy extensibility.
* Auto-intuits changes, description, summary, and version.
* Uses a manifest for safe and secure deployment.
* Provides 'sow' for quick project directory creation.
* Sow uses a simple ERB templating system allowing you to capture your
  project patterns

== SYNOPSIS:

  % sow [group] project

or:

  require 'hoe'
  
  Hoe.spec projectname do
    # ... project specific data ...
  end

  # ... project specific tasks ...

== REQUIREMENTS:

* rake
* rubyforge
* rubygems

== INSTALL:

* sudo gem install hoe

== LICENSE:

(The MIT License)

Copyright (c) Ryan Davis, Zen Spider Software

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
