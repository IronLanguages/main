class B
  Q = 'Q in B'
  T = 'T in B'
  W = 'W in B'
end

class D
  D = 'U in D'
end

module M
  P = 'P in M'
  R = 'R in M' 
  V = 'V in M'

  def puts_U
    puts U rescue puts $!
    puts W rescue puts $!
  end
  
  def self.puts_U
    puts U rescue puts $!
    puts W rescue puts $!
  end
end

class C < D
  S = 'S in C'
  Q = 'Q in C'
  P = 'P in C'
  class C < B
    include M    
    S = 'S in C::C'

    # constants in the current scope chain:
    puts C, P, Q, R, S, T
    
    # constants in base class/mixin of the current scope,
    # but not in the current scope chain
    puts U rescue puts $!
    puts V, W
    M.puts_U
    
    # it doesn't matter whether the constants are accessed
    # from instance/class method or the class body -- 
    # the lookup is driven by the inner most module lexical scope
    def ins
      puts '-- ins --'
      puts U rescue puts $!
      puts V, W
      puts_U
    end

    def self.cls
      puts '-- cls --'
      puts U rescue puts $!
      puts V, W
      M.puts_U
    end
  end 
end

C::C.new.ins
C::C.cls

module N
  Z = 'Z in N'
end

module O
  include N,M
  
  puts '-- module --'
  puts P, Z
end

class << Object.new
  P = 'P in singleton'
  class << Object.new
    include M
    
    puts '-- singleton --'
    puts P
  end
end
