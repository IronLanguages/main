#!/usr/bin/ruby

x='ᴧ'

p x
p x.length

begin
  puts __ENCODING__ 
  puts x.encoding
rescue
end