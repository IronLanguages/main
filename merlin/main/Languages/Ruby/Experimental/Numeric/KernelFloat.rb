class A
  def respond_to? name
    puts "? #{name}"
    false
  end
end

p Float(A.new) rescue p $!

puts '---'

class B  
  def to_f
    'to_f'
  end
  
  def respond_to? name
    puts "? #{name}"
    false
  end
  
  def method_missing *args
    p args
  end
end

p Float(B.new) rescue p $!

p Float("1.3")
p Float(1)
p Float(1.4)
p Float(true) rescue p $!
