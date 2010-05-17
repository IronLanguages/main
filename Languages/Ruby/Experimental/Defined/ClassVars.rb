module M
  def foo; @@foo = 1; end
  def foo_defined_on_M?
     p defined? @@foo                  
  end
end

module N
  def foo_defined?
     p defined? @@foo                  
  end
end

class C
  include M,N    
end

c = C.new
c.foo
c.foo_defined?
c.foo_defined_on_M?