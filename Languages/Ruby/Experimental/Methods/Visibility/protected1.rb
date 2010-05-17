module Kernel
  def n
    self.class.name
  end
end

class C
  def b
    binding
  end

  protected
  def foo
    puts 'ok'
  end   
end

class X
  def b
    binding
  end
end

class D < C
  def b
    binding
  end
end

$sc = C.new

class << $sc
  def b
    binding
  end
end

$sd = D.new

class << $sd
  def b
    binding
  end
end

$c,$d,$x = C.new,D.new,X.new

['c', 'd', 'sc', 'sd'].each do |target|
  [$c, $d, $sc, $sd, $x].each do |cls|
    print "#{target} in #{cls.n}: "
    eval("$#{target}.foo", cls.b) rescue p $!
  end
end



