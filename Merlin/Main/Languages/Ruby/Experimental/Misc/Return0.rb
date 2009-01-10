def m1() 
  puts 'm1'
  yield
end

def defb(&b)
  $b = b
end

def defc(&c)
  $c = c
end

def m2(a, &b)
  print "m2.begin(#{a})"
  if (a == 0)
      puts " calling m1(&b)"
      m1 &$b
  elsif (a == 4)
      puts " defining block 'b'"
      defb do
        puts 'b1'
        1.times { |x|
          puts 'b2'
          1.times { |y|
            puts 'b3'
            eval('return')
          }
        }
      end
      m2(a - 1)
  elsif (a == 8)
      puts
      m2(a - 1)
      
      puts "  calling m1(&c)"
      m1 &$c
  elsif (a == 10)
      puts " defining block 'c'"
      defc {
        puts 'c1'
        1.times { |x|
          puts 'c2'
          1.times { |y|
            puts 'c3'
            return
          }
        }
      }
      m2(a - 1)
  else 
    puts
    m2(a - 1)
  end
  puts "m2.end(#{a})"
end

m2(12)

puts '-----'

def br
    puts 'd1'
        1.times { |x|
          puts 'd2'
          1.times { |y|
            puts 'd3'
            break
          }
        }
    puts 'd4'
            
end

br   
     