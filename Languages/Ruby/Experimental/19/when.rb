def t(i)
  puts "t#{i}"
  true
end

def f(i)
  puts "f#{i}"
  false
end

case
  when *[f(1),f(2),f(3)], *[f(4), f(5)]; puts 'a'
  when *[t(6)]; puts 'b'
end