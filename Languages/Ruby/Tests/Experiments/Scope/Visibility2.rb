module M
  1.times { |x|
    
    # visibility overwrites module_function flag
    module_function
    private 
    
    def private1; puts '1'; end
    
    # module_function overwrites visibility flag
    public
    def public3; puts '3'; end
    
    # module_function overwrites visibility flag
    public
    module_function
    def private3; puts '3'; end
    
    # visibility doesn't flow out of the block scope
  }
  
  def public2; puts '2'; end  

  2.times { |x|
    if x == 0
      def public4; puts '4'; end  
    else
      def public5; puts '5'; end  
    end
    
    private
    
    # visibility doesn't flow back to the beginning of the block
  }
  
  public
  1.times {
    private 
    2.times { |x|
      # flows visibility from parent scope  
      
      if x == 0
        def private7; puts '7'; end
      else
        def private8; puts '8'; end
      end      
      
      public
    }
    
    def private9; puts '9'; end    
  }
end

class C
  include M
end

def call_all(t)
  t.private1 rescue puts $!
  t.private3 rescue puts $!
  t.public2 rescue puts $!
  t.public3 rescue puts $!
  t.public4 rescue puts $!
  t.public5 rescue puts $!
  t.private7 rescue puts $!
  t.private8 rescue puts $!
  t.private9 rescue puts $!
end

puts '-- instance on C.new -- '
call_all(C.new)

puts '-- module functions on M --'
call_all(M)
