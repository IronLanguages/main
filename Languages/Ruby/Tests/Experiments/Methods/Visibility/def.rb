class Module
  public :module_function
  
  def method_added name
    puts "I> #{self}::#{name}"
  end

  def singleton_method_added name
    puts "S> #{self}::#{name}"
  end
end

class A
end

class B
end

class C
  A.module_eval do
    B.module_eval do          # 1.8   1.9
      def def1                #  B     B
        def f1                #  C     B
        end
      end
    end
  end
  
  A.module_eval do
    B.module_eval do
      define_method :def2 do  #  B     B
        def f2                #  B     B
        end
      end
    end
  end
end

B.new.def1
B.new.def2
