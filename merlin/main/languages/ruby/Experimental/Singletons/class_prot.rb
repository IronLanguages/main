class C
end

class << C
  ::CP = self
end

puts 'ancestors:'
p CP.ancestors
puts 'instance:'
p CP.instance_methods(false)
puts 'private:'
p CP.private_methods(false)
puts 'protected:'
p CP.protected_methods(false)
puts 'singleton:'
p CP.singleton_methods(false)

p CP.allocate rescue p $!
p CP.new rescue p $!
p CP.superclass
p CP
