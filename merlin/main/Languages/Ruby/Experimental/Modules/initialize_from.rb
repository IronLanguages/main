module N
end

module M
  include N
  
  def foo
  end
  
  C = 1
  
  @@x = 1
end

def dump m
  puts "-- #{m} -----"
  p m.ancestors
  p m.included_modules
  p m.instance_methods(false)
  p m.constants()
  p m.class_variables()
end

dump M
dump M.dup

dump Object.dup
=begin

class B
  include N
end

class C < B
  include M   
end

dump C
dump C.dup

C.send(:initialize_copy, B) rescue p $!
C.send(:initialize) rescue p $!

X = Class.new
X.send(:initialize_copy, B) rescue p $!

Y = Class.new
Y.send(:initialize) rescue p $!

p M.send(:initialize_copy, N)
p M.send(:initialize)
dump M
=end