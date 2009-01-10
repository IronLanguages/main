x = true
puts !x
puts !!x
puts !!!x

if not x then print 'A' end
if not not x then print 'B' end
if not !x then print 'C' end
if not not !!!x then print 'D' end
# if !(x**2 == 1) then print 'E' end

puts

a = x ? !x : x

class C
  def bar(i, &b)
    puts b[i]
    self
  end
end

c = C.new 
a = !c.bar(1) { |x| x + 1 }.bar(2) { |x| x * 10 }.bar(3) { |x| x * 10 } ? 'X' : 'Y'
puts a

class D
  def foo=()
    print 'foo='
  end
  
  def bar()
    print 'bar'
  end
end

d = D.new
d.foo()


