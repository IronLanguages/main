class C
  def method_missing(*args)
    p args
  end
end

t = C.new
p (t && 4)

t = true 
p (t && 4)

p '---'

t = C.new
p (t &= 4)

t = true 
p (t &= 4)

p '---'

t = C.new
p (t &&= 4)      # doesn't call method '&&', does t = t && 4

t = true 
p (t &&= 4)      # doesn't call method '&&', does t = t && 4

p '---'

t = true
f = false
m = 0
n = nil

t &&= false
f ||= true
m &&= 1
n ||= 2

p t,f,m,n

p '---'

class C
  def [](*args)
    puts "#{args.inspect}"
    1
  end
end

c = C.new
c[0] &&= 1





