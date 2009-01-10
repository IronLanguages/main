require 'def.rb'

puts '------------------------ instance methods ------------------------'

puts 'C:'
p C.instance_methods(false)
puts 'Sx:'
p Sx.instance_methods(false)
puts 'S1:'
p S1.instance_methods(false)
puts 'S2:'
p S2.instance_methods(false)
puts 'S3:'
p S3.instance_methods(false)
puts 'DUMMY:'
p DUMMY.instance_methods(false)
puts 'Class:'
p Class.instance_methods(false)

puts '------------------------ class methods -----------------------------'

puts 'C:'
p C.singleton_methods(false)
puts 'Sx:'
p Sx.singleton_methods(false)
puts 'S1:'
p S1.singleton_methods(false)
puts 'S2:'
p S2.singleton_methods(false)
puts 'S3:'
p S3.singleton_methods(false)
puts 'DUMMY:'
p DUMMY.singleton_methods(false)
puts 'Class:'
p Class.singleton_methods(false)

puts '------------------------ inherited instance methods ------------------------'

puts 'C:'
p C.instance_methods() - Object.instance_methods()
puts "Sx:"
p Sx.instance_methods() - Object.instance_methods()
puts "S1:"
p S1.instance_methods() - Object.instance_methods()
puts "S2:"
p S2.instance_methods() - Object.instance_methods()
puts "S3:"
p S3.instance_methods() - Object.instance_methods()
puts 'DUMMY:'
p DUMMY.instance_methods() - Object.instance_methods()
puts 'Class:'
p Class.instance_methods() - Object.instance_methods()
