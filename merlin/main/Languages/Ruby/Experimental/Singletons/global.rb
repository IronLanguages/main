puts '-- global instance methods --'

class << self
  ::S0 = self
end

puts 'ancestors:'
p S0.ancestors
puts 'instance:'
p S0.instance_methods(false)
puts 'private:'
p S0.private_methods(false)
puts 'protected:'
p S0.protected_methods(false)
puts 'singleton:'
p S0.singleton_methods(false)

puts '-' * 20

module M1
  def foo
    puts 'foo'
  end
end

module M2
  def bar
    puts 'bar'
  end
end

# 'global' methods
puts 'global:'
class S0
  # undefines "include" instance method on main singleton
  #undef include 
  
  # calls Module::include
  puts 'including'
  p include(M1,M2)
  p ancestors
  p inherited(1)
end

# tries to call "include" on main singleton and fails if it has been undefined
puts 'including'
p include(M1,M2) rescue puts $!.class

foo
bar

p Module.private_methods.include?("include")
