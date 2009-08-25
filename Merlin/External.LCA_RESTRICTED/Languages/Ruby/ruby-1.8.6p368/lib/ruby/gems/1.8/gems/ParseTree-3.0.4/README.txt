= ParseTree

* http://rubyforge.org/projects/parsetree/
* http://www.zenspider.com/ZSS/Products/ParseTree/
* ryand-ruby@zenspider.com

== DESCRIPTION:

ParseTree is a C extension (using RubyInline) that extracts the parse
tree for an entire class or a specific method and returns it as a
s-expression (aka sexp) using ruby's arrays, strings, symbols, and
integers.

As an example:

  def conditional1(arg1)
    if arg1 == 0 then
      return 1
    end
    return 0
  end

becomes:

  [:defn,
    :conditional1,
    [:scope,
     [:block,
      [:args, :arg1],
      [:if,
       [:call, [:lvar, :arg1], :==, [:array, [:lit, 0]]],
       [:return, [:lit, 1]],
       nil],
      [:return, [:lit, 0]]]]]

== FEATURES/PROBLEMS:

* Uses RubyInline, so it just drops in.
* Uses UnifiedRuby by default, automatically rewriting ruby quirks.
* ParseTree#parse_tree_for_string lets you parse arbitrary strings of ruby.
* Includes parse_tree_show, which lets you quickly snoop code.
  * echo "1+1" | parse_tree_show -f for quick snippet output.
* Includes parse_tree_abc, which lets you get abc metrics on code.
  * abc metrics = numbers of assignments, branches, and calls.
  * whitespace independent metric for method complexity.
* Includes parse_tree_deps, which shows you basic class level dependencies.
* Does not work on the core classes, as they are not ruby (yet).

== SYNOPSYS:

  sexp_array = ParseTree.translate(klass)
  sexp_array = ParseTree.translate(klass, :method)
  sexp_array = ParseTree.translate("1+1")

or:

  % ./parse_tree_show myfile.rb

or:

  % echo "1+1" | ./parse_tree_show -f

or:

  % ./parse_tree_abc myfile.rb

== REQUIREMENTS:

* RubyInline 3.6 or better.

== INSTALL:

* sudo gem install ParseTree

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
