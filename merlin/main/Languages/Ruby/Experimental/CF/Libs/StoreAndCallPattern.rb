h = Hash.new {
  puts 'foo'
  break 'zoo'
  'bar'
}


puts h.default_proc
puts h.default

puts '---'
puts h['bob']
