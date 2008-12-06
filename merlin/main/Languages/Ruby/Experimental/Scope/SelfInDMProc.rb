class C
  def foo
    $x = lambda {
      p self.class
    }
    
    $x.call
  end
end

C.new.foo

class D
  define_method :goo, &$x
end

D.new.goo