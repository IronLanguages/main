module M
  $p = lambda {    
    eval <<-EE
      module N
        def bar
        end
      end  
    EE
  }      
end


class C
  define_method :foo,&$p
end

class D
  class_eval &$p  
end

c = C.new
d = D.new

c.foo

p M.instance_methods(false)
p M::N.instance_methods(false)
p C.instance_methods(false)
p D.instance_methods(false)

