p Struct.singleton_methods(false)

class Class
  alias :old_new :new

  def new *a
    puts "Class#new: #{self}.new(#{a.inspect})"
    begin
      x = old_new *a
    rescue
      puts "!"
      p $!
    end  
    x
  end
end

class Struct
  alias :init :initialize

  def initialize *a
    puts "Struct#initialize: #{a.inspect}"
    puts "                   #{self.inspect}"
    init *a
  end
  
  #class << self
  #  remove_method :new
  #end
end

class S < Struct
  def initialize *a
    puts "S#initialize: #{a.inspect}"
    puts "              #{self.inspect}"
    super
  end
end

p Struct.singleton_methods(false)
p Class.instance_methods(false)

begin

  puts '# no initializer called yet'
  I = S.new(:foo, :bar)
 
  class I
    p superclass
    p singleton_methods(false)
    p instance_methods(false)
    p private_instance_methods(false)
    
    class << self
      alias :old_new :new
      
      def new *a
        puts "I#new: #{self}.new(#{a.inspect})"
        old_new *a
      end	
      
      def [] *a
        puts "I#[]: #{self}.[](#{a.inspect})"
        old_new *a
      end
    end  
      
    def initialize *a   
	  puts "I#initialize: #{a.inspect}"
	  puts "              #{self.inspect}"
	  super
    end
  end
  
  puts "---"
  puts '# now it calls initializer (via new)'
  p I.new("x", "y")
  
  puts "---"
  puts '# now it calls initializer (via [])'
  p I["x", "y"]
  
  puts "---"
  puts '# remove new methods'
  
  class I
    class << self
      remove_method :new
    end
  end
  
  p I.new("U", "V")   # calls Struct#new
  puts '---'
  
  class Struct
    class << self
      remove_method :new
    end
  end
  
  p I.new("U", "V")   # creates the struct instance :)
  puts '---'
  
rescue
  p $!
end

