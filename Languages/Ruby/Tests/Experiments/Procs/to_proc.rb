class C
  def to_proc
    lambda { |x| puts x }
  end
  
  def respond_to? name
    puts 'respond?'
    true
  end
end

class Proc
  def to_proc
    puts 'to_proc'
  end
end

#class MyProc < Proc
#  def to_proc
#    puts 'to_my_proc'
#  end
#end

class D
  def to_proc
    1
  end
end

class X
end

pr = Proc.new { }

1.times &C.new
1.times &pr
1.times &lambda {} 
1.times { }
1.times &D.new rescue p $!
#1.times &MyProc.new {}
1.times &X.new rescue p $!



