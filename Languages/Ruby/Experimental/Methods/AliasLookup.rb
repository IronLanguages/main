p $K = class C; self; end.ancestors[-1]
p $O = class C; self; end.ancestors[-2]

if Module.nesting.length > 0 then
  p $AM = Module.nesting[0]

  AM = $AM
  module AM
    def i_am
    end
  end
end  

K = $K
module K
  def i_kernel
  end
end

O = $O
class O
  def i_object
  end
end

p AM.constants.sort if defined? AM

module N
	  
	def i_n
	end

	module M
		#undef i_object rescue puts '!undef i_object'
		#undef i_kernel rescue puts '!undef i_kernel'
		#undef i_n rescue puts '!undef i_n'
		#undef i_am rescue puts '!undef i_am'
										  
		alias x1 i_object rescue puts '!alias i_object'
		alias x2 i_kernel rescue puts '!alias i_kernel'
		alias x3 i_n rescue puts '!alias i_n'
		alias x4 i_am rescue puts '!alias i_am'
		
		p instance_methods(false).sort
	end
end

