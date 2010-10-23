=begin

Output matches Ruby 1.8 except for 
 - methods on singletons next to the dummy singletons (S3)

=end

require 'Ultimate.Defs'

$clr_only = ["of", "[]", "to_clr_type"]
$all = false

def dump ms
  if $all then
    ((ms - $clr_only).sort).inspect
  else
    ms.delete_if { |name| name.to_s.index("f_") != 0}
    ms.sort.map { |x| x.to_s }.inspect
  end  
end


puts '-- Module#instance_methods(false) ----'
$ordered_names.each { |name| 
  puts "#{name}:\n#{dump $classes[name].instance_methods(false)}"
}

puts '-- Module#instance_methods(true) ----'
$ordered_names.each { |name| 
  puts "#{name}:\n#{dump $classes[name].instance_methods(true)}"
}

puts '-- Kernel#singleton_methods(false) ----'
$ordered_names.each { |name| 
  puts "#{name}:\n#{dump $classes[name].singleton_methods(false)}"
}
puts "obj:\n#{dump $obj.singleton_methods(false)}"
puts "no_singleton:\n#{dump $no_singleton.singleton_methods(false)}"

puts '-- Kernel#singleton_methods(true) ----'
$ordered_names.each { |name| 
  puts "#{name}:\n#{dump $classes[name].singleton_methods(true)}"
}
puts "obj:\n#{dump $obj.singleton_methods(true)}"
puts "no_singleton:\n#{dump $no_singleton.singleton_methods(true)}"

puts '-- Kernel#methods(false) ----'
$ordered_names.each { |name| 
  puts "#{name}:\n#{dump $classes[name].methods(false)}"
}
puts "obj:\n#{dump $obj.methods(false)}"
puts "no_singleton:\n#{dump $no_singleton.methods(false)}"

puts '-- Kernel#methods(true) ----'
$ordered_names.each { |name| 
  puts "#{name}:\n#{dump $classes[name].methods(true)}"
}
puts "obj:\n#{dump $obj.methods(true)}"
puts "no_singleton:\n#{dump $no_singleton.methods(true)}"

puts '-- Kernel#public_methods(false) ----'
$ordered_names.each { |name| 
  puts "#{name}:\n#{dump $classes[name].public_methods(false)}"
}
puts "obj:\n#{dump $obj.public_methods(true)}"
puts "no_singleton:\n#{dump $no_singleton.public_methods(true)}"

puts '-- Kernel#public_methods(true) ----'
$ordered_names.each { |name| 
  puts "#{name}:\n#{dump $classes[name].public_methods(true)}"
}
puts "obj:\n#{dump $obj.public_methods(true)}"
puts "no_singleton:\n#{dump $no_singleton.public_methods(true)}"
