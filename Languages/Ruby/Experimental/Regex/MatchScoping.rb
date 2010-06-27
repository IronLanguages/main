def x
  match = /(1)/.match '1'
  
  eval <<-END
  class C
    p $~
    
	match = /(2)/.match '2'
	  
	module_eval do
	  define_method :foo do
	    1.times { p $~.captures }
	    1.times { match = /(3)/.match('3') }
	    1.times { p $~.captures }
	  end
	end
  end
  END
end

x
C.new.foo
