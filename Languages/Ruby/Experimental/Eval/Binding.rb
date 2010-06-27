def y
  yield 
end  

class C
  y {
    p self
  }
  
  define_method(:foo) { |&p|
    p self
    p block_given?
    yield rescue puts("ERROR: #{$!}")
    binding    
  }
end

c = C.new
b = c.foo { puts 'foo block' }

p c
eval("p self", b)
eval("puts block_given?", b)
eval("yield", b) rescue puts "ERROR: #{$!}"
