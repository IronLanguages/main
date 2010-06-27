require 'Ultimate.defs'

$ordered_names.each { |name| 
  puts "#{name}:\n#{p $classes[name].ancestors}"
}
