# coding: UTF-8

x='foo'

p x
p x.length

puts __ENCODING__ 
puts x.encoding

x[1] = "ᵹ"

p x
puts x.encoding
