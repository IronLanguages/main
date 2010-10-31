# encoding: utf-8

File.open("a.txt", "r:utf-8:sjis") do |f|

  x = f.read
  p x, x.encoding

end rescue p $!

File.open("b.txt", "r:binary:utf-8") do |f|

  x = f.read
  p x, x.encoding

end rescue p $!

File.open("b.txt", "r:utf-8:binary") do |f|

  x = f.read
  p x, x.encoding

end rescue p $!

File.open("b.txt", "r:sjis") do |f|

  x = f.read
  p x, x.encoding

end rescue p $!

File.open("b.txt", "r:binary:sjis") do |f|

  x = f.read
  p x, x.encoding

end rescue p $!

puts '-- read --'

File.open("a.txt", "r:utf-8") do |f|
  p [f.external_encoding, f.internal_encoding]

  b = f.read(1)
  p [b, b.encoding]
  
  buffer = "\0" * 10
  f.read(1, buffer)
  p [buffer, buffer.encoding]
  
  buffer = "\0" * 10
  buffer.force_encoding("sjis")
  f.read(1, buffer)
  p [buffer, buffer.encoding]
  
  f.read(nil, buffer)
  p [buffer, buffer.encoding]

end rescue p $!

puts '-rb-'

File.open("c.txt", "rb:utf-16be:utf-8") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("a.txt", "rb:binary") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("a.txt", "rb:us-ascii") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("a.txt", "rb:sjis") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("a.txt", "rb:utf-8") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("c.txt", "rb:binary") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

puts '-rt-'

File.open("c.txt", "rt:utf-16be:utf-8") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("a.txt", "rt:binary") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("a.txt", "rt:us-ascii") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("a.txt", "rt:sjis") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("a.txt", "rt:utf-8") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("c.txt", "rt:binary") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!

File.open("a.txt", "rt:utf-8:binary") do |f|
  x = f.read
  p [x, x.encoding]
end rescue p $!



