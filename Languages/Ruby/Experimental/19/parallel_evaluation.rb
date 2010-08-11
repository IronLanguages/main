def foo(i)
  puts "foo(#{i})"
  1
end

class C
  def []=(key, value)
    puts "[#{key}]=#{value}"    
  end
end

a = C.new

a[foo(0)], x, y, a[foo(1)] = 1,2,foo(3)

p x,y,a