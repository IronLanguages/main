class Module
  public :module_function
  
  def method_added name
    puts "I> #{name}"
  end

  def singleton_method_added name
    puts "S> #{name}"
  end
end

module M
end

class Object
  def foo
    puts "Object::foo"
  end
end

class B
  def self.foo
    puts "B::foo"
  end
end

class C < B
  M.module_function
  
  def foo
    puts "C::foo"
    super
  end

  p private_instance_methods(false)  
  p public_instance_methods(false)  
  p singleton_methods(false)  
end

C.foo
