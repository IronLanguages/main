# coding:UTF-8

p __ENCODING__

p "  foo ".strip

a = "\xe1\x9a\x80foo\000"
p a.encoding
p a.strip