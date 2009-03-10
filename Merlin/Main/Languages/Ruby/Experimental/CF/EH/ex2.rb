e = Exception.new("msg")
e.set_backtrace ['a', 'b']
e.send(:initialize)
p e.message
p e.backtrace


=begin
class E < Exception
  def backtrace
    puts 'backtrace'
    super
  end
  
  def set_backtrace *a
    puts 'set_backtrace'
    super
  end
end

e = E.new("msg")
e.set_backtrace ['a', 'b']
e.send(:initialize, 'msg2')
p e.message
p e.backtrace

puts '---'
e = E.new("msg")
e.set_backtrace ['a', 'b']
e.send(:initialize)
p e.message
p e.backtrace

=end