
x = [1].fetch(-5) { |i|
  puts i
  'missed'
  break
}

puts x