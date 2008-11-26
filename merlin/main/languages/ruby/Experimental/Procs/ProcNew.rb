class Class
  alias old_new new
  
  def new *args, &x
    puts "Class#new #{args[0]}, #{args[1]}, #{x}"
    old_new(*args, &x)
  end
end

class P < Proc
  def initialize *args, &x
    puts "P#initialize #{args[0]}, #{args[1]}, #{x}"
    p x.object_id == $y.object_id
    p x
    super
  end
end

$q = 1
class Q < Proc
  def initialize *args, &x
    puts "Q#initialize #{args[0]}, #{args[1]}, #{x.object_id} #{x.class} #{x[]}"
    $q = $q + 1
    retry if $q < 5
  end
end

puts '-- lambda'
$y = lambda { 'foo' }

puts '-- P.new ----------------'
x = P.new(&$y)
p x.class
p x.object_id == $y.object_id

puts '-- Q.new ----------------'
z = Q.new(&x)
p z.class
p z.object_id

puts '-- Proc.new ----------------'
x = Proc.new(&$y)
p x.class
p x.object_id == $y.object_id

puts '-- P.sm:'
p P.singleton_methods(false)

puts '-- Proc.sm:'
p Proc.singleton_methods(false)