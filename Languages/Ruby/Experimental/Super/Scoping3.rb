=begin
  
Shows that super is not bound to the the current lexical method scope.
  
=end

def y &p; yield; p; end

class C
  def f *a
    puts "C.f #{a.inspect}"
  end
  
  def e *a
    puts "C.e #{a.inspect}"
  end
end

class E
  def g *a
    puts "E.g #{a.inspect}"
  end
end

class A  
  def h *a
    puts "A.h #{a.inspect}"
  end
end

class F < E
  def g *a
    puts "F.g #{a.inspect}"
    
    1.times { 
      super        
      $p = y {
        1.times {
          super    
          y &$q
        }
      }
    }
    
    y &$p          
  end
end

class B < A  
  def h *a
    puts "B.h #{a.inspect}"
    $q = y {
      super        
    }
  end
end

B.new.h 1

puts '--'

F.new.g 2

puts '--'

class D < C
  define_method :f, &$p
  
  def e *a
    puts "D.e #{a.inspect}"    
    y &$p
  end
end

D.new.f(3)       

puts '--'

D.new.e(4)      


