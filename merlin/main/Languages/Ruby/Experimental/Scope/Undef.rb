module M
  def m1
    puts 'M::m1'
  end
  
  def m2
    puts 'M::m2'
  end
  
  def w
    puts 'M::w'
  end
  
  def u
    undef :w
  end
end

module N
 def m1
    puts 'N::m1'
  end
  
  def m2
    puts 'N::m2'
  end
end


class C
  include M

  def c1
    puts 'c1'
  end
  
  def c2
    puts 'c2'
  end
  
  undef :d2 rescue puts $!
end

class E < C
  undef m1
end

class D < E
  include N
  
  def d1
    puts 'd1'
  end
  
  def d2
    puts 'd2'
  end
  
  def w
    puts 'D::w'
  end
  
  def method_missing name, *a
    puts "missing: #{name}"
  end
end

c = C.new
d = D.new

c.m1
c.m2
c.c1
c.c2
d.m1
d.m2
d.c1
d.c2
d.d1
d.d2
d.u
d.w

