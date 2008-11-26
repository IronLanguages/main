class << Struct.new(:f,:g)
  p private_instance_methods(false)
  p instance_methods(false)
end

x = Struct.new(:a,:b,:c)
y = Struct.new(:x,:y)


y.send(:initialize_copy, x) rescue p $!

ix1 = x[1,2,3]
ix2 = x[10,20,30]
iy1 = y[6,7]
iy2 = y[60,70]

# invalid arg class:
ix1.send(:initialize_copy, iy1) rescue p $!

ix1.send(:initialize_copy, ix2) rescue p $!

p ix1

Y = y
class YY < Y  
end

iyy = YY.new

# error:
iyy.send(:initialize_copy, iy1) rescue p $!

puts '---'

class Struct
  def initialize_copy *a
    puts 'init_copy'
  end
end

x = Struct.new(:a)

p x.private_instance_methods(false)
#y = x.dup
p y.members

class MS < String
end
MS.dup
