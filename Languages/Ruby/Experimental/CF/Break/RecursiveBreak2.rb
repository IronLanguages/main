def foo &p
  puts 'foo1'
  3.times &p
  puts 'foo2'
end

def goo
  puts 'goo1'
  foo { |x| puts x; break }
  puts 'goo2'
end

goo