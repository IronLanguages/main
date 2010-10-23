def f x
  print x, ' '   
end

def x a
  print 'x '
  X
end

def y a
  print 'y '
  Y
end

class X
  def self.y z
    print 'X#y '
    Y
  end
end

class Y
  C = 1
  def self.const_missing(name)
    print "?#{name}"
  end  
end

def e x
  puts "raising exception #{x}"
  raise
end


# invalid number of params -> ok, no method calls
p defined?(x(x(1),2,3))

puts '--'

# invalid number of params -> const undefined
p defined?(y(1,2,3)::C)

puts '--'

# invalid number of params -> const undefined
p defined?(e(1)::C)
p defined?((e(1) rescue p $!;Y)::C)
p $!

puts '--'
p defined?(y(f(1))::C)
puts '--'
p defined?(x(f(1))::y(f(2))::C)
puts '--'
p defined?(y(f(1))::D)
puts '--'
p defined?(x(f(1))::y(f(2))::D)

puts '--'
p defined?(e(1)::e(2))
p defined?(x(1,3).y(2,3))

