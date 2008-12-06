require 'def.rb'

def c x
  puts "#{x.name}:                         #{x.inspect}, name = '#{x.name}', id = #{x.object_id}"
  s = x.class
  puts "#{x.name}.class:                   #{s.inspect}, name = '#{s.name}', id = #{s.object_id}"
  s = s.class
  puts "#{x.name}.class.class:             #{s.inspect}, name = '#{s.name}', id = #{s.object_id}"
end

puts '-- classes of C --'
c C
puts '-- classes of Sx --'
c Sx
puts '-- classes of Sy --'
c Sy
puts '-- classes of S1 --'
c S1
puts '-- classes of S2 --'
c S2
puts '-- classes of S3 --'
c S3
puts '-- classes of S4 --'
c S4
puts '-- classes of T1 --'
c T1
puts '-- classes of T2 --'
c T2
