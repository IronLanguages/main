W = 0

module M
  X = 1
end

module N
  Y = 2
  def cs_defined?
     puts defined? ::W
     puts defined? M::X
     puts defined? X           
     puts defined? Y
     puts defined? Z
  end
end

class C
  include M,N    
  Z = 3
end

C.new.cs_defined?