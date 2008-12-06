module M
  $p = lambda {    
    def goo
      eval <<-EE
        module N
          def bar
            puts 'bar'
          end
        end  
      EE
    end  
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

d.goo
c.foo
c.goo

p M.instance_methods(false)
p M::N.instance_methods(false)
p C.instance_methods(false)
p D.instance_methods(false)

