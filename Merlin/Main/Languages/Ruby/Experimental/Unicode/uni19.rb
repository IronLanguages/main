# encoding: UTF-8

p eval('__ENCODING__')

# MRI 1.9 handles incomplete characters
# Allows concating bytes to complete the incomplete chars.

puts "Σ".dump
puts "hello: Σ".dump
puts "Σ\xce".dump
puts "\u{03a3}".dump
puts "\xe2\x85\x9c".dump

puts '---'

puts "Σ\xce".inspect
puts "\xe2\x85\x9c".inspect

puts '---'

puts 'ab'.encoding
puts "abΣ".encoding
puts "abΣ\xce\xa3".encoding
puts "\xce".dump

puts '---'

a = "\xce"
b = "\xa3"

puts a.dump, a.encoding
puts b.dump, b.encoding
puts (a + b).dump

puts '---'

puts "\xce#{b}".dump

puts '---'

# explicit character stops escapes from completion
puts "\xceX".dump

s = "\xceX\xa3"
puts s.dump
puts s[0].dump, s[1].dump, s[2].dump
puts '---'
s[1] = s[2]
puts s.dump
puts s[0].dump, s[1].dump
puts '---'

puts s.delete('X') rescue p $! 

puts :x.encoding
puts :'ab'.encoding
puts :"abΣ".encoding
puts :"abΣ\xce\xa3".encoding

