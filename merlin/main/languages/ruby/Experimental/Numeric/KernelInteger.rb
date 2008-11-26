class A
  def respond_to? name
    puts "? #{name}"
    false
  end
end

p Integer(A.new) rescue p $!

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

p Integer(B.new) rescue p $!

p Integer("1.3")  rescue p $!
p Integer("0o111")  rescue p $!
p Integer("0b111")  rescue p $!
p Integer("0x111")  rescue p $!
p Integer("0d111")  rescue p $!
p Integer(1)
p Integer(1.4)
p Integer(true) rescue p $!
