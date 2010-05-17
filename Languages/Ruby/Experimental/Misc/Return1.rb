def y() 
  puts 'yielding'
  yield
end

def defb(&b)
  $b = b
end

def defc(&c)
  $c = c
end

def defd(&d)
  $d = d
end

def foo(a)
  print "foo.begin(#{a})"
  if (a == 10)
    puts " defining block 'b'" 
    defb {
      puts 'in b' 
      return
    }
    foo(a - 1)
  elsif (a == 7)
    puts " defining block 'c'"
    defc {
      puts 'in c' 
      return
    }
    foo(a - 1)
  elsif (a == 4)
    puts " calling &c" # try b,c,d
    y &$c
    foo(a - 1)
  elsif (a == 1)
    puts " calling &b"  # try b,c,d
    y &$b
    foo(a - 1)
  elsif (a == 0)
  else
    puts
    goo(2, a)
  end
  puts "foo.end(#{a})"
rescue Exception => e
  puts 'Caught!'  # unreachable
ensure
  puts "foo.ensure(#{a})"
end

def goo(b, a)
  print "  g.begin(#{a}, #{b})"
  if (a == 9 and b == 1)
    puts " defining block 'd'"
    defd {
      puts 'in d' 
      return
    }
    goo(b - 1, a)
  elsif (a == 5 and b == 2)
    puts " calling &b"  # try b,c,d
    y &$b
    goo(b - 1, a)
  elsif (b == 0)
    puts
    foo a - 1
  else
    puts
    goo(b - 1, a)
  end
  puts "  g.end(#{a}, #{b})"
rescue Exception => e
  puts 'Caught!'  # unreachable
ensure
  puts "goo.ensure(#{a},#{b})"
end

foo 12