# class C
#   <statements>
# end 
#
# means something like
#
# if !defined(C) then C = Class.new('C')
# <statements>_function(C, <statements>)
#
# global code -> function taking self, the self created by load/require
# 

u = nil
a = 1

puts u                  # nil
# error puts v          # undefined local

puts '- D ----------------------------'

class D 
    puts '  in D:'
    b = 'b'
    C = 1
    puts '    locals: ',local_variables()  # b
    puts '    constants: ',constants()  # C
end

puts '- global ----------------------------'

@@cls = 'cls'

puts 'locals in global code: ',local_variables()  # u,a,f
# undefined class_variables(): puts 'class vars in global code: ',class_variables()  # @@cls
# undefined constants(): puts 'constants in global code: ',constants() # D

puts '- class << Object.new ----------------------------'

class << Object.new
    puts '  in singleton:'
    class E
    end
    puts '    class vars:',class_variables()  # @@cls
    puts '    constants: ',constants()  # E
    class EE
    end
end

puts '- F ----------------------------'

module FM
  def FM.d()
    puts 'static d in FM'
  end
  
  def d()
    puts 'instance d in FM'
  end
end

class F
    puts '  in F:'
    puts self      #F

    def m()
        puts 'instance m in F:',self  #<Object of F>
    end

    def F.m() 
        puts 'class m in F:',self  #F
    end
    
    def F.k()
      puts 'static k in F'
    end
    
    def print(v)
      puts 'instance print in F'
    end
    
    include FM
    
    def g()
      puts 'in g():'
      m                         # instance m in F
      self.m                    # instance m in F
      # k                       # undefined 
      # self.k                  # undefined    
      d                         # instance d in FM
      self.d                    # instance d in FM
      # F.d                     # undefined
      FM.d                      # static d in FM
      print 'foo'               # instance print in F
    end
end

f = F.new

def F.n()
    puts 'class F.n:',self      #F
end

def f.n()
    puts 'instance f.n:',self      #<Object of F>
end

f.m
F.m
f.n
F.n
f.g

# undefined method n (n is just on object f): F.new.n

puts '- class self ----------------------------'

class G
  @a = 1
  @@b = 2
  
  puts 'self: ', self
  puts 'self.class: ', self.class
  puts 'instance: ', instance_variables()
  puts 'class: ', class_variables()
  puts 'self.instance: ', self.instance_variables()
  puts 'self.class: ', self.class.instance_variables()
end

puts '- self in modules --------------------'

module M
  puts self       # M
  puts self.class # Module
end

class C
  a = include M  # C
  puts a
  puts a.class
end

puts '- end ----------------------------'

