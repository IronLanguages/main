def foo b, a = b+1
  
  
  return 1
  
  
end

alias :bar :foo

module M
  def m
  end
end

class C
  include M
  
  def c  
  end
  
  def C.s
  end  
  
end

x = C.new

class << x
  def z
  end
  $sx = self
end

p x

def a
end

set_trace_func(proc { |*args|
  if args[0] == "call"
    p args 
    p args[5].class
    eval('puts a', args[4])
  end
})

bar 1
x.c
x.m
C.s
x.z
