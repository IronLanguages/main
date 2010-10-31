class D
  def foo(a,b)
    puts "D::foo(#{a},#{b})"    
  end

  def goo(a,b)
    puts "D::goo(#{a},#{b})"    
  end
  
  def hoo(a,b)
    puts "D::hoo(#{a},#{b})"        
  end
  
  def ioo(a,b)
    puts "D::ioo(#{a},#{b})"        
  end
  
  def joo(a,b)
    puts "D::joo(#{a},#{b})"        
  end
  
  def super_joo(a,b)
    puts "D::joo(#{a},#{b})"        
  end
  
  def koo(a,b)
    puts "D::koo(#{a},#{b})"        
  end
end

class C < D
  def foo(a,b)
    puts "C::foo(#{a},#{b})"
    super
  end
  
  define_method(:goo) { |a,b|
    puts "C::goo(#{a},#{b})"
    super
  }
  
  def hoo(a,b)
    puts "C::hoo(#{a},#{b})"
	yoo(5,6) { |x,y| super }
  end
  
  def yoo(a,b)
    yield a,b
  end
  
  def ioo(a,b)
    puts "C::ioo(#{a},#{b})"
    p = koo { |x, y| 
      if $emulate 
        if $s.nil? then super(a,b) else send(:"super_#{$s}", x, y) end
      else
        super
      end    
    }
    C::yoo2(5,6,&p) 
  end
  
  def koo &p
    p
  end
  
  def self.yoo2(a,b,&p)
    if $emulate    
      $p = p
      def joo(x,y)
        $s = :joo
        $p.call(x,y)
      end
    else
      define_method(:joo, &p)      
    end  
  end
end

c = C.new
c.foo(1,2)
puts '---'
c.goo(1,2)
puts '---'
c.hoo(1,2)
puts '---'

$emulate = false
c.ioo(10,20)
c.joo(100,200)

$emulate = true
c.ioo(10,20)
c.joo(100,200)
