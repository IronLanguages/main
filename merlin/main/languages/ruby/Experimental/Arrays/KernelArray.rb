# behavior differs between 1.8 and 1.9

class A
  def respond_to? name
    puts "? #{name}"
    false
  end
end

A.new.to_ary rescue p $!
p A.new.to_a rescue p $!

module Kernel
  remove_method :to_a rescue p $!
end

puts '---'

p Array(A.new)

puts '---'

class B
  def to_ary
    'to_ary'
  end
  
  def to_a
    'to_a'
  end
  
  def respond_to? name
    puts "? #{name}"
    false
  end
  
  def method_missing *args
    p args
  end
end

p Array(B.new) rescue p $!


