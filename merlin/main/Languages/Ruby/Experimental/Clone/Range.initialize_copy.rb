class Range
  p private_instance_methods(false)
end

x = 1...10

def x.foo; end
x.instance_variable_set(:@bar, 1); 
x.taint
x.freeze

y = 4..5

p y.send(:initialize_copy, x)

puts '---'

y.foo rescue p $!
p y.instance_variables
p y.tainted?
p y.frozen?
p y

puts '---'

h = 1..2
h.freeze
h.send(:initialize_copy, 5..6) rescue p $!

puts '---'

class Range
  def initialize_copy *a
    puts 'init_copy'
  end
end

x = 1..3
p x.dup
p y