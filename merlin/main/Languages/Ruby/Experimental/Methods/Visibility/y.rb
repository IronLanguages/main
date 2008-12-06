class Module
  def method_added name
    puts "#{self}::#{name}"
  end

  def dump
    puts '---'
  
    puts "#{self}:"
    puts "public: #{public_instance_methods(false).sort.inspect}"
    puts "private: #{private_instance_methods(false).sort.inspect}"
    puts "protected: #{protected_instance_methods(false).sort.inspect}" 
  
    puts "S(#{self}):"
    class << self
      puts "public: #{public_instance_methods(false).delete_if { |x| x[-2..-2] != "_" }.sort.inspect}"
      puts "private: #{private_instance_methods(false).delete_if { |x| x[-2..-1] != "_" }.sort.inspect}"
      puts "protected: #{protected_instance_methods(false).delete_if { |x| x[-2..-1] != "_" }.sort.inspect}"
    end
  
    puts '---'  
  end

end


module M
  def i_M
  end

  module_function

  def c_M
  end
  
  alias ai_M i_M
  alias ac_M c_M
  
  dump
end

module N
  def i_pub_N
  end
  
  private
  
  def i_pri_N
  end
  
  protected
  
  def i_pro_N
  end
    
  alias ai_pub_N i_pub_N
  alias ai_pri_N i_pri_N
  alias ai_pro_N i_pro_N
  
  dump
end