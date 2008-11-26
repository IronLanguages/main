class Module
  public :module_function
  
  def method_added name
    puts "I> #{self}::#{name}"
  end

  def singleton_method_added name
    puts "S> #{self}::#{name}"
  end
end

module M
end

# module_function affects the current non-block scope

class C
  def foo
    1.times {
      M.send :module_function      
    }
  
    def yyy                     # mf on C  
    end
  end
  
  x = C.new
  x.foo
  
  def xxx                       # instance on C
  end
end



