[
"l", "L",
"i", "I", 
"s", "S",
"n", "N", 
"v", "V", 
"c", "C", 
].each { |f| 

puts "-- #{f} --"
p [0xffffffff].pack(f) rescue p $!
p [0x100000000].pack(f) rescue p $!
p [-1].pack(f) rescue p $!

}

puts "========================"

["q", "Q"].each { |f| 

puts "-- #{f} --"
p [0xffffffff].pack(f) rescue p $!
p [0x100000000].pack(f) rescue p $!
p [0xffffffffffffffff].pack(f) rescue p $!
p [0x10000000000000000].pack(f) rescue p $!
p [-1].pack(f) rescue p $!

}

puts "========================"

["l", "L",
"i", "I", 
"s", "S",
"n", "N", 
"v", "V", 
"c", "C",
].each { |f|
  puts "-- #{f} --"
  p [0x12345678].pack(f).unpack("H*") rescue p $!
  p [0x1234].pack(f).unpack("H*") rescue p $!
}