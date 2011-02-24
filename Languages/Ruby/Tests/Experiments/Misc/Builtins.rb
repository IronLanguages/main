def dump(mod) 
    puts "#{mod.instance_of?(Module) ? "module" : "class"} #{mod}"
    
    puts "  class: #{mod.class}"
    puts "  super: #{mod.superclass}" if mod.method_defined?(:superclass)
    
    strict_ancestors = mod.ancestors - [mod]
    
    print '  ancestors: '
    mod.ancestors.each { |a| print "#{a}," }
    puts
    puts
    
    puts '  declared private instance methods:'
    ms = mod.private_instance_methods(false)
    ms.sort.each { |m| puts "    #{m}" }

    puts '  declared protected instance methods:'
    ms = mod.protected_instance_methods(false)
    ms.sort.each { |m| puts "    #{m}" }
    
    puts '  declared public instance methods:'
    ms = mod.public_instance_methods(false)
    ms.sort.each { |m| puts "    #{m}" }
    
    puts '  declared singleton methods:'
    ms = mod.singleton_methods(false)
    ms.sort.each { |m| puts "    #{m}" }
    
    puts '  declared class variables:'
    ms = mod.class_variables
    strict_ancestors.each { |a| ms = ms - a.class_variables }
    ms.sort.each { |m| puts "    #{m}" }
    
    puts '  declared constants:'
    ms = mod.constants
    strict_ancestors.each { |a| ms = ms - a.constants }
    ms.sort.each { |m| puts "    #{m}" }
    
    puts 'end'
    puts
end
