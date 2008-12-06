puts 'Begin'
x = class A
  puts 'Raise'
  1
  raise
  puts 'Unreachable'
  2
rescue IOError
  puts 'Rescue1'
  3
rescue
  puts 'Rescue2'
  4
else
  puts 'Else'
  6
ensure
  puts 'Ensure'
  5 
end
puts x
puts 'End'