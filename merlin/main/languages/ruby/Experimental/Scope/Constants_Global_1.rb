puts 'self.ancestors:'
class << self
  p ancestors
  $M = ancestors[0]
end

$S1 = self

puts '0:'
p ::C rescue puts $!

puts '1:'
(::C = 'D') rescue puts $!
puts "Object includes constant C?: " + Object.constants.include?("C").to_s
p $M.constants

puts '2a:'
p C rescue puts $!
p ::C rescue puts $!
eval('::C = "C1"') rescue puts $!

puts '2b:'
p ::C rescue puts $!
(::C = "C2") rescue puts $!

puts '3:'
define_c

puts '4:' 
p ::C rescue puts $!

puts '5:' 
p $tlb.object_id == TOPLEVEL_BINDING.object_id

puts '6:' 

def foo
  puts 'foo'
end

foo

puts "Object includes method private foo?: " + Object.private_instance_methods(false).include?("foo").to_s
puts "Object includes method public foo?: " + Object.instance_methods(false).include?("foo").to_s

p $M.instance_methods(false)
p $M.private_instance_methods(false)
p $M.protected_instance_methods(false)
