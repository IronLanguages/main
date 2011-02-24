class M < Module
  def init
    puts "initialize -> #{initialize.inspect}"
  end
end

Q = M.new

module Q
  def foo
    puts 'foo'
  end
  
  C = 1
end

p Q.constants
p Q.instance_methods(false)
Q.init
p Q.constants
p Q.instance_methods(false)

puts '---'

p Q


class C 
  include Q
end

C.new.foo

class << Q
  p self
end

puts '---'

R = Q.dup

module R
  def bar
  end
end

p R.name
p R.constants
p R.instance_methods(false)
p Q.instance_methods(false)
