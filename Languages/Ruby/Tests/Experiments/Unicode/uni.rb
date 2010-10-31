s = ""
j = 0

1000.times { |x|
  s << (x >> 8)
  s << (x & 0xff)
}

p s