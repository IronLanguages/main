f = -> *a, &b {  p a, b } 

p f.class
f.call {}
f.(123) {}

class Proc
  def call *args, &b
    puts "Proc: call #{args} &#{b}"    
  end
end

f.(1,2,3) {}
f::(1,2,3) {}

