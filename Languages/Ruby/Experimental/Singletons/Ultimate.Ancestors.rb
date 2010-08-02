require 'Ultimate.defs'

$ordered_names.each { |name| 
  puts "#{name}:\n#{$classes[name].ancestors.inspect}"
}
