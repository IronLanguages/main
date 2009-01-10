objects = [{}, [], '', Regexp.new('foo'), Object.new, Module.new, Class.new]

objects.each do |x| 
    puts x.class.name
    
    class << x
		CONST = 1
		
		def foo
		  3
		end		
	    
        instance_variable_set(:@iv_singleton_x, 2);
    end	

	x.instance_variable_set(:@iv_x, 4);
    y = x.clone
	
	class << y
		# constants copied:
		raise unless CONST == 1
		
        #singleton class methods not copied
        raise unless ((bar;false) rescue true)
        
		# instance variables not copied:
		raise unless instance_variables.size == 0
	end
	
	# methods copied:
    raise unless y.foo == 3
    raise unless y.instance_variable_get(:@iv_x) == 4

end
