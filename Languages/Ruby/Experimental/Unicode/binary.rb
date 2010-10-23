# encoding: BINARY

puts '- dump -'

puts "Σ".dump
puts "hello: Σ".dump
puts "Σ\xce".dump
#puts "\u{03a3}".dump
puts "\xe2\x85\x9c".dump
puts "\xF0\x92\x8D\x85".dump
puts "\xF0\x92\x8D".dump
puts "\xF0\x92".dump
puts "\xF0".dump

puts '- inspect -'

puts "Σ".inspect
puts "hello: Σ".inspect
puts "Σ\xce".inspect
#puts "\u{03a3}".inspect
puts "\xe2\x85\x9c".inspect
puts "\xF0\x92\x8D\x85".inspect
puts "\xF0\x92\x8D".inspect
puts "\xF0\x92".inspect
puts "\xF0".inspect
