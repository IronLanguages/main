=begin

  private_class_method defines methods one by one invoking singleton_method_added notification after each one.
  
=end

class D
  class << self
    def foo
    end
    
    $S = self    
  end  
end

class C < D
  class << self
    
    def singleton_method_added name
      puts "+ #{name}"
      $S.module_eval {
        def bar
        end
      }  
    end  
  end
  private_class_method :foo, :bar
end

