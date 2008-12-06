$InitConsts = Object.constants

module Kernel
  C_Kernel = 1
  
  class << self
    $SKernel = self
    C_SKernel = 1
  end
end

class Object
  C_Object = 1
  
  class << self
    $SObject = self
    C_SObject = 1
    
    class << self
      $SSObject = self
      C_SSObject = 1
    end
  end
end

class Module
  C_Module = 1
  
  class << self
    $SModule = self
    C_SModule = 1
    
    class << self
      $SSModule = self
      C_SSModule = 1
    end
  end
end

class Class
  C_Class = 1
  
  class << self
    $SClass = self
    C_SClass = 1
    
    class << self
      $SSClass = self
      C_SSClass = 1
    end
  end
end

module M
  C_M = 1
  
  class << self
    $SM = self
    C_SM = 1
    class << self
      $SSM = self
      C_SSM = 1
    end
  end  
end

module N
  C_N = 1
end

module MSC
  C_MSC = 1
end

module MSSC
  C_MSSC = 1
end

class C
  include M, N
  C_C = 1
  
  class << self
    include MSC
    $SC = self
    C_SC = 1
    
    class << self
      include MSSC
      $SSC = self
      C_SSC = 1
    end
  end
end

class D < C
  C_D = 1
  
  class << self
    $SD = self
    C_SD = 1
    
    class << self
      $SSD = self
      C_SSD = 1
      SSD = 1
    end
  end 
end

def d name,mod
  cons = mod.constants - $InitConsts - ["C","D","M","N","F"]
  
  #not_found = []
  #make_all(mod).each { |c|
  #   mod.const_get c rescue not_found << c
  #}
  
  print "#{name}\t\t#{mod.ancestors.inspect}:\t\t#{cons.sort.inspect}"
  #print "!#{not_found.inspect}" unless not_found.empty?
  puts 
end

def make_all m
  result = []
  m.ancestors.each { |c|
    result << "C_#{c.name}"
  }
  result
end

m = [
"Kernel", Kernel,
"Object", Object,
"Module", Module,
"Class", Class,
"S(Kernel)", $SKernel,
"S(Object)", $SObject,
"S(Module)", $SModule,
"S(Class)", $SClass,
"SS(Object)", $SSObject,
"SS(Module)", $SSModule,
"SS(Class)", $SSClass,
"C", C,
"D", D,
"S(C)", $SC,
"SS(C)", $SSC,
"D(C)", $SSC.superclass,
"S(D)", $SD,
"SS(D)", $SSD,
"D(D)", $SSD.superclass,
"M", M,
"S(M)", $SM,
"SS(M)", $SSM,
"D(M)", $SSM.superclass,
"Array", Array,
"Hash", Hash,
]

for i in 0..(m.size/2 - 1) do
  d m[i*2], m[i*2+1]
end

#p $SC.private_instance_methods(false)
p $SC.instance_methods(false)
