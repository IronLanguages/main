puts '--- 1 ---'
a,b,c = *nil
p a,b,c

puts '--- 2 ---'
a,b,c = *1
p a,b,c

puts '--- 3 ---'
a,b,c = *[]
p a,b,c

puts '--- 4 ---'
a,b,c = *[1]
p a,b,c

puts '--- 5 ---'
a,b,c = 1,*[2]
p a,b,c

puts '--- 6 ---'
a,b,c = *[1],*[2]
p a,b,c

puts '--- 7 ---'
a,b,*c,d,e = 1,2,3
p a,b,c,d,e

puts '--- 8 ---'
x = [1,2,3]
a,b,*c,d,e = x
p a,b,c,d,e

puts '--- 9 ---'
x = [1,2,3]
a,b,*c,d,e = *x
p a,b,c,d,e

puts '--- 10a ---'
x = [1,2,3]
*a = x
p a

puts '--- 10b ---'
x = [1,2,3]
a, = x
p a

puts '--- 10c ---'
x = [1,2,3]
a,b = x
p a,b

puts '--- 11 ---'
x = [1,2,3]
*a = *x
p a

puts '--- 12 ---'
x = [1,2,3]
*a = *1
p a

puts '--- 13 ---'
a = 1,2,3
p a

puts '--- 14 ---'
a = *[1,2,3]
p a

puts '--- 15 ---'

class C
  def to_ary
    puts 'to_ary'
    ['a','b']
  end
end

x = C.new
z = (*a = x)
p z,a

puts '--- 16 ---'
x = C.new
z = (a,b = x)
p z,a,b

puts '--- 17 ---'
x = [1,2]
z = ((((a,))) = x)
p z,a

puts '--- 18 ---'
class D
  def to_ary
    puts 'to_ary'
    [2,3]
  end
end

a,(b,c),d = 1,D.new,4
p a,b,c,d
