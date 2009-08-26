= Ruby Inline

* http://rubyforge.org/projects/rubyinline/
* http://rubyinline.rubyforge.org/RubyInline/
* http://www.zenspider.com/ZSS/Products/RubyInline/
* mailto:ryand-ruby@zenspider.com

== DESCRIPTION:

Inline allows you to write foreign code within your ruby code. It
automatically determines if the code in question has changed and
builds it only when necessary. The extensions are then automatically
loaded into the class/module that defines it.

You can even write extra builders that will allow you to write inlined
code in any language. Use Inline::C as a template and look at
Module#inline for the required API.

== PACKAGING:

To package your binaries into a gem, use hoe's INLINE and
FORCE_PLATFORM env vars.

Example:

  rake package INLINE=1

or:

  rake package INLINE=1 FORCE_PLATFORM=mswin32

See hoe for more details.

== FEATURES/PROBLEMS:

+ Quick and easy inlining of your C or C++ code embedded in your ruby script.
+ Extendable to work with other languages.
+ Automatic conversion between ruby and C basic types
	+ char, unsigned, unsigned int, char *, int, long, unsigned long
+ inline_c_raw exists for when the automatic conversion isn't sufficient.
+ Only recompiles if the inlined code has changed.
+ Pretends to be secure.
+ Only requires standard ruby libraries, nothing extra to download.

== SYNOPSYS:

  require "inline"
  class MyTest
    inline do |builder|
      builder.c "
        long factorial(int max) {
          int i=max, result=1;
          while (i >= 2) { result *= i--; }
          return result;
        }"
    end
  end
  t = MyTest.new()
  factorial_5 = t.factorial(5)

== SYNOPSYS (C++):

  require 'inline'
  class MyTest
    inline(:C) do |builder|
      builder.include '<iostream>'
      builder.add_compile_flags '-x c++', '-lstdc++'
      builder.c '
        void hello(int i) {
          while (i-- > 0) {
            std::cout << "hello" << std::endl;
          }
        }'
    end
  end
  t = MyTest.new()
  t.hello(3)

== (PSEUDO)BENCHMARKS:

  > make bench

  Running native
  Type = Native   , Iter = 1000000, T = 28.70058100 sec, 0.00002870 sec / iter
  Running primer - preloads the compiler and stuff
  With full builds
  Type = Inline C , Iter = 1000000, T = 7.55118600 sec, 0.00000755 sec / iter
  Type = InlineRaw, Iter = 1000000, T = 7.54488300 sec, 0.00000754 sec / iter
  Type = Alias    , Iter = 1000000, T = 7.53243100 sec, 0.00000753 sec / iter
  Without builds
  Type = Inline C , Iter = 1000000, T = 7.59543300 sec, 0.00000760 sec / iter
  Type = InlineRaw, Iter = 1000000, T = 7.54097200 sec, 0.00000754 sec / iter
  Type = Alias    , Iter = 1000000, T = 7.53654000 sec, 0.00000754 sec / iter

== PROFILING STRATEGY:

0) Always keep a log of your progress and changes.
1) Run code with 'time' and large dataset.
2) Run code with '-rprofile' and smaller dataset, large enough to get good #s.
3) Examine profile output and translate 1 bottleneck to C.
4) Run new code with 'time' and large dataset. Repeat 2-3 if unsatisfied.
5) Run final code with 'time' and compare to the first run.

== REQUIREMENTS:

+ Ruby - 1.8.2 has been used on FreeBSD 4.6+ and MacOSX.
+ POSIX compliant system (ie pretty much any UNIX, or Cygwin on MS platforms).
+ A C/C++ compiler (the same one that compiled your ruby interpreter).
+ test::unit for running tests ( http://testunit.talbott.ws/ ).

== INSTALL:

+ make test  (optional)
+ make install

== LICENSE:

(The MIT License)

Copyright (c) 2001-2007 Ryan Davis, Zen Spider Software

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
