class C
  def bar
    puts 'C::bar'
    'bar'
  end
  
  def baz
    puts 'C::baz'
    'baz'
  end
end

def foo
  C.new
end

puts <<L1, foo
  .bar
L1
  .baz