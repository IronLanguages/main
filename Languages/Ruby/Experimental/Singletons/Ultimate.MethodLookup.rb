module Kernel
  def i_k
    puts 'i_k'
  end

  def self.c_k
    puts 'c_k'
  end 
end

module X
  def i_x
    puts 'i_x'
  end

  def self.c_x
    puts 'c_x'
  end 
end

class Object
  include X

  def i_o
    puts 'i_o'
  end

  def self.c_o
    puts 'c_o'
  end 
end

class Class
  def i_c
    puts 'i_c'
  end

  def self.c_c
    puts 'c_c'
  end 
end

class Module
  def i_m
    puts 'i_m'
  end

  def self.c_m
    puts 'c_m'
  end 
end

$modules = [
    module MyModule; self; end,
    module MyModule; class << self; self; end; end,
    class MyClass; self; end,
    class << Object.new; self; end,
]

def test met, x, y

  $modules.each { |m|
    puts "-- #{m} --------------"
    
    m.module_eval do
      puts send(met, *[:ai_k, :i_k][x,y]) rescue p $!
      puts send(met, *[:ai_x, :i_x][x,y]) rescue p $!
      puts send(met, *[:ai_o, :i_o][x,y]) rescue p $!
      
      # errors:
      
      puts send(met, *[:ac_k, :c_k][x,y]) rescue p $!
      puts send(met, *[:ac_x, :c_x][x,y]) rescue p $!  
      puts send(met, *[:ac_o, :c_o][x,y]) rescue p $!
      
      puts send(met, *[:ai_c, :i_c][x,y]) rescue p $!    
      puts send(met, *[:ac_c, :c_c][x,y]) rescue p $!
      puts send(met, *[:ai_m, :i_m][x,y]) rescue p $!    
      puts send(met, *[:ac_m, :c_m][x,y]) rescue p $!
      
    end
    
    puts
  }
end

def quick_test met, x, y 
  object_lookup = ((MyModule.send(met, *[:ai_x, :i_x][x,y]); true) rescue false)  
  instance_lookup = ((MyModule.send(met, *[:ai_m, :i_m][x,y]); true) rescue false)

  print "#{met}: ", 
    instance_lookup ? ' instance' : '',
    object_lookup ? ' object' : '' 
    
  puts
end

$methods2 = [:alias_method]
$methods1 = [:method_defined?, :method, :public, :undef_method]

# quick test:
$methods2.each { |met| quick_test met, 0, 2 }
$methods1.each { |met| quick_test met, 1, 2 }

# full test:
$methods2.each { |met| 
  puts 
  puts "== #{met} =" + ('=' * 50)
  puts 
  puts 
  test met, 0, 2
}

$methods1.each { |met| 
  puts 
  puts "== #{met} =" + ('=' * 50)
  puts 
  puts 
  test met, 1, 2
}
