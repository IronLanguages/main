#!/usr/local/bin/ruby17 -w

begin
  require 'rubygems'
rescue LoadError
  $: << 'lib'
end
require 'inline'

class MyTest

  inline do |builder|

    builder.add_compile_flags %q(-x c++)
    builder.add_link_flags %q(-lstdc++)

    builder.c "
// stupid c++ comment
#include <iostream>
/* stupid c comment */
static
void
hello(int i) {
  while (i-- > 0) {
    std::cout << \"hello\" << std::endl;
  }
}
"
  end
end

t = MyTest.new()

t.hello(3)
