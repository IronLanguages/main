puts '---'

i = 0
while begin puts 'c'; i < 3; end do
  puts 'a'
  i = i + 1
  next
  puts 'b'
end

puts '---'

i = 0
while begin puts 'c'; i < 3; end do
  puts 'a'
  i = i + 1
  redo unless i > 4
  puts 'b'
end
