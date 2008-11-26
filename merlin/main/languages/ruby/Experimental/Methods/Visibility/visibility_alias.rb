class C
  private
  def foo
  end
  
  public
  alias bar foo 
  
  p private_instance_methods(false)
  p public_instance_methods(false)
  puts '---'
  
  public :bar
  
  p private_instance_methods(false)
  p public_instance_methods(false)  
  puts '---'
end

class D < C
  alias baz foo
  
  p private_instance_methods(false)
  p public_instance_methods(false)  
  puts '---'   
end

class X < D
end

$baz = X.new.method(:baz)
$u_baz = $baz.unbind

$bar = X.new.method(:bar)
$u_bar = $baz.unbind

p $baz, $u_baz, $bar, $u_bar

puts '---'

class Y < C
  define_method :baz1, $baz rescue p $!
  define_method :baz2, $u_baz rescue p $!
  define_method :bar1, $bar rescue p $!
  define_method :bar2, $u_bar rescue p $!
end



