class C
	def m_public1
		puts 'm_public1'
	end  


	$x = lambda { |x|

		private 
		
		def m_private2
		  puts 'm_private2'
		end	
	
		private
	}
	
	def m_public3
		puts 'm_public3'
	end 
	
	private
	
	2.times { |x|                       # we need a copy of scope per each call
  
		if x == 1 then
  
			def m_private3
				puts 'm_private3'
			end
		
		else
		
			def m_private4
				puts 'm_private4'
			end
		
		end	
		
		private
		
		def m_private1
			puts 'm_private1'
		end

		public    
	}
	
	public
	
	1.times &$x

	def m_public2
		puts 'm_public2'
	end
	
end

x = C.new
x.m_private1 rescue puts $!
x.m_private2 rescue puts $!
x.m_private3 rescue puts $!
x.m_private4 rescue puts $!
x.m_public1
x.m_public2
x.m_public3

p private.class
  

