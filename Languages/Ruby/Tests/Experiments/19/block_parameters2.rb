def foo(&b)
  $b = b
  yield 1,2,3,4,5
end

def goo
  foo do |a,b,c=:c,*d,e,&x|
    p [a,b,c,d,e,x]
    yield
  end
end

goo { puts 'ggg' }

$b.(1,2,3) { puts 'xxx' }

puts '---'

l = proc { |a,b,c;d,e|
  p local_variables
}

l.()

puts '---'

p0 = lambda { }
p1 = lambda { || }
p2 = lambda { |(x,y)| }
p3 = lambda { |x,w=1,q=2,*y,z,u| }
p4 = lambda { |x,y,&b| }
p5 = lambda { |x,y,| }
p6 = lambda { |x,y=1| }

p p0.arity
p p1.arity
p p2.arity
p p3.arity
p p4.arity
p p5.arity
p p6.arity