# should print 0 1 2 break

a = 3
10.times { |x|
  puts x
  a = a - 1
  eval('puts "break"; break') if (a == 0)
}

puts '---'

# should print:
# 0 redo 0 redo 0 1 2 3
# doesn't work in Ruby.NET (hangs since redo is redoing itself)

a = 10
4.times { |x|
  puts x
  a = a - 1
  eval('puts "redo"; redo') if (a > 0)
}

