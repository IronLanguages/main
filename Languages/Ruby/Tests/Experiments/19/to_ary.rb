class C
  def to_ary
    1
  end
end

x = C.new

(a,b = x) rescue p $!

puts '---'

class D
  def respond_to?(name)
    p name
    false
  end
  
  def to_a
    puts 'to_a'
    [1,2]
  end
end

x = D.new

(a,b = x) rescue p $!
p a,b

puts '---'

(a,b,c = 1,*x) rescue p $!
p a,b,c