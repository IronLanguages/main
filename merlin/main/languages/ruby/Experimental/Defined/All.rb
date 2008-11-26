class C
  def foo
    @x = 1
    p defined? @x
    
    @y = nil
    p defined? @y
    
    p defined? @z
    
    @@y = 1
    p defined? @@y

    @@y = nil
    p defined? @@y
    
    p defined? @@z   
    
  end
end

puts 1
p defined? C

puts 2
C.new.foo

puts 3
x1 = 1
1.times {|x2| 
  p defined? x1
  p defined? x2 
}

puts 4
p defined? 1

puts 5
p defined? dummy

puts 6
p defined? puts

puts 7
p defined? String

"123" =~ /(1)/

puts 8
p defined? $HELLO
p defined? $-
p defined? $-a
p defined? $-x
p defined? $!
p defined? $@
p defined? $:
p defined? $;
p defined? $"
p defined? $,
p defined? $;
p defined? $/
p defined? $\
p defined? $*
p defined? $$
p defined? $?
p defined? $=
p defined? $:
p defined? $"
p defined? $<
p defined? $>
p defined? $.
p defined? $0

p defined? $~
p defined? $&
p defined? $`
p defined? $'
p defined? $+
p defined? $1

puts 9
p defined? Math::PI

puts 10
p defined? a = 1
p defined? a += 1     #assignment
p defined?(a &&= 1)    #expression

puts 11
p defined? a

puts 12
p defined? 42.times
p defined? Kernel::puts

puts 13
p defined? 1 + foo
p defined? def foo; end
p defined? lambda {}

puts 14
p (s = defined? :foo)
p (t = defined? :bar)
p s.object_id == t.object_id

puts 15
p defined? nil
p defined? true
p defined? false
p defined? self

Q = nil

puts 16
p defined? P
p defined? Q

=begin
TODO: super is complicated - define_method, etc.

puts 17
class C
  def f
    p defined? super
  end  
end

class D < C
  def f
    p defined? super
    super
  end
end

D.new.f
=end

puts 18
  
def g 
  p defined? yield  
end
g
g {}

puts 19

undef g
undef g rescue p $!
p defined? g



