
p "abc".unpack("h")
p "abc".unpack("h0")
p "abc".unpack("h1")
p "abc".unpack("h2")
p "abc".unpack("h3")
p "abc".unpack("h4")
p "abc".unpack("h5")
p "abc".unpack("h6")
p "abc".unpack("h7")

puts '---'

p "abc".unpack("H")
p "abc".unpack("H0")
p "abc".unpack("H1")
p "abc".unpack("H2")
p "abc".unpack("H3")
p "abc".unpack("H4")
p "abc".unpack("H5")
p "abc".unpack("H6")
p "abc".unpack("H7")

puts '---'

p "\xef".unpack("H7")
p "\xef".unpack("h7")

puts '---'

p "abcd".unpack("H*")
p "abcd".unpack("H*")

