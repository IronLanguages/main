class K
  private
  module_eval {                    
    def pub1
    end
  }  
  
end

p K.public_instance_methods(false).sort
p K.private_instance_methods(false).sort