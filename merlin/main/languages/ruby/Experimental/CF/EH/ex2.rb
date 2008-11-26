class Ary
  def to_ary
    ['x','y']
  end
end

class E < Exception
end

class C
  def exception *a
    puts "exception #{a.inspect}"
    E.new *a
  end
  
  def set_backtrace *a
    puts "set_backtrace #{a.inspect}"
  end
end

x = C.new

begin
  raise x, "msg", ['x','y']
rescue Exception
  p $!
  p $@
end


