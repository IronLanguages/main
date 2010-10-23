i = 0
x = 5.times { |x|
  puts x
  i = i + 1  
  if i < 3 then next end
  puts 'bar'
}