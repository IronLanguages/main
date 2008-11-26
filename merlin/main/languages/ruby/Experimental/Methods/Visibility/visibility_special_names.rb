module M1
  puts 'accessors:'
      
  attr_accessor :initialize, :initialize_copy
  
  p instance_methods(false) 
  p private_instance_methods(false) 
  
end

module M2
  puts 'define_method:'
  
  define_method :initialize do
  end

  define_method :initialize_copy do
  end

  p instance_methods(false) 
  p private_instance_methods(false)   
end

module M3
  puts 'def:'
  
  def initialize
  end

  def initialize_copy
  end

  p instance_methods(false) 
  p private_instance_methods(false) 
end
