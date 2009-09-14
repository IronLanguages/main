class C
  class D
    class ::C
      class E
      end
    end
    
    class F
    end
    
    ::C.module_eval "
      class G
      end
    "
  end
  
  module Z
    class << self
      S = self
      puts S
      
      ::C::D.module_eval "
        class Q
        end
      "
    end
  end
end