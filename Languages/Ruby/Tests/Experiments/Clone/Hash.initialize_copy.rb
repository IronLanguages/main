class Hash
  p private_instance_methods(false)
end

x = Hash.new {}

def x.foo; end
x[1] = 2
x.instance_variable_set(:@bar, 1); 
x.taint
x.freeze

y = {3 => 4}

p y.send(:initialize_copy, x)

puts '---'

y.foo rescue p $!
p y.instance_variables
p y.tainted?
p y.frozen?
p y.default_proc

p y

puts '---'

h = {}
h.freeze
h.send(:initialize_copy, {}) rescue p $!

puts '---'

class Hash
  def initialize_copy *a
    puts 'init_copy'
  end
end

x = Hash.new {}
x[1] = 2
x.instance_variable_set(:@foo, 1)

y = x.dup
p y 
p y.instance_variables
p y.default_proc
