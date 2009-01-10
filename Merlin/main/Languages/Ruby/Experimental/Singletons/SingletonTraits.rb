class C
  $C = self
  class << self
    $SC = self  
    
    def foo
      puts 'foo'
    end
    
    class << self
      $SSC = self  
      class << self
        $SSSC = self  
        $DC = self.superclass
      end
    end
  end
end

$x = C.new
class << $x
  $Sx = self  
  class << self
    $SSx = self  
    class << self
      $SSSx = self  
      $Dx = self.superclass
    end
  end
end

$y = C.new
class << $y
  $Sy = self  
end

$z = C.new
class << $z
  $Sz = self  
  class << self
    $SSz = self  
  end
end

class << self
  $Smain = self
  class << self
    $SSmain = self
    class << self
      $SSSmain = self
      $Dmain = self.superclass
    end
  end
end

module M
  def self.d mod,name
    puts "#{name}.private_instance_methods: #{mod.private_instance_methods(false).sort.inspect}"
    puts "#{name}.protected_instance_methods: #{mod.protected_instance_methods(false).sort.inspect}"
    puts "#{name}.public_instance_methods: #{mod.public_instance_methods(false).sort.inspect}"
    puts "#{name}.instance_methods: #{mod.instance_methods(false).sort.inspect}"
    puts
  end
end

class << $SC
  #undef :nesting
end

[
[$Smain, "S(main)"],
[$SSmain, "SS(main)"],
[$SSSmain, "SSS(main)"],
[$Dmain, "D(main)"],

[$Sx, "S(x)"],
[$SSx, "SS(x)"],
[$SSSx, "SSS(x)"],
[$Dx, "D(x)"],

[$Sy, "S(y)"],

[$Sz, "S(z)"],
[$SSz, "SS(z)"],

[$C, "C"],
[$SC, "S(C)"],
[$SSC, "SS(C)"],
[$SSSC, "SSS(C)"],
[$DC, "D(C)"],

].each { |x,y| M.d x,y }


