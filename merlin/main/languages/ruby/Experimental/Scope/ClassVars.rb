module M
  def foo
    @@m = 5
  end
end

class X
  include M  
  
  def goo
    puts @@m
  end
end

x = X.new
x.foo
x.goo

class A
  @@a = 1
  class B
    @@b = 2
    class C
      @@c = 3
      module D
        @@d = 4
        puts @@a rescue puts "!a"
        puts @@b rescue puts "!b"
        puts @@c rescue puts "!c"
        puts @@d rescue puts "!d"
      end  
    end    
  end  
end

puts M.class_variables
puts A.class_variables
puts A::B.class_variables
puts A::B::C.class_variables
puts A::B::C::D.class_variables
