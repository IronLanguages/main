def i *a
  puts '---'
  a.each { |x| print x.inspect, ',' }
  puts
end

x1,(*x2),(x3,*x4) = 1,2,[1,2],[3,4]
i x1,x2,x3,x4

puts '***'

def bar
  yield [1,2,3],[4,5], [6]
end

bar { |(a,*b),*y| i a,b,y }  # 1, [2,3], [[4,5],[6]]
bar { |(*a),(*b),(*c)| i a,b,c } # [[1,2,3]], [[4,5,]], [[6]]

puts '*** baz ***'

def baz
  yield [1,2,3]
end

baz { |a| i a }              # [1,2,3]
baz { |a,b| i a,b }          # 1,2
baz { |a,b,c| i a,b,c }      # 1,2,3
baz { |*a| i a }             # [[1,2,3]]  (!)
baz { |a,*b| i a,b }         # 1, [2,3]

puts '*** foo ***'

def foo
  yield 1,2,3
end

foo { |a| i a }              # [1,2,3] W
foo { |a,b| i a,b }          # 1,2
foo { |a,b,c| i a,b,c }      # 1,2,3
foo { |*a| i a }             # [1,2,3]
foo { |a,*b| i a,b }         # 1, [2,3]


puts '*** goo ***'

def goo
  yield *[1,2,3]             
end

goo { |a| i a }              # [1,2,3] W
goo { |a,b| i a,b }          # 1,2
goo { |a,b,c| i a,b,c }      # 1,2,3
goo { |*a| i a }             # [1,2,3]   (!)
goo { |a,*b| i a,b }         # 1, [2,3]

puts '********'

x = [1,2,3]
*z = x
i z
*z = *x
i z




