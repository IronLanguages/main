module M
  C = 1
  
  def foo
    puts "foo: #{C}" 
    
    def foo2
      puts "foo2: #{C}"
    end
  end 

  define_method(:bob) { puts "bob: #{C}"; }
end

class C
  include M
  C = 2
  
  def bar
    puts "bar: #{C}"
    
    def bar2
      puts "bar2: #{C}"      
    end
  end
  
  define_method(:baz) { puts "baz: #{C}" }
end

x = C.new

x.foo
x.foo2
x.bar
x.bar2
x.bob
x.baz

M.module_eval('def f; puts "f: #{C}"; end')
C.module_eval('def g; puts "g: #{C}"; end')

x.f
x.g

x.instance_eval('def h; puts "h: #{C}"; end')

x.h






