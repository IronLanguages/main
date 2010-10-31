require 'enumerator'

p [1,2,3,4,5,6,7].each_slice(-1) { |*args| p args } rescue p $!
p [1,2,3,4,5,6,7].each_cons(0) { |*args| p args } rescue p $!
p [1,2,3,4].each_cons(10) rescue p $!
p [1,2,3,4].each_cons(1) rescue p $!

class D
  include Enumerable

  def initialize values
    @values = values
  end
  
  def each &b
    puts "each: #{b == $b}"
    @values.each(&b)
  end  
end

$b = proc { |*args| p args }


puts '- SLICE -------'
D.new([1,2]).each_slice(3, &$b)
puts '---'
D.new([1,2,3]).each_slice(3, &$b)
puts '---'
D.new([1,2,3,4,5,6,7]).each_slice(3, &$b)
puts '---'
p D.new([1,2,3,4,5,6,7]).each_slice(30) { break 123 }  rescue p $!

puts '- CONS --------'
D.new([1,2]).each_cons(3, &$b)
puts '---'
D.new([1,2,3]).each_cons(3, &$b)
puts '---'
D.new([1,2,3,4,5,6,7]).each_cons(3, &$b)

p D.new([1,2,3,4,5,6,7]).each_cons(30) { break 123 }  rescue p $!



