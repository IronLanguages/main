class Module
  def method_added name
    puts "I> #{self}::#{name}"
  end
  
  def singleton_method_added name
    puts "S> #{self}::#{name}"
  end
end

module M
  module_function
  
  def initialize
    puts 'foo'
  end
  
  private
  def self.foo
  end
     
  p private_instance_methods(false).sort
  p singleton_methods(false).sort
  
  class << self
    p public_instance_methods(false).map { |x| x.to_s }.include?("foo")
    p private_instance_methods(false).map { |x| x.to_s }.include?("foo")
  end
end

M.initialize rescue p$!
M.foo rescue p$!
