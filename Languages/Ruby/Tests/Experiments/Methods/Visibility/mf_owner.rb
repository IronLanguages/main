class C
end
$c = C.new

module M
  def $c.foo         # owner is S($c)    
  end
end

p $c.methods(false)

puts '---'

module O
  module_function
  
  define_method(:foo) {                
  
    # 1.8: defines private O_C::mf, public S(O_C)::mf  (!)  
    # 1.9: defines public (!) O::mf, public S(O)::mf
    # IR: defines private O::mf, public S(O)::mf
    def mf                              
    end    
  }
  
end
  
class O_C
  include O
  new.send :foo
end

p O.public_instance_methods(false)
p O.private_instance_methods(false)
p O.singleton_methods(false)
puts '---'
p O_C.public_instance_methods(false)
p O_C.private_instance_methods(false)
p O_C.singleton_methods(false)


