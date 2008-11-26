class Module
  public :private, :public, :module_function
end

module M
end

class C
  def foo
    1.times {
      M.private
  
      def pri1
      end
    }
  
    def pubXpri2             # private in 1.9, public in 1.8 (!) 
    end
  end
  
  new.foo

  def pub3
  end
end

p C.public_instance_methods(false).sort
p C.private_instance_methods(false).sort
puts '--'

class D
  1.times {
    private
    
    def pri1
    end
  }
  
  def pubXpri2               # private in 1.9, public in 1.8 (!) 
  end
end

p D.public_instance_methods(false).sort
p D.private_instance_methods(false).sort
puts '--'

class F1
  module_eval {
    private
    
    def pri1
    end
  }
  
  def pub2               # public in 1.8 and 1.9, ok
  end
end

p F1.public_instance_methods(false).sort
p F1.private_instance_methods(false).sort

puts '--'

class F2
  define_method(:foo) {
    M.private
    
    def pri1
    end
  }
  
  new.foo
  
  def pubXpri2               # public in 1.8, private in 1.9 (!)
  end
end

p F2.public_instance_methods(false).sort
p F2.private_instance_methods(false).sort

puts '--- module_function ---'
puts '- G -'

class G
  1.times {
    M.module_function
    
    def mf1
    end
  }
  
  def iXmf2                # mf in 1.9, instance in 1.8 (!)             
  end
rescue
  puts $!
end

p G.singleton_methods(false).sort
p G.private_instance_methods(false).sort
p G.public_instance_methods(false).sort
puts '- H -'

class H
  module_eval {
    M.module_function
    
    def mf1
    end
  }
  
  def i2               
  end
rescue
  puts $!
end

p H.singleton_methods(false).sort
p H.private_instance_methods(false).sort
p H.public_instance_methods(false).sort

puts '- I -'

class I
  define_method(:bar) {
    M.module_function
    
    def mf1
    end
  }
  
  new.bar
  
  def iXmf2                      # instance in 1.8, mf in 1.9 (!) 
  end
rescue
  puts $!
end

p I.singleton_methods(false).sort
p I.private_instance_methods(false).sort
p I.public_instance_methods(false).sort

puts '------ flag inheritance ----'
puts '- J -'
class J
  private
  1.times {                        # inherits
    1.times {                      # inherits
      def pri
      end
    }
  }
end

p J.public_instance_methods(false).sort
p J.private_instance_methods(false).sort

puts '- K -'
class K
  private
  module_eval {                    # doesn't inherit
    def pub1
    end
      
    private
    define_method(:priXpub1) {         # inherits (!); 
      def priXpub2                 # 1.9: public (!)
      end
      
      M.private
      class X                      # doesn't inherit
        def pub3
        end
      end
    }
  }
  
  new.send :priXpub1
end

p K.public_instance_methods(false).sort
p K.private_instance_methods(false).sort
p K::X.public_instance_methods(false).sort
p K::X.private_instance_methods(false).sort

puts '- L (1.9 bad visibility) -'
module L
  module_function
  
  1.times {                        
    1.times {                      
      def mf
      end
    }
  }
end

p L.public_instance_methods(false).sort
p L.private_instance_methods(false).sort
p L.singleton_methods(false).sort

puts '- N -'
module N
  private  
  module_function                      # overrides visibility to public
  
  module_eval {                        # doesn't inherit
    def mf
    end    
  }
end

p N.public_instance_methods(false).sort
p N.private_instance_methods(false).sort
p N.singleton_methods(false).sort

puts '- O (1.8 and 1.9 bugs), -'
module O
  module_function
  
  # 1.8: foo private, not mf (!)
  # 1.9: foo public, not mf
  define_method(:priXpub1) {                  # inherits 1.9 (!)
  
    # 1.8: defines O_C::mf, S(O_C)::mf  (!)
    # 1.9: defines O::mf, S(O)::mf
    def mf                              
    end    
  }
  
end
  
class O_C
  include O
  new.send :priXpub1
end

p O.public_instance_methods(false).sort
p O.private_instance_methods(false).sort
p O.singleton_methods(false).sort
p O_C.public_instance_methods(false).sort
p O_C.private_instance_methods(false).sort
p O_C.singleton_methods(false).sort

puts '- P -'
class P
  protected
    
  module_eval {               
    def pub1; end             # looks for the inner-most module/(real (!) method) scope
    private                   
    def pri2; end             
  }
  
  define_method(:priXpub0) {               
    def pro3; end             # looks for the inner-most module/(real (!) method) scope
    M.private                   
    def pri4; end             
  }
  
  new.send :priXpub0
  
  def self.bar
    private
  
    module_eval {               
      def pub5; end             # looks for the inner-most module/(real (!) method) scope
      public
      def pub6; end             
    }
  
    define_method(:priXpub10) {               
      def pri7; end             # looks for the inner-most module/(real (!) method) scope
      M.public                  
      def pub8; end             
    }
    
    new.send :priXpub10
    
    def priXpub9; end           # 1.8: pri, 1.9: pub (!)
  end
  
  bar
  
end

p P.public_instance_methods(false).sort
p P.private_instance_methods(false).sort
p P.protected_instance_methods(false).sort

