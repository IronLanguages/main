module CLR
  def method_missing name, *a
    get = :"get_#{name.to_s.capitalize}"
    c = self.class == Class ? self : self.class
    
    if c.method_defined?(get) then
      send get, *a
    else
      raise
    end  
  end  
end


# CLR class
class C
  include CLR

  # Rubd instance method:
  def foo 
    p "C::foo"   
  end
  
  # CLR instance method:
  def get_Bar
    p "C::get_Bar"   
  end
  
  class << self
    include CLR
  
    # Rubd class method:
    def foo 
      p "S(C)::foo"   
    end
  
    # CLR static method:
    def get_Bar
      p "S(C)::get_Bar"   
    end    
  end
  
end

# Rubd class
class D < C
  
  alias_method :goo, :foo
  alias_method :hoo, :bar rescue puts 'Error hoo/bar'
  
  alias :goo2 :foo
  alias :hoo2 :bar rescue puts 'Error hoo2/bar'
  
end

# Rubd class
class E < C
  def method_missing name,*a
    puts 'mm'
  end
end

c = C.new
d = D.new
e = E.new

print 'd.foo -> '
d.foo

print 'd.bar -> '
d.bar

print 'd.get_Bar -> '
d.get_Bar

print 'd.goo -> '
d.goo

print 'd.hoo -> '
d.hoo rescue puts 'Error'

print 'd.goo2 -> '
d.goo2 

print 'd.hoo2 -> '
d.hoo2 rescue puts 'Error'

print 'C.foo -> '
C.foo

print 'C.bar -> '
C.bar

print 'e.foo -> '
e.foo

print 'e.bar -> '
e.bar
