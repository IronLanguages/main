=begin
 
define_method doesn't change constant lookup via lexical scoping 
 
=end

S = 'S on Object' 

class D
  $p = lambda {
    puts S
  }
end

class C
  S = 'S on C'

  define_method :f, &$p
  
  def g
    yield
  end
end

C.new.f
C.new.g &$p