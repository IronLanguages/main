# coding: UTF-8

p "10".encoding
p "10".unpack("H")[0].encoding
p "a".ascii_only?
puts "Σa\u{1f}\u{12345}\xce".inspect
puts "Σa\u{1f}\u{12345}\xce".dump

puts '---'

p "Σ".ascii_only?

p "Σ".size
p "Σ".bytesize
p "Σ".bytes.entries
p "Σ".chars.collect { |x| x.dump[1..-2] }
p "Σ".codepoints.entries
p "Σ".getbyte(0)

puts '---' 
x = "Σ"
p x.ascii_only?
x.setbyte(1, 2)
p x.size rescue p $!
p x.bytes.entries
p x.chars.collect { |x| x.dump[1..-2] }
p x.codepoints.entries rescue p $!

=begin
ascii_only?
bytes
bytesize
chars
codepoints
each_byte
each_char
each_codepoint

encode
encode!
encoding
force_encoding

getbyte
setbyte
valid_encoding?
=end