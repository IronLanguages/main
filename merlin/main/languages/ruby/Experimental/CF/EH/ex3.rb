require 'mock'

class E < Exception
  def initialize *a
    puts "#{self.inspect}::initialize #{a.inspect}"
  end
end

x = E.new.exception(123)
p x.message

x = Exception.new(nil)
p x

puts '---'

e = E.new 'foo'
e.set_backtrace ['a', 'b']
f = e.exception 'bar'
p e,f,e.backtrace,f.backtrace,e.object_id == f.object_id

puts '---'

e = E.new
e.set_backtrace ['a', 'b']
f = e.exception
p e,f,e.backtrace,f.backtrace,e.object_id == f.object_id