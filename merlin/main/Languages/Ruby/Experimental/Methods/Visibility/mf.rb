module M
  module_function
  
  def foo
  end
  
  def M.bar
  end
  
end

class << M
  ::SM = self
end

p M.instance_method(:foo)
p SM.instance_method(:foo)

