require 'def.rb'

def d x
  puts "#{x.object_id % 1000}:                         #{x.inspect}, name = '#{x.name}', id = #{x.object_id % 1000}, ancestors = #{x.ancestors.inspect}"
  s = x.superclass
  return if s == nil 
  puts "#{x.object_id % 1000}.super:                   #{s.inspect}, name = '#{s.name}', id = #{s.object_id % 1000}"
  s = s.superclass
  return if s == nil 
  puts "#{x.object_id % 1000}.super.super:             #{s.inspect}, name = '#{s.name}', id = #{s.object_id % 1000}"
  s = s.superclass
  return if s == nil 
  puts "#{x.object_id % 1000}.super.super.super:       #{s.inspect}, name = '#{s.name}', id = #{s.object_id % 1000}"
  s = s.superclass
  return if s == nil 
  puts "#{x.object_id % 1000}.super.super.super.super: #{s.inspect}, name = '#{s.name}', id = #{s.object_id % 1000}"
end

puts '-- superclasses of C --'
d $C
puts '-- superclasses of Sx --'
d $Sx
puts '-- superclasses of Sx1 --'
d $Sx1
puts '-- superclasses of S1 --'
d $S1
puts '-- superclasses of S2 --'
d $S2
puts '-- superclasses of S3 --'
d $S3
puts '-- superclasses of S4 --'
d $S4
puts '-- superclasses of T1 --'
d $T1
puts '-- superclasses of T2 --'
d $T2

