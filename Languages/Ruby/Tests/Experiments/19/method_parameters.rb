def f1 a,b,c='c',d='d',*e,f,g,&h
  p [a,b,c,d,e,f,g,h]
end

f1 1,2,3,4,5
f1 1,2,3,4,5,6,7
f1 1,2,3,4,5,6,7,8
f1 1,2,3,4,5,6,7,8,9

def f2((a,(b,c)))
  p a,b,c
end

f2 [1,[2,3]]

def f3 (a,(b,c))
  p a,b,c
end

f3 1,[2,3]

def f4 (a,(b,*c))
  p a,b,c
end

f4 1,[2,3,4]

puts '--arity--'

def f5 a,b=1,c; end
def f6 a,b=1,c=1; end
def f7 b=1,c=1; end
def f8 b=1,c=1,*d; end
def f9 *d,b,c; end

def f10 a,b,c=1; end                       # -3
def f11 a,b=1,c; end                       # -3
def f12 a,b=1,c,d,e,f; end                 # -6
def f13 a,b=1,*x,c,d,e,f; end              # -6
def f14 a,b,*x,c,d,e,f; end                # -7
def f15 a,b=1,c=1,d=1,e=1,f=1; end         # -2

self.private_methods(false).each { |m| puts "#{m}.arity = #{method(m).arity}" }