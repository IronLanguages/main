class A
  def self.m_sa
  end
end

x = Object.new

module Kernel
    def singleton_method_added s
      puts "Kernel: #{self}##{s}"
    end

    def self.singleton_method_added s
      puts "s(Kernel): #{self}##{s}"
    end
end

class << x
  class << self
    def singleton_method_added s
      puts "S(x): #{self}##{s}"
    end
  end

  def m_sx
  end
end

class C
  def self.singleton_method_added s
    puts "S(C): #{self}##{s}"
  end

  def self.m_sc
  end  
end

=begin




#p Class.send :inherited, 1

class B
  def self.inherited name
    puts name.class
  end
 
end

class A < B
end

=end