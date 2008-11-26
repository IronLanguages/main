class String
  p private_instance_methods(false)
end

x = "xxx"

def x.foo; end
x.instance_variable_set(:@bar, 1); 
x.taint
x.freeze

y = "yyy"

p y.send(:initialize_copy, x)

puts '---'

y.foo rescue p $!
p y.instance_variables
p y.tainted?
p y.frozen?
p y

puts '---'

h = ""
h.freeze
h.send(:initialize_copy, "") rescue p $!

h = "g"
h.send(:initialize_copy, S.new) rescue p $!
p h

puts '---'

class String
  def initialize_copy *a
    puts 'init_copy'
  end
end

x = "qqq"
x.instance_variable_set(:@foo, 1)

y = x.dup
p y 
p y.instance_variables

puts '---'

x = {}
x["foo"] = 1
p x


