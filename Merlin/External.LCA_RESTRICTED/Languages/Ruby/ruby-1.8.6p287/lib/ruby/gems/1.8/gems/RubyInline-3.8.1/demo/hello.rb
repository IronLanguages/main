#!/usr/local/bin/ruby -w

begin require 'rubygems' rescue LoadError end
require 'inline'

class Hello
  inline do |builder|
    builder.include "<stdio.h>"
    builder.c 'void hello() { puts("hello world"); }'
  end
end

Hello.new.hello
