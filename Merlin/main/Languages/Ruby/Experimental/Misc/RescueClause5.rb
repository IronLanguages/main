# order of evaluation

class Module
  alias old ===

  def ===(other)
    puts "cmp(#{self}, #{other})"
    
    old other
  end
end

class A < Exception; end
class B < Exception; end
class C < Exception; end
class D < Exception; end
class E < Exception; end
class F < Exception; end
class G < Exception; end

def id(t)
  puts "r(#{t})"
  t
end

def foo
  raise F
rescue id(A),id(B),id(C)
  puts 'rescued 1'  # unreachable
rescue id(E),id(F),id(G)
  puts 'rescued 2'
end

foo
