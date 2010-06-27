class Module
  def method_added name
    puts "#{self}::#{name}"
  end
end

module M
  def i_m
  end

  module_function

  def mf_m
  end
  
  alias ai_m i_m
  alias amf_m mf_m
  
  puts '---'
  
  puts 'M:'
  p public_instance_methods(false)                      # 1.9 bug: instance mf public
  p private_instance_methods(false) 
  
  class << self
    puts 'S(M):'
    p instance_methods(false).delete_if { |x| x[-2..-1] != "_m" }
    p private_methods(false).delete_if { |x| x[-2..-1] != "_m" }
  end
  
end