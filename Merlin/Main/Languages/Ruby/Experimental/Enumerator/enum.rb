class Module
  def method_added name
    puts "> ma: #{self.inspect}::#{name}"
  end
end

require 'enumerator'

module Kernel
  p public_instance_methods(false) & ["to_enum", "enum_for"]
end

class C
  def f *args,&b
    p args
    2.times(&b)
  end
  
  def each *args,&b
    p args
    5.times(&b)
  end
end


h = C.new
puts '---'
p Enumerable::Enumerator.new.entries rescue p $!
p Enumerable::Enumerator.new(h).entries
p Enumerable::Enumerator.new(h, nil).entries
p Enumerable::Enumerator.new(h, :f, 1).entries
p Enumerable::Enumerator.new(h, :f, 1, 2).entries
p Enumerable::Enumerator.new(h, :f, 1, 2, 3).entries
puts '---'

e = Enumerable::Enumerator.new(h, :f, 1, 2, 3)
e.send(:initialize, h, nil, 'a', 'b')
p e.entries

class Enumerable::Enumerator
  def initialize *args
    p args
  end
end

e = Enumerable::Enumerator.new(h, :f, 1, 2, 3)
p e.entries rescue p $!

