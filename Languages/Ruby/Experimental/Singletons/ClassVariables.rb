# class variable is defined on the lexically inner most non-singleton class/module 
$x = "foo"

module M
  class C
    $C = self
    @@C = 1
  end

  class << C
    $SC = self
    @@SC = 1
    class_variable_set :@@SC1, 1
  end

  class << $SC
    $SSC = self
    @@SSC = 1
    class_variable_set :@@SSC1, 1
  end
  
  class << $x
    $Sx = self
    @@Sx = 1
    class_variable_set :@@Sx, 1  
  end
  
  class C
    puts @@C
    puts @@SC rescue puts 'C:!@@SC'
    puts @@SSC rescue puts 'C:!@@SSC'
  end
end

module M
  puts @@SC, @@SSC
end

puts '-' * 10

puts "M => #{M.class_variables.inspect}"
puts "M::C => #{M::C.class_variables.inspect}"
puts "S(M::C) => #{$SC.class_variables.inspect}"
puts "SS(M::C) => #{$SSC.class_variables.inspect}"
puts "S(x) => #{$Sx.class_variables.inspect}"

# not supported in IronRuby
#puts "D(M::C) => #{$SSC.superclass.class_variables.inspect}"
