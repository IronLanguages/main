class B
  def foo 
     puts 'super'
  end
end

class C < B
  def foo x
    super()
    a = 1
    puts 'foo'        
  end
end

x = C.new
m = x.method :foo
p m
1.times &m.to_proc

def foo m
  Proc.new { |*args| m.call(*args) }
end

#q = foo(m)

z = m.to_proc
eval("p local_variables", z)
eval("p self", z)


