= ruby2ruby

* http://seattlerb.rubyforge.org/
* http://rubyforge.org/projects/seattlerb

== DESCRIPTION:

ruby2ruby provides a means of generating pure ruby code easily from
ParseTree's Sexps. This makes making dynamic language processors much
easier in ruby than ever before.

== FEATURES/PROBLEMS:
  
* Clean, simple SexpProcessor generates ruby code from ParseTree's output.

== SYNOPSYS:

    RubyToRuby.translate(MyClass, :mymethod) # => "def mymethod..."

== REQUIREMENTS:

+ ParseTree

== INSTALL:

+ sudo gem install ruby2ruby

== LICENSE:

(The MIT License)

Copyright (c) 2006-2007 Ryan Davis

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
'Software'), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
