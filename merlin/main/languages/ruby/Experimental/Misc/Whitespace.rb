y = 1
z = 2
b = 8
c = [1,[2,3]]

def y(a)
  a * 10 
end

def z(a,b)
  b
end

def z?(a)
  a == 10
end

def z!(a)
  a == 20
end

# all these are the same (although Ruby Way claims they're different)
puts y + z            # 3    
puts y+z              # 3
puts y+ z             # 3
puts y +z             # 3  
puts y(+z)            # 20
puts y (+z)           # 20
puts y z              # 20, warning: parenthesize argument(s) for future version
puts y -z             # -1
puts -y -z            # -3
puts +y +z            # 3
puts -y(z)            # -20
puts -y (z)           # -20
# error puts -y z
# error puts +y z

puts z*b              # 16
puts z * b            # 16
puts z* b             # 16 
puts z *b             # 16 
puts z(*c)            # 2\n3

puts z?(1)            # false
puts z?(10)           # true
puts z ? 1 : 2        # 1
puts z? 10 ?1:2       # false
puts (z? 10) ?1:2     # 1
puts (z! 20) ?1:2     # 1
puts (!z? 10) ?1:2    # 2
puts (!z! 20) ?1:2    # 2
puts (!z! 20) ?!1:!2  # false

#error puts z?(1) : 2





