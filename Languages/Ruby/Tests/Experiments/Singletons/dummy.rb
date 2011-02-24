$c = Object.new
i = 0
all = []
all_ids = []
while i < 10 do
  all << $c
  all_ids << $c.object_id
  
  puts "#{i}: #{$c.object_id}"
  
  class << $c
    $c = self
  end
  
  i += 1
end

$c = all[1]
puts i, all_ids.uniq.size

puts $c.object_id % 1000
puts $c.superclass.object_id % 1000
puts $c.superclass.superclass.object_id % 1000
puts $c.superclass.superclass.superclass.object_id % 1000
puts $c.superclass.superclass.superclass.superclass.object_id % 1000
puts $c.superclass.superclass.superclass.superclass.superclass.object_id % 1000
puts $c.superclass.superclass.superclass.superclass.superclass.superclass.object_id % 1000
puts $c.superclass.superclass.superclass.superclass.superclass.superclass.superclass.object_id % 1000
puts $c.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.object_id % 1000
puts $c.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.object_id % 1000
puts '---'
# one before dummy
$d = all[9]  
# dummy
$e = $c.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass.superclass
puts $d.object_id % 1000
puts $e.object_id % 1000

class << $e
  $f = self
end

puts '---1'
puts $d.object_id % 1000                 
puts $e.object_id % 1000                 
puts $f.object_id % 1000                 
puts '---2'
puts $d.superclass.object_id % 1000                 
puts $e.superclass.object_id % 1000                 
puts $f.superclass.object_id % 1000                 

puts '---3'
class << $d
  puts self.object_id % 1000
end