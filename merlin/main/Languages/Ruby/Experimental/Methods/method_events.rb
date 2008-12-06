class Module
  def method_added name
    puts "+ #{self}##{name} -> #{self.instance_methods(false).include?(name.to_s)}"    
  end
  
  def method_removed name
    puts "- #{self}##{name} -> #{self.instance_methods(false).include?(name.to_s)}"
  end
  
  def method_undefined name
    puts "U #{self}##{name} -> #{self.instance_methods(false).include?(name.to_s)}"
  end
end

class Object
  def foo
  end
end

#require 'yaml'
#require 'thread'

module M
  def foo
  end
  
  puts '> module_function:'
  module_function :foo
  
  puts '> define_method:'
  define_method :bar do end
  
  puts '> alias_method:'
  alias_method :baz, :foo
  
  puts '> redef method:'
  def foo
  end
  
  puts '> attr:'
  attr :myattr
  
  puts '> remove_method:'
  remove_method :foo

  puts '> undef:'
  undef :bar
  
  
  class << self
    puts '> def S1.f:'
    def f; end  
    
    puts '> def S1.g:'
    def g; end  
  
    puts '> undef S1.f:'
    undef f

    puts '> remove S1.g:'
    remove_method :g

    class << self
      puts '> def S2.f:'
      def f
      end

      puts '> def S2.g:'
      def g; end  
  
      puts '> undef S2.f:'
      undef f     
      
      puts '> remove S2.g:'
      remove_method :g
    end  
  end 
  
end

