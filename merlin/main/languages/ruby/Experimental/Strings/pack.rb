p [61].pack("h") rescue p $!
p [nil].pack("H*")

p ["616263"].pack("H*")

p [""].pack("H*")
p ["6"].pack("H*")
p ["60"].pack("H*")
p ["61"].pack("H*")
p ["616"].pack("H*")
p ["6160"].pack("H*")
p ["6162"].pack("H*")

puts '---'

p [""].pack("h*")
p ["6"].pack("h*")
p ["06"].pack("h*")
p ["16"].pack("h*")
p ["161"].pack("h*")
p ["1606"].pack("h*")
p ["1626"].pack("h*")

puts '---'


p ["6162"].pack("H0")
p ["6162"].pack("H1")
p ["6162"].pack("H2")
p ["6162"].pack("H3")
p ["6162"].pack("H4")
p ["6162"].pack("H5")
p ["6162"].pack("H6")
p ["6162"].pack("H7")
p ["6162"].pack("H8")
p ["6162"].pack("H20")

puts '---'

(0..255).each { |b| 
  puts "#{b.chr.inspect} -> #{[b.chr].pack("h1")[0]}" rescue p $!
}


p u = 239.chr.unpack("H*")
p u.pack("H*")[0]


