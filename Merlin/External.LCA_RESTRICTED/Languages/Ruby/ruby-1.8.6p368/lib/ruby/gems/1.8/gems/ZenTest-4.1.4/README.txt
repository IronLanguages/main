= ZenTest

* http://www.zenspider.com/ZSS/Products/ZenTest/
* http://rubyforge.org/projects/zentest/
* mailto:ryand-ruby@zenspider.com

== DESCRIPTION

ZenTest provides 4 different tools: zentest, unit_diff, autotest, and
multiruby.

ZenTest scans your target and unit-test code and writes your missing
code based on simple naming rules, enabling XP at a much quicker
pace. ZenTest only works with Ruby and Test::Unit. Nobody uses this
tool anymore but it is the package namesake, so it stays.

unit_diff is a command-line filter to diff expected results from
actual results and allow you to quickly see exactly what is wrong.

autotest is a continous testing facility meant to be used during
development. As soon as you save a file, autotest will run the
corresponding dependent tests.

multiruby runs anything you want on multiple versions of ruby. Great
for compatibility checking! Use multiruby_setup to manage your
installed versions.

== STRATEGERY

There are two strategeries intended for ZenTest: test conformance
auditing and rapid XP.

For auditing, ZenTest provides an excellent means of finding methods
that have slipped through the testing process. I've run it against my
own software and found I missed a lot in a well tested
package. Writing those tests found 4 bugs I had no idea existed.

ZenTest can also be used to evaluate generated code and execute your
tests, allowing for very rapid development of both tests and
implementation.

== FEATURES

* Scans your ruby code and tests and generates missing methods for you.
* Includes a very helpful filter for Test/Spec output called unit_diff.
* Continually and intelligently test only those files you change with autotest.
* Test against multiple versions with multiruby.
* Enhance and automatically audit your rails tests using Test::Rails.
* Includes a LinuxJournal article on testing with ZenTest written by Pat Eyler.
* See also: http://blog.zenspider.com/archives/zentest/
* See also: http://blog.segment7.net/articles/category/zentest

== SYNOPSYS

  ZenTest MyProject.rb TestMyProject.rb > missing.rb

  ./TestMyProject.rb | unit_diff

  autotest

  multiruby_setup mri:svn:current
  multiruby ./TestMyProject.rb

  (and other stuff for Test::Rails)

== REQUIREMENTS

* Ruby 1.6+, JRuby 1.1.2+, or rubinius
* Test::Unit or miniunit (or something else ... I have no idea)
* Hoe
* rubygems
* diff.exe on windoze. Try http://gnuwin32.sourceforge.net/packages.html

== INSTALL

Using Rubygems:

* sudo gem install ZenTest

Using Rake:

* rake test
* sudo rake install

== LICENSE

(The MIT License)

Copyright (c) 2001-2006 Ryan Davis, Eric Hodel, Zen Spider Software

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

