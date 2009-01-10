module SMx
  CONST_SMx = 1
  $SMx = self

  def i_SMx
	:'i_SMx'
  end
  
  def SMx.c_SMx
    :'c_SMx'
  end
end


module SM1
  CONST_SM1 = 1
  $SM1 = self
  
  def i_SM1
	:'i_SM1'
  end
  
  def SM1.c_SM1
    :'c_SM1'
  end
end

class << $SM1
  CONST_SM1_1 = 1

  $SM1_1 = self
  
  def i_SM1_1
	:'i_SM1_1'
  end
  
  def self.c_SM1_1
    :'c_SM1_1'
  end
end

class D
  CONST_D = 1
  $D = self
  
  def i_D
    :'i_D'
  end
  
  def self.c_D
    :'c_D'
  end
end

class C < D
  CONST_C = 1
  $C = self
  
  def i_C
    :'i_C'
  end
  
  def self.c_C
    :'c_C'
  end
 
#  def order
#    puts "1.A"
#    super
#  end
  
#  def self.order
#    puts "1.B"
#    super
#  end
end

$X = C.new

class << $X
  CONST_Sx = 1
  
  $Sx = self
  
  include SMx
  
  def i_Sx
    :'i_Sx'
  end
  
  def self.c_Sx
    :'c_Sx'
  end
end

class << $Sx
  CONST_Sx1 = 1
  
  $Sx1 = self
  
  def i_Sx1
    :'i_Sx1'
  end
  
  def self.c_Sx1
    :'c_Sx1'
  end
end

class << C
  CONST_S1 = 1
  
  $S1 = self

  include SM1

  def i_S1
    :'i_S1'
  end
  
  def self.c_S1
    :'c_S1'
  end
  
#  def order
#    puts "2.A"
#    super
#  end
  
#  def self.order
#    puts "2.B"
#    super
#  end
  
  
  # the method allocate is defined in each singleton class:
  #undef allocate
  #undef superclass
end

class << $S1
  CONST_S2 = 1
  
  $S2 = self
  
  def i_S2
    :'i_S2'
  end
  
  def self.c_S2
    :'c_S2'
  end
  
end

class << $S2
  CONST_S3 = 1
  
  $S3 = self
  
  def i_S3
    :'i_S3'
  end
  
  def self.c_S3
    :'c_S3'
  end
end

class << $S3
  CONST_S4 = 1
  
  $S4 = self
  
  def i_S4
    :'i_S4'
  end
  
  def self.c_S4
    :'c_S4'
  end
end

class << D
  CONST_T1 = 1
  
  $T1 = self

  def i_T1
    :'i_T1'
  end
  
  def self.c_S1
    :'c_T1'
  end
  
#  def order
#    puts 3
#    super
#  end
end

class << $T1
  CONST_T2 = 1
  
  $T2 = self
  
  def i_T2
    :'i_T2'
  end
  
  def self.c_T2
    :'c_T2'
  end
end

class << $T2
  CONST_T3 = 1
  
  $T3 = self
  
  def i_T3
    :'i_T3'
  end
  
  def self.c_T3
    :'c_T3'
  end
end

class Object
  CONST_Object = 1
  $Object = self
  
  def i_Object
    :'i_Object'
  end
  
  def self.c_Object
    :'c_Object'
  end
end

class Module
  CONST_Module = 1
  $Module = self
  
  def i_Module
    :'i_Module'
  end
  
  def self.c_Module
    :'c_Module'
  end
end

class Class
  CONST_Class = 1
  $Class = self
  
  def i_Class
    :'i_Class'
  end
  
  def self.c_Class
    :'c_Class'
  end
end

class << Object
  CONST_Object1 = 1
  
  $Object1 = self

  def i_Object1
    :'i_Object1'
  end
  
  def self.c_Object1
    :'c_Object1'
  end
  
#  def order
#    puts 4
#    super rescue puts $!.class
#  end
end

class << Module
  CONST_Module1 = 1
  
  $Module1 = self

  def i_Module1
    :'i_Module1'
  end
  
  def self.c_Module1
    :'c_Module1'
  end
end

class << Class
  CONST_Class1 = 1
  
  $Class1 = self
  
  def i_Class1
    :'i_Class1'
  end
  
  def self.c_Class1
    :'c_Class1'
  end
end

# aliases:

d = [
[$Sx, :ai_Sx, :i_Sx],
[$Sx1, :ai_Sx1, :i_Sx1],
[$C, :ai_C, :i_C],
[$S1, :ai_S1, :i_S1],
[$S2, :ai_S2, :i_S2],
[$S3, :ai_S3, :i_S3],
[$D, :ai_D, :i_D],
[$T1, :ai_T1, :i_T1],
[$T2, :ai_T2, :i_T2],
[$T3, :ai_T3, :i_T3],
[$Object, :ai_Object, :i_Object],
[$Object1, :ai_Object1, :i_Object1],
[$Module, :ai_Module, :i_Module],
[$Module1, :ai_Module1, :i_Module1],
[$Class, :ai_Class, :i_Class],
[$Class1, :ai_Class1, :i_Class1],
[$SMx, :ai_SMx, :i_SMx],
[$SM1, :ai_SM1, :i_SM1],
[$SM1_1, :ai_SM1_1, :i_SM1_1],
]

d.each { |c, a, m|
    c.send(:alias_method, a, m)
}

