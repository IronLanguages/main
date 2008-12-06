module N
   def x
     puts 'x'
   end    
   
   def z
   end
end

module NN
   include N
   def y
     puts 'y'
   end
   
   def foo
     puts 'foo'
   end
   
   def bar
     puts 'bar'
   end
   
   def w
   end
end

module M
   include NN
     
   module_function :x, :y
   
   # module_function makes singleton :foo public, instance :foo remains private
   private :foo
   module_function :foo, "bar"
      
   module_function
   def goo
     puts 'goo'
   end
     
end

p M.methods(false)   
p N.methods(false)
p NN.methods(false)
   
p M.instance_methods(false)   
p N.instance_methods(false)
p NN.instance_methods(false)

M.foo
M.bar
M.goo
M.x
M.y
