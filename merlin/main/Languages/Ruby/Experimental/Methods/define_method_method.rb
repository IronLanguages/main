class Module
  def method_added name
    puts "+#{name}"
  end
end

class D
  def bar
    puts 'bar'
  end
end

class Method
  def to_proc
    puts 'to_proc'
  end
  
end

$m = D.new.method(:bar)
p $m.class
p $m

$u = $m.unbind

class C
  define_method(:foo, $m) rescue p $!
  define_method(:ufoo, $u) rescue p $!
end

puts 'call'

C.new.ufoo rescue p $!
C.new.foo rescue p $!

class C
  def x
    puts 'x'
  end
  
  p define_method(:y, self.new.method(:x))
  p define_method(:z, self.new.method(:x).unbind)
end

C.new.y
C.new.z


  
