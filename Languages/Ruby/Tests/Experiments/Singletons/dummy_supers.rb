$C = Object.new

class << $C
  $C1 = self
end

puts $C1.object_id % 1000
puts $C1.superclass.object_id % 1000
puts $C1.superclass.superclass.object_id % 1000
puts $C1.superclass.superclass.superclass.object_id % 1000
puts $C1.superclass.superclass.superclass.superclass.object_id % 1000
puts $C1.superclass.superclass.superclass.superclass.superclass.object_id # end!!!
