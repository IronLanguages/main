puts 'Begin'
x = class C
  puts 'Raise'
  1
rescue IOError
  puts 'Rescue1'
  3
else
  puts 'Else'
  5
ensure
  puts 'Ensure'
  6 
end
puts x
puts 'End'