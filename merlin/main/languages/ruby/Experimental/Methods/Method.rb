class Foo
  def foo
    puts 'foo'
  end
  
  alias bar foo
  
end

f = Foo.new
g = Foo.new

m1 = f.method :foo
m2 = f.method "bar"
m3 = f.method :bar
m4 = g.method :bar

p m1
p m2
p m3
p m4

# ids are all different
p m1.object_id
p m2.object_id
p m3.object_id
p m4.object_id

p m1 == m2 # true
p m1 == m3 # true
p m2 == m3 # true

p m1 == m4 # false
p m2 == m4 # false
p m3 == m4 # false

puts m1.to_s

puts '-' * 20

u1 = m1.unbind
u2 = m2.unbind
u3 = m3.unbind
u4 = m4.unbind
u44 = m4.unbind

# ids are all different
p u1.object_id
p u2.object_id
p u3.object_id
p u4.object_id
p u44.object_id

p u1
p u2
p u3
p u4
p u44

puts '-' * 20

p m1.clone
p u1.clone
 
class Method
  remove_method :clone
end

class UnboundMethod
  remove_method :clone
end

p m1.clone rescue p $!
p u1.clone rescue p $!

Object.new.method :foo rescue p $!

puts '-' * 20


module M
  def bar
    puts 'bar'
  end
end

class B
  def foo a,b
    puts a * b
  end
end

class C < B
  include M

  def foo a,b
    puts a + b
  end
end

class D < C
end

class E
  include M
end

b = B.new
c = C.new
d = D.new
e = E.new
s = class << d; self; end

p m = c.method(:foo)
p u = m.unbind
p ud = u.bind(d)

u.bind(nil) rescue p $!
u.bind(s) rescue p $!
u.bind(b) rescue p $!

m[1,2]
ud[1,2]

puts '-'*20

mbar = c.method(:bar)
ubar = mbar.unbind

p mbar
p ubar
ubar.bind(e) rescue p $!

puts '-'*20

im_bar = M.instance_method :bar
p im_bar.bind(e)   # Ruby 1.8 displays Object(M), Ruby 1.9 corrects it to E(M)
p im_bar.bind(Object.new) rescue p $!
p im_bar.bind(nil) rescue p $!






