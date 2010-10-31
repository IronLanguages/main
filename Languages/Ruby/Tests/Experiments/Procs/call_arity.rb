def test f
  puts "#{'-'*20} (#{f.arity}) #{'-'*20}"
  p begin f.call();0 end rescue p $!
  p begin f.call(1);1 end rescue p $!
  p begin f.call(1,2);2 end rescue p $!
  p begin f.call(1,2,3);3 end rescue p $!
  p begin f.call(1,2,3,4);4 end rescue p $!
end

test proc { || 1 }                # n == 0
test proc { |*| 1 }               # n >= 0
puts 'A' * 100
test proc { |x| 1 }               # warning if n != 1
test proc { |x,| 1 }              # n == 1
test proc { |x,*| 1 }             # n >= 1
puts 'B' * 100
test proc { |(*)| 1 }             # n >= 0
test proc { |(x,)| 1 }            # n == 1
test proc { |(x,*)| 1 }           # n >= 1
test proc { |(x,y)| 1 }           # n == 2
test proc { |(x,y,)| 1 }          # n == 2
test proc { |(x,y,z)| 1 }         # n == 3  
puts 'C' * 100
test proc { |x,y| 1 }             # n == 2
test proc { |x,y,| 1 }            # n == 2
test proc { |x,y,*| 1 }           # n >= 2
puts 'D' * 100
test proc { |x,y,z| 1 }           # n == 3
test proc { |x,y,z,| 1 }          # n == 3
test proc { |x,y,z,*| 1 }         # n >= 3

