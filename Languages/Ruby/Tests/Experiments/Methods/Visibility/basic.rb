class Object
  def s
    class << self; self; end
  end
  
  def singleton_method_added name
    puts "singleton method added: #{self}##{name}"
  end
end

class Module
  def method_added name
    puts "method added: #{self}##{name}"
  end
end

def dump obj, methodName
  puts "=== #{obj}##{methodName} ==="
  
  if obj.is_a? Module
    puts obj.public_instance_methods.include?(methodName) ? "public" : "-"
    puts obj.private_instance_methods.include?(methodName) ? "private" : "-"
  end
  
  puts obj.s.public_instance_methods.include?(methodName) ? "public" : "-"
  puts obj.s.private_instance_methods.include?(methodName) ? "private" : "-"  
end

$bob = Object.new

class Foo
  
  class << $bob
    private
  end
  
  private
  def $bob.xxx
  end
end

module M
  def yyy  
  end  
end

dump $bob, "xxx"
dump M, "yyy"

module M
  module_function :yyy
end

dump M, "yyy"

module N
  module_function 
  def zzz
  end
end

dump N, "zzz"

module P
  module_function 
  def initialize
  end
end

dump P, "initialize"

class Q
  public
  def initialize
  end
end

dump Q, "initialize"

class S
  def self.initialize
  end
end

dump S, "initialize"





