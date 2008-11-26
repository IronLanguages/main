def a; yield; end 
def b; yield 1; end 
def c; yield 1,2; end 
def d; yield []; end 
def e; yield [1]; end 
def f; yield [1,2]; end 
def g; yield [],[]; end 

a { |x| puts x.inspect }  # nil + W
puts '---'
b { |x| puts x.inspect }  # 1
puts '---'
c { |x| puts x.inspect }  # [1,2] + W
puts '---'
d { |x| puts x.inspect }  # []
puts '---'
e { |x| puts x.inspect }  # [1]
puts '---'
f { |x| puts x.inspect }  # [1,2]
puts '---'
g { |x| puts x.inspect }  # [[],[]] + W

puts '***************'

def a; yield; end 
def b; yield 1; end 
def c; yield 1,2; end 
def d; yield []; end 
def e; yield [1]; end 
def f; yield [1,2,3,4]; end 
def g; yield [1,2],*[3,4]; end 

a { |x,y| puts x.inspect, y.inspect }
puts '---'
b { |x,y| puts x.inspect, y.inspect }
puts '---'
c { |x,y| puts x.inspect, y.inspect }
puts '---'
d { |x,y| puts x.inspect, y.inspect }
puts '---'
e { |x,y| puts x.inspect, y.inspect }
puts '---'
f { |x,y| puts x.inspect, y.inspect }
puts '---'
f { |x,y,*z| puts x.inspect, y.inspect, z.inspect  }
puts '---'
g { |x,*y| puts x.inspect, y.inspect }
